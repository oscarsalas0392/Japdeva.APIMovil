using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LimitarComplejidadCiclomatica : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA050";
        private const string Titulo = "Complejidad ciclomática excesiva";
        private const string FormatoMensaje = "El método '{0}' tiene una complejidad ciclomática de {1}. El máximo recomendado es {2}. Considere refactorizar dividiendo el método en funciones más pequeñas";
        private const string Descripcion = "Los métodos con alta complejidad ciclomática son difíciles de mantener, testear y tienen mayor probabilidad de contener defectos. Se recomienda mantener la complejidad por debajo de 10.";
        private const string Categoria = "Maintainability";
        
        private const int COMPLEJIDAD_MAXIMA_WARNING = 10;
        private const int COMPLEJIDAD_MAXIMA_ERROR = 20;

        private static readonly DiagnosticDescriptor ReglaWarning = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://en.wikipedia.org/wiki/Cyclomatic_complexity");

        private static readonly DiagnosticDescriptor ReglaError = new DiagnosticDescriptor(
            DiagnosticId + "_ERROR",
            "Complejidad ciclomática crítica",
            "El método '{0}' tiene una complejidad ciclomática crítica de {1}. Es obligatorio refactorizar este método",
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Métodos con complejidad ciclomática superior a 20 representan un riesgo crítico para la mantenibilidad y calidad del código.",
            helpLinkUri: "https://en.wikipedia.org/wiki/Cyclomatic_complexity");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(ReglaWarning, ReglaError);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarMetodo, SyntaxKind.MethodDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarConstructor, SyntaxKind.ConstructorDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarPropiedad, SyntaxKind.PropertyDeclaration);
        }

        private static void AnalizarMetodo(SyntaxNodeAnalysisContext contexto)
        {
            var metodo = (MethodDeclarationSyntax)contexto.Node;
            
            // Excluir métodos abstractos o sin cuerpo
            if (metodo.Body == null && metodo.ExpressionBody == null)
                return;

            var complejidad = CalcularComplejidadCiclomatica(metodo);
            ReportarSiEsNecesario(contexto, metodo.Identifier.GetLocation(), metodo.Identifier.Text, complejidad);
        }

        private static void AnalizarConstructor(SyntaxNodeAnalysisContext contexto)
        {
            var constructor = (ConstructorDeclarationSyntax)contexto.Node;
            
            if (constructor.Body == null && constructor.ExpressionBody == null)
                return;

            var complejidad = CalcularComplejidadCiclomatica(constructor);
            ReportarSiEsNecesario(contexto, constructor.Identifier.GetLocation(), constructor.Identifier.Text, complejidad);
        }

        private static void AnalizarPropiedad(SyntaxNodeAnalysisContext contexto)
        {
            var propiedad = (PropertyDeclarationSyntax)contexto.Node;
            
            // Solo analizar propiedades con getter/setter complejos
            if (propiedad.AccessorList != null)
            {
                foreach (var accessor in propiedad.AccessorList.Accessors)
                {
                    if (accessor.Body != null || accessor.ExpressionBody != null)
                    {
                        var complejidad = CalcularComplejidadCiclomatica(accessor);
                        if (complejidad > 1) // Solo reportar si tiene lógica
                        {
                            var nombreAccesor = $"{propiedad.Identifier.Text}.{accessor.Keyword.Text}";
                            ReportarSiEsNecesario(contexto, accessor.GetLocation(), nombreAccesor, complejidad);
                        }
                    }
                }
            }
        }

        private static int CalcularComplejidadCiclomatica(SyntaxNode nodo)
        {
            int complejidad = 1; // Complejidad base

            var descendientes = nodo.DescendantNodes();

            foreach (var descendiente in descendientes)
            {
                switch (descendiente.Kind())
                {
                    // Estructuras de control de flujo
                    case SyntaxKind.IfStatement:
                    case SyntaxKind.ElseClause:
                    case SyntaxKind.WhileStatement:
                    case SyntaxKind.ForStatement:
                    case SyntaxKind.ForEachStatement:
                    case SyntaxKind.DoStatement:
                    case SyntaxKind.SwitchSection:
                    case SyntaxKind.CatchClause:
                    case SyntaxKind.ConditionalExpression: // Operador ternario ? :
                    case SyntaxKind.CoalesceExpression:    // Operador ?? 
                        complejidad++;
                        break;

                    // Operadores lógicos
                    case SyntaxKind.LogicalAndExpression: // &&
                    case SyntaxKind.LogicalOrExpression:  // ||
                        complejidad++;
                        break;

                    // Expresiones de patrón (C# moderno)
                    case SyntaxKind.WhenClause: // when en switch
                        complejidad++;
                        break;
                }
            }

            return complejidad;
        }

        private static void ReportarSiEsNecesario(SyntaxNodeAnalysisContext contexto, Location ubicacion, string nombreMetodo, int complejidad)
        {
            if (complejidad >= COMPLEJIDAD_MAXIMA_ERROR)
            {
                var diagnostico = Diagnostic.Create(
                    ReglaError,
                    ubicacion,
                    nombreMetodo,
                    complejidad);
                contexto.ReportDiagnostic(diagnostico);
            }
            else if (complejidad > COMPLEJIDAD_MAXIMA_WARNING)
            {
                var diagnostico = Diagnostic.Create(
                    ReglaWarning,
                    ubicacion,
                    nombreMetodo,
                    complejidad,
                    COMPLEJIDAD_MAXIMA_WARNING);
                contexto.ReportDiagnostic(diagnostico);
            }
        }
    }
}
