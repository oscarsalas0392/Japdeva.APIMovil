using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarNomenclaturaClasesServices : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA014";

        private const string Titulo = "Clase en carpeta Services debe usar PascalCase y terminar en Service";
        private const string FormatoMensaje = "La clase '{0}' en la carpeta Services debe seguir la convención PascalCase y terminar en 'Service' (ejemplo: '{1}')";
        private const string Descripcion = "Las clases ubicadas en la carpeta Services deben usar la convención PascalCase y terminar con la palabra 'Service' para identificar claramente su propósito y mantener la consistencia del código.";
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
            contexto.RegisterSyntaxNodeAction(AnalizarClase, SyntaxKind.ClassDeclaration);
        }

        private static void AnalizarClase(SyntaxNodeAnalysisContext contexto)
        {
            var clase = (ClassDeclarationSyntax)contexto.Node;
            var nombreClase = clase.Identifier.ValueText;

            // Verificar si la clase está en la carpeta Services
            if (!EstaEnCarpetaServices(contexto))
                return;

            // Validar nomenclatura de la clase
            ValidarNomenclaturaClaseService(contexto, clase, nombreClase);
        }

        private static bool EstaEnCarpetaServices(SyntaxNodeAnalysisContext contexto)
        {
            var rutaArchivo = contexto.Node.SyntaxTree.FilePath;
            
            if (string.IsNullOrEmpty(rutaArchivo))
                return false;

            // Normalizar la ruta para comparación
            var rutaNormalizada = rutaArchivo.Replace('\\', '/');
            
            // Verificar si contiene "Services" en la ruta
            var partesRuta = rutaNormalizada.Split('/');
            
            return partesRuta.Any(parte => 
                parte.Equals("Services", System.StringComparison.OrdinalIgnoreCase));
        }

        private static void ValidarNomenclaturaClaseService(SyntaxNodeAnalysisContext contexto, 
            ClassDeclarationSyntax clase, string nombreClase)
        {
            if (!EsClaseServiceEspecial(nombreClase, clase) && 
                (!EsNombrePascalCase(nombreClase) || !nombreClase.EndsWith("Service", System.StringComparison.Ordinal)))
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

        private static bool EsClaseServiceEspecial(string nombreClase, ClassDeclarationSyntax clase)
        {
            // Excluir clases que ya siguen la convención correcta
            if (EsNombrePascalCase(nombreClase) && nombreClase.EndsWith("Service", System.StringComparison.Ordinal))
                return true;

            // Excluir clases abstractas o interfaces de servicios base
            if (clase.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
            {
                // Permitir clases base que terminan en "ServiceBase" o "BaseService"
                if (nombreClase.EndsWith("ServiceBase", System.StringComparison.Ordinal) ||
                    nombreClase.EndsWith("BaseService", System.StringComparison.Ordinal))
                    return true;
            }

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
                            "Service", "Component", "Injectable", "Singleton",
                            "Transient", "Scoped", "Repository", "Controller",
                            "ApiController", "Obsolete", "GeneratedCode"
                        };

                        if (atributosEspeciales.Any(a => 
                            nombreAtributoClase.IndexOf(a, System.StringComparison.OrdinalIgnoreCase) != -1))
                        {
                            // Solo excluir si también tiene el sufijo correcto
                            return nombreClase.EndsWith("Service", System.StringComparison.Ordinal);
                        }
                    }
                }
            }

            // Excluir clases que implementan interfaces específicas de servicios
            if (ImplementaInterfazService(clase))
                return true;

            // Excluir clases internas o anidadas que pueden tener propósitos especiales
            if (EsClaseAnidada(clase) || EsClaseInterna(clase))
                return true;

            // Excluir clases que heredan de clases de servicio base conocidas
            if (HeredaDeClaseServiceBase(clase))
                return true;

            return false;
        }

        private static bool ImplementaInterfazService(ClassDeclarationSyntax clase)
        {
            if (clase.BaseList == null)
                return false;

            // Verificar si implementa interfaces relacionadas con servicios
            return clase.BaseList.Types.Any(tipo =>
            {
                var tipoString = tipo.ToString();
                
                // Interfaces típicas de servicios
                var interfacesService = new[]
                {
                    "IService", "IBaseService", "IApplicationService",
                    "IDomainService", "IBusinessService", "IDataService"
                };

                return interfacesService.Any(interfaz => 
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

        private static bool HeredaDeClaseServiceBase(ClassDeclarationSyntax clase)
        {
            if (clase.BaseList == null)
                return false;

            // Verificar si hereda de clases base de servicios conocidas
            return clase.BaseList.Types.Any(tipo =>
            {
                var tipoString = tipo.ToString();
                
                var clasesBaseService = new[]
                {
                    "ServiceBase", "BaseService", "AbstractService",
                    "ApplicationService", "DomainService", "BusinessService"
                };

                return clasesBaseService.Any(claseBase => 
                    tipoString.IndexOf(claseBase, System.StringComparison.OrdinalIgnoreCase) != -1);
            });
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
            var sufijosIncorrectos = new[] { "service", "Service", "svc", "Svc" };
            foreach (var sufijo in sufijosIncorrectos)
            {
                if (nombreBase.EndsWith(sufijo, System.StringComparison.OrdinalIgnoreCase) && 
                    !nombreBase.EndsWith("Service", System.StringComparison.Ordinal))
                {
                    nombreBase = nombreBase.Substring(0, nombreBase.Length - sufijo.Length);
                    break;
                }
            }

            // Convertir a PascalCase
            var nombrePascal = ConvertirAPascalCase(nombreBase);

            // Agregar sufijo "Service" si no lo tiene
            if (!nombrePascal.EndsWith("Service", System.StringComparison.Ordinal))
            {
                nombrePascal += "Service";
            }

            return nombrePascal;
        }

        private static string ConvertirAPascalCase(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return "DefaultService";

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



