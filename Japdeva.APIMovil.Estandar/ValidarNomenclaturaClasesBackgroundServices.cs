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
    public class ValidarNomenclaturaClasesBackgroundServices : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA019";

        private const string Titulo = "Clase en carpeta BackgroundServices debe usar PascalCase y terminar en BackgroundService";
        private const string FormatoMensaje = "La clase '{0}' en la carpeta BackgroundServices debe seguir la convención PascalCase y terminar en 'BackgroundService' (ejemplo: '{1}')";
        private const string Descripcion = "Las clases ubicadas en la carpeta BackgroundServices deben usar la convención PascalCase y terminar con la palabra 'BackgroundService' para identificar claramente su propósito y mantener la consistencia del código.";
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/aspnet/core/fundamentals/host/hosted-services");

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

            // Verificar si la clase está en la carpeta BackgroundServices
            if (!EstaEnCarpetaBackgroundServices(contexto))
                return;

            // Validar nomenclatura de la clase
            ValidarNomenclaturaClaseBackgroundService(contexto, clase, nombreClase);
        }

        private static bool EstaEnCarpetaBackgroundServices(SyntaxNodeAnalysisContext contexto)
        {
            var rutaArchivo = contexto.Node.SyntaxTree.FilePath;
            
            if (string.IsNullOrEmpty(rutaArchivo))
                return false;

            // Normalizar la ruta para comparación
            var rutaNormalizada = rutaArchivo.Replace('\\', '/');
            
            // Verificar si contiene "BackgroundServices" en la ruta
            var partesRuta = rutaNormalizada.Split('/');
            
            return partesRuta.Any(parte => 
                parte.Equals("BackgroundServices", System.StringComparison.OrdinalIgnoreCase) ||
                parte.Equals("BackgroundService", System.StringComparison.OrdinalIgnoreCase) ||
                parte.Equals("HostedServices", System.StringComparison.OrdinalIgnoreCase));
        }

        private static void ValidarNomenclaturaClaseBackgroundService(SyntaxNodeAnalysisContext contexto, 
            ClassDeclarationSyntax clase, string nombreClase)
        {
            if (!EsClaseBackgroundServiceEspecial(nombreClase, clase) && 
                (!EsNombrePascalCase(nombreClase) || !nombreClase.EndsWith("BackgroundService", System.StringComparison.Ordinal)))
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

        private static bool EsClaseBackgroundServiceEspecial(string nombreClase, ClassDeclarationSyntax clase)
        {
            // Excluir clases que ya siguen la convención correcta
            if (EsNombrePascalCase(nombreClase) && nombreClase.EndsWith("BackgroundService", System.StringComparison.Ordinal))
                return true;

            // Excluir clases abstractas o interfaces de background service base
            if (clase.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
            {
                // Permitir clases base que terminan en "BackgroundServiceBase" o "BaseBackgroundService"
                if (nombreClase.EndsWith("BackgroundServiceBase", System.StringComparison.Ordinal) ||
                    nombreClase.EndsWith("BaseBackgroundService", System.StringComparison.Ordinal) ||
                    nombreClase.EndsWith("HostedServiceBase", System.StringComparison.Ordinal))
                    return true;
            }

            // Excluir clases estáticas (utilities)
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
                            "HostedService", "Singleton", "Scoped", "Transient"
                        };

                        if (atributosEspeciales.Any(a => 
                            nombreAtributoClase.IndexOf(a, System.StringComparison.OrdinalIgnoreCase) != -1))
                        {
                            // Solo excluir si también tiene el sufijo correcto o es clase base
                            return nombreClase.EndsWith("BackgroundService", System.StringComparison.Ordinal) ||
                                   nombreClase.EndsWith("HostedService", System.StringComparison.Ordinal) ||
                                   clase.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
                        }
                    }
                }
            }

            // Excluir clases que implementan IHostedService específicamente
            if (ImplementaIHostedService(clase))
                return true;

            // Excluir clases internas o anidadas que pueden tener propósitos especiales
            if (EsClaseAnidada(clase) || EsClaseInterna(clase))
                return true;

            // Excluir clases que heredan de BackgroundService o clases base conocidas
            if (HeredaDeClaseBackgroundServiceBase(clase))
                return true;

            // Excluir extensiones de background service
            if (EsExtensionBackgroundService(clase, nombreClase))
                return true;

            return false;
        }

        private static bool ImplementaIHostedService(ClassDeclarationSyntax clase)
        {
            if (clase.BaseList == null)
                return false;

            // Verificar si implementa IHostedService o interfaces relacionadas
            return clase.BaseList.Types.Any(tipo =>
            {
                var tipoString = tipo.ToString();
                
                // Interfaces típicas de background services
                var interfacesBackgroundService = new[]
                {
                    "IHostedService", "IHostedLifecycleService", "BackgroundService"
                };

                return interfacesBackgroundService.Any(interfaz => 
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

        private static bool HeredaDeClaseBackgroundServiceBase(ClassDeclarationSyntax clase)
        {
            if (clase.BaseList == null)
                return false;

            // Verificar si hereda de clases base de background services conocidas
            return clase.BaseList.Types.Any(tipo =>
            {
                var tipoString = tipo.ToString();
                
                var clasesBaseBackgroundService = new[]
                {
                    "BackgroundService", "BackgroundServiceBase", "BaseBackgroundService",
                    "HostedService", "HostedServiceBase", "BaseHostedService",
                    "TimerHostedService", "PeriodicHostedService"
                };

                return clasesBaseBackgroundService.Any(claseBase => 
                    tipoString.IndexOf(claseBase, System.StringComparison.OrdinalIgnoreCase) != -1);
            });
        }

        private static bool EsExtensionBackgroundService(ClassDeclarationSyntax clase, string nombreClase)
        {
            // Verificar si es una clase de extensiones para background services
            if (clase.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
            {
                var nombresExtension = new[]
                {
                    "Extensions", "BackgroundServiceExtensions", "HostedServiceExtensions",
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
            var sufijosIncorrectos = new[] 
            { 
                "backgroundservice", "BackgroundService", "hostedservice", "HostedService",
                "background", "hosted", "service", "Service", "worker", "Worker",
                "job", "Job", "task", "Task"
            };
            
            foreach (var sufijo in sufijosIncorrectos)
            {
                if (nombreBase.EndsWith(sufijo, System.StringComparison.OrdinalIgnoreCase) && 
                    !nombreBase.EndsWith("BackgroundService", System.StringComparison.Ordinal))
                {
                    nombreBase = nombreBase.Substring(0, nombreBase.Length - sufijo.Length);
                    break;
                }
            }

            // Convertir a PascalCase
            var nombrePascal = ConvertirAPascalCase(nombreBase);

            // Agregar sufijo "BackgroundService" si no lo tiene
            if (!nombrePascal.EndsWith("BackgroundService", System.StringComparison.Ordinal))
            {
                nombrePascal += "BackgroundService";
            }

            return nombrePascal;
        }

        private static string ConvertirAPascalCase(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return "CustomBackgroundService";

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


