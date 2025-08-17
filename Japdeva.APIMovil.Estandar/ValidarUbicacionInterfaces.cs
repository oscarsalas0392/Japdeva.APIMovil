using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarUbicacionInterfaces : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA030";

        private const string Titulo = "Interfaz debe estar al mismo nivel que su implementación";
        private const string FormatoMensaje = "La interfaz '{0}' debe estar en el mismo namespace que las clases que la implementan. Se encontraron implementaciones en: {1}";
        private const string Descripcion = "Las interfaces deben estar ubicadas en el mismo namespace que las clases que las implementan para mantener la cohesión del código y facilitar el mantenimiento.";
        private const string Categoria = "Design";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces");

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

            // Validar ubicación de cada interfaz
            foreach (var interfaz in interfaces)
            {
                ValidarUbicacionInterfaz(contexto, interfaz, clases);
            }
        }

        private static void ValidarUbicacionInterfaz(CompilationAnalysisContext contexto, 
            InterfaceInfo interfaz, System.Collections.Generic.List<ClaseInfo> todasLasClases)
        {
            if (EsInterfazEspecial(interfaz))
                return;

            // Encontrar clases que implementan esta interfaz
            var implementaciones = EncontrarImplementaciones(interfaz, todasLasClases);

            if (!implementaciones.Any())
                return; // Si no hay implementaciones, no validar

            // Verificar si todas las implementaciones están en el mismo namespace que la interfaz
            var namespacesImplementaciones = implementaciones
                .Select(impl => impl.Namespace)
                .Where(ns => !string.IsNullOrEmpty(ns))
                .Distinct()
                .ToList();

            var interfazNamespace = interfaz.Namespace;

            // Si hay implementaciones en namespaces diferentes al de la interfaz
            var namespacesDiferentes = namespacesImplementaciones
                .Where(ns => !ns.Equals(interfazNamespace, System.StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (namespacesDiferentes.Any())
            {
                var namespacesTexto = string.Join(", ", namespacesDiferentes);
                
                var diagnostico = Diagnostic.Create(
                    Regla,
                    interfaz.Sintaxis.Identifier.GetLocation(),
                    interfaz.Nombre,
                    namespacesTexto);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static System.Collections.Generic.List<ClaseInfo> EncontrarImplementaciones(
            InterfaceInfo interfaz, System.Collections.Generic.List<ClaseInfo> todasLasClases)
        {
            var implementaciones = new System.Collections.Generic.List<ClaseInfo>();

            foreach (var clase in todasLasClases)
            {
                if (ClaseImplementaInterfaz(clase, interfaz))
                {
                    implementaciones.Add(clase);
                }
            }

            return implementaciones;
        }

        private static bool ClaseImplementaInterfaz(ClaseInfo clase, InterfaceInfo interfaz)
        {
            try
            {
                // Verificar si la clase implementa la interfaz
                var interfacesImplementadas = clase.Simbolo.AllInterfaces;
                
                return interfacesImplementadas.Any(i => 
                    SymbolEqualityComparer.Default.Equals(i, interfaz.Simbolo));
            }
            catch
            {
                // Fallback: verificar sintácticamente
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
                            "ComVisible", "ComImport", "Guid", "InterfaceType",
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

            // Excluir interfaces anidadas
            if (interfaz.Sintaxis.Parent is ClassDeclarationSyntax ||
                interfaz.Sintaxis.Parent is StructDeclarationSyntax ||
                interfaz.Sintaxis.Parent is InterfaceDeclarationSyntax)
                return true;

            // Excluir interfaces genéricas complejas
            if (interfaz.Sintaxis.TypeParameterList != null && 
                interfaz.Sintaxis.ConstraintClauses.Any())
                return true;

            return false;
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


