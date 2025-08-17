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
    public class ValidarNomenclaturaClasesMiddlewares : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA018";

        private const string Titulo = "Clase en carpeta Middlewares debe usar PascalCase y terminar en Middleware";
        private const string FormatoMensaje = "La clase '{0}' en la carpeta Middlewares debe seguir la convención PascalCase y terminar en 'Middleware' (ejemplo: '{1}')";
        private const string Descripcion = "Las clases ubicadas en la carpeta Middlewares deben usar la convención PascalCase y terminar con la palabra 'Middleware' para identificar claramente su propósito y mantener la consistencia del código.";
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/aspnet/core/fundamentals/middleware/write");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarClase, SyntaxKind.ClassDeclaration);
        }

        private static void AnalizarClase(SyntaxNodeAnalysisContext contexto)
        {
            var clase = (ClassDeclarationSyntax)contexto.Node;
            var nombreClase = clase.Identifier.ValueText;

            // Verificar si la clase está en la carpeta Middlewares
            if (!EstaEnCarpetaMiddlewares(contexto))
                return;

            // Validar nomenclatura de la clase
            ValidarNomenclaturaClaseMiddleware(contexto, clase, nombreClase);
        }

        private static bool EstaEnCarpetaMiddlewares(SyntaxNodeAnalysisContext contexto)
        {
            var rutaArchivo = contexto.Node.SyntaxTree.FilePath;
            
            if (string.IsNullOrEmpty(rutaArchivo))
                return false;

            // Normalizar la ruta para comparación
            var rutaNormalizada = rutaArchivo.Replace('\\', '/');
            
            // Verificar si contiene "Middlewares" en la ruta
            var partesRuta = rutaNormalizada.Split('/');
            
            return partesRuta.Any(parte => 
                parte.Equals("Middlewares", System.StringComparison.OrdinalIgnoreCase) ||
                parte.Equals("Middleware", System.StringComparison.OrdinalIgnoreCase));
        }

        private static void ValidarNomenclaturaClaseMiddleware(SyntaxNodeAnalysisContext contexto, 
            ClassDeclarationSyntax clase, string nombreClase)
        {
            if (!EsClaseMiddlewareEspecial(nombreClase, clase) && 
                (!EsNombrePascalCase(nombreClase) || !nombreClase.EndsWith("Middleware", System.StringComparison.Ordinal)))
            {
                var nombreSugerido = ObtenerNombreSugerido(nombreClase);
                
                var diagnostico = Diagnostic.Create(
                    Regla,
                    clase.Identifier.GetLocation(),
                    nombreClase,
                    nombreSugerido);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static bool EsClaseMiddlewareEspecial(string nombreClase, ClassDeclarationSyntax clase)
        {
            // Excluir clases que ya siguen la convención correcta
            if (EsNombrePascalCase(nombreClase) && nombreClase.EndsWith("Middleware", System.StringComparison.Ordinal))
                return true;

            // Excluir clases abstractas o interfaces de middleware base
            if (clase.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
            {
                // Permitir clases base que terminan en "MiddlewareBase" o "BaseMiddleware"
                if (nombreClase.EndsWith("MiddlewareBase", System.StringComparison.Ordinal) ||
                    nombreClase.EndsWith("BaseMiddleware", System.StringComparison.Ordinal))
                    return true;
            }

            // Excluir clases estáticas (middleware utilities)
            if (clase.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
                return true;

            // Excluir clases con atributos especiales
            if (clase.AttributeLists.Any())
            {
                foreach (var listaAtributos in clase.AttributeLists)
                {
                    foreach (var atributo in listaAtributos.Attributes)
                    {
                        var nombreAtributoClase = atributo.Name.ToString();
                        
                        // Atributos que pueden indicar clases especiales
                        var atributosEspeciales = new[]
                        {
                            "Obsolete", "GeneratedCode", "EditorBrowsable",
                            "DebuggerDisplay", "TestClass", "Fact", "Theory"
                        };

                        if (atributosEspeciales.Any(a => 
                            nombreAtributoClase.IndexOf(a, System.StringComparison.OrdinalIgnoreCase) != -1))
                        {
                            return true;
                        }
                    }
                }
            }

            // Excluir clases que implementan IMiddleware específicamente
            if (ImplementaIMiddleware(clase))
                return true;

            // Excluir clases internas o anidadas que pueden tener propósitos especiales
            if (EsClaseAnidada(clase) || EsClaseInterna(clase))
                return true;

            // Excluir clases que heredan de clases de middleware base conocidas
            if (HeredaDeClaseMiddlewareBase(clase))
                return true;

            // Excluir extensiones de middleware
            if (EsExtensionMiddleware(clase, nombreClase))
                return true;

            return false;
        }

        private static bool ImplementaIMiddleware(ClassDeclarationSyntax clase)
        {
            if (clase.BaseList == null)
                return false;

            // Verificar si implementa IMiddleware o interfaces relacionadas
            return clase.BaseList.Types.Any(tipo =>
            {
                var tipoString = tipo.ToString();
                
                // Interfaces típicas de middleware
                var interfacesMiddleware = new[]
                {
                    "IMiddleware", "IApplicationMiddleware", "IRequestMiddleware"
                };

                return interfacesMiddleware.Any(interfaz => 
                    tipoString.IndexOf(interfaz, System.StringComparison.OrdinalIgnoreCase) != -1);
            });
        }

        private static bool EsClaseAnidada(ClassDeclarationSyntax clase)
        {
            return clase.Parent is ClassDeclarationSyntax || 
                   clase.Parent is StructDeclarationSyntax ||
                   clase.Parent is RecordDeclarationSyntax;
        }

        private static bool EsClaseInterna(ClassDeclarationSyntax clase)
        {
            return clase.Modifiers.Any(m => m.IsKind(SyntaxKind.InternalKeyword)) ||
                   clase.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword));
        }

        private static bool HeredaDeClaseMiddlewareBase(ClassDeclarationSyntax clase)
        {
            if (clase.BaseList == null)
                return false;

            // Verificar si hereda de clases base de middleware conocidas
            return clase.BaseList.Types.Any(tipo =>
            {
                var tipoString = tipo.ToString();
                
                var clasesBaseMiddleware = new[]
                {
                    "MiddlewareBase", "BaseMiddleware", "AbstractMiddleware",
                    "ApplicationMiddleware", "RequestMiddleware", "ResponseMiddleware"
                };

                return clasesBaseMiddleware.Any(claseBase => 
                    tipoString.IndexOf(claseBase, System.StringComparison.OrdinalIgnoreCase) != -1);
            });
        }

        private static bool EsExtensionMiddleware(ClassDeclarationSyntax clase, string nombreClase)
        {
            // Verificar si es una clase de extensiones para middleware
            if (clase.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
            {
                var nombresExtension = new[]
                {
                    "Extensions", "MiddlewareExtensions", "ApplicationBuilderExtensions",
                    "ServiceCollectionExtensions"
                };

                return nombresExtension.Any(nombre => 
                    nombreClase.IndexOf(nombre, System.StringComparison.OrdinalIgnoreCase) != -1);
            }

            return false;
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
            
            return Regex.IsMatch(nombre, patron);
        }

        private static string ObtenerNombreSugerido(string nombreClase)
        {
            var nombreBase = nombreClase;

            // Remover sufijos incorrectos comunes
            var sufijosIncorrectos = new[] { "middleware", "Middleware", "MW", "mw" };
            foreach (var sufijo in sufijosIncorrectos)
            {
                if (nombreBase.EndsWith(sufijo, System.StringComparison.OrdinalIgnoreCase) && 
                    !nombreBase.EndsWith("Middleware", System.StringComparison.Ordinal))
                {
                    nombreBase = nombreBase.Substring(0, nombreBase.Length - sufijo.Length);
                    break;
                }
            }

            // Convertir a PascalCase
            var nombrePascal = ConvertirAPascalCase(nombreBase);

            // Agregar sufijo "Middleware" si no lo tiene
            if (!nombrePascal.EndsWith("Middleware", System.StringComparison.Ordinal))
            {
                nombrePascal += "Middleware";
            }

            return nombrePascal;
        }

        private static string ConvertirAPascalCase(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return "CustomMiddleware";

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

