using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarClasesExtensionsEstaticas : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA021";

        private const string Titulo = "Clase Extensions debe ser estática";
        private const string FormatoMensaje = "La clase '{0}' en la carpeta Extensions debe ser estática para contener métodos de extensión";
        private const string Descripcion = "Las clases de extensión deben ser estáticas para poder contener métodos de extensión que amplíen la funcionalidad de tipos existentes.";
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/extension-methods");

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

            // Verificar si la clase está en la carpeta Extensions
            if (!EstaEnCarpetaExtensions(contexto))
                return;

            // Validar que sea estática
            ValidarClaseEstatica(contexto, clase, nombreClase);
        }

        private static bool EstaEnCarpetaExtensions(SyntaxNodeAnalysisContext contexto)
        {
            var rutaArchivo = contexto.Node.SyntaxTree.FilePath;
            
            if (string.IsNullOrEmpty(rutaArchivo))
                return false;

            // Normalizar la ruta para comparación
            var rutaNormalizada = rutaArchivo.Replace('\\', '/');
            
            // Verificar si contiene "Extensions" en la ruta
            var partesRuta = rutaNormalizada.Split('/');
            
            return partesRuta.Any(parte => 
                parte.Equals("Extensions", System.StringComparison.OrdinalIgnoreCase) ||
                parte.Equals("Extension", System.StringComparison.OrdinalIgnoreCase));
        }

        private static void ValidarClaseEstatica(SyntaxNodeAnalysisContext contexto, 
            ClassDeclarationSyntax clase, string nombreClase)
        {
            if (!EsClaseExtensionEspecial(nombreClase, clase) && 
                !clase.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
            {
                var diagnostico = Diagnostic.Create(
                    Regla,
                    clase.Identifier.GetLocation(),
                    nombreClase);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static bool EsClaseExtensionEspecial(string nombreClase, ClassDeclarationSyntax clase)
        {
            // Excluir clases que ya son estáticas
            if (clase.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
                return true;

            // Excluir clases abstractas (pueden ser clases base)
            if (clase.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
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
                            "CompilerGenerated", "DebuggerNonUserCode"
                        };

                        if (atributosEspeciales.Any(a => 
                            nombreAtributoClase.IndexOf(a, System.StringComparison.OrdinalIgnoreCase) != -1))
                        {
                            return true;
                        }
                    }
                }
            }

            // Excluir interfaces (no pueden ser estáticas)
            if (clase.Parent is InterfaceDeclarationSyntax)
                return true;

            // Excluir clases anidadas que pueden tener propósitos especiales
            if (EsClaseAnidada(clase))
                return true;

            // Excluir clases internas que pueden ser utilities
            if (EsClaseInterna(clase))
                return true;

            // Excluir clases que contienen métodos de extensión pero no siguen la convención estricta
            if (ContieneMetodosExtension(clase))
                return true;

            return false;
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

        private static bool ContieneMetodosExtension(ClassDeclarationSyntax clase)
        {
            // Verificar si la clase contiene métodos de extensión
            return clase.Members.OfType<MethodDeclarationSyntax>().Any(metodo =>
            {
                // Un método de extensión debe ser estático y tener al menos un parámetro con 'this'
                var esEstatico = metodo.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
                if (!esEstatico)
                    return false;

                var parametros = metodo.ParameterList.Parameters;
                if (!parametros.Any())
                    return false;

                var primerParametro = parametros.First();
                return primerParametro.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword));
            });
        }
    }
}

