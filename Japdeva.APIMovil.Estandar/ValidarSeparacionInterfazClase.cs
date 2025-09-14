using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarSeparacionInterfazClase : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA080";

        private const string Titulo = "Interfaz y clase no pueden estar en el mismo archivo";
        private const string FormatoMensaje = "La interfaz '{0}' y la clase '{1}' no pueden estar en el mismo archivo. Sepárelas en archivos diferentes";
        private const string Descripcion = "Las interfaces y clases deben estar en archivos separados para mejorar la organización del código y seguir el principio de responsabilidad única.";
        private const string Categoria = "Design";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarArchivo, SyntaxKind.CompilationUnit);
        }

        private static void AnalizarArchivo(SyntaxNodeAnalysisContext contexto)
        {
            try
            {
                var compilationUnit = (CompilationUnitSyntax)contexto.Node;

                // Obtener todas las interfaces y clases en el archivo
                var interfaces = compilationUnit.DescendantNodes()
                    .OfType<InterfaceDeclarationSyntax>()
                    .Where(i => i.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                    .ToList();

                var clases = compilationUnit.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(c => c.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                    .ToList();

                // Verificar si hay tanto interfaces como clases públicas en el mismo archivo
                if (interfaces.Count > 0 && clases.Count > 0)
                {
                    // Reportar error para cada combinación de interfaz y clase
                    foreach (var interfaz in interfaces)
                    {
                        foreach (var clase in clases)
                        {
                            var nombreInterfaz = interfaz.Identifier.ValueText;
                            var nombreClase = clase.Identifier.ValueText;

                            var diagnostico = Diagnostic.Create(
                                Regla,
                                interfaz.Identifier.GetLocation(),
                                nombreInterfaz,
                                nombreClase);

                            contexto.ReportDiagnostic(diagnostico);
                        }
                    }
                }
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
    }
}