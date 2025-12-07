using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    /// <summary>
    /// Analizador que detecta inicializadores de objetos complejos que dificultan la detección de errores.
    /// Recomienda separar la inicialización en múltiples líneas para mejor debugging.
    /// Permite excepciones para transformaciones LINQ como Select.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarInicializadoresComplejos : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA056";

        private const string Titulo = "Inicializador de objeto complejo dificulta debugging";
        private const string FormatoMensaje = "El inicializador del objeto '{0}' con {1} propiedades debería separarse en líneas individuales para facilitar la detección de errores";
        private const string Descripcion = "Los inicializadores de objetos con múltiples propiedades en una sola expresión dificultan la identificación de la línea exacta que causa errores durante la ejecución. Se recomienda asignar cada propiedad individualmente para mejor debugging y mantenibilidad.";
        private const string Categoria = "Maintainability";

        private const int LIMITE_WARNING = 4;
        private const int LIMITE_ERROR = 6;

        private static readonly DiagnosticDescriptor ReglaWarning = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/how-to-initialize-objects-by-using-an-object-initializer");

        private static readonly DiagnosticDescriptor ReglaError = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/how-to-initialize-objects-by-using-an-object-initializer");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(ReglaWarning, ReglaError);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarInicializadorObjeto, SyntaxKind.ObjectCreationExpression);
        }

        private static void AnalizarInicializadorObjeto(SyntaxNodeAnalysisContext contexto)
        {
            var creacionObjeto = (ObjectCreationExpressionSyntax)contexto.Node;

            // Verificar si tiene inicializador
            if (creacionObjeto.Initializer == null)
                return;

            var inicializador = creacionObjeto.Initializer;
            
            // NUEVA LÓGICA: Verificar si está dentro de un Select u otra transformación LINQ
            if (EstaEnExpresionLinqPermitida(creacionObjeto))
                return; // No aplicar la regla en transformaciones LINQ

            // Contar las expresiones de inicialización
            var expresionesInicializacion = inicializador.Expressions.Count;

            // Determinar severidad basada en el número de propiedades
            DiagnosticDescriptor reglaAplicar = null;
            
            if (expresionesInicializacion >= LIMITE_ERROR)
            {
                reglaAplicar = ReglaError;
            }
            else if (expresionesInicializacion >= LIMITE_WARNING)
            {
                reglaAplicar = ReglaWarning;
            }

            // Si aplica alguna regla, reportar el diagnóstico
            if (reglaAplicar != null)
            {
                // Obtener el nombre del tipo si es posible
                string nombreTipo = ObtenerNombreTipo(creacionObjeto);

                var diagnostico = Diagnostic.Create(
                    reglaAplicar,
                    inicializador.GetLocation(),
                    nombreTipo,
                    expresionesInicializacion);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        /// <summary>
        /// Verifica si el inicializador de objeto está dentro de una expresión LINQ permitida.
        /// </summary>
        /// <param name="creacionObjeto">La expresión de creación del objeto.</param>
        /// <returns>True si está dentro de Select, Where, u otras transformaciones LINQ permitidas.</returns>
        private static bool EstaEnExpresionLinqPermitida(ObjectCreationExpressionSyntax creacionObjeto)
        {
            // Buscar hacia arriba en el árbol sintáctico para encontrar expresiones LINQ
            var nodoActual = creacionObjeto.Parent;
            
            while (nodoActual != null)
            {
                // Verificar si está en una expresión lambda
                if (nodoActual is SimpleLambdaExpressionSyntax || nodoActual is ParenthesizedLambdaExpressionSyntax)
                {
                    // Verificar si la lambda está en una invocación de método LINQ
                    var invocacionMetodo = nodoActual.Ancestors().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                    if (invocacionMetodo != null)
                    {
                        var nombreMetodo = ObtenerNombreMetodoInvocado(invocacionMetodo);
                        
                        // Lista de métodos LINQ que permiten inicializadores complejos
                        var metodosLinqPermitidos = new[]
                        {
                            "Select", "SelectMany", "Where", "OrderBy", "OrderByDescending",
                            "ThenBy", "ThenByDescending", "GroupBy", "Join", "GroupJoin"
                        };
                        
                        return metodosLinqPermitidos.Contains(nombreMetodo);
                    }
                }
                
                nodoActual = nodoActual.Parent;
            }
            
            return false;
        }

        /// <summary>
        /// Obtiene el nombre del método que se está invocando.
        /// </summary>
        /// <param name="invocacion">La expresión de invocación del método.</param>
        /// <returns>El nombre del método como string.</returns>
        private static string ObtenerNombreMetodoInvocado(InvocationExpressionSyntax invocacion)
        {
            try
            {
                if (invocacion.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    return memberAccess.Name.Identifier.ValueText;
                }
                else if (invocacion.Expression is IdentifierNameSyntax identifier)
                {
                    return identifier.Identifier.ValueText;
                }
                
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Obtiene el nombre del tipo que se está inicializando.
        /// </summary>
        /// <param name="creacionObjeto">Expresión de creación del objeto.</param>
        /// <returns>Nombre del tipo como string.</returns>
        private static string ObtenerNombreTipo(ObjectCreationExpressionSyntax creacionObjeto)
        {
            try
            {
                if (creacionObjeto.Type != null)
                {
                    return creacionObjeto.Type.ToString();
                }

                return "objeto";
            }
            catch
            {
                return "objeto";
            }
        }
    }
}
