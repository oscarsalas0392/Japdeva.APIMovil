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
    public class ValidarNomenclaturaClasesModels : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA024";

        private const string Titulo = "Clase en carpeta Models debe usar PascalCase y terminar en Model";
        private const string FormatoMensaje = "La clase '{0}' en la carpeta Models debe seguir la convenci�n PascalCase y terminar en 'Model' (ejemplo: '{1}')";
        private const string Descripcion = "Las clases ubicadas en la carpeta Models deben usar la convenci�n PascalCase y terminar con la palabra 'Model' para identificar claramente su prop�sito y mantener la consistencia del c�digo.";
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
            contexto.RegisterSyntaxNodeAction(AnalizarEnum, SyntaxKind.EnumDeclaration);
        }

        private static void AnalizarClase(SyntaxNodeAnalysisContext contexto)
        {
            var clase = (ClassDeclarationSyntax)contexto.Node;
            var nombreClase = clase.Identifier.ValueText;

            // Verificar si la clase est� en la carpeta Models
            if (!EstaEnCarpetaModels(contexto))
                return;

            // Validar nomenclatura de la clase
            ValidarNomenclaturaClaseModel(contexto, clase, nombreClase);
        }

        private static void AnalizarEnum(SyntaxNodeAnalysisContext contexto)
        {
            var enumDeclaration = (EnumDeclarationSyntax)contexto.Node;
            var nombreEnum = enumDeclaration.Identifier.ValueText;

            // Verificar si el enum está en la carpeta Models
            if (!EstaEnCarpetaModels(contexto))
                return;

            // Validar nomenclatura del enum
            ValidarNomenclaturaEnumModel(contexto, enumDeclaration, nombreEnum);
        }

        private static bool EstaEnCarpetaModels(SyntaxNodeAnalysisContext contexto)
        {
            var rutaArchivo = contexto.Node.SyntaxTree.FilePath;
            
            if (string.IsNullOrEmpty(rutaArchivo))
                return false;

            // Normalizar la ruta para comparaci�n
            var rutaNormalizada = rutaArchivo.Replace('\\', '/');
            
            // Verificar si contiene "Models" en la ruta
            var partesRuta = rutaNormalizada.Split('/');
            
            return partesRuta.Any(parte => 
                parte.Equals("Models", System.StringComparison.OrdinalIgnoreCase) ||
                parte.Equals("Model", System.StringComparison.OrdinalIgnoreCase) ||
                parte.Equals("DTOs", System.StringComparison.OrdinalIgnoreCase) ||
                parte.Equals("ViewModels", System.StringComparison.OrdinalIgnoreCase));
        }

        private static void ValidarNomenclaturaClaseModel(SyntaxNodeAnalysisContext contexto, 
            ClassDeclarationSyntax clase, string nombreClase)
        {
            if (!EsClaseModelEspecial(nombreClase, clase) && 
                (!EsNombrePascalCase(nombreClase) || !nombreClase.EndsWith("Model", System.StringComparison.Ordinal)))
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

        private static void ValidarNomenclaturaEnumModel(SyntaxNodeAnalysisContext contexto, 
            EnumDeclarationSyntax enumDeclaration, string nombreEnum)
        {
            if (!EsNombrePascalCase(nombreEnum) || !nombreEnum.EndsWith("Model", System.StringComparison.Ordinal))
            {
                var nombreSugerido = ObtenerNombreSugerido(nombreEnum);
                
                var diagnostico = Diagnostic.Create(
                    Regla,
                    enumDeclaration.Identifier.GetLocation(),
                    nombreEnum,
                    nombreSugerido);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static bool EsClaseModelEspecial(string nombreClase, ClassDeclarationSyntax clase)
        {
            // Excluir clases que ya siguen la convenci�n correcta
            if (EsNombrePascalCase(nombreClase) && nombreClase.EndsWith("Model", System.StringComparison.Ordinal))
                return true;

            // Excluir clases abstractas o interfaces de model base
            if (clase.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
            {
                // Permitir clases base que terminan en "ModelBase" o "BaseModel"
                if (nombreClase.EndsWith("ModelBase", System.StringComparison.Ordinal) ||
                    nombreClase.EndsWith("BaseModel", System.StringComparison.Ordinal) ||
                    nombreClase.EndsWith("Entity", System.StringComparison.Ordinal))
                    return true;
            }

            // Excluir clases est�ticas (model utilities)
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
                            "DebuggerDisplay", "TestClass", "Fact", "Theory",
                            "Entity", "Table", "JsonObject", "DataContract",
                            "Serializable", "XmlRoot", "ProtoContract"
                        };

                        if (atributosEspeciales.Any(a => 
                            nombreAtributoClase.IndexOf(a, System.StringComparison.OrdinalIgnoreCase) != -1))
                        {
                            // Para entidades con atributos espec�ficos, permitir nombres sin sufijo
                            if (nombreAtributoClase.IndexOf("Entity", System.StringComparison.OrdinalIgnoreCase) != -1 ||
                                nombreAtributoClase.IndexOf("Table", System.StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                return true;
                            }
                            
                            // Solo excluir si tambi�n tiene el sufijo correcto
                            return nombreClase.EndsWith("Model", System.StringComparison.Ordinal);
                        }
                    }
                }
            }

            // Excluir clases que implementan IModel o interfaces relacionadas
            if (ImplementaIModel(clase))
                return true;

            // Excluir clases internas o anidadas que pueden tener prop�sitos especiales
            if (EsClaseAnidada(clase) || EsClaseInterna(clase))
                return true;

            // Excluir clases que heredan de clases de model base conocidas
            if (HeredaDeClaseModelBase(clase))
                return true;

            // Excluir extensiones de model
            if (EsExtensionModel(clase, nombreClase))
                return true;

            // Excluir DTOs y ViewModels que pueden tener sufijos espec�ficos
            if (EsDTO(nombreClase) || EsViewModel(nombreClase))
                return true;

            return false;
        }

        private static bool ImplementaIModel(ClassDeclarationSyntax clase)
        {
            if (clase.BaseList == null)
                return false;

            // Verificar si implementa IModel o interfaces relacionadas
            return clase.BaseList.Types.Any(tipo =>
            {
                var tipoString = tipo.ToString();
                
                // Interfaces t�picas de models
                var interfacesModel = new[]
                {
                    "IModel", "IEntity", "IBaseModel", "IViewModel",
                    "IDto", "IDataTransferObject", "INotifyPropertyChanged"
                };

                return interfacesModel.Any(interfaz => 
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

        private static bool HeredaDeClaseModelBase(ClassDeclarationSyntax clase)
        {
            if (clase.BaseList == null)
                return false;

            // Verificar si hereda de clases base de models conocidas
            return clase.BaseList.Types.Any(tipo =>
            {
                var tipoString = tipo.ToString();
                
                var clasesBaseModel = new[]
                {
                    "Model", "ModelBase", "BaseModel", "Entity", "BaseEntity",
                    "ViewModel", "BaseViewModel", "DTO", "BaseDTO", "DataTransferObject"
                };

                return clasesBaseModel.Any(claseBase => 
                    tipoString.IndexOf(claseBase, System.StringComparison.OrdinalIgnoreCase) != -1);
            });
        }

        private static bool EsExtensionModel(ClassDeclarationSyntax clase, string nombreClase)
        {
            // Verificar si es una clase de extensiones para models
            if (clase.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
            {
                var nombresExtension = new[]
                {
                    "Extensions", "ModelExtensions", "EntityExtensions",
                    "DTOExtensions", "ViewModelExtensions"
                };

                return nombresExtension.Any(nombre => 
                    nombreClase.IndexOf(nombre, System.StringComparison.OrdinalIgnoreCase) != -1);
            }

            return false;
        }

        private static bool EsDTO(string nombreClase)
        {
            // Verificar si es un DTO que puede tener su propio sufijo
            return nombreClase.EndsWith("DTO", System.StringComparison.OrdinalIgnoreCase) ||
                   nombreClase.EndsWith("Dto", System.StringComparison.Ordinal);
        }

        private static bool EsViewModel(string nombreClase)
        {
            // Verificar si es un ViewModel que puede tener su propio sufijo
            return nombreClase.EndsWith("ViewModel", System.StringComparison.Ordinal) ||
                   nombreClase.EndsWith("VM", System.StringComparison.Ordinal);
        }

        private static bool EsNombrePascalCase(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return false;

            // Patr�n para PascalCase:
            // - Empieza con letra may�scula
            // - Puede contener letras, n�meros
            // - Las palabras siguientes empiezan con may�scula
            var patron = @"^[A-Z][a-zA-Z0-9]*$";
            
            return Regex.IsMatch(nombre, patron);
        }

        private static string ObtenerNombreSugerido(string nombreClase)
        {
            var nombreBase = nombreClase;

            // Remover sufijos incorrectos comunes (pero no DTO, ViewModel, etc.)
            var sufijosIncorrectos = new[] 
            { 
                "model", "entity", "dto", "pojo", "bean", "data", "obj", "object"
            };
            
            foreach (var sufijo in sufijosIncorrectos)
            {
                if (nombreBase.EndsWith(sufijo, System.StringComparison.OrdinalIgnoreCase) && 
                    !nombreBase.EndsWith("Model", System.StringComparison.Ordinal) &&
                    !nombreBase.EndsWith("DTO", System.StringComparison.OrdinalIgnoreCase) &&
                    !nombreBase.EndsWith("ViewModel", System.StringComparison.Ordinal))
                {
                    nombreBase = nombreBase.Substring(0, nombreBase.Length - sufijo.Length);
                    break;
                }
            }

            // Convertir a PascalCase
            var nombrePascal = ConvertirAPascalCase(nombreBase);

            // Agregar sufijo "Model" si no lo tiene (y no es DTO o ViewModel)
            if (!nombrePascal.EndsWith("Model", System.StringComparison.Ordinal) &&
                !nombrePascal.EndsWith("DTO", System.StringComparison.OrdinalIgnoreCase) &&
                !nombrePascal.EndsWith("ViewModel", System.StringComparison.Ordinal))
            {
                nombrePascal += "Model";
            }

            return nombrePascal;
        }

        private static string ConvertirAPascalCase(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return "CustomModel";

            // Si ya est� en formato correcto, no cambiar
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

