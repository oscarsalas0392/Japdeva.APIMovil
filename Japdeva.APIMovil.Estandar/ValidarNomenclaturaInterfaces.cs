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
    public class ValidarNomenclaturaInterfaces : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA015";

        private const string Titulo = "Interfaz debe usar IPascalCase";
        private const string FormatoMensaje = "La interfaz '{0}' debe empezar con 'I' seguido de PascalCase (ejemplo: '{1}')";
        private const string Descripcion = "Las interfaces deben usar la convención IPascalCase (empezar con 'I' mayúscula seguido de un sustantivo o frase sustantiva en PascalCase) para identificar claramente que son interfaces y mantener la consistencia del código.";
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarInterfaz, SyntaxKind.InterfaceDeclaration);
        }

        private static void AnalizarInterfaz(SyntaxNodeAnalysisContext contexto)
        {
            var interfaz = (InterfaceDeclarationSyntax)contexto.Node;
            var nombreInterfaz = interfaz.Identifier.ValueText;

            // Validar nomenclatura de la interfaz
            ValidarNomenclaturaInterfaz(contexto, interfaz, nombreInterfaz);
        }

        private static void ValidarNomenclaturaInterfaz(SyntaxNodeAnalysisContext contexto, 
            InterfaceDeclarationSyntax interfaz, string nombreInterfaz)
        {
            if (!EsInterfazEspecial(nombreInterfaz, interfaz) && 
                !EsNombreInterfazValido(nombreInterfaz))
            {
                var nombreSugerido = ObtenerNombreSugerido(nombreInterfaz);
                
                var diagnostico = Diagnostic.Create(
                    Regla,
                    interfaz.Identifier.GetLocation(),
                    nombreInterfaz,
                    nombreSugerido);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static bool EsInterfazEspecial(string nombreInterfaz, InterfaceDeclarationSyntax interfaz)
        {
            // Excluir interfaces que ya siguen la convención correcta
            if (EsNombreInterfazValido(nombreInterfaz))
                return true;

            // Excluir interfaces con atributos especiales
            if (interfaz.AttributeLists.Any())
            {
                foreach (var listaAtributos in interfaz.AttributeLists)
                {
                    foreach (var atributo in listaAtributos.Attributes)
                    {
                        var nombreAtributoClase = atributo.Name.ToString();
                        
                        // Atributos que pueden indicar interfaces especiales
                        var atributosEspeciales = new[]
                        {
                            "ComVisible", "ComImport", "Guid", "InterfaceType",
                            "Obsolete", "GeneratedCode", "EditorBrowsable",
                            "TypeLibType", "DispId", "ServiceContract",
                            "OperationContract", "DataContract"
                        };

                        if (atributosEspeciales.Any(a => 
                            nombreAtributoClase.IndexOf(a, System.StringComparison.OrdinalIgnoreCase) != -1))
                        {
                            // Para interfaces COM o generadas, permitir nombres especiales
                            return true;
                        }
                    }
                }
            }

            // Excluir interfaces anidadas que pueden tener propósitos especiales
            if (EsInterfazAnidada(interfaz))
                return true;

            // Excluir interfaces genéricas con restricciones especiales
            if (TieneRestriccionesGenericas(interfaz))
                return true;

            return false;
        }

        private static bool EsInterfazAnidada(InterfaceDeclarationSyntax interfaz)
        {
            return interfaz.Parent is ClassDeclarationSyntax || 
                   interfaz.Parent is StructDeclarationSyntax ||
                   interfaz.Parent is RecordDeclarationSyntax ||
                   interfaz.Parent is InterfaceDeclarationSyntax;
        }

        private static bool TieneRestriccionesGenericas(InterfaceDeclarationSyntax interfaz)
        {
            if (interfaz.TypeParameterList == null)
                return false;

            // Verificar si tiene restricciones de tipo
            if (interfaz.ConstraintClauses.Any())
            {
                foreach (var restriccion in interfaz.ConstraintClauses)
                {
                    foreach (var constraint in restriccion.Constraints)
                    {
                        // Si tiene restricciones específicas, puede ser una interfaz especial
                        if (constraint.ToString().Contains("class") ||
                            constraint.ToString().Contains("struct") ||
                            constraint.ToString().Contains("new()"))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool EsNombreInterfazValido(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return false;

            // Patrón para IPascalCase:
            // - Empieza con exactamente "I"
            // - Seguido de letra mayúscula
            // - Puede contener letras, números
            // - Las palabras siguientes empiezan con mayúscula
            var patron = @"^I[A-Z][a-zA-Z0-9]*$";
            
            if (!Regex.IsMatch(nombre, patron))
                return false;

            // Verificar que la parte después de "I" sea PascalCase válido
            var partePostI = nombre.Substring(1);
            return EsNombrePascalCase(partePostI);
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

        private static string ObtenerNombreSugerido(string nombreInterfaz)
        {
            var nombreBase = nombreInterfaz;

            // Si ya empieza con "I", trabajar con la parte restante
            if (nombreBase.StartsWith("I", System.StringComparison.Ordinal) && nombreBase.Length > 1)
            {
                // Verificar si ya es válido
                if (EsNombreInterfazValido(nombreBase))
                    return nombreBase;

                // Trabajar con la parte después de "I"
                nombreBase = nombreBase.Substring(1);
            }
            else if (nombreBase.StartsWith("i", System.StringComparison.Ordinal) && nombreBase.Length > 1)
            {
                // Convertir "i" minúscula a "I" mayúscula
                nombreBase = nombreBase.Substring(1);
            }

            // Convertir la parte base a PascalCase
            var nombrePascal = ConvertirAPascalCase(nombreBase);

            // Si está vacío o es muy corto, usar un nombre por defecto
            if (string.IsNullOrEmpty(nombrePascal) || nombrePascal.Length < 2)
            {
                return "IInterface";
            }

            // Agregar prefijo "I"
            return "I" + nombrePascal;
        }

        private static string ConvertirAPascalCase(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return "Interface";

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

            // Limpiar caracteres especiales y números al inicio
            resultado = Regex.Replace(resultado, @"^[^a-zA-Z]+", "");
            
            // Si queda vacío, usar nombre por defecto
            if (string.IsNullOrEmpty(resultado))
                return "Interface";

            // Asegurar que empiece con mayúscula
            if (char.IsLower(resultado[0]))
            {
                resultado = char.ToUpperInvariant(resultado[0]) + resultado.Substring(1);
            }

            return resultado;
        }
    }
}
