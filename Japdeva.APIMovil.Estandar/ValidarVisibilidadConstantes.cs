using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarVisibilidadConstantes : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA006";

        private const string Titulo = "Constante debe ser privada";
        private const string FormatoMensaje = "La constante '{0}' debe ser privada en lugar de {1}";
        private const string Descripcion = "Las constantes deben ser privadas para mantener el encapsulamiento y evitar dependencias externas en valores internos.";
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/constants");

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
                ValidarVisibilidadConstante(contexto, campo, variable, nombreConstante);
            }
        }

        private static void ValidarVisibilidadConstante(SyntaxNodeAnalysisContext contexto, FieldDeclarationSyntax campo, 
            VariableDeclaratorSyntax variable, string nombreConstante)
        {
            var esPrivada = !campo.Modifiers.Any(m => 
                m.IsKind(SyntaxKind.PublicKeyword) || 
                m.IsKind(SyntaxKind.ProtectedKeyword) || 
                m.IsKind(SyntaxKind.InternalKeyword));

            // Si no es privada, reportar el problema
            if (!esPrivada)
            {
                var tipoVisibilidad = ObtenerTipoVisibilidad(campo);
                
                // Excluir constantes que deben ser públicas por diseño
                if (!EsConstanteQueDebeSerPublica(nombreConstante, campo))
                {
                    var diagnostico = Diagnostic.Create(
                        Regla,
                        variable.Identifier.GetLocation(),
                        nombreConstante,
                        tipoVisibilidad);

                    contexto.ReportDiagnostic(diagnostico);
                }
            }
        }

        private static string ObtenerTipoVisibilidad(FieldDeclarationSyntax campo)
        {
            if (campo.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                return "public";
            if (campo.Modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword)))
                return "protected";
            if (campo.Modifiers.Any(m => m.IsKind(SyntaxKind.InternalKeyword)))
                return "internal";

            return "sin modificador explícito";
        }

        private static bool EsConstanteQueDebeSerPublica(string nombreConstante, FieldDeclarationSyntax campo)
        {
            // Excepciones para constantes que legítimamente deben ser públicas
            
            // Constantes de API o configuración pública
            var patronesPublicos = new[]
            {
                "Version", "ApiVersion", "ApplicationName", "ProductName",
                "DefaultTimeout", "MaxRetries", "BufferSize"
            };

            if (patronesPublicos.Any(patron => nombreConstante.Contains(patron)))
                return true;

            // Si está en una clase estática pública, podría ser parte de una API
            var claseContenedora = campo.Parent as ClassDeclarationSyntax;
            if (claseContenedora != null && 
                claseContenedora.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)) &&
                claseContenedora.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
            {
                return true;
            }

            return false;
        }
    }
}
