using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ValidarOrdenCamposReadonlyConst : DiagnosticAnalyzer
{
    public const string DiagnosticId = "JAPDEVA084";
    
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Los campos readonly deben declararse antes que las constantes",
        "En la clase '{0}', los campos readonly deben declararse antes que las constantes. Campo const '{1}' encontrado antes del campo readonly '{2}'",
        "CodeOrganization",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Para mantener un orden consistente en las clases, todos los campos readonly deben declararse antes que las constantes.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        
        // Obtener todos los campos de la clase
        var fieldDeclarations = classDeclaration.Members
            .OfType<FieldDeclarationSyntax>()
            .ToList();

        if (fieldDeclarations.Count == 0)
            return;

        // Separar campos readonly y const con su posición
        var readonlyFields = fieldDeclarations
            .Where(f => f.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword)))
            .Select(f => new { Field = f, Position = GetFieldPosition(f, classDeclaration) })
            .ToList();

        var constFields = fieldDeclarations
            .Where(f => f.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword)))
            .Select(f => new { Field = f, Position = GetFieldPosition(f, classDeclaration) })
            .ToList();

        // Verificar que todos los readonly vengan antes que todos los const
        foreach (var constField in constFields)
        {
            foreach (var readonlyField in readonlyFields)
            {
                if (constField.Position < readonlyField.Position)
                {
                    // Encontramos un const antes de un readonly - reportar error
                    var constFieldName = GetFieldName(constField.Field);
                    var readonlyFieldName = GetFieldName(readonlyField.Field);
                    
                    var diagnostic = Diagnostic.Create(Rule,
                        constField.Field.GetLocation(),
                        classDeclaration.Identifier.ValueText,
                        constFieldName,
                        readonlyFieldName);
                    
                    context.ReportDiagnostic(diagnostic);
                    return; // Solo reportar el primer error encontrado
                }
            }
        }
    }

    private static int GetFieldPosition(FieldDeclarationSyntax field, ClassDeclarationSyntax classDeclaration)
    {
        return classDeclaration.Members.IndexOf(field);
    }

    private static string GetFieldName(FieldDeclarationSyntax field)
    {
        var variable = field.Declaration.Variables.FirstOrDefault();
        return variable?.Identifier.ValueText ?? "campo";
    }
}