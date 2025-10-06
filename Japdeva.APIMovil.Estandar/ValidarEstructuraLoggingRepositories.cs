using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ValidarEstructuraLoggingRepositories : DiagnosticAnalyzer
{
    public const string DiagnosticId = "JAPDEVA083";
    
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Los métodos de repositorios deben seguir el patrón de logging estándar",
        "El método '{0}' en la clase '{1}' debe seguir el patrón de logging: try-catch-finally con _logger.Inicio, _logger.Error y _logger.Fin",
        "Logging",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Los métodos de las clases Repository deben implementar el patrón de logging estándar con try-catch-finally y las llamadas correspondientes al logger.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        var classDeclaration = methodDeclaration.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        
        if (classDeclaration == null)
            return;

        // Verificar si es una clase Repository
        string className = classDeclaration.Identifier.ValueText;
        if (!className.EndsWith("Repository"))
            return;

        // Verificar si es un método público (excluir constructores)
        if (methodDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)) && 
            !methodDeclaration.Identifier.ValueText.Equals(className))
        {
            if (!ValidateLoggingPattern(methodDeclaration))
            {
                var diagnostic = Diagnostic.Create(Rule, 
                    methodDeclaration.Identifier.GetLocation(), 
                    methodDeclaration.Identifier.ValueText, 
                    className);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool ValidateLoggingPattern(MethodDeclarationSyntax method)
    {
        // Buscar el bloque try-catch-finally
        var tryStatement = method.DescendantNodes().OfType<TryStatementSyntax>().FirstOrDefault();
        if (tryStatement == null)
            return false;

        // Verificar que tiene catch y finally
        if (tryStatement.Catches.Count == 0 || tryStatement.Finally == null)
            return false;

        // Verificar llamada a _logger.Inicio en try
        bool hasLoggerInicio = HasLoggerCall(tryStatement.Block, "Inicio");
        
        // Verificar llamada a _logger.Error en catch
        bool hasLoggerError = tryStatement.Catches.Any(c => HasLoggerCall(c.Block, "Error"));
        
        // Verificar llamada a _logger.Fin en finally
        bool hasLoggerFin = HasLoggerCall(tryStatement.Finally.Block, "Fin");

        return hasLoggerInicio && hasLoggerError && hasLoggerFin;
    }

    private static bool HasLoggerCall(BlockSyntax block, string methodName)
    {
        if (block == null)
            return false;

        return block.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Any(inv => inv.Expression is MemberAccessExpressionSyntax memberAccess &&
                       memberAccess.Expression.ToString().Contains("_logger") &&
                       memberAccess.Name.Identifier.ValueText == methodName);
    }
}