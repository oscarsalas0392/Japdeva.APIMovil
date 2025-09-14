using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    /// <summary>
    /// Analizador que verifica que todos los middlewares implementen su interfaz correspondiente.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarImplementacionInterfazMiddlewares : DiagnosticAnalyzer
    {
        // Regla para verificar que el middleware implemente su interfaz
        public static readonly DiagnosticDescriptor JAPDEVA078 = new DiagnosticDescriptor(
            "JAPDEVA078",
            "Los middlewares deben implementar su interfaz correspondiente",
            "El middleware '{0}' debe implementar la interfaz 'I{0}'",
            "Naming",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Todos los middlewares deben implementar su interfaz correspondiente para mantener la consistencia arquitectónica.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(JAPDEVA078);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationAction(AnalyzeCompilation);
        }

        private static void AnalyzeCompilation(CompilationAnalysisContext context)
        {
            var syntaxTrees = context.Compilation.SyntaxTrees;
            var middlewareClasses = new System.Collections.Generic.List<(ClassDeclarationSyntax ClassDeclaration, string ClassName, SyntaxTree SyntaxTree)>();
            var interfaces = new System.Collections.Generic.HashSet<string>();

            // Primero, recopilar todos los middlewares e interfaces
            foreach (var syntaxTree in syntaxTrees)
            {
                var root = syntaxTree.GetRoot();
                
                // Buscar clases middleware
                var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (var classDeclaration in classDeclarations)
                {
                    var className = classDeclaration.Identifier.ValueText;
                    if (IsMiddlewareClass(classDeclaration, className))
                    {
                        middlewareClasses.Add((classDeclaration, className, syntaxTree));
                    }
                }

                // Buscar interfaces
                var interfaceDeclarations = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();
                foreach (var interfaceDeclaration in interfaceDeclarations)
                {
                    var interfaceName = interfaceDeclaration.Identifier.ValueText;
                    var namespaceDeclaration = interfaceDeclaration.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
                    var namespaceName = namespaceDeclaration?.Name.ToString();
                    
                    if (namespaceName != null && namespaceName.Contains("Middlewares") && interfaceName.StartsWith("I") && interfaceName.EndsWith("Middleware"))
                    {
                        interfaces.Add(interfaceName);
                    }
                }
            }

            // Analizar cada middleware
            foreach (var (classDeclaration, className, syntaxTree) in middlewareClasses)
            {
                var expectedInterfaceName = $"I{className}";
                
                // Verificar si implementa la interfaz
                var implementsInterface = classDeclaration.BaseList?.Types
                    .Any(baseType => baseType.Type.ToString() == expectedInterfaceName) == true;

                if (!implementsInterface)
                {
                    var diagnostic = Diagnostic.Create(
                        JAPDEVA078,
                        classDeclaration.Identifier.GetLocation(),
                        className);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static bool IsMiddlewareClass(ClassDeclarationSyntax classDeclaration, string className)
        {
            // Verificar si es una clase middleware basándose en:
            // 1. El nombre termina con "Middleware"
            if (!className.EndsWith("Middleware"))
                return false;

            // 2. Está en el namespace correcto (cualquier sub-namespace de Middlewares)
            var namespaceDeclaration = classDeclaration.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            var namespaceName = namespaceDeclaration?.Name.ToString();
            if (namespaceName == null || !namespaceName.Contains("Middlewares"))
                return false;

            // 3. Tiene un método InvokeAsync
            var hasInvokeAsyncMethod = classDeclaration.Members
                .OfType<MethodDeclarationSyntax>()
                .Any(m => m.Identifier.ValueText == "InvokeAsync");

            return hasInvokeAsyncMethod;
        }
    }
}
