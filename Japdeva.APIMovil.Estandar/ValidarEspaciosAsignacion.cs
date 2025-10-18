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
    /// Analizador que verifica que haya espacios correctos alrededor de operadores de asignación.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarEspaciosAsignacion : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA093";

        private const string Titulo = "Espacios incorrectos alrededor del operador de asignación";
        private const string FormatoMensaje = "Debe haber exactamente un espacio antes y después del operador '{0}'";
        private const string Descripcion = "Los operadores de asignación deben tener exactamente un espacio antes y después para mantener consistencia y legibilidad.";
        private const string Categoria = "Style";

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
                contexto.RegisterSyntaxNodeAction(AnalizarAsignacion, SyntaxKind.SimpleAssignmentExpression);
                contexto.RegisterSyntaxNodeAction(AnalizarAsignacionCompuesta, 
                    SyntaxKind.AddAssignmentExpression,
                    SyntaxKind.SubtractAssignmentExpression,
                    SyntaxKind.MultiplyAssignmentExpression,
                    SyntaxKind.DivideAssignmentExpression,
                    SyntaxKind.ModuloAssignmentExpression);
            }
            catch (Exception)
            {
                // No hacer nada en caso de error durante la inicialización
            }
        }

        private static void AnalizarAsignacion(SyntaxNodeAnalysisContext contexto)
        {
            try
            {
                var asignacion = (AssignmentExpressionSyntax)contexto.Node;
                ValidarEspaciosOperador(contexto, asignacion.OperatorToken);
            }
            catch (Exception)
            {
                // No hacer nada en caso de error durante el análisis
            }
        }

        private static void AnalizarAsignacionCompuesta(SyntaxNodeAnalysisContext contexto)
        {
            try
            {
                var asignacion = (AssignmentExpressionSyntax)contexto.Node;
                ValidarEspaciosOperador(contexto, asignacion.OperatorToken);
            }
            catch (Exception)
            {
                // No hacer nada en caso de error durante el análisis
            }
        }

        private static void ValidarEspaciosOperador(SyntaxNodeAnalysisContext contexto, SyntaxToken operadorToken)
        {
            try
            {
                var textoFuente = contexto.Node.SyntaxTree.GetText();
                
                // Verificar espacio antes del operador
                var posicionAntes = operadorToken.SpanStart - 1;
                if (posicionAntes >= 0)
                {
                    var caracterAntes = textoFuente[posicionAntes];
                    if (caracterAntes != ' ')
                    {
                        ReportarDiagnostico(contexto, operadorToken, operadorToken.ValueText);
                        return;
                    }
                    
                    // Verificar que no haya múltiples espacios antes
                    if (posicionAntes > 0 && textoFuente[posicionAntes - 1] == ' ')
                    {
                        ReportarDiagnostico(contexto, operadorToken, operadorToken.ValueText);
                        return;
                    }
                }

                // Verificar espacio después del operador
                var posicionDespues = operadorToken.Span.End;
                if (posicionDespues < textoFuente.Length)
                {
                    var caracterDespues = textoFuente[posicionDespues];
                    if (caracterDespues != ' ')
                    {
                        ReportarDiagnostico(contexto, operadorToken, operadorToken.ValueText);
                        return;
                    }
                    
                    // Verificar que no haya múltiples espacios después
                    if (posicionDespues + 1 < textoFuente.Length && textoFuente[posicionDespues + 1] == ' ')
                    {
                        ReportarDiagnostico(contexto, operadorToken, operadorToken.ValueText);
                        return;
                    }
                }
            }
            catch (Exception)
            {
                // No hacer nada en caso de error
            }
        }

        private static void ReportarDiagnostico(SyntaxNodeAnalysisContext contexto, SyntaxToken operadorToken, string operador)
        {
            try
            {
                var diagnostico = Diagnostic.Create(
                    Regla,
                    operadorToken.GetLocation(),
                    operador);

                contexto.ReportDiagnostic(diagnostico);
            }
            catch (Exception)
            {
                // No hacer nada en caso de error
            }
        }
    }
}