using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarDocumentacionXML : DiagnosticAnalyzer
    {
        public const string DiagnosticIdClase = "JAPDEVA044";
        public const string DiagnosticIdInterfaz = "JAPDEVA045";
        public const string DiagnosticIdMetodo = "JAPDEVA046";
        public const string DiagnosticIdPropiedad = "JAPDEVA047";
        public const string DiagnosticIdConstructor = "JAPDEVA048";

        private const string TituloClase = "Clase pública sin documentación XML";
        private const string TituloInterfaz = "Interfaz pública sin documentación XML";
        private const string TituloMetodo = "Método público sin documentación XML";
        private const string TituloPropiedad = "Propiedad pública sin documentación XML";
        private const string TituloConstructor = "Constructor público sin documentación XML";

        private const string FormatoMensajeClase = "La clase pública '{0}' debe tener documentación XML con <summary>";
        private const string FormatoMensajeInterfaz = "La interfaz pública '{0}' debe tener documentación XML con <summary>";
        private const string FormatoMensajeMetodo = "El método público '{0}' debe tener documentación XML con <summary>";
        private const string FormatoMensajePropiedad = "La propiedad pública '{0}' debe tener documentación XML con <summary>";
        private const string FormatoMensajeConstructor = "El constructor público '{0}' debe tener documentación XML con <summary>";

        private const string Descripcion = "Los miembros públicos deben estar documentados con comentarios XML para mejorar la comprensión y facilitar la generación de documentación automática.";
        private const string Categoria = "Documentation";

        private static readonly DiagnosticDescriptor ReglaClase = new DiagnosticDescriptor(
            DiagnosticIdClase,
            TituloClase,
            FormatoMensajeClase,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/csharp/codedoc",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        private static readonly DiagnosticDescriptor ReglaInterfaz = new DiagnosticDescriptor(
            DiagnosticIdInterfaz,
            TituloInterfaz,
            FormatoMensajeInterfaz,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/csharp/codedoc",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        private static readonly DiagnosticDescriptor ReglaMetodo = new DiagnosticDescriptor(
            DiagnosticIdMetodo,
            TituloMetodo,
            FormatoMensajeMetodo,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/csharp/codedoc",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        private static readonly DiagnosticDescriptor ReglaPropiedad = new DiagnosticDescriptor(
            DiagnosticIdPropiedad,
            TituloPropiedad,
            FormatoMensajePropiedad,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/csharp/codedoc",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        private static readonly DiagnosticDescriptor ReglaConstructor = new DiagnosticDescriptor(
            DiagnosticIdConstructor,
            TituloConstructor,
            FormatoMensajeConstructor,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/csharp/codedoc",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(ReglaClase, ReglaInterfaz, ReglaMetodo, ReglaPropiedad, ReglaConstructor);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarClase, SyntaxKind.ClassDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarInterfaz, SyntaxKind.InterfaceDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarMetodo, SyntaxKind.MethodDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarPropiedad, SyntaxKind.PropertyDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarConstructor, SyntaxKind.ConstructorDeclaration);
        }

        private static void AnalizarClase(SyntaxNodeAnalysisContext contexto)
        {
            var clase = (ClassDeclarationSyntax)contexto.Node;
            
            if (EsPublico(clase.Modifiers) && !TieneDocumentacionXML(clase))
            {
                var diagnostico = Diagnostic.Create(
                    ReglaClase,
                    clase.Identifier.GetLocation(),
                    clase.Identifier.Text);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static void AnalizarInterfaz(SyntaxNodeAnalysisContext contexto)
        {
            var interfaz = (InterfaceDeclarationSyntax)contexto.Node;
            
            if (EsPublico(interfaz.Modifiers) && !TieneDocumentacionXML(interfaz))
            {
                var diagnostico = Diagnostic.Create(
                    ReglaInterfaz,
                    interfaz.Identifier.GetLocation(),
                    interfaz.Identifier.Text);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static void AnalizarMetodo(SyntaxNodeAnalysisContext contexto)
        {
            var metodo = (MethodDeclarationSyntax)contexto.Node;
            
            if (EsPublico(metodo.Modifiers) && !TieneDocumentacionXML(metodo) && !EsMetodoEspecial(metodo))
            {
                var diagnostico = Diagnostic.Create(
                    ReglaMetodo,
                    metodo.Identifier.GetLocation(),
                    metodo.Identifier.Text);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static void AnalizarPropiedad(SyntaxNodeAnalysisContext contexto)
        {
            var propiedad = (PropertyDeclarationSyntax)contexto.Node;
            
            if (EsPublico(propiedad.Modifiers) && !TieneDocumentacionXML(propiedad))
            {
                var diagnostico = Diagnostic.Create(
                    ReglaPropiedad,
                    propiedad.Identifier.GetLocation(),
                    propiedad.Identifier.Text);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static void AnalizarConstructor(SyntaxNodeAnalysisContext contexto)
        {
            var constructor = (ConstructorDeclarationSyntax)contexto.Node;
            
            if (EsPublico(constructor.Modifiers) && !TieneDocumentacionXML(constructor))
            {
                var diagnostico = Diagnostic.Create(
                    ReglaConstructor,
                    constructor.Identifier.GetLocation(),
                    constructor.Identifier.Text);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static bool EsPublico(SyntaxTokenList modifiers)
        {
            return modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        }

        private static bool TieneDocumentacionXML(SyntaxNode nodo)
        {
            // Obtener el trivia (comentarios) que precede al nodo
            var leadingTrivia = nodo.GetLeadingTrivia();

            // Buscar comentarios de documentación XML
            foreach (var trivia in leadingTrivia)
            {
                if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                    trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                {
                    var textoDocumentacion = trivia.ToString();
                    
                    // Verificar que contiene al menos un tag <summary>
                    if (textoDocumentacion.Contains("<summary>") || 
                        textoDocumentacion.Contains("<Summary>"))
                    {
                        return true;
                    }
                }
            }

            // También revisar si hay comentarios XML de una línea (///)
            var triviaCompleto = nodo.GetLeadingTrivia().ToFullString();
            if (triviaCompleto.Contains("/// <summary>") || 
                triviaCompleto.Contains("///<summary>") ||
                triviaCompleto.Contains("/// <Summary>") ||
                triviaCompleto.Contains("///<Summary>"))
            {
                return true;
            }

            return false;
        }

        private static bool EsMetodoEspecial(MethodDeclarationSyntax metodo)
        {
            // Excluir ciertos métodos especiales que pueden no necesitar documentación
            var nombreMetodo = metodo.Identifier.Text;
            
            // Métodos override pueden heredar documentación
            if (metodo.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)))
            {
                return true;
            }

            // Métodos de interfaces implementadas pueden no necesitar documentación redundante
            // (esto requeriría análisis semántico más complejo, por ahora se omite)
            
            // Métodos con atributos especiales (como test methods)
            if (metodo.AttributeLists.Count > 0)
            {
                foreach (var attributeList in metodo.AttributeLists)
                {
                    foreach (var attribute in attributeList.Attributes)
                    {
                        var nombreAtributo = attribute.Name.ToString();
                        if (nombreAtributo.Contains("Test") || 
                            nombreAtributo.Contains("Fact") || 
                            nombreAtributo.Contains("Theory") ||
                            nombreAtributo.Contains("HttpGet") ||
                            nombreAtributo.Contains("HttpPost") ||
                            nombreAtributo.Contains("HttpPut") ||
                            nombreAtributo.Contains("HttpDelete") ||
                            nombreAtributo.Contains("Route"))
                        {
                            // Los métodos de API pueden requerir documentación, pero ser más flexibles
                            // Por ahora no los excluimos completamente
                            break;
                        }
                    }
                }
            }

            return false;
        }
    }
}
