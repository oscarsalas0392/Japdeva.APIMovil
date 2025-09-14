using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarClasesEstaticasEnServices : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA079";

        private const string Titulo = "Clase estática no permitida en carpeta Services";
        private const string FormatoMensaje = "La clase estática '{0}' no está permitida en la carpeta Services. Use servicios con inyección de dependencias";
        private const string Descripcion = "Las clases estáticas no están permitidas en la carpeta Services para mantener la testabilidad y seguir el patrón de inyección de dependencias.";
        private const string Categoria = "Design";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/aspnet/core/fundamentals/dependency-injection");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarClase, SyntaxKind.ClassDeclaration);
        }

        private static void AnalizarClase(SyntaxNodeAnalysisContext contexto)
        {
            try
            {
                var claseDeclaracion = (ClassDeclarationSyntax)contexto.Node;

                // Verificar si la clase es estática
                var esEstatica = claseDeclaracion.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
                if (!esEstatica)
                    return;

                // Obtener el path del archivo
                var archivoPath = contexto.Node.SyntaxTree.FilePath;
                if (string.IsNullOrEmpty(archivoPath))
                    return;

                // Verificar si está en carpeta Services
                if (!EstaEnCarpetaServices(archivoPath))
                    return;

                var nombreClase = claseDeclaracion.Identifier.ValueText;

                var diagnostico = Diagnostic.Create(
                    Regla,
                    claseDeclaracion.Identifier.GetLocation(),
                    nombreClase);

                contexto.ReportDiagnostic(diagnostico);
            }
            catch (System.Exception)
            {
                // Ignore analysis errors silently
            }
            finally
            {
                // Cleanup
            }
        }

        private static bool EstaEnCarpetaServices(string archivoPath)
        {
            try
            {
                // Normalizar el path
                var pathNormalizado = Path.GetFullPath(archivoPath).Replace('\\', '/');
                
                // Verificar si contiene /Services/ en el path
                return pathNormalizado.IndexOf("/Services/", System.StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch (System.Exception)
            {
                return false;
            }
            finally
            {
                // Cleanup
            }
        }
    }
}