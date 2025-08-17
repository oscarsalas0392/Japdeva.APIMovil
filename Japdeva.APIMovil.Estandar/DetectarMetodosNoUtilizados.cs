using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DetectarMetodosNoUtilizados : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA004";
        private const string Titulo = "Método no utilizado";
        private const string FormatoMensaje = "El método '{0}' está declarado pero nunca se utiliza";
        private const string Descripcion = "Los métodos que se declaran pero nunca se utilizan deben ser removidos para mantener el código limpio y reducir la complejidad.";
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

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarClase, SyntaxKind.ClassDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarStruct, SyntaxKind.StructDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarRecord, SyntaxKind.RecordDeclaration);
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

            // Recopilar todos los métodos declarados en este tipo
            RecopilarMetodosDeclarados(tipoDeclaracion, metodosDeclarados);

            // Recopilar todas las utilizaciones de métodos en este tipo y sus miembros
            RecopilarUtilizacionesMetodos(tipoDeclaracion, metodosUtilizados, modeloSemantico);

            // Reportar métodos no utilizados
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

                // También analizar la expresión de invocación
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
                // En caso de error, asumir que se está utilizando
                metodosUtilizados.Add(identificador.Identifier.ValueText);
            }
        }

        private static bool EsMetodoEspecial(MethodDeclarationSyntax metodo, SemanticModel modeloSemantico)
        {
            // Excluir métodos públicos (pueden ser utilizados externamente)
            if (metodo.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                return true;

            // Excluir métodos protegidos
            if (metodo.Modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword)))
                return true;

            // Excluir métodos virtuales, abstractos u override
            if (metodo.Modifiers.Any(m =>
                m.IsKind(SyntaxKind.VirtualKeyword) ||
                m.IsKind(SyntaxKind.AbstractKeyword) ||
                m.IsKind(SyntaxKind.OverrideKeyword)))
                return true;

            // Excluir métodos con atributos especiales
            if (TieneAtributosEspeciales(metodo))
                return true;

            // Verificar implementación de interfaces de manera compatible con .NET 9
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

            // Excluir métodos de prueba (que contengan "Test" en el nombre)
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
                        // Verificar si el método actual implementa el método de la interfaz
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

    // Extensión para verificar si un método implementa una interfaz
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
                        // Verificar si el método actual implementa el método de la interfaz
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

