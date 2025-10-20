using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Japdeva.APIMovil.Estandar
{
    /// <summary>
    /// Analizador que verifica que los métodos de controladores usen expresiones lambda de una sola línea
    /// en lugar de bloques de código completos para mantener consistencia y simplicidad.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarEstructuraMetodosControllers : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "JAPDEVA095",
            "Los métodos de controlador deben usar expresiones lambda de una sola línea",
            "El método '{0}' en el controlador debe usar una expresión lambda (=>) en lugar de un bloque de código para mantener simplicidad y consistencia",
            "Design",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Los métodos de controlador deben delegar la lógica a servicios y usar expresiones lambda de una sola línea para mantener los controladores ligeros y consistentes.",
            helpLinkUri: "https://docs.microsoft.com/aspnet/core/web-api/action-return-types");

        public static readonly DiagnosticDescriptor RuleReturnType = new DiagnosticDescriptor(
            "JAPDEVA096",
            "Los métodos de controlador deben devolver IActionResult",
            "El método '{0}' en el controlador debe devolver IActionResult o Task<IActionResult> para mantener consistencia en las respuestas HTTP",
            "Design",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Todos los métodos de controlador deben devolver IActionResult para estandarizar el manejo de respuestas HTTP y facilitar el testing.",
            helpLinkUri: "https://docs.microsoft.com/aspnet/core/web-api/action-return-types");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, RuleReturnType);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = context.Node as MethodDeclarationSyntax;
            if (methodDeclaration == null)
                return;

            // Verificar si estamos en una clase Controller
            if (!EstaEnClaseController(methodDeclaration))
                return;

            // Verificar si es un método público
            if (!EsMetodoPublico(methodDeclaration))
                return;

            // Verificar si tiene atributos HTTP (HttpGet, HttpPost, etc.)
            if (!TieneAtributosHttp(methodDeclaration))
                return;

            // Verificar si el tipo de retorno es IActionResult
            if (!DevuelveIActionResult(methodDeclaration))
            {
                var diagnostic = Diagnostic.Create(
                    RuleReturnType,
                    methodDeclaration.ReturnType.GetLocation(),
                    methodDeclaration.Identifier.ValueText);
                context.ReportDiagnostic(diagnostic);
            }

            // Verificar si usa expresión lambda en lugar de bloque de código
            if (TieneBloqueCodigoEnLugarDeExpresion(methodDeclaration))
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    methodDeclaration.Identifier.GetLocation(),
                    methodDeclaration.Identifier.ValueText);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool EstaEnClaseController(MethodDeclarationSyntax methodDeclaration)
        {
            var classDeclaration = methodDeclaration.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classDeclaration == null)
                return false;

            // Verificar si la clase termina con "Controller"
            if (classDeclaration.Identifier.ValueText.EndsWith("Controller"))
                return true;

            // Verificar si hereda de ControllerBase o Controller
            if (classDeclaration.BaseList?.Types.Any(t => 
                t.Type.ToString().Contains("ControllerBase") || 
                t.Type.ToString().Contains("Controller")) == true)
                return true;

            // Verificar si tiene el atributo [ApiController]
            return classDeclaration.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr => attr.Name.ToString().Contains("ApiController"));
        }

        private static bool EsMetodoPublico(MethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        }

        private static bool TieneAtributosHttp(MethodDeclarationSyntax methodDeclaration)
        {
            var atributosHttp = new[] { "HttpGet", "HttpPost", "HttpPut", "HttpDelete", "HttpPatch", "HttpHead", "HttpOptions" };
            
            return methodDeclaration.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr => atributosHttp.Any(httpAttr => attr.Name.ToString().Contains(httpAttr)));
        }

        private static bool TieneBloqueCodigoEnLugarDeExpresion(MethodDeclarationSyntax methodDeclaration)
        {
            // Si tiene expresión lambda (=>), está bien
            if (methodDeclaration.ExpressionBody != null)
                return false;

            // Si tiene un bloque de código con llaves, necesita ser cambiado a expresión lambda
            if (methodDeclaration.Body != null)
            {
                // Permitir bloques de código solo si son muy complejos (más de una declaración simple)
                var statements = methodDeclaration.Body.Statements;
                
                // Si solo tiene una declaración return, debería ser expresión lambda
                if (statements.Count == 1 && statements[0] is ReturnStatementSyntax)
                    return true;

                // Si tiene más de 3 declaraciones, puede mantener el bloque
                if (statements.Count > 3)
                    return false;

                // Para 2-3 declaraciones simples, recomendar expresión lambda
                return true;
            }

            return false;
        }

        private static bool DevuelveIActionResult(MethodDeclarationSyntax methodDeclaration)
        {
            var returnType = methodDeclaration.ReturnType.ToString();
            
            // Verificar diferentes formas de IActionResult
            return returnType.Contains("IActionResult") ||
                   returnType.Contains("ActionResult") ||
                   returnType.Contains("Task<IActionResult>") ||
                   returnType.Contains("Task<ActionResult");
        }
    }
}