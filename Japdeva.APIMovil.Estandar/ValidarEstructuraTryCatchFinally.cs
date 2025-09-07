using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    /// <summary>
    /// Analizador que valida que todos los métodos (excepto métodos de extensión) 
    /// tengan bloques try-catch-finally para manejo consistente de errores y logging.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarEstructuraTryCatchFinally : DiagnosticAnalyzer
    {
        public const string DiagnosticIdTryCatch = "JAPDEVA058";
        public const string DiagnosticIdFinally = "JAPDEVA059";

        private const string TituloTryCatch = "Método debe tener estructura try-catch";
        private const string TituloFinally = "Método debe tener bloque finally";
        
        private const string FormatoMensajeTryCatch = "El método '{0}' debe tener estructura try-catch para manejo consistente de errores";
        private const string FormatoMensajeFinally = "El método '{0}' debe tener bloque finally para logging y cleanup";
        
        private const string DescripcionTryCatch = "Todos los métodos deben implementar manejo de errores mediante bloques try-catch para garantizar la captura y logging adecuado de excepciones.";
        private const string DescripcionFinally = "Todos los métodos deben implementar bloques finally para asegurar que el logging de finalización y cleanup se ejecuten independientemente del resultado.";
        
        private const string Categoria = "Reliability";

        private static readonly DiagnosticDescriptor ReglaTryCatch = new DiagnosticDescriptor(
            DiagnosticIdTryCatch,
            TituloTryCatch,
            FormatoMensajeTryCatch,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: DescripcionTryCatch,
            helpLinkUri: "https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/try-catch");

        private static readonly DiagnosticDescriptor ReglaFinally = new DiagnosticDescriptor(
            DiagnosticIdFinally,
            TituloFinally,
            FormatoMensajeFinally,
            Categoria,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: DescripcionFinally,
            helpLinkUri: "https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/try-finally");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(ReglaTryCatch, ReglaFinally);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarMetodo, SyntaxKind.MethodDeclaration);
        }

        private static void AnalizarMetodo(SyntaxNodeAnalysisContext contexto)
        {
            var metodo = (MethodDeclarationSyntax)contexto.Node;

            // Excluir métodos que no necesitan try-catch-finally
            if (DebeExcluirseMetodo(metodo))
                return;

            // Validar estructura try-catch-finally
            ValidarEstructuraMetodo(contexto, metodo);
        }

        /// <summary>
        /// Determina si un método debe excluirse de la validación de try-catch-finally.
        /// </summary>
        /// <param name="metodo">Método a evaluar.</param>
        /// <returns>True si debe excluirse, false si debe validarse.</returns>
        private static bool DebeExcluirseMetodo(MethodDeclarationSyntax metodo)
        {
            // Excluir métodos de extensión
            if (EsMetodoExtension(metodo))
                return true;

            // Excluir métodos de controller con atributos HTTP
            if (EsMetodoController(metodo))
                return true;

            // Excluir métodos abstractos o de interfaz
            if (metodo.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
                return true;

            // Excluir métodos sin cuerpo (declaraciones de interfaz)
            if (metodo.Body == null && metodo.ExpressionBody == null)
                return true;

            // Excluir métodos muy simples (expresiones lambda)
            if (metodo.ExpressionBody != null)
                return true;

            // Excluir propiedades automáticas y métodos especiales
            if (EsMetodoEspecial(metodo))
                return true;

            // Excluir métodos vacíos o con solo una línea
            if (EsMetodoMuySimple(metodo))
                return true;

            return false;
        }

        /// <summary>
        /// Verifica si el método es un método de controller con atributos HTTP.
        /// </summary>
        /// <param name="metodo">Método a evaluar.</param>
        /// <returns>True si es método de controller, false en caso contrario.</returns>
        private static bool EsMetodoController(MethodDeclarationSyntax metodo)
        {
            // Verificar si el método tiene atributos HTTP
            var atributosHttp = new[]
            {
                "HttpGet", "HttpPost", "HttpPut", "HttpDelete", "HttpPatch",
                "Get", "Post", "Put", "Delete", "Patch", "Route"
            };

            foreach (var listaAtributos in metodo.AttributeLists)
            {
                foreach (var atributo in listaAtributos.Attributes)
                {
                    string nombreAtributo = atributo.Name.ToString();
                    if (atributosHttp.Any(a => nombreAtributo.Contains(a)))
                        return true;
                }
            }

            // Verificar si está en una clase Controller
            var claseContenedora = metodo.Parent as ClassDeclarationSyntax;
            if (claseContenedora != null)
            {
                string nombreClase = claseContenedora.Identifier.ValueText;
                if (nombreClase.EndsWith("Controller"))
                    return true;

                // Verificar si la clase hereda de Controller o ControllerBase
                if (claseContenedora.BaseList != null)
                {
                    foreach (var tipoBase in claseContenedora.BaseList.Types)
                    {
                        string nombreTipoBase = tipoBase.ToString();
                        if (nombreTipoBase.Contains("Controller") || nombreTipoBase.Contains("ControllerBase"))
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Verifica si el método es un método de extensión.
        /// </summary>
        /// <param name="metodo">Método a evaluar.</param>
        /// <returns>True si es método de extensión, false en caso contrario.</returns>
        private static bool EsMetodoExtension(MethodDeclarationSyntax metodo)
        {
            // Verificar si el método tiene el modificador static
            if (!metodo.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
                return false;

            // Verificar si la clase contenedora es static
            var claseContenedora = metodo.Parent as ClassDeclarationSyntax;
            if (claseContenedora?.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)) != true)
                return false;

            // Verificar si el primer parámetro tiene el modificador 'this'
            var primerParametro = metodo.ParameterList.Parameters.FirstOrDefault();
            return primerParametro?.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword)) == true;
        }

        /// <summary>
        /// Verifica si el método es un método especial que no necesita try-catch-finally.
        /// </summary>
        /// <param name="metodo">Método a evaluar.</param>
        /// <returns>True si es método especial, false en caso contrario.</returns>
        private static bool EsMetodoEspecial(MethodDeclarationSyntax metodo)
        {
            string nombreMetodo = metodo.Identifier.ValueText;

            // Métodos especiales del framework
            var metodosEspeciales = new[]
            {
                "ToString", "GetHashCode", "Equals", "Dispose",
                "Main", "Configure", "ConfigureServices"
            };

            return metodosEspeciales.Contains(nombreMetodo);
        }

        /// <summary>
        /// Verifica si el método es muy simple y no requiere try-catch-finally.
        /// </summary>
        /// <param name="metodo">Método a evaluar.</param>
        /// <returns>True si es muy simple, false en caso contrario.</returns>
        private static bool EsMetodoMuySimple(MethodDeclarationSyntax metodo)
        {
            if (metodo.Body == null)
                return true;

            // Contar statements en el método
            var statements = metodo.Body.Statements.Count;
            
            // Si tiene 0 statements, es vacío
            if (statements == 0)
                return true;

            // Si tiene solo 1 statement y es un return simple, es muy simple
            if (statements == 1)
            {
                var statement = metodo.Body.Statements[0];
                if (statement is ReturnStatementSyntax)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Valida la estructura try-catch-finally del método.
        /// </summary>
        /// <param name="contexto">Contexto del análisis.</param>
        /// <param name="metodo">Método a validar.</param>
        private static void ValidarEstructuraMetodo(SyntaxNodeAnalysisContext contexto, MethodDeclarationSyntax metodo)
        {
            if (metodo.Body == null)
                return;

            // Buscar bloques try en el método
            var bloquesTor = metodo.Body.DescendantNodes().OfType<TryStatementSyntax>().ToList();

            if (!bloquesTor.Any())
            {
                // No tiene try-catch, reportar error
                var diagnostico = Diagnostic.Create(
                    ReglaTryCatch,
                    metodo.Identifier.GetLocation(),
                    metodo.Identifier.ValueText);

                contexto.ReportDiagnostic(diagnostico);
                return;
            }

            // Verificar que tenga al menos un bloque con finally
            bool tieneFinally = bloquesTor.Any(t => t.Finally != null);
            
            if (!tieneFinally)
            {
                var diagnostico = Diagnostic.Create(
                    ReglaFinally,
                    metodo.Identifier.GetLocation(),
                    metodo.Identifier.ValueText);

                contexto.ReportDiagnostic(diagnostico);
            }
        }
    }
}
