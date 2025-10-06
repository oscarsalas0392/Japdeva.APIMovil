using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ValidarComparacionNull : DiagnosticAnalyzer
{
    public const string DiagnosticIdEquals = "JAPDEVA085";
    public const string DiagnosticIdNotEquals = "JAPDEVA086";
    
    private static readonly DiagnosticDescriptor RuleEquals = new DiagnosticDescriptor(
        DiagnosticIdEquals,
        "Usar 'is null' en lugar de '== null'",
        "Usar 'is null' en lugar de '== null' para comparar con null. Reemplazar '{0} == null' por '{0} is null'",
        "Style",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Las comparaciones con null deben usar pattern matching 'is null' en lugar de '== null' para mejor performance y legibilidad.");

    private static readonly DiagnosticDescriptor RuleNotEquals = new DiagnosticDescriptor(
        DiagnosticIdNotEquals,
        "Usar 'is not null' en lugar de '!= null'",
        "Usar 'is not null' en lugar de '!= null' para comparar con null. Reemplazar '{0} != null' por '{0} is not null'",
        "Style",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Las comparaciones con null deben usar pattern matching 'is not null' en lugar de '!= null' para mejor performance y legibilidad.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
        ImmutableArray.Create(RuleEquals, RuleNotEquals);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.EqualsExpression);
        context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.NotEqualsExpression);
    }

    private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        var binaryExpression = (BinaryExpressionSyntax)context.Node;

        // Verificar si alguno de los operandos es null
        bool leftIsNull = IsNullLiteral(binaryExpression.Left);
        bool rightIsNull = IsNullLiteral(binaryExpression.Right);
        
        if (leftIsNull || rightIsNull)
        {
            string variableName = GetVariableName(binaryExpression, leftIsNull);
            
            if (binaryExpression.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken))
            {
                var diagnostic = Diagnostic.Create(RuleEquals, 
                    binaryExpression.GetLocation(), 
                    variableName);
                context.ReportDiagnostic(diagnostic);
            }
            else if (binaryExpression.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken))
            {
                var diagnostic = Diagnostic.Create(RuleNotEquals, 
                    binaryExpression.GetLocation(), 
                    variableName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsNullLiteral(ExpressionSyntax expression)
    {
        return expression.IsKind(SyntaxKind.NullLiteralExpression);
    }

    private static string GetVariableName(BinaryExpressionSyntax binaryExpression, bool leftIsNull)
    {
        var variableExpression = leftIsNull ? binaryExpression.Right : binaryExpression.Left;
        return variableExpression.ToString();
    }
}