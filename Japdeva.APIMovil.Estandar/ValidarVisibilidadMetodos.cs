using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Japdeva.APIMovil.Estandar
{
    /// <summary>
    /// Analizador que valida que todos los métodos sean públicos para facilitar las pruebas unitarias.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarVisibilidadMetodos : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA055";

        private const string Titulo = "Método debe ser público para pruebas unitarias";
        private const string FormatoMensaje = "El método '{0}' debe ser público para facilitar las pruebas unitarias. Visibilidad actual: {1}";
        private const string Descripcion = "Todos los métodos deben ser públicos para permitir su testeo directo mediante pruebas unitarias y mejorar la cobertura de código.";
        private const string Categoria = "Design";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/core/testing/");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarMetodo, SyntaxKind.MethodDeclaration);
        }

        private static void AnalizarMetodo(SyntaxNodeAnalysisContext contexto)
        {
            var metodo = contexto.Node as MethodDeclarationSyntax;
            if (metodo == null)
                return;

            // Excluir métodos de interfaces (son implícitamente públicos)
            if (EstaEnInterfaz(metodo))
                return;

            // Excluir constructores, destructores y métodos especiales
            if (EsMetodoEspecial(metodo))
                return;

            // Verificar si el método no es público
            if (!EsMetodoPublico(metodo))
            {
                string visibilidadActual = ObtenerVisibilidad(metodo);
                string nombreMetodo = metodo.Identifier.ValueText;

                var diagnostico = Diagnostic.Create(
                    Regla,
                    metodo.Identifier.GetLocation(),
                    nombreMetodo,
                    visibilidadActual);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        /// <summary>
        /// Verifica si el método está declarado dentro de una interfaz.
        /// </summary>
        /// <param name="metodo">Método a evaluar.</param>
        /// <returns>True si está en una interfaz, false en caso contrario.</returns>
        private static bool EstaEnInterfaz(MethodDeclarationSyntax metodo)
        {
            var padre = metodo.Parent;
            while (padre != null)
            {
                if (padre is InterfaceDeclarationSyntax)
                    return true;
                padre = padre.Parent;
            }
            return false;
        }

        /// <summary>
        /// Determina si un método es especial y debe ser excluido de la validación.
        /// </summary>
        /// <param name="metodo">Método a evaluar.</param>
        /// <returns>True si el método debe ser excluido, false en caso contrario.</returns>
        private static bool EsMetodoEspecial(MethodDeclarationSyntax metodo)
        {
            string nombreMetodo = metodo.Identifier.ValueText;

            // Excluir métodos que tradicionalmente no son públicos
            var metodosEspeciales = new[]
            {
                "Main",           // Método principal de aplicación
                "Dispose",        // Método de IDisposable
                "Finalize",       // Finalizador
                "GetHashCode",    // Override de Object
                "Equals",         // Override de Object
                "ToString"        // Override de Object
            };

            if (metodosEspeciales.Contains(nombreMetodo))
                return true;

            // Excluir métodos que implementan interfaces conocidas
            if (ImplementaInterfazConocida(metodo))
                return true;

            // Excluir métodos con atributos especiales
            if (TieneAtributosEspeciales(metodo))
                return true;

            return false;
        }

        /// <summary>
        /// Verifica si el método implementa una interfaz conocida que puede requerir visibilidad específica.
        /// </summary>
        /// <param name="metodo">Método a evaluar.</param>
        /// <returns>True si implementa una interfaz conocida, false en caso contrario.</returns>
        private static bool ImplementaInterfazConocida(MethodDeclarationSyntax metodo)
        {
            // Verificar si es un override (normalmente de interfaces o clases base)
            if (metodo.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)))
                return true;

            // Verificar si tiene implementation explícita de interfaz
            if (metodo.ExplicitInterfaceSpecifier != null)
                return true;

            return false;
        }

        /// <summary>
        /// Verifica si el método tiene atributos especiales que justifican su visibilidad.
        /// </summary>
        /// <param name="metodo">Método a evaluar.</param>
        /// <returns>True si tiene atributos especiales, false en caso contrario.</returns>
        private static bool TieneAtributosEspeciales(MethodDeclarationSyntax metodo)
        {
            if (metodo.AttributeLists.Count == 0)
                return false;

            var atributosEspeciales = new[]
            {
                "Test",           // Métodos de prueba
                "TestMethod",     // Métodos de prueba MSTest
                "Fact",           // Métodos de prueba xUnit
                "Theory",         // Métodos de prueba xUnit
                "HttpGet",        // Métodos de controlador
                "HttpPost",       // Métodos de controlador
                "HttpPut",        // Métodos de controlador
                "HttpDelete",     // Métodos de controlador
                "Route",          // Métodos de controlador
                "JsonIgnore",     // Propiedades serializables
                "NonAction"       // Métodos de controlador no-acción
            };

            foreach (var listaAtributos in metodo.AttributeLists)
            {
                foreach (var atributo in listaAtributos.Attributes)
                {
                    string nombreAtributo = atributo.Name.ToString();
                    if (atributosEspeciales.Any(a => nombreAtributo.Contains(a)))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Verifica si un método es público.
        /// </summary>
        /// <param name="metodo">Método a evaluar.</param>
        /// <returns>True si el método es público, false en caso contrario.</returns>
        private static bool EsMetodoPublico(MethodDeclarationSyntax metodo)
        {
            // Si tiene el modificador public explícito
            if (metodo.Modifiers.Any(modificador => modificador.IsKind(SyntaxKind.PublicKeyword)))
                return true;

            // En interfaces, los métodos son implícitamente públicos
            if (EstaEnInterfaz(metodo))
                return true;

            return false;
        }

        /// <summary>
        /// Obtiene la visibilidad actual del método.
        /// </summary>
        /// <param name="metodo">Método a evaluar.</param>
        /// <returns>String que describe la visibilidad actual.</returns>
        private static string ObtenerVisibilidad(MethodDeclarationSyntax metodo)
        {
            if (metodo.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)))
                return "private";
            
            if (metodo.Modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword)))
                return "protected";
            
            if (metodo.Modifiers.Any(m => m.IsKind(SyntaxKind.InternalKeyword)))
                return "internal";

            // Si no tiene modificador explícito, es private por defecto
            return "private (implícito)";
        }
    }
}
