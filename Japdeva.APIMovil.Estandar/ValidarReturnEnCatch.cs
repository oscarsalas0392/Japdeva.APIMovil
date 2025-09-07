using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    /// <summary>
    /// Analizador que detecta métodos que hacen return desde bloques catch en lugar de throw.
    /// Esto es una mala práctica porque puede ocultar errores importantes.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarReturnEnCatch : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA057";

        private const string Titulo = "Return en bloque catch oculta errores";
        private const string FormatoMensaje = "El método '{0}' hace return en el bloque catch. Use throw para propagar la excepción o maneje el error apropiadamente";
        private const string Descripcion = "Los métodos no deberían hacer return desde bloques catch ya que esto oculta errores importantes. En su lugar, use throw para propagar la excepción o maneje el error de manera apropiada antes de retornar desde el try.";
        private const string Categoria = "Reliability";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2200");

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
            var metodo = (MethodDeclarationSyntax)contexto.Node;

            // Buscar bloques try-catch en el método
            var bloquesTorCatch = metodo.DescendantNodes().OfType<TryStatementSyntax>();

            foreach (var bloqueTry in bloquesTorCatch)
            {
                // Analizar cada bloque catch
                foreach (var catch_ in bloqueTry.Catches)
                {
                    AnalizarBloqueCatch(contexto, catch_, metodo);
                }
            }
        }

        private static void AnalizarBloqueCatch(SyntaxNodeAnalysisContext contexto, CatchClauseSyntax bloqueCatch, MethodDeclarationSyntax metodo)
        {
            // Buscar statements return en el bloque catch
            var returnsEnCatch = bloqueCatch.Block.DescendantNodes().OfType<ReturnStatementSyntax>();

            foreach (var returnStatement in returnsEnCatch)
            {
                // Verificar que el return no esté dentro de un bloque try anidado
                if (!EstaEnBloqueTriAnidado(returnStatement, bloqueCatch))
                {
                    string nombreMetodo = metodo.Identifier.ValueText;

                    var diagnostico = Diagnostic.Create(
                        Regla,
                        returnStatement.GetLocation(),
                        nombreMetodo);

                    contexto.ReportDiagnostic(diagnostico);
                }
            }
        }

        /// <summary>
        /// Verifica si el return statement está dentro de un bloque try anidado dentro del catch.
        /// </summary>
        /// <param name="returnStatement">Statement return a verificar.</param>
        /// <param name="bloqueCatch">Bloque catch que lo contiene.</param>
        /// <returns>True si está en un try anidado, false si está directamente en el catch.</returns>
        private static bool EstaEnBloqueTriAnidado(ReturnStatementSyntax returnStatement, CatchClauseSyntax bloqueCatch)
        {
            var ancestros = returnStatement.Ancestors().ToList();
            var indiceCatch = ancestros.IndexOf(bloqueCatch);

            if (indiceCatch == -1)
                return false;

            // Buscar si hay un try statement entre el return y el catch
            for (int i = 0; i < indiceCatch; i++)
            {
                if (ancestros[i] is TryStatementSyntax)
                    return true;
            }

            return false;
        }
    }
}
