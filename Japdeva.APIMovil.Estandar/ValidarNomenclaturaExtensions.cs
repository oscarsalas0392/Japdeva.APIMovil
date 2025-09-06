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
    public class ValidarNomenclaturaExtensions : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA020";

        private const string Titulo = "Clase en carpeta Extensions debe usar PascalCase y terminar en Extensions";
        private const string FormatoMensaje = "La clase '{0}' en la carpeta Extensions debe seguir la convención PascalCase y terminar en 'Extensions' (ejemplo: '{1}')";
        private const string Descripcion = "Las clases ubicadas en la carpeta Extensions deben usar la convención PascalCase y terminar con la palabra 'Extensions' para identificar claramente su propósito y mantener la consistencia del código.";
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/extension-methods");

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

            // Verificar si la clase está en la carpeta Extensions
            if (!EstaEnCarpetaExtensions(contexto))
                return;

            // Validar nomenclatura de la clase
            ValidarNomenclaturaClaseExtension(contexto, clase, nombreClase);
        }

        private static bool EstaEnCarpetaExtensions(SyntaxNodeAnalysisContext contexto)
        {
            var rutaArchivo = contexto.Node.SyntaxTree.FilePath;
            
            if (string.IsNullOrEmpty(rutaArchivo))
                return false;

            // Normalizar la ruta para comparación
            var rutaNormalizada = rutaArchivo.Replace('\\', '/');
            
            // Verificar si contiene "Extensions" en la ruta
            var partesRuta = rutaNormalizada.Split('/');
            
            return partesRuta.Any(parte => 
                parte.Equals("Extensions", System.StringComparison.OrdinalIgnoreCase) ||
                parte.Equals("Extension", System.StringComparison.OrdinalIgnoreCase));
        }

        private static void ValidarNomenclaturaClaseExtension(SyntaxNodeAnalysisContext contexto, 
            ClassDeclarationSyntax clase, string nombreClase)
        {
            if (!EsClaseExtensionEspecial(nombreClase, clase) && 
                (!EsNombrePascalCase(nombreClase) || !nombreClase.EndsWith("Extension", System.StringComparison.Ordinal)))
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

        private static bool EsClaseExtensionEspecial(string nombreClase, ClassDeclarationSyntax clase)
        {
            // Excluir clases que ya siguen la convención correcta
            if (EsNombrePascalCase(nombreClase) && nombreClase.EndsWith("Extension", System.StringComparison.Ordinal))
                return true;

            // Excluir clases abstractas (pueden ser clases base)
            if (clase.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
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
                            "DebuggerDisplay", "TestClass", "Fact", "Theory",
                            "CompilerGenerated", "DebuggerNonUserCode"
                        };

                        if (atributosEspeciales.Any(a => 
                            nombreAtributoClase.IndexOf(a, System.StringComparison.OrdinalIgnoreCase) != -1))
                        {
                            return true;
                        }
                    }
                }
            }

            // Excluir clases anidadas que pueden tener propósitos especiales
            if (EsClaseAnidada(clase))
                return true;

            // Excluir clases internas que pueden ser utilities
            if (EsClaseInterna(clase))
                return true;

            return false;
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
            var sufijosIncorrectos = new[] 
            { 
                "extensions", "Extensions", "extension", "Extension",
                "ext", "Ext", "helper", "Helper", "util", "Util",
                "utils", "Utils", "utilities", "Utilities"
            };
            
            foreach (var sufijo in sufijosIncorrectos)
            {
                if (nombreBase.EndsWith(sufijo, System.StringComparison.OrdinalIgnoreCase) && 
                    !nombreBase.EndsWith("Extensions", System.StringComparison.Ordinal))
                {
                    nombreBase = nombreBase.Substring(0, nombreBase.Length - sufijo.Length);
                    break;
                }
            }

            // Convertir a PascalCase
            var nombrePascal = ConvertirAPascalCase(nombreBase);

            // Agregar sufijo "Extensions" si no lo tiene
            if (!nombrePascal.EndsWith("Extension", System.StringComparison.Ordinal))
            {
                nombrePascal += "Extensions";
            }

            return nombrePascal;
        }

        private static string ConvertirAPascalCase(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return "CustomExtensions";

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

