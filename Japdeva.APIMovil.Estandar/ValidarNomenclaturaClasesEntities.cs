using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ValidarNomenclaturaClasesEntities : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
        "JAPDEVA091",
        "Las clases en carpetas 'Entities' deben terminar en 'Entity'",
        "La clase '{0}' está en una carpeta 'Entities' pero no termina en 'Entity'. Nombre sugerido: '{1}'",
        "Nomenclatura",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Las clases ubicadas en carpetas llamadas 'Entities' deben seguir la convención de nomenclatura terminando en 'Entity' para mantener consistencia en el proyecto.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Regla);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var className = classDeclaration.Identifier.ValueText;

        // Obtener la ruta del archivo
        var filePath = context.Node.SyntaxTree.FilePath;
        if (string.IsNullOrEmpty(filePath))
            return;

        // Verificar si está en una carpeta "Entities"
        if (!EstaEnCarpetaEntities(filePath))
            return;

        // Excluir clases especiales
        if (EsClaseEspecial(classDeclaration, className))
            return;

        // Validar nomenclatura
        ValidarNomenclaturaClaseEntity(context, classDeclaration, className);
    }

    private static bool EstaEnCarpetaEntities(string rutaArchivo)
    {
        if (string.IsNullOrEmpty(rutaArchivo))
            return false;

        // Normalizar la ruta para comparación
        var rutaNormalizada = rutaArchivo.Replace('\\', '/');
        
        // Verificar si contiene "Entities" en la ruta
        var partesRuta = rutaNormalizada.Split('/');
        
        return partesRuta.Any(parte => 
            parte.Equals("Entities", System.StringComparison.OrdinalIgnoreCase));
    }

    private static bool EsClaseEspecial(ClassDeclarationSyntax clase, string nombreClase)
    {
        // Excluir clases que ya terminan en "Entity"
        if (nombreClase.EndsWith("Entity", System.StringComparison.Ordinal))
            return true;

        // Excluir clases abstractas base
        if (clase.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
        {
            // Permitir clases base especiales
            if (nombreClase.EndsWith("EntityBase", System.StringComparison.Ordinal) ||
                nombreClase.EndsWith("BaseEntity", System.StringComparison.Ordinal) ||
                nombreClase.StartsWith("Base", System.StringComparison.Ordinal) ||
                nombreClase.StartsWith("Abstract", System.StringComparison.Ordinal))
                return true;
        }

        // Excluir clases estáticas (utilities)
        if (clase.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
            return true;

        // Excluir interfaces
        if (clase.Modifiers.Any(m => m.IsKind(SyntaxKind.InterfaceKeyword)))
            return true;

        // Excluir clases con atributos especiales
        var atributos = clase.AttributeLists.SelectMany(al => al.Attributes);
        foreach (var atributo in atributos)
        {
            var nombreAtributo = atributo.Name.ToString();
            // Excluir clases con atributos de Entity Framework u ORM
            if (nombreAtributo.Contains("Table") ||
                nombreAtributo.Contains("Entity") ||
                nombreAtributo.Contains("DbContext") ||
                nombreAtributo.Contains("Keyless"))
            {
                return true;
            }
        }

        return false;
    }

    private static void ValidarNomenclaturaClaseEntity(SyntaxNodeAnalysisContext contexto, 
        ClassDeclarationSyntax clase, string nombreClase)
    {
        if (!EsNombrePascalCase(nombreClase) || !nombreClase.EndsWith("Entity", System.StringComparison.Ordinal))
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

    private static bool EsNombrePascalCase(string nombre)
    {
        if (string.IsNullOrEmpty(nombre))
            return false;

        // Debe empezar con mayúscula
        if (!char.IsUpper(nombre[0]))
            return false;

        // No debe tener espacios, guiones o caracteres especiales
        return nombre.All(c => char.IsLetterOrDigit(c));
    }

    private static string ObtenerNombreSugerido(string nombreClase)
    {
        // Si ya termina en "Entity", solo corregir PascalCase
        if (nombreClase.EndsWith("Entity", System.StringComparison.OrdinalIgnoreCase))
        {
            return CorregirPascalCase(nombreClase);
        }

        // Agregar "Entity" al final
        var nombreCorregido = CorregirPascalCase(nombreClase);
        
        // Evitar duplicación como "EntityEntity"
        if (nombreCorregido.EndsWith("Entity", System.StringComparison.OrdinalIgnoreCase))
        {
            return nombreCorregido;
        }

        return nombreCorregido + "Entity";
    }

    private static string CorregirPascalCase(string nombre)
    {
        if (string.IsNullOrEmpty(nombre))
            return nombre;

        // Convertir primera letra a mayúscula
        return char.ToUpper(nombre[0]) + nombre.Substring(1);
    }
}