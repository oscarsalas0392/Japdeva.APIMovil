using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarNomenclaturaMetodos : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA012";

        private const string Titulo = "Método debe usar PascalCase";
        private const string FormatoMensaje = "El método '{0}' debe seguir la convención PascalCase (ejemplo: '{1}')";
        private const string Descripcion = "Los nombres de métodos deben usar la convención PascalCase (primera letra mayúscula, palabras siguientes con primera letra mayúscula) para mantener la consistencia del código.";
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/standard/design-guidelines/capitalization-conventions");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarMetodo, SyntaxKind.MethodDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarConstructor, SyntaxKind.ConstructorDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarDestructor, SyntaxKind.DestructorDeclaration);
        }

        private static void AnalizarMetodo(SyntaxNodeAnalysisContext contexto)
        {
            var metodo = (MethodDeclarationSyntax)contexto.Node;
            var nombreMetodo = metodo.Identifier.ValueText;

            // Validar nomenclatura del método
            ValidarNomenclaturaMetodo(contexto, metodo, nombreMetodo);
        }

        private static void AnalizarConstructor(SyntaxNodeAnalysisContext contexto)
        {
            var constructor = (ConstructorDeclarationSyntax)contexto.Node;
            var nombreConstructor = constructor.Identifier.ValueText;

            // Los constructores normalmente siguen el nombre de la clase (PascalCase)
            // Solo validar si no es un constructor especial
            if (!EsConstructorEspecial(constructor))
            {
                ValidarNomenclaturaConstructor(contexto, constructor, nombreConstructor);
            }
        }

        private static void AnalizarDestructor(SyntaxNodeAnalysisContext contexto)
        {
            // Los destructores siguen el patrón ~ClassName, no los validamos
            return;
        }

        private static void ValidarNomenclaturaMetodo(SyntaxNodeAnalysisContext contexto, 
            MethodDeclarationSyntax metodo, string nombreMetodo)
        {
            if (!EsMetodoEspecial(nombreMetodo, metodo) && 
                !EsNombrePascalCase(nombreMetodo))
            {
                var nombreSugerido = ConvertirAPascalCase(nombreMetodo);
                
                var diagnostico = Diagnostic.Create(
                    Regla,
                    metodo.Identifier.GetLocation(),
                    nombreMetodo,
                    nombreSugerido);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static void ValidarNomenclaturaConstructor(SyntaxNodeAnalysisContext contexto, 
            ConstructorDeclarationSyntax constructor, string nombreConstructor)
        {
            if (!EsNombrePascalCase(nombreConstructor))
            {
                var nombreSugerido = ConvertirAPascalCase(nombreConstructor);
                
                var diagnostico = Diagnostic.Create(
                    Regla,
                    constructor.Identifier.GetLocation(),
                    nombreConstructor,
                    nombreSugerido);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static bool EsMetodoEspecial(string nombreMetodo, MethodDeclarationSyntax metodo)
        {
            // Excluir métodos que comienzan con underscore (convención especial)
            if (nombreMetodo.StartsWith("_"))
                return true;

            // Excluir métodos Main (punto de entrada de aplicación)
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
                        
                        // Atributos que pueden requerir nombres específicos
                        var atributosEspeciales = new[]
                        {
                            "Test", "TestMethod", "Fact", "Theory", "TestCase",
                            "HttpGet", "HttpPost", "HttpPut", "HttpDelete", "HttpPatch",
                            "Route", "ActionName", "Obsolete", "OnActionExecuting",
                            "OnActionExecuted", "Authorize", "AllowAnonymous",
                            "DllImport", "MethodImpl", "ComVisible", "Conditional"
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

            // Excluir métodos async que pueden tener sufijo "Async"
            if (metodo.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)) && 
                nombreMetodo.EndsWith("Async", System.StringComparison.OrdinalIgnoreCase))
            {
                // Validar la parte sin "Async"
                var nombreSinAsync = nombreMetodo.Substring(0, nombreMetodo.Length - 5);
                return EsNombrePascalCase(nombreSinAsync);
            }

            // Verificar si implementa una interfaz implícitamente
            if (ImplementaInterfaz(metodo, nombreMetodo))
                return true;

            return false;
        }

        private static bool EsConstructorEspecial(ConstructorDeclarationSyntax constructor)
        {
            // Los constructores estáticos tienen nombre especial
            if (constructor.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
                return true;

            return false;
        }

        private static bool ImplementaInterfaz(MethodDeclarationSyntax metodo, string nombreMetodo)
        {
            // Obtener la clase contenedora
            var claseContenedora = metodo.Parent as TypeDeclarationSyntax;
            if (claseContenedora == null)
                return false;

            // Verificar si la clase implementa interfaces
            if (claseContenedora.BaseList == null)
                return false;

            // Por simplicidad, asumir que si hay interfaces en la clase,
            // el método podría ser parte de una implementación
            var tieneInterfaces = claseContenedora.BaseList.Types.Any(tipo =>
            {
                var tipoString = tipo.ToString();
                // Heurística simple: las interfaces suelen empezar con "I"
                return tipoString.StartsWith("I") && char.IsUpper(tipoString.Length > 1 ? tipoString[1] : ' ');
            });

            return tieneInterfaces;
        }

        private static bool EsNombrePascalCase(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return false;

            // Patrón para PascalCase:
            // - Empieza con letra mayúscula
            // - Puede contener letras, números
            // - Las palabras siguientes empiezan con mayúscula
            var patron = @"^[A-Z][a-zA-Z0-9]*$";
            
            if (!Regex.IsMatch(nombre, patron))
                return false;

            return true;
        }

        private static string ConvertirAPascalCase(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return nombre;

            // Si ya está en formato correcto, no cambiar
            if (EsNombrePascalCase(nombre))
                return nombre;

            var resultado = nombre;

            // Convertir camelCase a PascalCase
            if (char.IsLower(resultado[0]))
            {
                resultado = char.ToUpperInvariant(resultado[0]) + resultado.Substring(1);
            }

            // Convertir snake_case a PascalCase
            if (resultado.Contains("_"))
            {
                var partes = resultado.Split('_');
                resultado = "";
                
                foreach (var parte in partes)
                {
                    if (!string.IsNullOrEmpty(parte))
                    {
                        resultado += char.ToUpperInvariant(parte[0]) + 
                                   parte.Substring(1).ToLowerInvariant();
                    }
                }
            }

            // Convertir SCREAMING_SNAKE_CASE a PascalCase
            if (resultado.ToUpperInvariant() == resultado && resultado.Contains("_"))
            {
                var partes = resultado.Split('_');
                resultado = "";
                
                foreach (var parte in partes)
                {
                    if (!string.IsNullOrEmpty(parte))
                    {
                        var parteLower = parte.ToLowerInvariant();
                        resultado += char.ToUpperInvariant(parteLower[0]) + parteLower.Substring(1);
                    }
                }
            }

            // Convertir kebab-case a PascalCase
            if (resultado.Contains("-"))
            {
                var partes = resultado.Split('-');
                resultado = "";
                
                foreach (var parte in partes)
                {
                    if (!string.IsNullOrEmpty(parte))
                    {
                        resultado += char.ToUpperInvariant(parte[0]) + 
                                   parte.Substring(1).ToLowerInvariant();
                    }
                }
            }

            return resultado;
        }
    }
}


