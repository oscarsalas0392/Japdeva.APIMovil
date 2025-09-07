using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarUbicacionClaseConInterfaz : DiagnosticAnalyzer
    {
        public const string DiagnosticIdClase = "JAPDEVA033";
        public const string DiagnosticIdInterfaz = "JAPDEVA054";

        private const string TituloClase = "Clase con interfaz debe estar en carpeta con su nombre";
        private const string TituloInterfaz = "Interfaz debe estar en la misma carpeta que su implementación";
        
        private const string FormatoMensajeClase = "La clase '{0}' que implementa la interfaz '{1}' debe estar en una carpeta llamada '{0}'. Ubicación actual: {2}";
        private const string FormatoMensajeInterfaz = "La interfaz '{0}' debe estar en la misma carpeta que su implementación '{1}'. Ubicación interfaz: {2}, ubicación implementación: {3}";
        
        private const string DescripcionClase = "Las clases que implementan interfaces deben estar organizadas en carpetas que lleven el nombre de la clase para mantener una estructura de proyecto clara y facilitar la navegación del código.";
        private const string DescripcionInterfaz = "Las interfaces deben estar ubicadas en la misma carpeta que sus implementaciones principales para mantener cohesión y facilitar el mantenimiento.";
        private const string Categoria = "Design";

        private static readonly DiagnosticDescriptor ReglaClase = new DiagnosticDescriptor(
            DiagnosticIdClase,
            TituloClase,
            FormatoMensajeClase,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: DescripcionClase,
            helpLinkUri: "https://docs.microsoft.com/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces");

        private static readonly DiagnosticDescriptor ReglaInterfaz = new DiagnosticDescriptor(
            DiagnosticIdInterfaz,
            TituloInterfaz,
            FormatoMensajeInterfaz,
            Categoria,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: DescripcionInterfaz,
            helpLinkUri: "https://docs.microsoft.com/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(ReglaClase, ReglaInterfaz);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterCompilationAction(AnalizarCompilacionCompleta);
        }

        private static void AnalizarCompilacionCompleta(CompilationAnalysisContext contexto)
        {
            // Obtener todas las interfaces y clases del proyecto
            var todosLosArboles = contexto.Compilation.SyntaxTrees;
            var interfaces = new System.Collections.Generic.List<InterfaceInfo>();
            var clases = new System.Collections.Generic.List<ClaseInfo>();

            foreach (var arbol in todosLosArboles)
            {
                var raiz = arbol.GetRoot(contexto.CancellationToken);
                var modeloSemantico = contexto.Compilation.GetSemanticModel(arbol);

                // Recopilar interfaces
                var interfacesEnArchivo = raiz.DescendantNodes().OfType<InterfaceDeclarationSyntax>();
                foreach (var interfazSintaxis in interfacesEnArchivo)
                {
                    var simboloInterfaz = modeloSemantico.GetDeclaredSymbol(interfazSintaxis);
                    if (simboloInterfaz != null)
                    {
                        interfaces.Add(new InterfaceInfo
                        {
                            Nombre = interfazSintaxis.Identifier.ValueText,
                            Namespace = ObtenerNamespace(interfazSintaxis),
                            RutaArchivo = arbol.FilePath,
                            Sintaxis = interfazSintaxis,
                            Simbolo = simboloInterfaz
                        });
                    }
                }

                // Recopilar clases
                var clasesEnArchivo = raiz.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (var claseSintaxis in clasesEnArchivo)
                {
                    var simboloClase = modeloSemantico.GetDeclaredSymbol(claseSintaxis);
                    if (simboloClase != null)
                    {
                        clases.Add(new ClaseInfo
                        {
                            Nombre = claseSintaxis.Identifier.ValueText,
                            Namespace = ObtenerNamespace(claseSintaxis),
                            RutaArchivo = arbol.FilePath,
                            Sintaxis = claseSintaxis,
                            Simbolo = simboloClase
                        });
                    }
                }
            }

            // Validar ubicación de cada clase que implementa interfaces
            foreach (var clase in clases)
            {
                ValidarUbicacionClase(contexto, clase, interfaces);
            }

            // Validar ubicación de cada interfaz
            foreach (var interfaz in interfaces)
            {
                ValidarUbicacionInterfaz(contexto, interfaz, clases);
            }
        }

        private static void ValidarUbicacionClase(CompilationAnalysisContext contexto, 
            ClaseInfo clase, System.Collections.Generic.List<InterfaceInfo> todasLasInterfaces)
        {
            if (EsClaseEspecial(clase))
                return;

            // Encontrar interfaces que esta clase implementa
            var interfacesImplementadas = EncontrarInterfacesImplementadas(clase, todasLasInterfaces);

            if (!interfacesImplementadas.Any())
                return; // Si no implementa interfaces, no validar

            // Verificar si la clase est� en la carpeta correcta
            if (!string.IsNullOrEmpty(clase.RutaArchivo))
            {
                var carpetaEsperada = clase.Nombre;
                var carpetaActual = ObtenerCarpetaDelArchivo(clase.RutaArchivo);
                var rutaRelativa = ObtenerRutaRelativa(clase.RutaArchivo);

                if (!carpetaActual.Equals(carpetaEsperada, System.StringComparison.OrdinalIgnoreCase))
                {
                    // Tomar la primera interfaz como referencia para el mensaje
                    var interfazPrincipal = interfacesImplementadas.First();

                    var diagnostico = Diagnostic.Create(
                        ReglaClase,
                        clase.Sintaxis.Identifier.GetLocation(),
                        clase.Nombre,
                        interfazPrincipal.Nombre,
                        rutaRelativa);

                    contexto.ReportDiagnostic(diagnostico);
                }
            }
        }

        private static void ValidarUbicacionInterfaz(CompilationAnalysisContext contexto,
            InterfaceInfo interfaz, System.Collections.Generic.List<ClaseInfo> todasLasClases)
        {
            // Encontrar clases que implementan esta interfaz
            var clasesQueImplementan = todasLasClases.Where(c => ClaseImplementaInterfaz(c, interfaz)).ToList();
            
            if (!clasesQueImplementan.Any())
                return; // Si no tiene implementaciones, no validar

            // Para cada clase que implementa la interfaz, verificar que la interfaz esté en la misma carpeta
            foreach (var claseImplementadora in clasesQueImplementan)
            {
                if (EsClaseEspecial(claseImplementadora))
                    continue;

                var carpetaInterfaz = ObtenerCarpetaDelArchivo(interfaz.RutaArchivo);
                var carpetaClase = ObtenerCarpetaDelArchivo(claseImplementadora.RutaArchivo);
                
                if (!carpetaInterfaz.Equals(carpetaClase, System.StringComparison.OrdinalIgnoreCase))
                {
                    var rutaRelativaInterfaz = ObtenerRutaRelativa(interfaz.RutaArchivo);
                    var rutaRelativaClase = ObtenerRutaRelativa(claseImplementadora.RutaArchivo);

                    var diagnostico = Diagnostic.Create(
                        ReglaInterfaz,
                        interfaz.Sintaxis.Identifier.GetLocation(),
                        interfaz.Nombre,
                        claseImplementadora.Nombre,
                        rutaRelativaInterfaz,
                        rutaRelativaClase);

                    contexto.ReportDiagnostic(diagnostico);
                }
            }
        }

        private static System.Collections.Generic.List<InterfaceInfo> EncontrarInterfacesImplementadas(
            ClaseInfo clase, System.Collections.Generic.List<InterfaceInfo> todasLasInterfaces)
        {
            var interfacesImplementadas = new System.Collections.Generic.List<InterfaceInfo>();

            foreach (var interfaz in todasLasInterfaces)
            {
                if (ClaseImplementaInterfaz(clase, interfaz))
                {
                    interfacesImplementadas.Add(interfaz);
                }
            }

            return interfacesImplementadas;
        }

        private static bool ClaseImplementaInterfaz(ClaseInfo clase, InterfaceInfo interfaz)
        {
            try
            {
                // Verificar si la clase implementa la interfaz usando an�lisis sem�ntico
                var interfacesImplementadas = clase.Simbolo.AllInterfaces;
                
                return interfacesImplementadas.Any(i => 
                    SymbolEqualityComparer.Default.Equals(i, interfaz.Simbolo));
            }
            catch
            {
                // Fallback: verificar sint�cticamente
                return ClaseImplementaInterfazSintacticamente(clase, interfaz);
            }
        }

        private static bool ClaseImplementaInterfazSintacticamente(ClaseInfo clase, InterfaceInfo interfaz)
        {
            if (clase.Sintaxis.BaseList == null)
                return false;

            return clase.Sintaxis.BaseList.Types.Any(tipo =>
            {
                var tipoString = tipo.ToString();
                return tipoString.Equals(interfaz.Nombre, System.StringComparison.OrdinalIgnoreCase) ||
                       tipoString.EndsWith("." + interfaz.Nombre, System.StringComparison.OrdinalIgnoreCase);
            });
        }

        private static bool EsClaseEspecial(ClaseInfo clase)
        {
            // Excluir clases del sistema
            if (clase.Namespace.StartsWith("System", System.StringComparison.OrdinalIgnoreCase) ||
                clase.Namespace.StartsWith("Microsoft", System.StringComparison.OrdinalIgnoreCase))
                return true;

            // Excluir clases abstractas
            if (clase.Sintaxis.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
                return true;

            // Excluir clases est�ticas
            if (clase.Sintaxis.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
                return true;

            // Excluir clases con atributos especiales
            if (clase.Sintaxis.AttributeLists.Any())
            {
                foreach (var listaAtributos in clase.Sintaxis.AttributeLists)
                {
                    foreach (var atributo in listaAtributos.Attributes)
                    {
                        var nombreAtributo = atributo.Name.ToString();
                        
                        var atributosEspeciales = new[]
                        {
                            "Obsolete", "GeneratedCode", "TestClass",
                            "Controller", "ApiController", "Entity", "Table",
                            "Program", "Startup"
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
            if (clase.Sintaxis.Parent is ClassDeclarationSyntax ||
                clase.Sintaxis.Parent is StructDeclarationSyntax ||
                clase.Sintaxis.Parent is RecordDeclarationSyntax)
                return true;

            // Excluir clases espec�ficas por nombre
            var clasesEspecialesExcluidas = new[]
            {
                "Program", "Startup", "Global", "AssemblyInfo"
            };

            if (clasesEspecialesExcluidas.Any(nombre => 
                clase.Nombre.Equals(nombre, System.StringComparison.OrdinalIgnoreCase)))
                return true;

            // Excluir clases en carpetas espec�ficas que no requieren esta organizaci�n
            if (!string.IsNullOrEmpty(clase.RutaArchivo))
            {
                var rutaNormalizada = clase.RutaArchivo.Replace('\\', '/');
                var carpetasExcluidas = new[]
                {
                    "/Controllers/", "/Models/", "/Views/", "/Areas/",
                    "/Migrations/", "/Configuration/", "/Extensions/",
                    "/Attributes/", "/Filters/", "/Middleware/"
                };

                if (carpetasExcluidas.Any(carpeta => 
                    rutaNormalizada.IndexOf(carpeta, System.StringComparison.OrdinalIgnoreCase) != -1))
                    return true;
            }

            return false;
        }

        private static string ObtenerCarpetaDelArchivo(string rutaArchivo)
        {
            if (string.IsNullOrEmpty(rutaArchivo))
                return string.Empty;

            try
            {
                var directorio = Path.GetDirectoryName(rutaArchivo);
                if (string.IsNullOrEmpty(directorio))
                    return string.Empty;

                return Path.GetFileName(directorio);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string ObtenerRutaRelativa(string rutaArchivo)
        {
            if (string.IsNullOrEmpty(rutaArchivo))
                return string.Empty;

            try
            {
                // Obtener solo las �ltimas 2-3 carpetas para mostrar contexto
                var partesRuta = rutaArchivo.Replace('\\', '/').Split('/');
                if (partesRuta.Length >= 3)
                {
                    var ultimasPartes = partesRuta.Skip(partesRuta.Length - 3).ToArray();
                    return string.Join("/", ultimasPartes);
                }
                
                return Path.GetFileName(rutaArchivo);
            }
            catch
            {
                return rutaArchivo;
            }
        }

        private static string ObtenerNamespace(SyntaxNode nodo)
        {
            // Replace the problematic line with the following:
            var namespacePadre = nodo.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            return namespacePadre?.Name?.ToString() ?? string.Empty;
        }

        private class InterfaceInfo
        {
            public string Nombre { get; set; } = string.Empty;
            public string Namespace { get; set; } = string.Empty;
            public string RutaArchivo { get; set; } = string.Empty;
            public InterfaceDeclarationSyntax Sintaxis { get; set; }
            public INamedTypeSymbol Simbolo { get; set; }
        }

        private class ClaseInfo
        {
            public string Nombre { get; set; } = string.Empty;
            public string Namespace { get; set; } = string.Empty;
            public string RutaArchivo { get; set; } = string.Empty;
            public ClassDeclarationSyntax Sintaxis { get; set; }
            public INamedTypeSymbol Simbolo { get; set; }
        }
    }
}


