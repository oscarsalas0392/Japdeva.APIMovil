using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ValidarUbicacionUsings : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        "JAPDEVA090",
        "Los using statements deben estar fuera del namespace",
        "Los using statements deben declararse fuera del namespace, no dentro de él",
        "Estructura",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Los using statements deben colocarse al inicio del archivo, fuera de cualquier declaración de namespace para mantener consistencia en el código.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNamespace, SyntaxKind.NamespaceDeclaration);
    }

    private static void AnalyzeNamespace(SyntaxNodeAnalysisContext context)
    {
        var namespaceDeclaration = (NamespaceDeclarationSyntax)context.Node;
        
        // Buscar using statements dentro del namespace
        var usingsInsideNamespace = namespaceDeclaration.Usings;
        
        foreach (var usingDirective in usingsInsideNamespace)
        {
            var diagnostic = Diagnostic.Create(Rule,
                usingDirective.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}