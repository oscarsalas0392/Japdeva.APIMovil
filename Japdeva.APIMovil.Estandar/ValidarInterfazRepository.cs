using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarInterfazRepository : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA029";

        private const string Titulo = "Clase Repository debe implementar una interfaz";
        private const string FormatoMensaje = "La clase '{0}' que termina en 'Repository' debe implementar una interfaz (ejemplo: '{1}')";
        private const string Descripcion = "Las clases que terminan en 'Repository' deben implementar una interfaz para seguir el principio de inversión de dependencias, facilitar las pruebas unitarias y mejorar la mantenibilidad del código.";
        private const string Categoria = "Design";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design");

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

            // Verificar si es una clase Repository
            if (!EsClaseRepository(nombreClase))
                return;

            // Validar que implemente una interfaz
            ValidarImplementacionInterfaz(contexto, clase, nombreClase);
        }

        private static bool EsClaseRepository(string nombreClase)
        {
            // Verificar si el nombre termina en "Repository"
            return nombreClase.EndsWith("Repository", System.StringComparison.Ordinal);
        }

        private static void ValidarImplementacionInterfaz(SyntaxNodeAnalysisContext contexto, 
            ClassDeclarationSyntax clase, string nombreClase)
        {
            if (!EsClaseRepositoryEspecial(nombreClase, clase) && 
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

        private static bool EsClaseRepositoryEspecial(string nombreClase, ClassDeclarationSyntax clase)
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
                "RepositoryBase", "BaseRepository", "GenericRepository",
                "AbstractRepository", "RepositoryFoundation"
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
                            "Entity", "Table", "DbContext"
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

            // Excluir clases que heredan de otras clases Repository (pueden ser especializaciones)
            if (HeredaDeClaseRepository(clase))
                return true;

            // Excluir contextos de Entity Framework que terminan en Repository
            if (EsContextoEntityFramework(clase, nombreClase))
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

        private static bool HeredaDeClaseRepository(ClassDeclarationSyntax clase)
        {
            if (clase.BaseList == null)
                return false;

            // Verificar si hereda de otra clase Repository (no interfaz)
            return clase.BaseList.Types.Any(tipo =>
            {
                var tipoString = tipo.ToString();
                
                // Clases base conocidas de repositories
                var clasesBaseRepository = new[]
                {
                    "RepositoryBase", "BaseRepository", "GenericRepository",
                    "AbstractRepository", "EntityRepository", "DbRepository"
                };

                return clasesBaseRepository.Any(claseBase => 
                    tipoString.IndexOf(claseBase, System.StringComparison.OrdinalIgnoreCase) != -1) ||
                    // O cualquier clase que termine en "Repository" (heurística)
                    (tipoString.EndsWith("Repository", System.StringComparison.OrdinalIgnoreCase) &&
                     !tipoString.StartsWith("I"));
            });
        }

        private static bool EsContextoEntityFramework(ClassDeclarationSyntax clase, string nombreClase)
        {
            // Verificar si hereda de DbContext
            if (clase.BaseList != null)
            {
                var heredaDeDbContext = clase.BaseList.Types.Any(tipo =>
                {
                    var tipoString = tipo.ToString();
                    return tipoString.IndexOf("DbContext", System.StringComparison.OrdinalIgnoreCase) != -1;
                });

                if (heredaDeDbContext)
                    return true;
            }

            // Verificar si tiene características de DbContext
            var propiedadesDbSet = clase.Members.OfType<PropertyDeclarationSyntax>()
                .Where(prop => prop.Type.ToString().Contains("DbSet"));

            return propiedadesDbSet.Any();
        }

        private static string ObtenerNombreInterfazSugerida(string nombreClase)
        {
            // Si la clase termina en "Repository", crear interfaz con el mismo nombre
            if (nombreClase.EndsWith("Repository", System.StringComparison.Ordinal))
            {
                return "I" + nombreClase;
            }

            // Si no termina en Repository (caso raro), agregar "Repository" e interfaz
            return "I" + nombreClase + "Repository";
        }
    }
}

