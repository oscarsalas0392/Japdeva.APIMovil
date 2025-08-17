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
    public class ValidarNomenclaturaPropiedades : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA010";

        private const string Titulo = "Propiedad debe usar PascalCase";
        private const string FormatoMensaje = "La propiedad '{0}' debe seguir la convención PascalCase (ejemplo: '{1}')";
        private const string Descripcion = "Las propiedades de las clases deben usar la convención PascalCase (primera letra mayúscula, palabras siguientes con primera letra mayúscula) para mantener la consistencia del código.";
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
            contexto.RegisterSyntaxNodeAction(AnalizarPropiedad, SyntaxKind.PropertyDeclaration);
        }

        private static void AnalizarPropiedad(SyntaxNodeAnalysisContext contexto)
        {
            var propiedad = (PropertyDeclarationSyntax)contexto.Node;
            var nombrePropiedad = propiedad.Identifier.ValueText;

            // Validar nomenclatura de la propiedad
            ValidarNomenclaturaPropiedad(contexto, propiedad, nombrePropiedad);
        }

        private static void ValidarNomenclaturaPropiedad(SyntaxNodeAnalysisContext contexto, 
            PropertyDeclarationSyntax propiedad, string nombrePropiedad)
        {
            if (!EsPropiedadEspecial(nombrePropiedad, propiedad) && 
                !EsNombrePascalCase(nombrePropiedad))
            {
                var nombreSugerido = ConvertirAPascalCase(nombrePropiedad);
                
                var diagnostico = Diagnostic.Create(
                    Regla,
                    propiedad.Identifier.GetLocation(),
                    nombrePropiedad,
                    nombreSugerido);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static bool EsPropiedadEspecial(string nombrePropiedad, PropertyDeclarationSyntax propiedad)
        {
            // Excluir propiedades que comienzan con underscore (convención especial)
            if (nombrePropiedad.StartsWith("_"))
                return true;

            // Excluir propiedades con atributos especiales que pueden requerir nombres específicos
            if (propiedad.AttributeLists.Any())
            {
                foreach (var listaAtributos in propiedad.AttributeLists)
                {
                    foreach (var atributo in listaAtributos.Attributes)
                    {
                        var nombreAtributoClase = atributo.Name.ToString();
                        
                        // Atributos que pueden requerir nombres específicos
                        var atributosEspeciales = new[]
                        {
                            "JsonPropertyName", "JsonProperty", "DataMember", 
                            "XmlElement", "XmlAttribute", "Column", "Key",
                            "Display", "DisplayName", "Required", "Range",
                            "BsonElement", "BsonId", "MessagePack", "ProtoBuf"
                        };

                        if (atributosEspeciales.Any(a => 
                            nombreAtributoClase.IndexOf(a, System.StringComparison.OrdinalIgnoreCase) != -1))
                        {
                            return true;
                        }
                    }
                }
            }

            // Excluir propiedades override (implementan interfaz o clase base)
            if (propiedad.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)))
                return true;

            // Excluir propiedades que implementan interfaces explícitamente
            if (propiedad.ExplicitInterfaceSpecifier != null)
                return true;

            // Verificar si implementa una interfaz implícitamente
            if (ImplementaInterfaz(propiedad, nombrePropiedad))
                return true;

            return false;
        }

        private static bool ImplementaInterfaz(PropertyDeclarationSyntax propiedad, string nombrePropiedad)
        {
            // Obtener la clase contenedora
            var claseContenedora = propiedad.Parent as TypeDeclarationSyntax;
            if (claseContenedora == null)
                return false;

            // Verificar si la clase implementa interfaces
            if (claseContenedora.BaseList == null)
                return false;

            // Por simplicidad, asumir que si hay interfaces en la clase,
            // la propiedad podría ser parte de una implementación
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

