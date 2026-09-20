using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DetectarMetodosNoUtilizados : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA004";
        private const string Titulo = "M�todo no utilizado";
        private const string FormatoMensaje = "El m�todo '{0}' est� declarado pero nunca se utiliza";
        private const string Descripcion = "Los m�todos que se declaran pero nunca se utilizan deben ser removidos para mantener el c�digo limpio y reducir la complejidad.";
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0051");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        // Cache para conteo de referencias por archivo
        private static readonly ConcurrentDictionary<string, Dictionary<string, int>> _cacheConteoReferencias = 
            new ConcurrentDictionary<string, Dictionary<string, int>>();

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            
            // OPTIMIZACIÓN: Análisis a nivel de documento completo
            contexto.RegisterSyntaxTreeAction(AnalizarArbolSintactico);
        }

        private static void AnalizarArbolSintactico(SyntaxTreeAnalysisContext contexto)
        {
            var raiz = contexto.Tree.GetRoot();
            var cacheKey = contexto.Tree.FilePath + "_" + raiz.GetHashCode();
            
            // OPTIMIZACIÓN: Cache de conteo de referencias
            if (!_cacheConteoReferencias.TryGetValue(cacheKey, out var conteoReferencias))
            {
                conteoReferencias = new Dictionary<string, int>();
                
                // Contar todas las referencias de identificadores (métodos)
                var identificadores = raiz.DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Select(id => id.Identifier.ValueText)
                    .Where(name => !string.IsNullOrEmpty(name));
                
                // También contar invocaciones de métodos
                var invocaciones = raiz.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Select(inv => ExtraerNombreMetodo(inv))
                    .Where(name => !string.IsNullOrEmpty(name));
                
                // También contar accesos a miembros
                var accesosMiembros = raiz.DescendantNodes()
                    .OfType<MemberAccessExpressionSyntax>()
                    .Select(ma => ma.Name.Identifier.ValueText)
                    .Where(name => !string.IsNullOrEmpty(name));
                
                // Combinar todas las referencias
                var todasLasReferencias = identificadores.Concat(invocaciones).Concat(accesosMiembros);
                
                foreach (var referencia in todasLasReferencias)
                {
                    if (conteoReferencias.ContainsKey(referencia))
                        conteoReferencias[referencia]++;
                    else
                        conteoReferencias[referencia] = 1;
                }
                
                _cacheConteoReferencias.TryAdd(cacheKey, conteoReferencias);
            }

            // Analizar tipos para métodos no utilizados
            var tipos = raiz.DescendantNodes().Where(n => 
                n.IsKind(SyntaxKind.ClassDeclaration) || 
                n.IsKind(SyntaxKind.StructDeclaration) || 
                n.IsKind(SyntaxKind.RecordDeclaration));

            foreach (var tipo in tipos)
            {
                AnalizarTipoOptimizado(contexto, tipo, conteoReferencias);
            }
        }

        private static string ExtraerNombreMetodo(InvocationExpressionSyntax invocacion)
        {
            if (invocacion.Expression is IdentifierNameSyntax identificador)
            {
                return identificador.Identifier.ValueText;
            }
            else if (invocacion.Expression is MemberAccessExpressionSyntax accesoMiembro)
            {
                return accesoMiembro.Name.Identifier.ValueText;
            }
            return string.Empty;
        }

        private static void AnalizarTipoOptimizado(SyntaxTreeAnalysisContext contexto, SyntaxNode tipo, Dictionary<string, int> conteoReferencias)
        {
            var miembros = GetMiembros(tipo);
            if (miembros == null) return;

            // Verificar si el tipo implementa una interfaz
            var tieneInterfaz = TipoImplementaInterfaz(tipo);

            // OPTIMIZACIÓN: Procesar solo métodos
            var metodos = miembros.Value.OfType<MethodDeclarationSyntax>()
                .Where(m => !EsMetodoEspecialRapido(m));

            foreach (var metodo in metodos)
            {
                var nombreMetodo = metodo.Identifier.ValueText;
                
                // Obtener el conteo de referencias para este método
                var numeroReferencias = conteoReferencias.ContainsKey(nombreMetodo) 
                    ? conteoReferencias[nombreMetodo] 
                    : 0;

                // Aplicar lógica basada en si tiene interfaz o no
                bool esNoUtilizado = false;
                
                if (tieneInterfaz)
                {
                    // Si tiene interfaz, las referencias deben ser mayor a 1
                    // (1 sería solo la declaración)
                    esNoUtilizado = numeroReferencias <= 1;
                }
                else
                {
                    // Si no tiene interfaz, las referencias deben ser mayor a 0
                    esNoUtilizado = numeroReferencias <= 0;
                }

                if (esNoUtilizado)
                {
                    var diagnostico = Diagnostic.Create(
                        Regla,
                        metodo.Identifier.GetLocation(),
                        nombreMetodo);

                    contexto.ReportDiagnostic(diagnostico);
                }
            }
        }

        private static SyntaxList<MemberDeclarationSyntax>? GetMiembros(SyntaxNode tipo)
        {
            // Usar if-else en lugar de switch expression para C# 7.3
            if (tipo is ClassDeclarationSyntax clase)
                return clase.Members;
            if (tipo is StructDeclarationSyntax estructura)
                return estructura.Members;
            if (tipo is RecordDeclarationSyntax record)
                return record.Members;
            return null;
        }

        private static bool TipoImplementaInterfaz(SyntaxNode tipo)
        {
            // Verificar si el tipo implementa alguna interfaz
            BaseListSyntax baseList = null;
            
            if (tipo is ClassDeclarationSyntax clase)
                baseList = clase.BaseList;
            else if (tipo is StructDeclarationSyntax estructura)
                baseList = estructura.BaseList;
            else if (tipo is RecordDeclarationSyntax record)
                baseList = record.BaseList;
            
            if (baseList == null || !baseList.Types.Any())
                return false;

            // Verificar si alguno de los tipos base es una interfaz
            // Las interfaces generalmente empiezan con 'I' seguido de mayúscula
            // o contienen la palabra "Interface" en el nombre
            foreach (var baseType in baseList.Types)
            {
                var nombreTipo = baseType.Type.ToString();
                
                // Verificar patrones comunes de interfaces
                if (nombreTipo.StartsWith("I") && nombreTipo.Length > 1 && 
                    char.IsUpper(nombreTipo[1]))
                {
                    return true;
                }
                
                if (nombreTipo.Contains("Interface"))
                {
                    return true;
                }
            }
            
            return false;
        }

        private static bool EsMetodoEspecialRapido(MethodDeclarationSyntax metodo)
        {
            // OPTIMIZACIÓN: Verificaciones rápidas sin análisis semántico
            var modifiers = metodo.Modifiers;
            
            // Métodos públicos, protegidos o internos
            if (modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword) || 
                                  m.IsKind(SyntaxKind.ProtectedKeyword) ||
                                  m.IsKind(SyntaxKind.InternalKeyword)))
            {
                return true;
            }

            // Métodos virtuales, abstractos u override
            if (modifiers.Any(m =>
                m.IsKind(SyntaxKind.VirtualKeyword) ||
                m.IsKind(SyntaxKind.AbstractKeyword) ||
                m.IsKind(SyntaxKind.OverrideKeyword)))
            {
                return true;
            }

            // Métodos con atributos (probablemente especiales)
            if (metodo.AttributeLists.Count > 0)
            {
                return true;
            }

            // Métodos que empiezan con _ (convención para métodos privados especiales)
            if (metodo.Identifier.ValueText.StartsWith("_"))
            {
                return true;
            }

            // Main method
            if (metodo.Identifier.ValueText.Equals("Main", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Métodos de prueba
            if (metodo.Identifier.ValueText.IndexOf("Test", System.StringComparison.OrdinalIgnoreCase) != -1)
            {
                return true;
            }

            return false;
        }

        private static void AnalizarClase(SyntaxNodeAnalysisContext contexto)
        {
            var clase = (ClassDeclarationSyntax)contexto.Node;
            AnalizarTipoParaMetodos(contexto, clase);
        }

        private static void AnalizarStruct(SyntaxNodeAnalysisContext contexto)
        {
            var estructura = (StructDeclarationSyntax)contexto.Node;
            AnalizarTipoParaMetodos(contexto, estructura);
        }

        private static void AnalizarRecord(SyntaxNodeAnalysisContext contexto)
        {
            var record = (RecordDeclarationSyntax)contexto.Node;
            AnalizarTipoParaMetodos(contexto, record);
        }

        private static void AnalizarTipoParaMetodos(SyntaxNodeAnalysisContext contexto, TypeDeclarationSyntax tipoDeclaracion)
        {
            var modeloSemantico = contexto.SemanticModel;
            var metodosDeclarados = new Dictionary<string, MethodDeclarationSyntax>();
            var metodosUtilizados = new HashSet<string>();

            // Recopilar todos los m�todos declarados en este tipo
            RecopilarMetodosDeclarados(tipoDeclaracion, metodosDeclarados);

            // Recopilar todas las utilizaciones de m�todos en este tipo y sus miembros
            RecopilarUtilizacionesMetodos(tipoDeclaracion, metodosUtilizados, modeloSemantico);

            // Reportar m�todos no utilizados
            foreach (var metodo in metodosDeclarados)
            {
                var nombreMetodo = metodo.Key;
                var declaracionMetodo = metodo.Value;

                if (!metodosUtilizados.Contains(nombreMetodo) &&
                    !EsMetodoEspecial(declaracionMetodo, modeloSemantico))
                {
                    var diagnostico = Diagnostic.Create(
                        Regla,
                        declaracionMetodo.Identifier.GetLocation(),
                        nombreMetodo);

                    contexto.ReportDiagnostic(diagnostico);
                }
            }
        }

        private static void RecopilarMetodosDeclarados(TypeDeclarationSyntax tipoDeclaracion, Dictionary<string, MethodDeclarationSyntax> metodosDeclarados)
        {
            foreach (var miembro in tipoDeclaracion.Members)
            {
                if (miembro is MethodDeclarationSyntax metodo)
                {
                    var nombreMetodo = metodo.Identifier.ValueText;
                    if (!metodosDeclarados.ContainsKey(nombreMetodo))
                    {
                        metodosDeclarados[nombreMetodo] = metodo;
                    }
                }
            }
        }

        private static void RecopilarUtilizacionesMetodos(SyntaxNode nodo, HashSet<string> metodosUtilizados, SemanticModel modeloSemantico)
        {
            foreach (var descendiente in nodo.DescendantNodes())
            {
                switch (descendiente)
                {
                    case InvocationExpressionSyntax invocacion:
                        AnalizarInvocacionMetodo(invocacion, metodosUtilizados, modeloSemantico);
                        break;

                    case MemberAccessExpressionSyntax accesoMiembro:
                        AnalizarAccesoMiembro(accesoMiembro, metodosUtilizados, modeloSemantico);
                        break;

                    case IdentifierNameSyntax identificador:
                        AnalizarIdentificadorMetodo(identificador, metodosUtilizados, modeloSemantico);
                        break;
                }
            }
        }

        private static void AnalizarInvocacionMetodo(InvocationExpressionSyntax invocacion, HashSet<string> metodosUtilizados, SemanticModel modeloSemantico)
        {
            try
            {
                var infoSimbolo = modeloSemantico.GetSymbolInfo(invocacion);
                var simbolo = infoSimbolo.Symbol as IMethodSymbol;

                if (simbolo != null)
                {
                    metodosUtilizados.Add(simbolo.Name);
                }

                // Tambi�n analizar la expresi�n de invocaci�n
                if (invocacion.Expression is MemberAccessExpressionSyntax accesoMiembro)
                {
                    metodosUtilizados.Add(accesoMiembro.Name.Identifier.ValueText);
                }
                else if (invocacion.Expression is IdentifierNameSyntax identificador)
                {
                    metodosUtilizados.Add(identificador.Identifier.ValueText);
                }
            }
            catch
            {
                // En caso de error, intentar obtener el nombre directamente
                if (invocacion.Expression is IdentifierNameSyntax identificador)
                {
                    metodosUtilizados.Add(identificador.Identifier.ValueText);
                }
            }
        }

        private static void AnalizarAccesoMiembro(MemberAccessExpressionSyntax accesoMiembro, HashSet<string> metodosUtilizados, SemanticModel modeloSemantico)
        {
            try
            {
                var infoSimbolo = modeloSemantico.GetSymbolInfo(accesoMiembro);
                var simbolo = infoSimbolo.Symbol;

                if (simbolo is IMethodSymbol metodo)
                {
                    metodosUtilizados.Add(metodo.Name);
                }
            }
            catch
            {
                // En caso de error, usar el nombre del miembro
                metodosUtilizados.Add(accesoMiembro.Name.Identifier.ValueText);
            }
        }

        private static void AnalizarIdentificadorMetodo(IdentifierNameSyntax identificador, HashSet<string> metodosUtilizados, SemanticModel modeloSemantico)
        {
            try
            {
                var infoSimbolo = modeloSemantico.GetSymbolInfo(identificador);
                var simbolo = infoSimbolo.Symbol;

                if (simbolo is IMethodSymbol metodo)
                {
                    metodosUtilizados.Add(metodo.Name);
                }
            }
            catch
            {
                // En caso de error, asumir que se est� utilizando
                metodosUtilizados.Add(identificador.Identifier.ValueText);
            }
        }

        private static bool EsMetodoEspecial(MethodDeclarationSyntax metodo, SemanticModel modeloSemantico)
        {
            // Excluir m�todos p�blicos (pueden ser utilizados externamente)
            if (metodo.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                return true;

            // Excluir m�todos protegidos
            if (metodo.Modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword)))
                return true;

            // Excluir m�todos virtuales, abstractos u override
            if (metodo.Modifiers.Any(m =>
                m.IsKind(SyntaxKind.VirtualKeyword) ||
                m.IsKind(SyntaxKind.AbstractKeyword) ||
                m.IsKind(SyntaxKind.OverrideKeyword)))
                return true;

            // Excluir m�todos con atributos especiales
            if (TieneAtributosEspeciales(metodo))
                return true;

            // Verificar implementaci�n de interfaces de manera compatible con .NET 9
            try
            {
                var simboloMetodo = modeloSemantico.GetDeclaredSymbol(metodo) as IMethodSymbol;
                if (simboloMetodo != null && ImplementaInterfaz(simboloMetodo))
                    return true;
            }
            catch
            {
                // En caso de error, asumir que es especial
                return true;
            }

            // Excluir m�todos de prueba (que contengan "Test" en el nombre)
            if (metodo.Identifier.ValueText.IndexOf("Test", System.StringComparison.OrdinalIgnoreCase) != -1)
                return true;

            // Excluir Main method
            if (metodo.Identifier.ValueText.Equals("Main", System.StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        private static bool ImplementaInterfaz(IMethodSymbol simboloMetodo)
        {
            try
            {
                var tipoContenedor = simboloMetodo.ContainingType;

                // Verificar si el tipo implementa alguna interfaz
                if (!tipoContenedor.AllInterfaces.Any())
                    return false;

                // Verificar cada interfaz implementada
                foreach (var interfaz in tipoContenedor.AllInterfaces)
                {
                    var miembrosInterfaz = interfaz.GetMembers().OfType<IMethodSymbol>();

                    foreach (var metodoInterfaz in miembrosInterfaz)
                    {
                        // Verificar si el m�todo actual implementa el m�todo de la interfaz
                        var implementacion = tipoContenedor.FindImplementationForInterfaceMember(metodoInterfaz);
                        if (implementacion != null && SymbolEqualityComparer.Default.Equals(implementacion, simboloMetodo))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                // En caso de error, asumir que implementa interfaz
                return true;
            }
        }

        private static bool TieneAtributosEspeciales(MethodDeclarationSyntax metodo)
        {
            var atributosEspeciales = new[]
            {
                "Test", "TestMethod", "Fact", "Theory", "TestCase",
                "HttpGet", "HttpPost", "HttpPut", "HttpDelete", "HttpPatch",
                "Route", "ActionName", "Obsolete", "OnActionExecuting",
                "OnActionExecuted", "Authorize", "AllowAnonymous"
            };

            foreach (var listaAtributos in metodo.AttributeLists)
            {
                foreach (var atributo in listaAtributos.Attributes)
                {
                    var nombreAtributo = atributo.Name.ToString();

                    if (atributosEspeciales.Any(a =>
                        nombreAtributo.IndexOf(a, System.StringComparison.OrdinalIgnoreCase) != -1))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    // Extensi�n para verificar si un m�todo implementa una interfaz
    internal static class MethodSymbolExtensions
    {
        public static bool ImplementsInterface(IMethodSymbol simboloMetodo)
        {
            try
            {
                var tipoContenedor = simboloMetodo.ContainingType;

                // Verificar si el tipo implementa alguna interfaz
                if (!tipoContenedor.AllInterfaces.Any())
                    return false;

                // Verificar cada interfaz implementada
                foreach (var interfaz in tipoContenedor.AllInterfaces)
                {
                    var miembrosInterfaz = interfaz.GetMembers().OfType<IMethodSymbol>();

                    foreach (var metodoInterfaz in miembrosInterfaz)
                    {
                        // Verificar si el m�todo actual implementa el m�todo de la interfaz
                        var implementacion = tipoContenedor.FindImplementationForInterfaceMember(metodoInterfaz);
                        if (implementacion != null && SymbolEqualityComparer.Default.Equals(implementacion, simboloMetodo))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                // En caso de error, asumir que implementa interfaz
                return true;
            }
        }
    }
}

