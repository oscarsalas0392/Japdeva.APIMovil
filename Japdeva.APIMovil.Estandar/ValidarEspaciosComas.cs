using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarEspaciosComas : DiagnosticAnalyzer
    {
        public const string DiagnosticIdSinEspacio = "JAPDEVA041";
        public const string DiagnosticIdEspaciosExcesivos = "JAPDEVA042";
        
        private const string TituloSinEspacio = "Falta espacio después de la coma";
        private const string FormatoMensajeSinEspacio = "Debe haber exactamente un espacio después de la coma en la posición {0}";
        private const string DescripcionSinEspacio = "Las comas en listas de parámetros, argumentos y elementos deben ir seguidas de exactamente un espacio para mejorar la legibilidad.";
        
        private const string TituloEspaciosExcesivos = "Espacios excesivos después de la coma";
        private const string FormatoMensajeEspaciosExcesivos = "Debe haber exactamente un espacio después de la coma, se encontraron {0} espacios en la posición {1}";
        private const string DescripcionEspaciosExcesivos = "Las comas en listas de parámetros, argumentos y elementos deben ir seguidas de exactamente un espacio, no múltiples espacios.";
        
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor ReglaSinEspacio = new DiagnosticDescriptor(
            DiagnosticIdSinEspacio,
            TituloSinEspacio,
            FormatoMensajeSinEspacio,
            Categoria,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: DescripcionSinEspacio,
            helpLinkUri: "https://docs.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        private static readonly DiagnosticDescriptor ReglaEspaciosExcesivos = new DiagnosticDescriptor(
            DiagnosticIdEspaciosExcesivos,
            TituloEspaciosExcesivos,
            FormatoMensajeEspaciosExcesivos,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: DescripcionEspaciosExcesivos,
            helpLinkUri: "https://docs.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(ReglaSinEspacio, ReglaEspaciosExcesivos);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarDeclaracionMetodo, SyntaxKind.MethodDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarDeclaracionConstructor, SyntaxKind.ConstructorDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarLlamadaMetodo, SyntaxKind.InvocationExpression);
            contexto.RegisterSyntaxNodeAction(AnalizarCreacionObjeto, SyntaxKind.ObjectCreationExpression);
            contexto.RegisterSyntaxNodeAction(AnalizarExpresionCreacionImplicita, SyntaxKind.ImplicitObjectCreationExpression);
            contexto.RegisterSyntaxNodeAction(AnalizarInicializadorArray, SyntaxKind.ArrayInitializerExpression);
            contexto.RegisterSyntaxNodeAction(AnalizarInicializadorColeccion, SyntaxKind.CollectionInitializerExpression);
            contexto.RegisterSyntaxNodeAction(AnalizarListaParametrosTipo, SyntaxKind.TypeParameterList);
            contexto.RegisterSyntaxNodeAction(AnalizarListaArgumentosTipo, SyntaxKind.TypeArgumentList);
        }

        private static void AnalizarDeclaracionMetodo(SyntaxNodeAnalysisContext contexto)
        {
            var metodo = (MethodDeclarationSyntax)contexto.Node;
            if (metodo.ParameterList?.Parameters.Count > 1)
            {
                AnalizarListaParametros(contexto, metodo.ParameterList.Parameters);
            }
        }

        private static void AnalizarDeclaracionConstructor(SyntaxNodeAnalysisContext contexto)
        {
            var constructor = (ConstructorDeclarationSyntax)contexto.Node;
            if (constructor.ParameterList?.Parameters.Count > 1)
            {
                AnalizarListaParametros(contexto, constructor.ParameterList.Parameters);
            }
        }

        private static void AnalizarLlamadaMetodo(SyntaxNodeAnalysisContext contexto)
        {
            var llamada = (InvocationExpressionSyntax)contexto.Node;
            if (llamada.ArgumentList?.Arguments.Count > 1)
            {
                AnalizarListaArgumentos(contexto, llamada.ArgumentList.Arguments);
            }
        }

        private static void AnalizarCreacionObjeto(SyntaxNodeAnalysisContext contexto)
        {
            var creacion = (ObjectCreationExpressionSyntax)contexto.Node;
            if (creacion.ArgumentList?.Arguments.Count > 1)
            {
                AnalizarListaArgumentos(contexto, creacion.ArgumentList.Arguments);
            }
        }

        private static void AnalizarExpresionCreacionImplicita(SyntaxNodeAnalysisContext contexto)
        {
            var creacion = (ImplicitObjectCreationExpressionSyntax)contexto.Node;
            if (creacion.ArgumentList?.Arguments.Count > 1)
            {
                AnalizarListaArgumentos(contexto, creacion.ArgumentList.Arguments);
            }
        }

        private static void AnalizarInicializadorArray(SyntaxNodeAnalysisContext contexto)
        {
            var inicializador = (InitializerExpressionSyntax)contexto.Node;
            if (inicializador.Expressions.Count > 1)
            {
                AnalizarListaExpresiones(contexto, inicializador.Expressions);
            }
        }

        private static void AnalizarInicializadorColeccion(SyntaxNodeAnalysisContext contexto)
        {
            var inicializador = (InitializerExpressionSyntax)contexto.Node;
            if (inicializador.Expressions.Count > 1)
            {
                AnalizarListaExpresiones(contexto, inicializador.Expressions);
            }
        }

        private static void AnalizarListaParametrosTipo(SyntaxNodeAnalysisContext contexto)
        {
            var lista = (TypeParameterListSyntax)contexto.Node;
            if (lista.Parameters.Count > 1)
            {
                AnalizarListaParametrosTipoGenerico(contexto, lista.Parameters);
            }
        }

        private static void AnalizarListaArgumentosTipo(SyntaxNodeAnalysisContext contexto)
        {
            var lista = (TypeArgumentListSyntax)contexto.Node;
            if (lista.Arguments.Count > 1)
            {
                AnalizarListaArgumentosTipoGenerico(contexto, lista.Arguments);
            }
        }

        private static void AnalizarListaParametros<T>(SyntaxNodeAnalysisContext contexto, SeparatedSyntaxList<T> elementos) where T : SyntaxNode
        {
            for (int i = 0; i < elementos.SeparatorCount; i++)
            {
                var separador = elementos.GetSeparator(i);
                var siguienteElemento = elementos[i + 1];
                
                AnalizarEspacioDespuesComa(contexto, separador, siguienteElemento);
            }
        }

        private static void AnalizarListaArgumentos<T>(SyntaxNodeAnalysisContext contexto, SeparatedSyntaxList<T> elementos) where T : SyntaxNode
        {
            for (int i = 0; i < elementos.SeparatorCount; i++)
            {
                var separador = elementos.GetSeparator(i);
                var siguienteElemento = elementos[i + 1];
                
                AnalizarEspacioDespuesComa(contexto, separador, siguienteElemento);
            }
        }

        private static void AnalizarListaExpresiones(SyntaxNodeAnalysisContext contexto, SeparatedSyntaxList<ExpressionSyntax> elementos)
        {
            for (int i = 0; i < elementos.SeparatorCount; i++)
            {
                var separador = elementos.GetSeparator(i);
                var siguienteElemento = elementos[i + 1];
                
                AnalizarEspacioDespuesComa(contexto, separador, siguienteElemento);
            }
        }

        private static void AnalizarListaParametrosTipoGenerico(SyntaxNodeAnalysisContext contexto, SeparatedSyntaxList<TypeParameterSyntax> elementos)
        {
            for (int i = 0; i < elementos.SeparatorCount; i++)
            {
                var separador = elementos.GetSeparator(i);
                var siguienteElemento = elementos[i + 1];
                
                AnalizarEspacioDespuesComa(contexto, separador, siguienteElemento);
            }
        }

        private static void AnalizarListaArgumentosTipoGenerico(SyntaxNodeAnalysisContext contexto, SeparatedSyntaxList<TypeSyntax> elementos)
        {
            for (int i = 0; i < elementos.SeparatorCount; i++)
            {
                var separador = elementos.GetSeparator(i);
                var siguienteElemento = elementos[i + 1];
                
                AnalizarEspacioDespuesComa(contexto, separador, siguienteElemento);
            }
        }

        private static void AnalizarEspacioDespuesComa(SyntaxNodeAnalysisContext contexto, SyntaxToken coma, SyntaxNode siguienteElemento)
        {
            // Obtener el trivia (espacios en blanco) después de la coma
            var triviaPostComa = coma.TrailingTrivia;
            
            // Contar espacios en blanco
            var espacios = 0;
            var hayEspacios = false;
            
            foreach (var trivia in triviaPostComa)
            {
                if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    hayEspacios = true;
                    espacios += trivia.ToString().Length;
                }
                else if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    // Si hay salto de línea después de la coma, no aplica la regla
                    return;
                }
            }

            // Si no hay espacios después de la coma
            if (!hayEspacios || espacios == 0)
            {
                var ubicacionComa = coma.GetLocation();
                var posicion = ubicacionComa.GetLineSpan().StartLinePosition;
                
                var diagnostico = Diagnostic.Create(
                    ReglaSinEspacio,
                    ubicacionComa,
                    $"línea {posicion.Line + 1}, columna {posicion.Character + 1}");

                contexto.ReportDiagnostic(diagnostico);
            }
            // Si hay más de un espacio después de la coma
            else if (espacios > 1)
            {
                var ubicacionComa = coma.GetLocation();
                var posicion = ubicacionComa.GetLineSpan().StartLinePosition;
                
                var diagnostico = Diagnostic.Create(
                    ReglaEspaciosExcesivos,
                    ubicacionComa,
                    espacios,
                    $"línea {posicion.Line + 1}, columna {posicion.Character + 1}");

                contexto.ReportDiagnostic(diagnostico);
            }
            // Si hay exactamente un espacio, está correcto, no reportar nada
        }
    }
}
