using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarImplementacionInterfazServices : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA016";

        private const string Titulo = "Clase Service debe implementar una interfaz";
        private const string FormatoMensaje = "La clase Service '{0}' debe implementar una interfaz (ejemplo: '{1}')";
        private const string Descripcion = "Las clases Service deben implementar una interfaz para seguir el principio de inversión de dependencias, facilitar las pruebas unitarias y mejorar la mantenibilidad del código.";
        private const string Categoria = "Design";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/standard/modern-web-apps-azure-architecture/architectural-principles#dependency-inversion");

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

            // Verificar si es una clase Service
            if (!EsClaseService(nombreClase, clase))
                return;

            // Validar que implemente una interfaz
            ValidarImplementacionInterfaz(contexto, clase, nombreClase);
        }

        private static bool EsClaseService(string nombreClase, ClassDeclarationSyntax clase)
        {
            // Verificar si el nombre termina en "Service"
            if (!nombreClase.EndsWith("Service", System.StringComparison.Ordinal))
                return false;

            // Verificar si está en la carpeta Services o tiene características de servicio
            return EstaEnCarpetaServices(clase) || TieneCaracteristicasDeServicio(clase);
        }

        private static bool EstaEnCarpetaServices(ClassDeclarationSyntax clase)
        {
            var rutaArchivo = clase.SyntaxTree.FilePath;
            
            if (string.IsNullOrEmpty(rutaArchivo))
                return false;

            // Normalizar la ruta para comparación
            var rutaNormalizada = rutaArchivo.Replace('\\', '/');
            
            // Verificar si contiene "Services" en la ruta
            var partesRuta = rutaNormalizada.Split('/');
            
            return partesRuta.Any(parte => 
                parte.Equals("Services", System.StringComparison.OrdinalIgnoreCase));
        }

        private static bool TieneCaracteristicasDeServicio(ClassDeclarationSyntax clase)
        {
            // Verificar atributos relacionados con servicios
            if (clase.AttributeLists.Any())
            {
                foreach (var listaAtributos in clase.AttributeLists)
                {
                    foreach (var atributo in listaAtributos.Attributes)
                    {
                        var nombreAtributo = atributo.Name.ToString();
                        
                        var atributosServicio = new[]
                        {
                            "Service", "Component", "Injectable", "Singleton",
                            "Transient", "Scoped", "Repository"
                        };

                        if (atributosServicio.Any(a => 
                            nombreAtributo.IndexOf(a, System.StringComparison.OrdinalIgnoreCase) != -1))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static void ValidarImplementacionInterfaz(SyntaxNodeAnalysisContext contexto, 
            ClassDeclarationSyntax clase, string nombreClase)
        {
            if (!EsClaseServiceEspecial(nombreClase, clase) && 
                !ImplementaInterfaz(clase))
            {
                var nombreInterfazSugerida = ObtenerNombreInterfazSugerida(nombreClase);
                
                var diagnostico = Diagnostic.Create(
                    Regla,
                    clase.Identifier.GetLocation(),
                    nombreClase,
                    nombreInterfazSugerida);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static bool EsClaseServiceEspecial(string nombreClase, ClassDeclarationSyntax clase)
        {
            // Excluir clases que ya implementan interfaces
            if (ImplementaInterfaz(clase))
                return true;

            // Excluir clases abstractas (pueden ser clases base)
            if (clase.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
                return true;

            // Excluir clases estáticas
            if (clase.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
                return true;

            // Excluir clases base conocidas
            var clasesBaseExcluidas = new[]
            {
                "ServiceBase", "BaseService", "AbstractService",
                "ServiceFoundation", "ServiceConfiguration"
            };

            if (clasesBaseExcluidas.Any(claseBase => 
                nombreClase.Equals(claseBase, System.StringComparison.OrdinalIgnoreCase)))
                return true;

            // Excluir clases con atributos especiales que no requieren interfaz
            if (clase.AttributeLists.Any())
            {
                foreach (var listaAtributos in clase.AttributeLists)
                {
                    foreach (var atributo in listaAtributos.Attributes)
                    {
                        var nombreAtributo = atributo.Name.ToString();
                        
                        var atributosEspeciales = new[]
                        {
                            "Obsolete", "GeneratedCode", "TestClass",
                            "Controller", "ApiController"
                        };

                        if (atributosEspeciales.Any(a => 
                            nombreAtributo.IndexOf(a, System.StringComparison.OrdinalIgnoreCase) != -1))
                        {
                            return true;
                        }
                    }
                }
            }

            // Excluir clases anidadas
            if (EsClaseAnidada(clase))
                return true;

            // Excluir clases que heredan de otras clases Service (pueden ser especializaciones)
            if (HeredaDeClaseService(clase))
                return true;

            return false;
        }

        private static bool ImplementaInterfaz(ClassDeclarationSyntax clase)
        {
            if (clase.BaseList == null)
                return false;

            // Verificar si implementa alguna interfaz
            return clase.BaseList.Types.Any(tipo =>
            {
                var tipoString = tipo.ToString();
                
                // Las interfaces típicamente empiezan con "I" seguido de mayúscula
                return tipoString.StartsWith("I") && 
                       tipoString.Length > 1 && 
                       char.IsUpper(tipoString[1]);
            });
        }

        private static bool EsClaseAnidada(ClassDeclarationSyntax clase)
        {
            return clase.Parent is ClassDeclarationSyntax || 
                   clase.Parent is StructDeclarationSyntax ||
                   clase.Parent is RecordDeclarationSyntax;
        }

        private static bool HeredaDeClaseService(ClassDeclarationSyntax clase)
        {
            if (clase.BaseList == null)
                return false;

            // Verificar si hereda de otra clase (no interfaz)
            return clase.BaseList.Types.Any(tipo =>
            {
                var tipoString = tipo.ToString();
                
                // Clases base conocidas de servicios
                var clasesBaseService = new[]
                {
                    "ServiceBase", "BaseService", "AbstractService",
                    "ApplicationService", "DomainService", "BusinessService"
                };

                return clasesBaseService.Any(claseBase => 
                    tipoString.IndexOf(claseBase, System.StringComparison.OrdinalIgnoreCase) != -1) ||
                    // O cualquier clase que termine en "Service" (heurística)
                    (tipoString.EndsWith("Service", System.StringComparison.OrdinalIgnoreCase) &&
                     !tipoString.StartsWith("I"));
            });
        }

        private static string ObtenerNombreInterfazSugerida(string nombreClase)
        {
            // Si la clase termina en "Service", crear interfaz con el mismo nombre
            if (nombreClase.EndsWith("Service", System.StringComparison.Ordinal))
            {
                return "I" + nombreClase;
            }

            // Si no termina en Service, agregar "Service" e interfaz
            return "I" + nombreClase + "Service";
        }
    }
}

