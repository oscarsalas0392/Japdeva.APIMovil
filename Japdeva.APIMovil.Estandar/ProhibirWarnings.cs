using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ProhibirWarnings : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA049";
        private const string Titulo = "No se permiten warnings en el proyecto";
        private const string FormatoMensaje = "Se detectaron warnings en el código. Los proyectos deben compilar sin warnings para mantener la máxima calidad. Configure TreatWarningsAsErrors=true en .csproj";
        private const string Descripcion = "Los proyectos deben configurarse para tratar todos los warnings como errores usando TreatWarningsAsErrors=true. Esto garantiza código limpio sin warnings y máxima calidad.";
        private const string Categoria = "Quality";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/core/project-sdk/msbuild-props#treatwarningsaserrors");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterCompilationAction(AnalizarCompilacion);
        }

        private static void AnalizarCompilacion(CompilationAnalysisContext contexto)
        {
            // Verificar si hay warnings en la compilación actual
            var diagnosticos = contexto.Compilation.GetDiagnostics();
            var warnings = diagnosticos.Where(d => d.Severity == DiagnosticSeverity.Warning && !d.IsSuppressed).ToArray();

            if (warnings.Any())
            {
                // Reportar un error por cada warning detectado
                foreach (var warning in warnings.Take(5)) // Limitar a 5 para evitar spam
                {
                    var diagnostico = Diagnostic.Create(
                        Regla,
                        warning.Location,
                        $"Warning detectado: {warning.Id} - {warning.GetMessage()}");

                    contexto.ReportDiagnostic(diagnostico);
                }

                // Si hay más de 5 warnings, reportar un resumen
                if (warnings.Length > 5)
                {
                    var diagnosticoResumen = Diagnostic.Create(
                        new DiagnosticDescriptor(
                            DiagnosticId,
                            Titulo,
                            $"Se detectaron {warnings.Length} warnings en total. Se muestran solo los primeros 5. Configure TreatWarningsAsErrors=true para resolverlos todos",
                            Categoria,
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        Location.None);

                    contexto.ReportDiagnostic(diagnosticoResumen);
                }
            }
        }
    }
}
