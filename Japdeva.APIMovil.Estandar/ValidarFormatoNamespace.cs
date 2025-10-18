using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarFormatoNamespace : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA082";
        private const string Titulo = "Formato de namespace incorrecto";
        private const string FormatoMensaje = "El namespace debe usar formato clásico con llaves 'namespace Nombre {{ }}'";
        private const string Descripcion = "Todos los namespaces deben seguir el formato tradicional con llaves para mantener consistencia en el código.";
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/namespace");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarCompilationUnit, SyntaxKind.CompilationUnit);
        }

        private static void AnalizarCompilationUnit(SyntaxNodeAnalysisContext contexto)
        {
            var compilationUnit = (CompilationUnitSyntax)contexto.Node;
            
            // Verificar que tenga namespaces tradicionales con llaves
            var namespacesTradicionales = compilationUnit.Members
                .OfType<NamespaceDeclarationSyntax>()
                .ToList();

            // Verificar si hay clases directamente en compilation unit sin namespace
            var tiposDirectos = compilationUnit.Members
                .OfType<TypeDeclarationSyntax>()
                .ToList();

            if (tiposDirectos.Any() && !namespacesTradicionales.Any())
            {
                foreach (var tipo in tiposDirectos)
                {
                    var diagnostico = Diagnostic.Create(
                        Regla,
                        tipo.GetLocation(),
                        $"Clase '{tipo.Identifier.Text}' debe estar dentro de un namespace con formato tradicional");

                    contexto.ReportDiagnostic(diagnostico);
                }
            }

            // Verificar que todos los namespaces usen el formato tradicional con llaves
            foreach (var namespaceNode in namespacesTradicionales)
            {
                if (!TieneFormatoTradicional(namespaceNode))
                {
                    var diagnostico = Diagnostic.Create(
                        Regla,
                        namespaceNode.GetLocation(),
                        namespaceNode.Name.ToString());

                    contexto.ReportDiagnostic(diagnostico);
                }
            }
        }

        private static bool TieneFormatoTradicional(NamespaceDeclarationSyntax namespaceNode)
        {
            // Verificar que el namespace tenga llaves de apertura y cierre
            var tieneOpenBrace = namespaceNode.OpenBraceToken.IsKind(SyntaxKind.OpenBraceToken);
            var tieneCloseBrace = namespaceNode.CloseBraceToken.IsKind(SyntaxKind.CloseBraceToken);
            
            return tieneOpenBrace && tieneCloseBrace;
        }
    }
}