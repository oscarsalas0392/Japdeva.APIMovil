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
        private const int LIMITE_CARACTERES_LINEA = 150; // Aumentado de 80 a 150

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error, // Cambiado de Error a Warning
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

                // Excluir llamadas async simples de servicios (patrón común en la aplicación)
                if (EsLlamadaAsyncServicioSimple(declaracion))
                    return;

                // Solo verificar si la línea es extremadamente larga (más de 150 caracteres)
                if (textoLinea.Length > LIMITE_CARACTERES_LINEA)
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

        private static bool EsLlamadaAsyncServicioSimple(ExpressionStatementSyntax declaracion)
        {
            try
            {
                // Verificar si es una llamada async simple a un servicio
                var awaitExpression = declaracion.DescendantNodes().OfType<AwaitExpressionSyntax>().FirstOrDefault();
                if (awaitExpression?.Expression is InvocationExpressionSyntax invocation)
                {
                    // Verificar si es una llamada a un servicio (contiene "Service" en el nombre)
                    if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                    {
                        var expresion = memberAccess.Expression.ToString();
                        var metodo = memberAccess.Name.ToString();
                        
                        // Es una llamada a servicio si:
                        // 1. La expresión contiene "Service"
                        // 2. El método termina en "Async"
                        // 3. Tiene máximo 5 argumentos (límite razonable)
                        if (expresion.Contains("Service") && 
                            metodo.EndsWith("Async") && 
                            invocation.ArgumentList?.Arguments.Count <= 5)
                        {
                            return true;
                        }
                    }
                }
                
                return false;
            }
            catch (Exception)
            {
                return false;
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
                // Es compleja si tiene inicializadores con múltiples propiedades (más de 3)
                if (creacion.Initializer?.Expressions.Count > 3)
                    return true;

                // Es compleja si tiene muchos argumentos del constructor (más de 4)
                if (creacion.ArgumentList?.Arguments.Count > 4)
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
                // Aumentar tolerancia para servicios async
                if (llamada.ArgumentList?.Arguments.Count <= 5) // Aumentado de 2 a 5
                {
                    // Verificar si es una llamada a método que termina en Async
                    if (llamada.Expression is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Name.ToString().EndsWith("Async"))
                    {
                        return false; // No considerar complejas las llamadas async con 5 o menos argumentos
                    }
                    
                    // Para métodos que no son async, mantener límite de 3
                    return llamada.ArgumentList?.Arguments.Count > 3;
                }

                // Es compleja si tiene más de 5 argumentos
                if (llamada.ArgumentList?.Arguments.Count > 5)
                    return true;

                // Es compleja si los argumentos contienen otras llamadas anidadas
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