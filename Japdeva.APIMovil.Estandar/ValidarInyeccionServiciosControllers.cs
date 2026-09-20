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
    /// Analizador que verifica que los servicios en los controladores se inyecten usando [FromServices] 
    /// como parámetros del método en lugar de inyección por constructor.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarInyeccionServiciosControllers : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "JAPDEVA097",
            "Los parámetros de controladores deben usar atributos de origen de datos correctos",
            "El método '{0}' debe usar [FromServices], [FromBody], [FromHeader], o [FromRoute] en sus parámetros para especificar el origen de los datos",
            "Design",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Los parámetros en métodos de controlador deben usar atributos explícitos ([FromServices], [FromBody], [FromHeader], [FromRoute]) para especificar claramente el origen de los datos.",
            helpLinkUri: "https://docs.microsoft.com/aspnet/core/mvc/models/model-binding");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

            // Verificar parámetros que necesitan atributos de origen de datos
            foreach (var parameter in methodDeclaration.ParameterList.Parameters)
            {
                if (NecesitaAtributoOrigenDatos(parameter) && !TieneAtributoOrigenDatos(parameter))
                {
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        parameter.GetLocation(),
                        methodDeclaration.Identifier.ValueText);
                    context.ReportDiagnostic(diagnostic);
                }
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

        private static bool NecesitaAtributoOrigenDatos(ParameterSyntax parameter)
        {
            var parameterType = parameter.Type.ToString();
            var parameterName = parameter.Identifier.ValueText;

            // Excluir tipos básicos que ASP.NET Core maneja automáticamente
            var tiposBasicos = new[] { "CancellationToken", "HttpContext", "HttpRequest", "HttpResponse" };
            if (tiposBasicos.Any(tipo => parameterType.Contains(tipo)))
                return false;

            // Todos los demás parámetros necesitan atributo de origen
            return true;
        }

        private static bool TieneAtributoOrigenDatos(ParameterSyntax parameter)
        {
            var atributosOrigenDatos = new[] { "FromServices", "FromBody", "FromHeader", "FromRoute", "FromQuery", "FromForm" };
            
            return parameter.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr => atributosOrigenDatos.Any(fromAttr => attr.Name.ToString().Contains(fromAttr)));
        }
    }
}