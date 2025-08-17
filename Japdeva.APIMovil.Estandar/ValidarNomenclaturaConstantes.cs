using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarNomenclaturaConstantes : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA007";

        private const string Titulo = "Constante debe usar SCREAMING_SNAKE_CASE";
        private const string FormatoMensaje = "La constante '{0}' debe seguir la convención SCREAMING_SNAKE_CASE (ejemplo: '{1}')";
        private const string Descripcion = "Las constantes deben usar la convención SCREAMING_SNAKE_CASE (todas las letras en mayúsculas separadas por guiones bajos) para distinguirlas claramente de otras variables.";
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/standard/design-guidelines/field");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarCampo, SyntaxKind.FieldDeclaration);
        }

        private static void AnalizarCampo(SyntaxNodeAnalysisContext contexto)
        {
            var campo = (FieldDeclarationSyntax)contexto.Node;

            // Verificar si es una constante
            var esConstante = campo.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword));
            if (!esConstante)
                return;

            // Analizar cada variable en la declaración del campo
            foreach (var variable in campo.Declaration.Variables)
            {
                var nombreConstante = variable.Identifier.ValueText;
                ValidarNomenclaturaConstante(contexto, variable, nombreConstante);
            }
        }

        private static void ValidarNomenclaturaConstante(SyntaxNodeAnalysisContext contexto, 
            VariableDeclaratorSyntax variable, string nombreConstante)
        {
            if (!EsNombreScreamingSnakeCase(nombreConstante))
            {
                var nombreSugerido = ConvertirAScreamingSnakeCase(nombreConstante);
                
                var diagnostico = Diagnostic.Create(
                    Regla,
                    variable.Identifier.GetLocation(),
                    nombreConstante,
                    nombreSugerido);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static bool EsNombreScreamingSnakeCase(string nombre)
        {
            // Patrón para SCREAMING_SNAKE_CASE:
            // - Solo letras mayúsculas, números y guiones bajos
            // - No puede empezar o terminar con guión bajo
            // - No puede tener guiones bajos consecutivos
            var patron = @"^[A-Z]([A-Z0-9]*(_[A-Z0-9]+)*)?$";
            return Regex.IsMatch(nombre, patron);
        }

        private static string ConvertirAScreamingSnakeCase(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return nombre;

            // Convertir PascalCase/camelCase a SCREAMING_SNAKE_CASE
            var resultado = Regex.Replace(nombre, @"([a-z])([A-Z])", "$1_$2");
            
            // Convertir a mayúsculas
            resultado = resultado.ToUpperInvariant();
            
            // Limpiar guiones bajos múltiples
            resultado = Regex.Replace(resultado, @"_+", "_");
            
            // Limpiar guiones bajos al inicio y final
            resultado = resultado.Trim('_');
            
            return resultado;
        }
    }
}
