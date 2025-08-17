using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarNomenclaturaMetodosAsincronicos : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA013";

        private const string Titulo = "Método asíncrono debe terminar en Async";
        private const string FormatoMensaje = "El método asíncrono '{0}' debe terminar en 'Async' (ejemplo: '{1}')";
        private const string Descripcion = "Los métodos asíncronos deben terminar en 'Async' para seguir las convenciones de nomenclatura de .NET y facilitar su identificación en el código.";
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/csharp/async#recognize-cpu-bound-and-io-bound-work");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarMetodo, SyntaxKind.MethodDeclaration);
        }

        private static void AnalizarMetodo(SyntaxNodeAnalysisContext contexto)
        {
            var metodo = (MethodDeclarationSyntax)contexto.Node;
            var nombreMetodo = metodo.Identifier.ValueText;

            // Verificar si es un método asíncrono
            if (!EsMetodoAsincrono(metodo))
                return;

            // Validar nomenclatura del método asíncrono
            ValidarNomenclaturaMetodoAsincrono(contexto, metodo, nombreMetodo);
        }

        private static bool EsMetodoAsincrono(MethodDeclarationSyntax metodo)
        {
            // Verificar si tiene el modificador async
            var tieneModificadorAsync = metodo.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword));
            
            // Verificar si retorna Task, Task<T>, ValueTask o ValueTask<T>
            var tipoRetorno = metodo.ReturnType.ToString();
            var retornaTask = tipoRetorno.StartsWith("Task") || 
                             tipoRetorno.StartsWith("ValueTask") ||
                             tipoRetorno.Contains("Task<") ||
                             tipoRetorno.Contains("ValueTask<");

            // Es asíncrono si tiene el modificador async O retorna Task/ValueTask
            return tieneModificadorAsync || retornaTask;
        }

        private static void ValidarNomenclaturaMetodoAsincrono(SyntaxNodeAnalysisContext contexto, 
            MethodDeclarationSyntax metodo, string nombreMetodo)
        {
            if (!EsMetodoAsincronoEspecial(nombreMetodo, metodo) && 
                !nombreMetodo.EndsWith("Async", System.StringComparison.Ordinal))
            {
                var nombreSugerido = ObtenerNombreSugerido(nombreMetodo);
                
                var diagnostico = Diagnostic.Create(
                    Regla,
                    metodo.Identifier.GetLocation(),
                    nombreMetodo,
                    nombreSugerido);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static bool EsMetodoAsincronoEspecial(string nombreMetodo, MethodDeclarationSyntax metodo)
        {
            // Excluir métodos que ya terminan en Async
            if (nombreMetodo.EndsWith("Async", System.StringComparison.Ordinal))
                return true;

            // Excluir métodos que comienzan con underscore (convención especial)
            if (nombreMetodo.StartsWith("_"))
                return true;

            // Excluir métodos Main (punto de entrada asíncrono)
            if (nombreMetodo.Equals("Main", System.StringComparison.OrdinalIgnoreCase))
                return true;

            // Excluir métodos con atributos especiales
            if (metodo.AttributeLists.Any())
            {
                foreach (var listaAtributos in metodo.AttributeLists)
                {
                    foreach (var atributo in listaAtributos.Attributes)
                    {
                        var nombreAtributoClase = atributo.Name.ToString();
                        
                        // Atributos que pueden requerir nombres específicos sin Async
                        var atributosEspeciales = new[]
                        {
                            "Test", "TestMethod", "Fact", "Theory", "TestCase",
                            "HttpGet", "HttpPost", "HttpPut", "HttpDelete", "HttpPatch",
                            "Route", "ActionName", "OnActionExecuting",
                            "OnActionExecuted", "DllImport", "MethodImpl",
                            "ComVisible", "Conditional", "WebMethod"
                        };

                        if (atributosEspeciales.Any(a => 
                            nombreAtributoClase.IndexOf(a, System.StringComparison.OrdinalIgnoreCase) != -1))
                        {
                            return true;
                        }
                    }
                }
            }

            // Excluir métodos override (implementan clase base)
            if (metodo.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)))
                return true;

            // Excluir métodos que implementan interfaces explícitamente
            if (metodo.ExplicitInterfaceSpecifier != null)
                return true;

            // Verificar si implementa una interfaz que no requiere Async
            if (ImplementaInterfazSinAsync(metodo, nombreMetodo))
                return true;

            // Excluir event handlers (métodos que manejan eventos)
            if (EsEventHandler(metodo, nombreMetodo))
                return true;

            return false;
        }

        private static bool ImplementaInterfazSinAsync(MethodDeclarationSyntax metodo, string nombreMetodo)
        {
            // Obtener la clase contenedora
            var claseContenedora = metodo.Parent as TypeDeclarationSyntax;
            if (claseContenedora == null)
                return false;

            // Verificar si la clase implementa interfaces
            if (claseContenedora.BaseList == null)
                return false;

            // Heurística: si hay interfaces y el método no termina en Async,
            // podría ser una implementación de interfaz que no usa el sufijo
            var tieneInterfaces = claseContenedora.BaseList.Types.Any(tipo =>
            {
                var tipoString = tipo.ToString();
                return tipoString.StartsWith("I") && char.IsUpper(tipoString.Length > 1 ? tipoString[1] : ' ');
            });

            // Si implementa interfaces, permitir métodos async sin sufijo Async
            // (esto es una simplificación; idealmente se verificaría la interfaz específica)
            return tieneInterfaces;
        }

        private static bool EsEventHandler(MethodDeclarationSyntax metodo, string nombreMetodo)
        {
            // Verificar si el método sigue el patrón de event handler
            var parametros = metodo.ParameterList.Parameters;
            
            // Event handlers típicos tienen 2 parámetros: sender y EventArgs
            if (parametros.Count == 2)
            {
                var primerParametro = parametros[0].Type?.ToString();
                var segundoParametro = parametros[1].Type?.ToString();
                
                // Patrones comunes de event handlers
                if ((primerParametro?.Contains("object") == true || 
                     primerParametro?.Contains("sender") == true) &&
                    (segundoParametro?.Contains("EventArgs") == true ||
                     segundoParametro?.Contains("Args") == true))
                {
                    return true;
                }
            }

            // Verificar nombres típicos de event handlers
            var nombresEventHandler = new[]
            {
                "OnClick", "OnLoad", "OnChanged", "OnUpdated", "OnCreated",
                "OnDeleted", "OnError", "OnCompleted", "OnStarted", "OnStopped",
                "HandleClick", "HandleLoad", "HandleChanged", "HandleError"
            };

            return nombresEventHandler.Any(nombre => 
                nombreMetodo.StartsWith(nombre, System.StringComparison.OrdinalIgnoreCase));
        }

        private static string ObtenerNombreSugerido(string nombreMetodo)
        {
            // Si el método ya termina en "Async", no cambiar
            if (nombreMetodo.EndsWith("Async", System.StringComparison.Ordinal))
                return nombreMetodo;

            // Agregar "Async" al final
            return nombreMetodo + "Async";
        }
    }
}


