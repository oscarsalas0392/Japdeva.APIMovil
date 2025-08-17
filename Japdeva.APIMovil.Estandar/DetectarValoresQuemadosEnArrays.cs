using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DetectarValoresQuemadosEnArrays : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA034";

        private const string Titulo = "Array con valores hardcodeados detectado";
        private const string FormatoMensaje = "El array '{0}' contiene valores hardcodeados que deberían extraerse a constantes o a un archivo de configuración";
        private const string Descripcion = "Los arrays con valores hardcodeados deben extraerse como constantes, configuración o recursos para mejorar la mantenibilidad y facilitar las traducciones o cambios de configuración.";
        private const string Categoria = "Maintainability";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1802");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarArrayCreation, SyntaxKind.ArrayCreationExpression);
            contexto.RegisterSyntaxNodeAction(AnalizarImplicitArrayCreation, SyntaxKind.ImplicitArrayCreationExpression);
            contexto.RegisterSyntaxNodeAction(AnalizarFieldDeclaration, SyntaxKind.FieldDeclaration);
        }

        private static void AnalizarArrayCreation(SyntaxNodeAnalysisContext contexto)
        {
            var arrayCreation = (ArrayCreationExpressionSyntax)contexto.Node;
            
            if (arrayCreation.Initializer != null)
            {
                AnalizarInicializadorArray(contexto, arrayCreation.Initializer, arrayCreation);
            }
        }

        private static void AnalizarImplicitArrayCreation(SyntaxNodeAnalysisContext contexto)
        {
            var implicitArray = (ImplicitArrayCreationExpressionSyntax)contexto.Node;
            
            if (implicitArray.Initializer != null)
            {
                AnalizarInicializadorArray(contexto, implicitArray.Initializer, implicitArray);
            }
        }

        private static void AnalizarFieldDeclaration(SyntaxNodeAnalysisContext contexto)
        {
            var fieldDeclaration = (FieldDeclarationSyntax)contexto.Node;
            
            // Solo analizar arrays que no sean constantes
            var esConstante = fieldDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword));
            if (esConstante)
                return;

            foreach (var variable in fieldDeclaration.Declaration.Variables)
            {
                if (variable.Initializer?.Value != null)
                {
                    // Verificar si es un array con inicializador
                    switch (variable.Initializer.Value)
                    {
                        case ArrayCreationExpressionSyntax arrayCreation when arrayCreation.Initializer != null:
                            AnalizarInicializadorArray(contexto, arrayCreation.Initializer, arrayCreation, variable.Identifier.ValueText);
                            break;
                            
                        case ImplicitArrayCreationExpressionSyntax implicitArray when implicitArray.Initializer != null:
                            AnalizarInicializadorArray(contexto, implicitArray.Initializer, implicitArray, variable.Identifier.ValueText);
                            break;
                    }
                }
            }
        }

        private static void AnalizarInicializadorArray(SyntaxNodeAnalysisContext contexto, 
            InitializerExpressionSyntax inicializador, SyntaxNode nodoArray, string nombreVariable = null)
        {
            if (EsArrayEspecial(nodoArray, nombreVariable))
                return;

            var valoresHardcodeados = new List<string>();
            var tipoElemento = DeterminarTipoElemento(inicializador);

            // Recopilar valores hardcodeados
            foreach (var expresion in inicializador.Expressions)
            {
                if (expresion is LiteralExpressionSyntax literal)
                {
                    var valor = ObtenerValorLiteral(literal);
                    if (EsValorHardcodeado(valor, tipoElemento))
                    {
                        valoresHardcodeados.Add(valor);
                    }
                }
            }

            // Si hay múltiples valores hardcodeados, reportar
            if (valoresHardcodeados.Count >= 3 || (valoresHardcodeados.Count >= 2 && EsTipoSignificativo(tipoElemento)))
            {
                var nombreArray = nombreVariable ?? "array";
                
                var diagnostico = Diagnostic.Create(
                    Regla,
                    nodoArray.GetLocation(),
                    nombreArray);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static bool EsArrayEspecial(SyntaxNode nodoArray, string nombreVariable)
        {
            // Excluir arrays en atributos
            if (nodoArray.Ancestors().OfType<AttributeSyntax>().Any())
                return true;

            // Excluir arrays de test data
            if (!string.IsNullOrEmpty(nombreVariable))
            {
                var nombresEspeciales = new[]
                {
                    "testdata", "testcases", "unittest", "mockdata", "sampledata",
                    "examples", "fixtures", "seeds", "defaults"
                };

                if (nombresEspeciales.Any(nombre => 
                    nombreVariable.IndexOf(nombre, System.StringComparison.OrdinalIgnoreCase) != -1))
                    return true;
            }

            // Excluir arrays dentro de métodos de test
            var metodoContenedor = nodoArray.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (metodoContenedor != null)
            {
                // Verificar si tiene atributos de test
                if (metodoContenedor.AttributeLists.Any(attrList =>
                    attrList.Attributes.Any(attr =>
                    {
                        var nombre = attr.Name.ToString();
                        return nombre.IndexOf("Test", System.StringComparison.OrdinalIgnoreCase) != -1 ||
                               nombre.IndexOf("Fact", System.StringComparison.OrdinalIgnoreCase) != -1 ||
                               nombre.IndexOf("Theory", System.StringComparison.OrdinalIgnoreCase) != -1;
                    })))
                {
                    return true;
                }

                // Verificar nombre del método
                var nombreMetodo = metodoContenedor.Identifier.ValueText;
                if (nombreMetodo.IndexOf("Test", System.StringComparison.OrdinalIgnoreCase) != -1 ||
                    nombreMetodo.IndexOf("Should", System.StringComparison.OrdinalIgnoreCase) != -1)
                {
                    return true;
                }
            }

            // Excluir arrays en clases de configuración o constantes
            var claseContenedora = nodoArray.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (claseContenedora != null)
            {
                var nombreClase = claseContenedora.Identifier.ValueText;
                var clasesEspeciales = new[]
                {
                    "Constants", "Config", "Settings", "Options", "Defaults",
                    "TestData", "MockData", "Fixtures", "Seeds"
                };

                if (clasesEspeciales.Any(clase => 
                    nombreClase.IndexOf(clase, System.StringComparison.OrdinalIgnoreCase) != -1))
                    return true;
            }

            return false;
        }

        private static string DeterminarTipoElemento(InitializerExpressionSyntax inicializador)
        {
            var primerElemento = inicializador.Expressions.FirstOrDefault();
            if (primerElemento is LiteralExpressionSyntax literal)
            {
                if (literal.Token.IsKind(SyntaxKind.StringLiteralToken))
                    return "string";
                if (literal.Token.IsKind(SyntaxKind.NumericLiteralToken))
                    return "numeric";
                if (literal.Token.IsKind(SyntaxKind.TrueKeyword) || literal.Token.IsKind(SyntaxKind.FalseKeyword))
                    return "boolean";
            }

            return "unknown";
        }

        private static bool EsValorHardcodeado(string valor, string tipoElemento)
        {
            if (string.IsNullOrEmpty(valor))
                return false;

            // Para strings, cualquier literal no vacío es hardcodeado
            if (tipoElemento == "string")
            {
                // Excluir strings muy cortos o comunes
                if (valor.Length <= 3)
                    return false;
                
                var valoresComunes = new[] { "test", "temp", "default", "null", "empty" };
                return !valoresComunes.Any(comun => 
                    valor.IndexOf(comun, System.StringComparison.OrdinalIgnoreCase) != -1);
            }

            // Para números, excluir valores muy simples
            if (tipoElemento == "numeric")
            {
                if (int.TryParse(valor, out int numero))
                {
                    // Excluir números del 0 al 10 y algunos comunes
                    return numero < 0 || numero > 10;
                }
                return true;
            }

            // Para booleanos, no son hardcodeados problemáticos
            if (tipoElemento == "boolean")
                return false;

            return true;
        }

        private static bool EsTipoSignificativo(string tipoElemento)
        {
            // Los strings son más significativos para configuración
            return tipoElemento == "string";
        }

        private static string ObtenerValorLiteral(LiteralExpressionSyntax literal)
        {
            return literal.Token.ValueText;
        }
    }
}

