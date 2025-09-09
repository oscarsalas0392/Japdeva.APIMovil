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
    public class ValidarUbicacionInterfacesMiddlewares : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA075";

        private const string Titulo = "Interfaz de Middleware debe estar en carpeta Middlewares";
        private const string FormatoMensaje = "La interfaz '{0}' debe estar ubicada en la carpeta 'Middlewares' junto con su implementación '{1}'. Ubicación actual: {2}";
        private const string Descripcion = "Las interfaces de middlewares deben estar ubicadas en la carpeta Middlewares junto con sus implementaciones para mantener la organización y facilitar el mantenimiento.";
        private const string Categoria = "Design";

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
            contexto.RegisterCompilationAction(AnalizarCompilacionCompleta);
        }

        private static void AnalizarCompilacionCompleta(CompilationAnalysisContext contexto)
        {
            var todosLosArboles = contexto.Compilation.SyntaxTrees;
            var interfaces = new System.Collections.Generic.List<InterfaceInfo>();
            var clases = new System.Collections.Generic.List<ClaseInfo>();

            // Recopilar todas las interfaces y clases
            foreach (var arbol in todosLosArboles)
            {
                var raiz = arbol.GetRoot(contexto.CancellationToken);

                // Obtener interfaces
                var interfacesSintaxis = raiz.DescendantNodes().OfType<InterfaceDeclarationSyntax>();
                foreach (var interfazSintaxis in interfacesSintaxis)
                {
                    var namespaceDeclaracion = interfazSintaxis.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
                    var nombreNamespace = namespaceDeclaracion?.Name?.ToString() ?? "";

                    interfaces.Add(new InterfaceInfo
                    {
                        Nombre = interfazSintaxis.Identifier.ValueText,
                        Namespace = nombreNamespace,
                        Sintaxis = interfazSintaxis,
                        RutaArchivo = arbol.FilePath
                    });
                }

                // Obtener clases
                var clasesSintaxis = raiz.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (var claseSintaxis in clasesSintaxis)
                {
                    var namespaceDeclaracion = claseSintaxis.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
                    var nombreNamespace = namespaceDeclaracion?.Name?.ToString() ?? "";

                    clases.Add(new ClaseInfo
                    {
                        Nombre = claseSintaxis.Identifier.ValueText,
                        Namespace = nombreNamespace,
                        Sintaxis = claseSintaxis,
                        RutaArchivo = arbol.FilePath
                    });
                }
            }

            // Validar cada interfaz de middleware
            foreach (var interfaz in interfaces)
            {
                ValidarUbicacionInterfazMiddleware(contexto, interfaz, clases);
            }
        }

        private static void ValidarUbicacionInterfazMiddleware(CompilationAnalysisContext contexto, 
            InterfaceInfo interfaz, System.Collections.Generic.List<ClaseInfo> todasLasClases)
        {
            // Solo validar interfaces que terminen en "Middleware"
            if (!interfaz.Nombre.EndsWith("Middleware", System.StringComparison.Ordinal))
                return;

            // Excluir interfaces del sistema o especiales
            if (EsInterfazEspecial(interfaz))
                return;

            // Encontrar la clase que implementa esta interfaz
            var implementacion = EncontrarImplementacionMiddleware(interfaz, todasLasClases);
            if (implementacion == null)
                return; // Si no hay implementación, no validar

            // Verificar si la interfaz está en carpeta Middlewares
            if (!EstaEnCarpetaMiddlewares(interfaz.RutaArchivo))
            {
                var carpetaActual = ObtenerCarpetaDelArchivo(interfaz.RutaArchivo);
                
                var diagnostico = Diagnostic.Create(
                    Regla,
                    interfaz.Sintaxis.Identifier.GetLocation(),
                    interfaz.Nombre,
                    implementacion.Nombre,
                    carpetaActual);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static bool EsInterfazEspecial(InterfaceInfo interfaz)
        {
            // Excluir interfaces del sistema
            if (interfaz.Namespace.StartsWith("System", System.StringComparison.OrdinalIgnoreCase) ||
                interfaz.Namespace.StartsWith("Microsoft", System.StringComparison.OrdinalIgnoreCase))
                return true;

            // Excluir interfaces con atributos especiales
            if (interfaz.Sintaxis.AttributeLists.Any())
            {
                foreach (var listaAtributos in interfaz.Sintaxis.AttributeLists)
                {
                    foreach (var atributo in listaAtributos.Attributes)
                    {
                        var nombreAtributo = atributo.Name.ToString();
                        
                        var atributosEspeciales = new[]
                        {
                            "Obsolete", "GeneratedCode", "EditorBrowsable"
                        };

                        if (atributosEspeciales.Any(a => 
                            nombreAtributo.IndexOf(a, System.StringComparison.OrdinalIgnoreCase) != -1))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static ClaseInfo EncontrarImplementacionMiddleware(InterfaceInfo interfaz, 
            System.Collections.Generic.List<ClaseInfo> todasLasClases)
        {
            foreach (var clase in todasLasClases)
            {
                if (ClaseImplementaInterfaz(clase, interfaz))
                {
                    return clase;
                }
            }

            return null;
        }

        private static bool ClaseImplementaInterfaz(ClaseInfo clase, InterfaceInfo interfaz)
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

        private static bool EstaEnCarpetaMiddlewares(string rutaArchivo)
        {
            if (string.IsNullOrEmpty(rutaArchivo))
                return false;

            var rutaNormalizada = rutaArchivo.Replace('\\', '/');
            var partesRuta = rutaNormalizada.Split('/');
            
            return partesRuta.Any(parte => 
                parte.Equals("Middlewares", System.StringComparison.OrdinalIgnoreCase) ||
                parte.Equals("Middleware", System.StringComparison.OrdinalIgnoreCase));
        }

        private static string ObtenerCarpetaDelArchivo(string rutaArchivo)
        {
            if (string.IsNullOrEmpty(rutaArchivo))
                return "Ubicación desconocida";

            try
            {
                var directorio = Path.GetDirectoryName(rutaArchivo);
                if (string.IsNullOrEmpty(directorio))
                    return "Ubicación desconocida";

                return directorio.Replace('\\', '/');
            }
            catch
            {
                return "Ubicación desconocida";
            }
        }

        // Clases de apoyo para almacenar información
        private class InterfaceInfo
        {
            public string Nombre { get; set; } = "";
            public string Namespace { get; set; } = "";
            public InterfaceDeclarationSyntax Sintaxis { get; set; }
            public string RutaArchivo { get; set; } = "";
        }

        private class ClaseInfo
        {
            public string Nombre { get; set; } = "";
            public string Namespace { get; set; } = "";
            public ClassDeclarationSyntax Sintaxis { get; set; }
            public string RutaArchivo { get; set; } = "";
        }
    }
}
