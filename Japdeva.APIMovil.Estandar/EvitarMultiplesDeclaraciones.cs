using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Japdeva.APIMovil.Estandar
{
    /// <summary>
    /// Analizador que verifica que las líneas no contengan múltiples declaraciones complejas.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EvitarMultiplesDeclaraciones : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA094";

        private const string Titulo = "Evitar múltiples declaraciones complejas en una línea";
        private const string FormatoMensaje = "Separar declaraciones complejas en líneas diferentes para mejorar legibilidad";
        private const string Descripcion = "Las declaraciones complejas como creación de objetos y llamadas a métodos deben estar en líneas separadas para mantener el código legible y fácil de debuggear.";
        private const string Categoria = "Style";
        private const int LIMITE_CARACTERES_LINEA = 80;

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/csharp/programming-guide/statements-expressions-operators");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext contexto)
        {
            try
            {
                contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
                contexto.EnableConcurrentExecution();
                contexto.RegisterSyntaxNodeAction(AnalizarDeclaracion, SyntaxKind.ExpressionStatement);
                contexto.RegisterSyntaxNodeAction(AnalizarAsignacion, SyntaxKind.SimpleAssignmentExpression);
            }
            catch (Exception)
            {
                // No hacer nada en caso de error durante la inicialización
            }
        }

        private static void AnalizarDeclaracion(SyntaxNodeAnalysisContext contexto)
        {
            try
            {
                var declaracion = (ExpressionStatementSyntax)contexto.Node;
                ValidarComplejidadLinea(contexto, declaracion);
            }
            catch (Exception)
            {
                // No hacer nada en caso de error durante el análisis
            }
        }

        private static void AnalizarAsignacion(SyntaxNodeAnalysisContext contexto)
        {
            try
            {
                var asignacion = (AssignmentExpressionSyntax)contexto.Node;
                
                // Solo analizar si la asignación está en un ExpressionStatement
                if (asignacion.Parent is ExpressionStatementSyntax declaracion)
                {
                    // Excluir asignaciones que contienen expresiones lambda
                    if (ContieneExpresionesLambdaEnAsignacion(asignacion))
                        return;

                    ValidarComplejidadLinea(contexto, declaracion);
                }
            }
            catch (Exception)
            {
                // No hacer nada en caso de error durante el análisis
            }
        }

        private static void ValidarComplejidadLinea(SyntaxNodeAnalysisContext contexto, ExpressionStatementSyntax declaracion)
        {
            try
            {
                var textoLinea = ObtenerTextoLinea(contexto, declaracion);
                if (string.IsNullOrEmpty(textoLinea))
                    return;

                // Excluir líneas que contienen expresiones lambda con =>
                if (ContieneExpresionesLambda(declaracion))
                    return;

                // Solo verificar si la línea es extremadamente larga (más de 120 caracteres)
                if (textoLinea.Length > 120)
                {
                    ReportarDiagnostico(contexto, declaracion);
                    return;
                }

                // Verificar si contiene múltiples operaciones complejas
                if (ContieneMultiplesOperacionesComplejas(declaracion))
                {
                    ReportarDiagnostico(contexto, declaracion);
                    return;
                }
            }
            catch (Exception)
            {
                // No hacer nada en caso de error
            }
        }

        private static bool ContieneMultiplesOperacionesComplejas(ExpressionStatementSyntax declaracion)
        {
            try
            {
                var operacionesComplejas = 0;

                // Contar creaciones de objetos (solo si son complejas)
                var creaciones = declaracion.DescendantNodes()
                    .OfType<ObjectCreationExpressionSyntax>()
                    .Where(c => EsCreacionCompleja(c))
                    .Count();
                operacionesComplejas += creaciones;

                // Contar llamadas a métodos (solo si no es una llamada simple)
                var llamadas = declaracion.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Where(l => EsLlamadaCompleja(l))
                    .Count();
                operacionesComplejas += llamadas;

                // Contar expresiones lambda
                operacionesComplejas += declaracion.DescendantNodes()
                    .OfType<LambdaExpressionSyntax>()
                    .Count();

                return operacionesComplejas > 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool EsCreacionCompleja(ObjectCreationExpressionSyntax creacion)
        {
            try
            {
                // Es compleja si tiene inicializadores con múltiples propiedades
                if (creacion.Initializer?.Expressions.Count > 2)
                    return true;

                // Es compleja si tiene argumentos del constructor
                if (creacion.ArgumentList?.Arguments.Count > 0)
                    return true;

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool EsLlamadaCompleja(InvocationExpressionSyntax llamada)
        {
            try
            {
                // No es compleja si es una llamada async simple con 2 argumentos o menos
                if (llamada.ArgumentList?.Arguments.Count <= 2)
                    return false;

                // Es compleja si tiene más de 2 argumentos
                if (llamada.ArgumentList?.Arguments.Count > 2)
                    return true;

                // Es compleja si los argumentos contienen otras llamadas
                var tieneArgumentosComplejos = llamada.ArgumentList?.Arguments
                    .Any(arg => arg.DescendantNodes().OfType<InvocationExpressionSyntax>().Any()) ?? false;

                return tieneArgumentosComplejos;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool ContieneCreacionYLlamada(ExpressionStatementSyntax declaracion)
        {
            try
            {
                var tieneCreacion = declaracion.DescendantNodes()
                    .OfType<ObjectCreationExpressionSyntax>()
                    .Any();

                var tieneLlamada = declaracion.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Any();

                return tieneCreacion && tieneLlamada;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string ObtenerTextoLinea(SyntaxNodeAnalysisContext contexto, SyntaxNode nodo)
        {
            try
            {
                var textoFuente = contexto.Node.SyntaxTree.GetText();
                var lineaInicio = textoFuente.Lines.GetLineFromPosition(nodo.SpanStart);
                return lineaInicio.ToString().Trim();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static void ReportarDiagnostico(SyntaxNodeAnalysisContext contexto, ExpressionStatementSyntax declaracion)
        {
            try
            {
                var diagnostico = Diagnostic.Create(
                    Regla,
                    declaracion.GetLocation());

                contexto.ReportDiagnostic(diagnostico);
            }
            catch (Exception)
            {
                // No hacer nada en caso de error
            }
        }

        private static bool ContieneExpresionesLambda(ExpressionStatementSyntax declaracion)
        {
            try
            {
                // Buscar expresiones lambda (SimpleLambdaExpression y ParenthesizedLambdaExpression)
                var expresionesLambda = declaracion.DescendantNodes()
                    .Where(node => node.IsKind(SyntaxKind.SimpleLambdaExpression) || 
                                   node.IsKind(SyntaxKind.ParenthesizedLambdaExpression))
                    .Any();

                return expresionesLambda;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool ContieneExpresionesLambdaEnAsignacion(AssignmentExpressionSyntax asignacion)
        {
            try
            {
                // Buscar expresiones lambda en el lado derecho de la asignación
                var expresionesLambda = asignacion.Right.DescendantNodesAndSelf()
                    .Where(node => node.IsKind(SyntaxKind.SimpleLambdaExpression) || 
                                   node.IsKind(SyntaxKind.ParenthesizedLambdaExpression))
                    .Any();

                return expresionesLambda;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}