using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;


namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarNombreInterfazClase : DiagnosticAnalyzer
    {
        public const string DiagnosticIdInterfaz = "JAPDEVA031";
        public const string DiagnosticIdClase = "JAPDEVA032";

        private const string TituloInterfaz = "Interfaz debe tener el mismo nombre que su implementación con 'I' al frente";
        private const string FormatoMensajeInterfaz = "La interfaz '{0}' debe llamarse '{1}' para corresponder con la clase '{2}' que la implementa";
        private const string DescripcionInterfaz = "Las interfaces deben tener el mismo nombre que las clases que las implementan, precedido por 'I', para mantener la correspondencia clara entre interfaz e implementación.";

        private const string TituloClase = "Clase debe tener el mismo nombre que la interfaz que implementa sin 'I'";
        private const string FormatoMensajeClase = "La clase '{0}' debe llamarse '{1}' para corresponder con la interfaz '{2}' que implementa";
        private const string DescripcionClase = "Las clases deben tener el mismo nombre que las interfaces que implementan, sin el prefijo 'I', para mantener la correspondencia clara entre interfaz e implementación.";

        private const string Categoria = "Naming";

        private static readonly DiagnosticDescriptor ReglaInterfaz = new DiagnosticDescriptor(
            DiagnosticIdInterfaz,
            TituloInterfaz,
            FormatoMensajeInterfaz,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: DescripcionInterfaz,
            helpLinkUri: "https://docs.microsoft.com/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces");

        private static readonly DiagnosticDescriptor ReglaClase = new DiagnosticDescriptor(
            DiagnosticIdClase,
            TituloClase,
            FormatoMensajeClase,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: DescripcionClase,
            helpLinkUri: "https://docs.microsoft.com/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(ReglaInterfaz, ReglaClase);

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
                            Sintaxis = claseSintaxis,
                            Simbolo = simboloClase
                        });
                    }
                }
            }

            // Validar nombres de interfaces basándose en sus implementaciones
            foreach (var interfaz in interfaces)
            {
                ValidarNombreInterfaz(contexto, interfaz, clases);
            }

            // Validar nombres de clases basándose en las interfaces que implementan
            foreach (var clase in clases)
            {
                ValidarNombreClase(contexto, clase, interfaces);
            }
        }

        private static void ValidarNombreInterfaz(CompilationAnalysisContext contexto, 
            InterfaceInfo interfaz, System.Collections.Generic.List<ClaseInfo> todasLasClases)
        {
            if (EsInterfazEspecial(interfaz))
                return;

            // Encontrar clases que implementan esta interfaz
            var implementaciones = EncontrarImplementaciones(interfaz, todasLasClases);

            if (!implementaciones.Any())
                return; // Si no hay implementaciones, no validar

            // Si hay múltiples implementaciones, tomar la primera como referencia
            var implementacionPrincipal = implementaciones.First();
            var nombreClaseEsperado = implementacionPrincipal.Nombre;
            var nombreInterfazEsperado = "I" + nombreClaseEsperado;

            // Verificar si el nombre de la interfaz corresponde
            if (!interfaz.Nombre.Equals(nombreInterfazEsperado, System.StringComparison.Ordinal))
            {
                var diagnostico = Diagnostic.Create(
                    ReglaInterfaz,
                    interfaz.Sintaxis.Identifier.GetLocation(),
                    interfaz.Nombre,
                    nombreInterfazEsperado,
                    implementacionPrincipal.Nombre);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static void ValidarNombreClase(CompilationAnalysisContext contexto, 
            ClaseInfo clase, System.Collections.Generic.List<InterfaceInfo> todasLasInterfaces)
        {
            if (EsClaseEspecial(clase))
                return;

            // Encontrar interfaces que esta clase implementa
            var interfacesImplementadas = EncontrarInterfacesImplementadas(clase, todasLasInterfaces);

            if (!interfacesImplementadas.Any())
                return; // Si no implementa interfaces, no validar

            // Verificar cada interfaz implementada
            foreach (var interfazImplementada in interfacesImplementadas)
            {
                // Solo validar interfaces que empiecen con "I" seguido de mayúscula
                if (!interfazImplementada.Nombre.StartsWith("I") || 
                    interfazImplementada.Nombre.Length < 2 ||
                    !char.IsUpper(interfazImplementada.Nombre[1]))
                    continue;

                var nombreClaseEsperado = interfazImplementada.Nombre.Substring(1); // Quitar la "I"

                // Verificar si el nombre de la clase corresponde
                if (!clase.Nombre.Equals(nombreClaseEsperado, System.StringComparison.Ordinal))
                {
                    var diagnostico = Diagnostic.Create(
                        ReglaClase,
                        clase.Sintaxis.Identifier.GetLocation(),
                        clase.Nombre,
                        nombreClaseEsperado,
                        interfazImplementada.Nombre);

                    contexto.ReportDiagnostic(diagnostico);
                }
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
                // Verificar si la clase implementa la interfaz usando análisis semántico
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

        private static bool EsClaseEspecial(ClaseInfo clase)
        {
            // Excluir clases del sistema
            if (clase.Namespace.StartsWith("System", System.StringComparison.OrdinalIgnoreCase) ||
                clase.Namespace.StartsWith("Microsoft", System.StringComparison.OrdinalIgnoreCase))
                return true;

            // Excluir clases abstractas
            if (clase.Sintaxis.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
                return true;

            // Excluir clases estáticas
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
                            "Controller", "ApiController", "Entity", "Table"
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

            // Excluir clases que heredan de otras clases (no solo implementan interfaces)
            if (clase.Sintaxis.BaseList != null)
            {
                var tieneClaseBase = clase.Sintaxis.BaseList.Types.Any(tipo =>
                {
                    var tipoString = tipo.ToString();
                    // Si no empieza con "I" y tiene mayúscula después, probablemente es una clase base
                    return !tipoString.StartsWith("I") || 
                           (tipoString.Length > 1 && !char.IsUpper(tipoString[1]));
                });

                if (tieneClaseBase)
                    return true;
            }

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
            public InterfaceDeclarationSyntax Sintaxis { get; set; }
            public INamedTypeSymbol Simbolo { get; set; }
        }

        private class ClaseInfo
        {
            public string Nombre { get; set; } = string.Empty;
            public string Namespace { get; set; } = string.Empty;
            public ClassDeclarationSyntax Sintaxis { get; set; }
            public INamedTypeSymbol Simbolo { get; set; }
        }
    }
}

