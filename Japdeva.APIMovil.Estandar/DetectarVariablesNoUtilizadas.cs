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
    public class DetectarVariablesNoUtilizadas : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA003";
        private const string Titulo = "Variable no utilizada";
        private const string FormatoMensaje = "La variable '{0}' est� declarada pero nunca se utiliza";
        private const string Descripcion = "Las variables que se declaran pero nunca se utilizan deben ser removidas para mantener el c�digo limpio y evitar confusi�n.";
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0059");

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
                
                // Contar todas las referencias de identificadores (variables)
                var identificadores = raiz.DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Select(id => id.Identifier.ValueText)
                    .Where(name => !string.IsNullOrEmpty(name));
                
                foreach (var referencia in identificadores)
                {
                    if (conteoReferencias.ContainsKey(referencia))
                        conteoReferencias[referencia]++;
                    else
                        conteoReferencias[referencia] = 1;
                }
                
                _cacheConteoReferencias.TryAdd(cacheKey, conteoReferencias);
            }

            // Analizar métodos y constructores para variables no utilizadas
            var metodosYConstructores = raiz.DescendantNodes().Where(n => 
                n.IsKind(SyntaxKind.MethodDeclaration) || 
                n.IsKind(SyntaxKind.ConstructorDeclaration));

            foreach (var metodoOConstructor in metodosYConstructores)
            {
                AnalizarMetodoOptimizado(contexto, metodoOConstructor, conteoReferencias);
            }
        }

        private static void AnalizarMetodoOptimizado(SyntaxTreeAnalysisContext contexto, SyntaxNode metodoOConstructor, Dictionary<string, int> conteoReferenciasGlobal)
        {
            BlockSyntax bloque = null;
            
            if (metodoOConstructor is MethodDeclarationSyntax metodo)
                bloque = metodo.Body;
            else if (metodoOConstructor is ConstructorDeclarationSyntax constructor)
                bloque = constructor.Body;
                
            if (bloque == null) return;

            // Verificar si el método/constructor está en una clase que implementa interfaz
            var claseContenedora = metodoOConstructor.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            var tieneInterfaz = claseContenedora != null && TipoImplementaInterfaz(claseContenedora);

            // Recopilar variables declaradas en este bloque
            var variablesDeclaradas = new Dictionary<string, VariableDeclaratorSyntax>();
            RecopilarDeclaracionesVariablesOptimizado(bloque, variablesDeclaradas);

            // Contar referencias específicamente en este método para las variables declaradas aquí
            var conteoReferenciasLocal = new Dictionary<string, int>();
            foreach (var nombreVariable in variablesDeclaradas.Keys)
            {
                conteoReferenciasLocal[nombreVariable] = ContarReferenciasEnBloque(bloque, nombreVariable);
            }

            // Analizar cada variable declarada
            foreach (var variable in variablesDeclaradas)
            {
                var nombreVariable = variable.Key;
                var declaradorVariable = variable.Value;

                if (EsVariableEspecial(nombreVariable, declaradorVariable))
                    continue;

                // Obtener el conteo de referencias para esta variable en este método
                var numeroReferencias = conteoReferenciasLocal.ContainsKey(nombreVariable) 
                    ? conteoReferenciasLocal[nombreVariable] 
                    : 0;

                // Para variables locales, simplificar: debe usarse al menos una vez después de la declaración
                // Contar solo usos (excluyendo la declaración)
                var usosRealEs = ContarUsosExcluyendoDeclaracion(bloque, nombreVariable, declaradorVariable);
                
                // Aplicar lógica basada en si la clase tiene interfaz o no
                bool esNoUtilizada = false;
                
                if (tieneInterfaz)
                {
                    // Si tiene interfaz, debe tener al menos 1 uso real (sin contar declaración)
                    esNoUtilizada = usosRealEs <= 0;
                }
                else
                {
                    // Si no tiene interfaz, debe tener al menos 1 uso real (sin contar declaración)
                    esNoUtilizada = usosRealEs <= 0;
                }

                if (esNoUtilizada)
                {
                    var diagnostico = Diagnostic.Create(
                        Regla,
                        declaradorVariable.Identifier.GetLocation(),
                        nombreVariable);

                    contexto.ReportDiagnostic(diagnostico);
                }
            }
        }

        private static int ContarReferenciasEnBloque(SyntaxNode bloque, string nombreVariable)
        {
            var contador = 0;
            
            foreach (var nodo in bloque.DescendantNodes())
            {
                if (nodo is IdentifierNameSyntax identificador && 
                    identificador.Identifier.ValueText == nombreVariable)
                {
                    contador++;
                }
            }
            
            return contador;
        }

        private static int ContarUsosExcluyendoDeclaracion(SyntaxNode bloque, string nombreVariable, VariableDeclaratorSyntax declaracion)
        {
            var contador = 0;
            var lineaDeclaracion = declaracion.GetLocation().GetLineSpan().StartLinePosition.Line;
            
            foreach (var nodo in bloque.DescendantNodes())
            {
                if (nodo is IdentifierNameSyntax identificador && 
                    identificador.Identifier.ValueText == nombreVariable)
                {
                    var lineaUso = identificador.GetLocation().GetLineSpan().StartLinePosition.Line;
                    
                    // Solo contar si no es la misma línea que la declaración
                    // (o si está después en la misma línea, lo cual indicaría uso real)
                    if (lineaUso > lineaDeclaracion)
                    {
                        contador++;
                    }
                    else if (lineaUso == lineaDeclaracion)
                    {
                        // Si está en la misma línea, verificar si está después de la declaración
                        var columnaDeclaracion = declaracion.GetLocation().GetLineSpan().StartLinePosition.Character;
                        var columnaUso = identificador.GetLocation().GetLineSpan().StartLinePosition.Character;
                        
                        if (columnaUso > columnaDeclaracion)
                        {
                            contador++;
                        }
                    }
                }
            }
            
            return contador;
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

        private static void RecopilarDeclaracionesVariablesOptimizado(SyntaxNode nodo, Dictionary<string, VariableDeclaratorSyntax> variablesDeclaradas)
        {
            // OPTIMIZACIÓN: Recopilar declaraciones de manera más eficiente
            foreach (var declaracion in nodo.DescendantNodes())
            {
                switch (declaracion)
                {
                    case LocalDeclarationStatementSyntax declaracionLocal:
                        // Manejar todas las declaraciones locales, incluidas las using como: using var pbkdf2 = new Rfc2898DeriveBytes(...)
                        foreach (var variable in declaracionLocal.Declaration.Variables)
                        {
                            var nombreVariable = variable.Identifier.ValueText;
                            if (!variablesDeclaradas.ContainsKey(nombreVariable))
                            {
                                variablesDeclaradas[nombreVariable] = variable;
                            }
                        }
                        break;

                    case ForEachStatementSyntax forEachStatement:
                        var nombreForEach = forEachStatement.Identifier.ValueText;
                        if (!variablesDeclaradas.ContainsKey(nombreForEach))
                        {
                            var declaradorFicticio = SyntaxFactory.VariableDeclarator(forEachStatement.Identifier);
                            variablesDeclaradas[nombreForEach] = declaradorFicticio;
                        }
                        break;

                    case ForStatementSyntax forStatement:
                        if (forStatement.Declaration != null)
                        {
                            foreach (var variable in forStatement.Declaration.Variables)
                            {
                                var nombreVariable = variable.Identifier.ValueText;
                                if (!variablesDeclaradas.ContainsKey(nombreVariable))
                                {
                                    variablesDeclaradas[nombreVariable] = variable;
                                }
                            }
                        }
                        break;

                    case UsingStatementSyntax usingStatement:
                        if (usingStatement.Declaration != null)
                        {
                            foreach (var variable in usingStatement.Declaration.Variables)
                            {
                                var nombreVariable = variable.Identifier.ValueText;
                                if (!variablesDeclaradas.ContainsKey(nombreVariable))
                                {
                                    variablesDeclaradas[nombreVariable] = variable;
                                }
                            }
                        }
                        break;

                    case CatchClauseSyntax catchClause:
                        if (catchClause.Declaration?.Identifier != null && 
                            !catchClause.Declaration.Identifier.IsKind(SyntaxKind.None))
                        {
                            var nombreCatch = catchClause.Declaration.Identifier.ValueText;
                            if (!string.IsNullOrEmpty(nombreCatch) && !variablesDeclaradas.ContainsKey(nombreCatch))
                            {
                                var declaradorFicticio = SyntaxFactory.VariableDeclarator(catchClause.Declaration.Identifier);
                                variablesDeclaradas[nombreCatch] = declaradorFicticio;
                            }
                        }
                        break;
                }
            }
        }

        private static void AnalizarBloqueParaVariables(SyntaxNodeAnalysisContext contexto, BlockSyntax bloque)
        {
            var modeloSemantico = contexto.SemanticModel;
            var variablesDeclaradas = new Dictionary<string, VariableDeclaratorSyntax>();
            var variablesUtilizadas = new HashSet<string>();

            // Recopilar todas las declaraciones de variables en este bloque (sin recursi�n)
            RecopilarDeclaracionesVariables(bloque, variablesDeclaradas);

            // Recopilar todas las utilizaciones de variables en este bloque
            RecopilarUtilizacionesVariables(bloque, variablesUtilizadas, modeloSemantico);

            // Reportar variables no utilizadas
            foreach (var variable in variablesDeclaradas)
            {
                var nombreVariable = variable.Key;
                var declaradorVariable = variable.Value;

                if (!variablesUtilizadas.Contains(nombreVariable) &&
                    !EsVariableEspecial(nombreVariable, declaradorVariable))
                {
                    var diagnostico = Diagnostic.Create(
                        Regla,
                        declaradorVariable.Identifier.GetLocation(),
                        nombreVariable);

                    contexto.ReportDiagnostic(diagnostico);
                }
            }
        }

        private static void RecopilarDeclaracionesVariables(SyntaxNode nodo, Dictionary<string, VariableDeclaratorSyntax> variablesDeclaradas)
        {
            // Solo analizar nodos directos, sin recursi�n profunda para evitar problemas
            foreach (var statement in nodo.ChildNodes())
            {
                switch (statement)
                {
                    case LocalDeclarationStatementSyntax declaracionLocal:
                        foreach (var variable in declaracionLocal.Declaration.Variables)
                        {
                            var nombreVariable = variable.Identifier.ValueText;
                            if (!variablesDeclaradas.ContainsKey(nombreVariable))
                            {
                                variablesDeclaradas[nombreVariable] = variable;
                            }
                        }
                        break;

                    case ForEachStatementSyntax forEachStatement:
                        var nombreForEach = forEachStatement.Identifier.ValueText;
                        if (!variablesDeclaradas.ContainsKey(nombreForEach))
                        {
                            var declaradorFicticio = SyntaxFactory.VariableDeclarator(forEachStatement.Identifier);
                            variablesDeclaradas[nombreForEach] = declaradorFicticio;
                        }
                        break;

                    case ForStatementSyntax forStatement:
                        if (forStatement.Declaration != null)
                        {
                            foreach (var variable in forStatement.Declaration.Variables)
                            {
                                var nombreVariable = variable.Identifier.ValueText;
                                if (!variablesDeclaradas.ContainsKey(nombreVariable))
                                {
                                    variablesDeclaradas[nombreVariable] = variable;
                                }
                            }
                        }
                        break;

                    case UsingStatementSyntax usingStatement:
                        if (usingStatement.Declaration != null)
                        {
                            foreach (var variable in usingStatement.Declaration.Variables)
                            {
                                var nombreVariable = variable.Identifier.ValueText;
                                if (!variablesDeclaradas.ContainsKey(nombreVariable))
                                {
                                    variablesDeclaradas[nombreVariable] = variable;
                                }
                            }
                        }
                        break;

                    case TryStatementSyntax tryStatement:
                        foreach (var catchClause in tryStatement.Catches)
                        {
                            if (catchClause.Declaration?.Identifier != null && 
                                !catchClause.Declaration.Identifier.IsKind(SyntaxKind.None))
                            {
                                var nombreCatch = catchClause.Declaration.Identifier.ValueText;
                                if (!string.IsNullOrEmpty(nombreCatch) && !variablesDeclaradas.ContainsKey(nombreCatch))
                                {
                                    var declaradorFicticio = SyntaxFactory.VariableDeclarator(catchClause.Declaration.Identifier);
                                    variablesDeclaradas[nombreCatch] = declaradorFicticio;
                                }
                            }
                        }
                        break;
                }

                // Recursi�n controlada para bloques anidados
                if (statement is BlockSyntax bloqueAnidado)
                {
                    RecopilarDeclaracionesVariables(bloqueAnidado, variablesDeclaradas);
                }
                else if (statement.ChildNodes().Any(child => child is BlockSyntax))
                {
                    foreach (var childBlock in statement.ChildNodes().OfType<BlockSyntax>())
                    {
                        RecopilarDeclaracionesVariables(childBlock, variablesDeclaradas);
                    }
                }
            }
        }

        private static void RecopilarUtilizacionesVariables(SyntaxNode nodo, HashSet<string> variablesUtilizadas, SemanticModel modeloSemantico)
        {
            foreach (var descendiente in nodo.DescendantNodes())
            {
                if (descendiente is IdentifierNameSyntax identificador)
                {
                    try
                    {
                        var infoSimbolo = modeloSemantico.GetSymbolInfo(identificador);
                        var simbolo = infoSimbolo.Symbol;

                        // Verificar si es una variable local
                        if (simbolo is ILocalSymbol variableLocal)
                        {
                            variablesUtilizadas.Add(variableLocal.Name);
                        }
                        // Verificar si es un par�metro
                        else if (simbolo is IParameterSymbol parametro)
                        {
                            variablesUtilizadas.Add(parametro.Name);
                        }
                    }
                    catch
                    {
                        // En caso de error, asumir que se est� utilizando
                        variablesUtilizadas.Add(identificador.Identifier.ValueText);
                    }
                }
            }
        }

        private static bool EsVariableEspecial(string nombreVariable, VariableDeclaratorSyntax declaradorVariable)
        {
            // Excluir variables que comienzan con underscore (convenci�n para variables no utilizadas)
            if (nombreVariable.StartsWith("_"))
                return true;

            // Excluir variables llamadas "ex" o "exception" (com�n en catch)
            if (nombreVariable.Equals("ex", System.StringComparison.OrdinalIgnoreCase) ||
                nombreVariable.Equals("exception", System.StringComparison.OrdinalIgnoreCase))
                return true;

            // Excluir variables que tienen inicializador que puede tener efectos secundarios
            if (declaradorVariable.Initializer != null)
            {
                var inicializador = declaradorVariable.Initializer.Value;
                if (TieneEfectosSecundarios(inicializador))
                    return true;
            }

            return false;
        }

        private static bool TieneEfectosSecundarios(ExpressionSyntax expresion)
        {
            // Verificar si la expresi�n puede tener efectos secundarios
            return expresion.DescendantNodesAndSelf().Any(nodo =>
                nodo is InvocationExpressionSyntax ||           // Llamadas a m�todos
                nodo is ObjectCreationExpressionSyntax ||       // Creaci�n de objetos
                nodo is AssignmentExpressionSyntax ||           // Asignaciones
                nodo is PostfixUnaryExpressionSyntax ||         // Incremento/decremento postfijo
                nodo is PrefixUnaryExpressionSyntax ||          // Incremento/decremento prefijo
                nodo is ConditionalExpressionSyntax);           // Operador condicional
        }
    }
}
