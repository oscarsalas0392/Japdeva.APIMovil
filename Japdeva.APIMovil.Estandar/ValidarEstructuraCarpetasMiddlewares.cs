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
    public class ValidarEstructuraCarpetasMiddlewares : DiagnosticAnalyzer
    {
        public const string DiagnosticIdClase = "JAPDEVA076";
        public const string DiagnosticIdInterfaz = "JAPDEVA077";

        private const string TituloClase = "Middleware debe estar en subcarpeta con su nombre completo";
        private const string TituloInterfaz = "Interfaz de middleware debe estar en subcarpeta con nombre de implementación";
        
        private const string FormatoMensajeClase = "El middleware '{0}' debe estar en una subcarpeta llamada '{0}' dentro de Middlewares. Ubicación actual: {1}";
        private const string FormatoMensajeInterfaz = "La interfaz '{0}' debe estar en la misma subcarpeta que su implementación '{1}'. Ubicación esperada: Middlewares/{1}/";
        
        private const string DescripcionClase = "Los middlewares deben estar organizados en subcarpetas dentro de Middlewares que lleven el nombre completo del middleware para mantener una estructura clara.";
        private const string DescripcionInterfaz = "Las interfaces de middleware deben estar en la misma subcarpeta que su implementación para mantener cohesión del código.";
        private const string Categoria = "Design";

        private static readonly DiagnosticDescriptor ReglaClase = new DiagnosticDescriptor(
            DiagnosticIdClase,
            TituloClase,
            FormatoMensajeClase,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: DescripcionClase,
            helpLinkUri: "https://docs.microsoft.com/aspnet/core/fundamentals/middleware/write");

        private static readonly DiagnosticDescriptor ReglaInterfaz = new DiagnosticDescriptor(
            DiagnosticIdInterfaz,
            TituloInterfaz,
            FormatoMensajeInterfaz,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: DescripcionInterfaz,
            helpLinkUri: "https://docs.microsoft.com/aspnet/core/fundamentals/middleware/write");

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
            var todosLosArboles = contexto.Compilation.SyntaxTrees;
            var middlewares = new System.Collections.Generic.List<MiddlewareInfo>();

            // Recopilar información de middlewares (clases e interfaces)
            foreach (var arbol in todosLosArboles)
            {
                var raiz = arbol.GetRoot(contexto.CancellationToken);

                // Obtener clases de middleware
                var clases = raiz.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .Where(c => c.Identifier.ValueText.EndsWith("Middleware", System.StringComparison.Ordinal) &&
                               EstaEnCarpetaMiddlewares(arbol.FilePath));

                foreach (var clase in clases)
                {
                    var namespaceDeclaracion = clase.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
                    var nombreNamespace = namespaceDeclaracion?.Name?.ToString() ?? "";

                    middlewares.Add(new MiddlewareInfo
                    {
                        Nombre = clase.Identifier.ValueText,
                        Namespace = nombreNamespace,
                        RutaArchivo = arbol.FilePath,
                        TipoElemento = TipoElementoMiddleware.Clase,
                        SintaxisClase = clase
                    });
                }

                // Obtener interfaces de middleware
                var interfaces = raiz.DescendantNodes().OfType<InterfaceDeclarationSyntax>()
                    .Where(i => i.Identifier.ValueText.EndsWith("Middleware", System.StringComparison.Ordinal) &&
                               EstaEnCarpetaMiddlewares(arbol.FilePath));

                foreach (var interfaz in interfaces)
                {
                    var namespaceDeclaracion = interfaz.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
                    var nombreNamespace = namespaceDeclaracion?.Name?.ToString() ?? "";

                    middlewares.Add(new MiddlewareInfo
                    {
                        Nombre = interfaz.Identifier.ValueText,
                        Namespace = nombreNamespace,
                        RutaArchivo = arbol.FilePath,
                        TipoElemento = TipoElementoMiddleware.Interfaz,
                        SintaxisInterfaz = interfaz
                    });
                }
            }

            // Validar estructura de carpetas para cada middleware
            foreach (var middleware in middlewares)
            {
                if (middleware.TipoElemento == TipoElementoMiddleware.Clase)
                {
                    ValidarEstructuraClaseMiddleware(contexto, middleware, middlewares);
                }
                else if (middleware.TipoElemento == TipoElementoMiddleware.Interfaz)
                {
                    ValidarEstructuraInterfazMiddleware(contexto, middleware, middlewares);
                }
            }
        }

        private static void ValidarEstructuraClaseMiddleware(CompilationAnalysisContext contexto, 
            MiddlewareInfo claseMiddleware, System.Collections.Generic.List<MiddlewareInfo> todosMiddlewares)
        {
            // Excluir middlewares especiales
            if (EsMiddlewareEspecial(claseMiddleware))
                return;

            var nombreCompleto = claseMiddleware.Nombre;
            var carpetaEsperada = $"/{nombreCompleto}/";
            
            if (!ContieneSubcarpetaCorrecta(claseMiddleware.RutaArchivo, carpetaEsperada))
            {
                var carpetaActual = ObtenerCarpetaDelArchivo(claseMiddleware.RutaArchivo);
                
                var diagnostico = Diagnostic.Create(
                    ReglaClase,
                    claseMiddleware.SintaxisClase.Identifier.GetLocation(),
                    claseMiddleware.Nombre,
                    carpetaActual);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static void ValidarEstructuraInterfazMiddleware(CompilationAnalysisContext contexto,
            MiddlewareInfo interfazMiddleware, System.Collections.Generic.List<MiddlewareInfo> todosMiddlewares)
        {
            // Buscar la clase que implementa esta interfaz
            var implementacion = todosMiddlewares
                .Where(m => m.TipoElemento == TipoElementoMiddleware.Clase)
                .FirstOrDefault(clase => ClaseImplementaInterfaz(clase, interfazMiddleware));

            if (implementacion == null)
                return; // Sin implementación, no validar

            var nombreCompletoImplementacion = implementacion.Nombre;
            var carpetaEsperada = $"/{nombreCompletoImplementacion}/";

            if (!ContieneSubcarpetaCorrecta(interfazMiddleware.RutaArchivo, carpetaEsperada))
            {
                var diagnostico = Diagnostic.Create(
                    ReglaInterfaz,
                    interfazMiddleware.SintaxisInterfaz.Identifier.GetLocation(),
                    interfazMiddleware.Nombre,
                    implementacion.Nombre);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static bool EstaEnCarpetaMiddlewares(string rutaArchivo)
        {
            if (string.IsNullOrEmpty(rutaArchivo))
                return false;

            var rutaNormalizada = rutaArchivo.Replace('\\', '/');
            return rutaNormalizada.IndexOf("/Middlewares/", System.StringComparison.OrdinalIgnoreCase) != -1 ||
                   rutaNormalizada.IndexOf("/Middleware/", System.StringComparison.OrdinalIgnoreCase) != -1;
        }

        private static bool EsMiddlewareEspecial(MiddlewareInfo middleware)
        {
            // Excluir middlewares del sistema
            if (middleware.Namespace.StartsWith("System", System.StringComparison.OrdinalIgnoreCase) ||
                middleware.Namespace.StartsWith("Microsoft", System.StringComparison.OrdinalIgnoreCase))
                return true;

            // Excluir middlewares abstractos o estáticos
            if (middleware.SintaxisClase != null)
            {
                if (middleware.SintaxisClase.Modifiers.Any(m => 
                    m.IsKind(SyntaxKind.AbstractKeyword) || m.IsKind(SyntaxKind.StaticKeyword)))
                    return true;
            }

            // Excluir middlewares con atributos especiales
            var sintaxis = (SyntaxNode)middleware.SintaxisClase ?? middleware.SintaxisInterfaz;
            if (sintaxis is MemberDeclarationSyntax member && member.AttributeLists.Any())
            {
                foreach (var listaAtributos in member.AttributeLists)
                {
                    foreach (var atributo in listaAtributos.Attributes)
                    {
                        var nombreAtributo = atributo.Name.ToString();
                        
                        var atributosEspeciales = new[]
                        {
                            "Obsolete", "GeneratedCode", "TestClass"
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

        private static bool ContieneSubcarpetaCorrecta(string rutaArchivo, string subcarpetaEsperada)
        {
            if (string.IsNullOrEmpty(rutaArchivo))
                return false;

            var rutaNormalizada = rutaArchivo.Replace('\\', '/');
            return rutaNormalizada.IndexOf($"/Middlewares{subcarpetaEsperada}", System.StringComparison.OrdinalIgnoreCase) != -1 ||
                   rutaNormalizada.IndexOf($"/Middleware{subcarpetaEsperada}", System.StringComparison.OrdinalIgnoreCase) != -1;
        }

        private static bool ClaseImplementaInterfaz(MiddlewareInfo clase, MiddlewareInfo interfaz)
        {
            if (clase.SintaxisClase?.BaseList == null)
                return false;

            return clase.SintaxisClase.BaseList.Types.Any(tipo =>
            {
                var tipoString = tipo.ToString();
                return tipoString.Equals(interfaz.Nombre, System.StringComparison.OrdinalIgnoreCase) ||
                       tipoString.EndsWith("." + interfaz.Nombre, System.StringComparison.OrdinalIgnoreCase);
            });
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

        // Clases de apoyo
        private class MiddlewareInfo
        {
            public string Nombre { get; set; } = "";
            public string Namespace { get; set; } = "";
            public string RutaArchivo { get; set; } = "";
            public TipoElementoMiddleware TipoElemento { get; set; }
            public ClassDeclarationSyntax SintaxisClase { get; set; }
            public InterfaceDeclarationSyntax SintaxisInterfaz { get; set; }
        }

        private enum TipoElementoMiddleware
        {
            Clase,
            Interfaz
        }
    }
}
