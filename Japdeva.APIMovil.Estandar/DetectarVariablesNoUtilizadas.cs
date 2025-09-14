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

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarMetodo, SyntaxKind.MethodDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarConstructor, SyntaxKind.ConstructorDeclaration);
        }

        private static void AnalizarMetodo(SyntaxNodeAnalysisContext contexto)
        {
            var metodo = (MethodDeclarationSyntax)contexto.Node;
            if (metodo.Body != null)
            {
                AnalizarBloqueParaVariables(contexto, metodo.Body);
            }
        }

        private static void AnalizarConstructor(SyntaxNodeAnalysisContext contexto)
        {
            var constructor = (ConstructorDeclarationSyntax)contexto.Node;
            if (constructor.Body != null)
            {
                AnalizarBloqueParaVariables(contexto, constructor.Body);
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
