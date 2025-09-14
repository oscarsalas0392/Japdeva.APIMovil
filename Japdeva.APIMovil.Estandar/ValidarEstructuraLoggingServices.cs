using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarEstructuraLoggingServices : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA081";

        private const string Titulo = "Método en servicio debe seguir estructura estándar de logging";
        private const string FormatoMensaje = "El método '{0}' en el servicio debe incluir: string nombreMetodo = this.ObtenerNombreMetodo(), this._logger.Inicio(), this._logger.Error() en catch, y this._logger.Fin() en finally";
        private const string Descripcion = "Todos los métodos públicos en servicios deben seguir la estructura estándar de logging para mantener consistencia y trazabilidad.";
        private const string Categoria = "Design";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/aspnet/core/fundamentals/logging");

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
            try
            {
                var metodo = (MethodDeclarationSyntax)contexto.Node;

                // Solo analizar métodos públicos
                var esPublico = metodo.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
                if (!esPublico)
                    return;

                // Solo analizar si está en carpeta Services
                var archivoPath = contexto.Node.SyntaxTree.FilePath;
                if (string.IsNullOrEmpty(archivoPath) || !EstaEnCarpetaServices(archivoPath))
                    return;

                // Excluir constructores
                if (metodo.Identifier.ValueText == metodo.Parent?.ChildTokens().FirstOrDefault(t => t.IsKind(SyntaxKind.IdentifierToken)).ValueText)
                    return;

                // Verificar estructura de logging
                if (!TieneEstructuraLoggingCompleta(metodo))
                {
                    var nombreMetodo = metodo.Identifier.ValueText;

                    var diagnostico = Diagnostic.Create(
                        Regla,
                        metodo.Identifier.GetLocation(),
                        nombreMetodo);

                    contexto.ReportDiagnostic(diagnostico);
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

        private static bool EstaEnCarpetaServices(string archivoPath)
        {
            try
            {
                // Normalizar el path
                var pathNormalizado = System.IO.Path.GetFullPath(archivoPath).Replace('\\', '/');
                
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

        private static bool TieneEstructuraLoggingCompleta(MethodDeclarationSyntax metodo)
        {
            try
            {
                var cuerpoMetodo = metodo.Body;
                if (cuerpoMetodo == null)
                    return true; // Métodos sin cuerpo (abstractos, interfaces) no se validan

                var todasLasDeclaraciones = cuerpoMetodo.DescendantNodes().ToList();

                // Verificar declaración de nombreMetodo
                var tieneNombreMetodo = todasLasDeclaraciones
                    .OfType<VariableDeclaratorSyntax>()
                    .Any(v => v.Identifier.ValueText == "nombreMetodo" &&
                             v.Initializer?.Value?.ToString().Contains("this.ObtenerNombreMetodo()") == true);

                // Verificar try-catch-finally structure
                var tryStatements = todasLasDeclaraciones.OfType<TryStatementSyntax>().ToList();
                if (!tryStatements.Any())
                    return false;

                bool tieneEstructuraCompleta = false;

                foreach (var tryStatement in tryStatements)
                {
                    // Verificar _logger.Inicio en try
                    var tieneInicio = tryStatement.Block.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Any(inv => inv.ToString().Contains("_logger.Inicio"));

                    // Verificar _logger.Error en catch
                    var tieneError = tryStatement.Catches.Any(c =>
                        c.Block.DescendantNodes()
                            .OfType<InvocationExpressionSyntax>()
                            .Any(inv => inv.ToString().Contains("_logger.Error")));

                    // Verificar _logger.Fin en finally
                    var tieneFin = tryStatement.Finally?.Block.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Any(inv => inv.ToString().Contains("_logger.Fin")) == true;

                    if (tieneInicio && tieneError && tieneFin)
                    {
                        tieneEstructuraCompleta = true;
                        break;
                    }
                }

                return tieneNombreMetodo && tieneEstructuraCompleta;
            }
            catch (System.Exception)
            {
                return true; // En caso de error, no reportar falso positivo
            }
            finally
            {
                // Cleanup
            }
        }
    }
}