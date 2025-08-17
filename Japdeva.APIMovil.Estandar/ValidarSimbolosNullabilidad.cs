using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarSimbolosNullabilidad : DiagnosticAnalyzer
    {
        public const string DiagnosticIdNullConditional = "JAPDEVA027";
        public const string DiagnosticIdNullForgiving = "JAPDEVA028";

        private const string TituloNullConditional = "No usar operador de nullabilidad (?)";
        private const string FormatoMensajeNullConditional = "No se debe usar el operador '?' para nullabilidad. Use verificaciones explícitas de null en su lugar";
        private const string DescripcionNullConditional = "El operador '?' de nullabilidad debe evitarse para mantener el código explícito y reducir warnings de null. Use verificaciones explícitas de null en su lugar.";

        private const string TituloNullForgiving = "No usar operador null-forgiving (!)";
        private const string FormatoMensajeNullForgiving = "No se debe usar el operador '!' null-forgiving. Use verificaciones explícitas de null en su lugar";
        private const string DescripcionNullForgiving = "El operador '!' null-forgiving debe evitarse para mantener el código explícito y reducir warnings de null. Use verificaciones explícitas de null en su lugar.";

        private const string Categoria = "Design";

        private static readonly DiagnosticDescriptor ReglaNullConditional = new DiagnosticDescriptor(
            DiagnosticIdNullConditional,
            TituloNullConditional,
            FormatoMensajeNullConditional,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: DescripcionNullConditional,
            helpLinkUri: "https://docs.microsoft.com/dotnet/csharp/language-reference/operators/null-conditional-operators");

        private static readonly DiagnosticDescriptor ReglaNullForgiving = new DiagnosticDescriptor(
            DiagnosticIdNullForgiving,
            TituloNullForgiving,
            FormatoMensajeNullForgiving,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: DescripcionNullForgiving,
            helpLinkUri: "https://docs.microsoft.com/dotnet/csharp/language-reference/operators/null-forgiving");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(ReglaNullConditional, ReglaNullForgiving);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxTreeAction(AnalizarArchivoCompleto);
        }

        private static void AnalizarArchivoCompleto(SyntaxTreeAnalysisContext contexto)
        {
            var raiz = contexto.Tree.GetRoot(contexto.CancellationToken);
            
            // Verificar si el archivo contiene pruebas unitarias
            if (EsArchivoPruebasUnitarias(raiz))
                return; // No aplicar la regla en archivos de pruebas

            AnalizarNodosParaNullabilidad(contexto, raiz);
        }

        private static bool EsArchivoPruebasUnitarias(SyntaxNode raiz)
        {
            // Verificar si hay métodos marcados con [Fact]
            var metodos = raiz.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var metodo in metodos)
            {
                if (TieneAtributoFact(metodo))
                    return true;
            }

            return false;
        }

        private static bool TieneAtributoFact(MethodDeclarationSyntax metodo)
        {
            if (!metodo.AttributeLists.Any())
                return false;

            foreach (var listaAtributos in metodo.AttributeLists)
            {
                foreach (var atributo in listaAtributos.Attributes)
                {
                    var nombreAtributo = atributo.Name.ToString();
                    
                    // Verificar atributos de pruebas unitarias comunes
                    var atributosPrueba = new[]
                    {
                        "Fact", "Theory", "Test", "TestMethod", "TestCase"
                    };

                    if (atributosPrueba.Any(a => 
                        nombreAtributo.IndexOf(a, System.StringComparison.OrdinalIgnoreCase) != -1))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void AnalizarNodosParaNullabilidad(SyntaxTreeAnalysisContext contexto, SyntaxNode raiz)
        {
            // Analizar operadores null-conditional (?.)
            var operadoresNullConditional = raiz.DescendantNodes()
                .OfType<ConditionalAccessExpressionSyntax>();

            foreach (var operador in operadoresNullConditional)
            {
                var diagnostico = Diagnostic.Create(
                    ReglaNullConditional,
                    operador.OperatorToken.GetLocation());

                contexto.ReportDiagnostic(diagnostico);
            }

            // Analizar operadores null-forgiving (!)
            var operadoresNullForgiving = raiz.DescendantNodes()
                .OfType<PostfixUnaryExpressionSyntax>()
                .Where(expr => expr.IsKind(SyntaxKind.SuppressNullableWarningExpression));

            foreach (var operador in operadoresNullForgiving)
            {
                var diagnostico = Diagnostic.Create(
                    ReglaNullForgiving,
                    operador.OperatorToken.GetLocation());

                contexto.ReportDiagnostic(diagnostico);
            }

            // Analizar tipos nullable con ? en declaraciones
            AnalizarTiposNullable(contexto, raiz);
        }

        private static void AnalizarTiposNullable(SyntaxTreeAnalysisContext contexto, SyntaxNode raiz)
        {
            // Analizar tipos nullable en declaraciones de variables
            var declaracionesVariable = raiz.DescendantNodes().OfType<VariableDeclarationSyntax>();

            foreach (var declaracion in declaracionesVariable)
            {
                if (declaracion.Type is NullableTypeSyntax tipoNullable)
                {
                    var diagnostico = Diagnostic.Create(
                        ReglaNullConditional,
                        tipoNullable.QuestionToken.GetLocation());

                    contexto.ReportDiagnostic(diagnostico);
                }
            }

            // Analizar tipos nullable en parámetros de métodos
            var parametros = raiz.DescendantNodes().OfType<ParameterSyntax>();

            foreach (var parametro in parametros)
            {
                if (parametro.Type is NullableTypeSyntax tipoNullable)
                {
                    var diagnostico = Diagnostic.Create(
                        ReglaNullConditional,
                        tipoNullable.QuestionToken.GetLocation());

                    contexto.ReportDiagnostic(diagnostico);
                }
            }

            // Analizar tipos de retorno nullable
            var metodos = raiz.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var metodo in metodos)
            {
                if (metodo.ReturnType is NullableTypeSyntax tipoRetornoNullable)
                {
                    var diagnostico = Diagnostic.Create(
                        ReglaNullConditional,
                        tipoRetornoNullable.QuestionToken.GetLocation());

                    contexto.ReportDiagnostic(diagnostico);
                }
            }

            // Analizar propiedades con tipos nullable
            var propiedades = raiz.DescendantNodes().OfType<PropertyDeclarationSyntax>();

            foreach (var propiedad in propiedades)
            {
                if (propiedad.Type is NullableTypeSyntax tipoPropiedadNullable)
                {
                    var diagnostico = Diagnostic.Create(
                        ReglaNullConditional,
                        tipoPropiedadNullable.QuestionToken.GetLocation());

                    contexto.ReportDiagnostic(diagnostico);
                }
            }

            // Analizar campos con tipos nullable
            var campos = raiz.DescendantNodes().OfType<FieldDeclarationSyntax>();

            foreach (var campo in campos)
            {
                if (campo.Declaration.Type is NullableTypeSyntax tipoCampoNullable)
                {
                    var diagnostico = Diagnostic.Create(
                        ReglaNullConditional,
                        tipoCampoNullable.QuestionToken.GetLocation());

                    contexto.ReportDiagnostic(diagnostico);
                }
            }
        }
    }
}


