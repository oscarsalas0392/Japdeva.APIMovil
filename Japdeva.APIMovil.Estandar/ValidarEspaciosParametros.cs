using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarEspaciosParametros : DiagnosticAnalyzer
    {
        public const string DiagnosticIdDeclaracion = "JAPDEVA036";
        public const string DiagnosticIdLlamada = "JAPDEVA037";

        private const string TituloDeclaracion = "Falta espacio después de la coma en parámetros de método";
        private const string FormatoMensajeDeclaracion = "Debe haber un espacio después de la coma en la declaración de parámetros";
        private const string DescripcionDeclaracion = "Los parámetros de métodos deben tener un espacio después de cada coma para mejorar la legibilidad del código.";

        private const string TituloLlamada = "Falta espacio después de la coma en argumentos de método";
        private const string FormatoMensajeLlamada = "Debe haber un espacio después de la coma en los argumentos del método";
        private const string DescripcionLlamada = "Los argumentos de métodos deben tener un espacio después de cada coma para mejorar la legibilidad del código.";

        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor ReglaDeclaracion = new DiagnosticDescriptor(
            DiagnosticIdDeclaracion,
            TituloDeclaracion,
            FormatoMensajeDeclaracion,
            Categoria,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: DescripcionDeclaracion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0055");

        private static readonly DiagnosticDescriptor ReglaLlamada = new DiagnosticDescriptor(
            DiagnosticIdLlamada,
            TituloLlamada,
            FormatoMensajeLlamada,
            Categoria,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: DescripcionLlamada,
            helpLinkUri: "https://docs.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0055");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(ReglaDeclaracion, ReglaLlamada);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            
            // Registrar análisis para declaraciones de métodos
            contexto.RegisterSyntaxNodeAction(AnalizarMetodo, SyntaxKind.MethodDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarConstructor, SyntaxKind.ConstructorDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarIndexer, SyntaxKind.IndexerDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarDelegate, SyntaxKind.DelegateDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarFuncionLocal, SyntaxKind.LocalFunctionStatement);
            
            // Registrar análisis para llamadas a métodos
            contexto.RegisterSyntaxNodeAction(AnalizarLlamadaMetodo, SyntaxKind.InvocationExpression);
            contexto.RegisterSyntaxNodeAction(AnalizarCreacionObjeto, SyntaxKind.ObjectCreationExpression);
            contexto.RegisterSyntaxNodeAction(AnalizarLlamadaElementAccess, SyntaxKind.ElementAccessExpression);
            contexto.RegisterSyntaxNodeAction(AnalizarCreacionArray, SyntaxKind.ArrayCreationExpression);
            contexto.RegisterSyntaxNodeAction(AnalizarCreacionImplicita, SyntaxKind.ImplicitArrayCreationExpression);
        }

        #region Análisis de Declaraciones

        private static void AnalizarMetodo(SyntaxNodeAnalysisContext contexto)
        {
            var metodo = (MethodDeclarationSyntax)contexto.Node;
            ValidarEspaciosEnParametros(contexto, metodo.ParameterList.Parameters);
        }

        private static void AnalizarConstructor(SyntaxNodeAnalysisContext contexto)
        {
            var constructor = (ConstructorDeclarationSyntax)contexto.Node;
            ValidarEspaciosEnParametros(contexto, constructor.ParameterList.Parameters);
        }

        private static void AnalizarIndexer(SyntaxNodeAnalysisContext contexto)
        {
            var indexer = (IndexerDeclarationSyntax)contexto.Node;
            ValidarEspaciosEnParametros(contexto, indexer.ParameterList.Parameters);
        }

        private static void AnalizarDelegate(SyntaxNodeAnalysisContext contexto)
        {
            var delegateDeclaration = (DelegateDeclarationSyntax)contexto.Node;
            ValidarEspaciosEnParametros(contexto, delegateDeclaration.ParameterList.Parameters);
        }

        private static void AnalizarFuncionLocal(SyntaxNodeAnalysisContext contexto)
        {
            var funcionLocal = (LocalFunctionStatementSyntax)contexto.Node;
            ValidarEspaciosEnParametros(contexto, funcionLocal.ParameterList.Parameters);
        }

        private static void ValidarEspaciosEnParametros(SyntaxNodeAnalysisContext contexto, SeparatedSyntaxList<ParameterSyntax> parametros)
        {
            if (parametros.Count <= 1)
                return;

            var textoFuente = contexto.Node.SyntaxTree.GetText(contexto.CancellationToken);

            for (int i = 0; i < parametros.SeparatorCount; i++)
            {
                var separador = parametros.GetSeparator(i);
                if (separador.IsKind(SyntaxKind.CommaToken))
                {
                    ValidarEspacioDespuesComa(contexto, separador, textoFuente, ReglaDeclaracion);
                }
            }
        }

        #endregion

        #region Análisis de Llamadas

        private static void AnalizarLlamadaMetodo(SyntaxNodeAnalysisContext contexto)
        {
            var llamada = (InvocationExpressionSyntax)contexto.Node;
            ValidarEspaciosEnArgumentos(contexto, llamada.ArgumentList);
        }

        private static void AnalizarCreacionObjeto(SyntaxNodeAnalysisContext contexto)
        {
            var creacion = (ObjectCreationExpressionSyntax)contexto.Node;
            ValidarEspaciosEnArgumentos(contexto, creacion.ArgumentList);
        }

        private static void AnalizarLlamadaElementAccess(SyntaxNodeAnalysisContext contexto)
        {
            var elementAccess = (ElementAccessExpressionSyntax)contexto.Node;

            // Convert BracketedArgumentListSyntax to SeparatedSyntaxList<ArgumentSyntax>
            var argumentos = elementAccess.ArgumentList.Arguments;

            // Convert SeparatedSyntaxList<ArgumentSyntax> to SeparatedSyntaxList<ExpressionSyntax>
            var expresiones = SyntaxFactory.SeparatedList(argumentos.Select(arg => arg.Expression));

            // Validate spaces in the expressions
            ValidarEspaciosEnListaExpresiones(contexto, expresiones);
        }

        private static void AnalizarCreacionArray(SyntaxNodeAnalysisContext contexto)
        {
            var arrayCreation = (ArrayCreationExpressionSyntax)contexto.Node;
            
            // Validar argumentos en las dimensiones del array
            if (arrayCreation.Type.RankSpecifiers.Any())
            {
                foreach (var rankSpecifier in arrayCreation.Type.RankSpecifiers)
                {
                    ValidarEspaciosEnListaExpresiones(contexto, rankSpecifier.Sizes);
                }
            }

            // Validar inicializador si existe
            if (arrayCreation.Initializer != null)
            {
                ValidarEspaciosEnInicializador(contexto, arrayCreation.Initializer);
            }
        }

        private static void AnalizarCreacionImplicita(SyntaxNodeAnalysisContext contexto)
        {
            var implicitArray = (ImplicitArrayCreationExpressionSyntax)contexto.Node;
            
            if (implicitArray.Initializer != null)
            {
                ValidarEspaciosEnInicializador(contexto, implicitArray.Initializer);
            }
        }

        private static void ValidarEspaciosEnArgumentos(SyntaxNodeAnalysisContext contexto, ArgumentListSyntax argumentos)
        {
            if (argumentos == null || argumentos.Arguments.Count <= 1)
                return;

            var textoFuente = argumentos.SyntaxTree.GetText(contexto.CancellationToken);

            for (int i = 0; i < argumentos.Arguments.Count - 1; i++)
            {
                var argumentoActual = argumentos.Arguments[i];
                var siguienteArgumento = argumentos.Arguments[i + 1];

                // Buscar la coma entre los argumentos
                var tokenComa = argumentoActual.GetLastToken().GetNextToken();
                while (tokenComa.Span.End <= siguienteArgumento.Span.Start && !tokenComa.IsKind(SyntaxKind.CommaToken))
                {
                    tokenComa = tokenComa.GetNextToken();
                }

                if (tokenComa.IsKind(SyntaxKind.CommaToken))
                {
                    ValidarEspacioDespuesComa(contexto, tokenComa, textoFuente, ReglaLlamada);
                }
            }
        }

        private static void ValidarEspaciosEnInicializador(SyntaxNodeAnalysisContext contexto, InitializerExpressionSyntax inicializador)
        {
            if (inicializador.Expressions.Count <= 1)
                return;

            var textoFuente = inicializador.SyntaxTree.GetText(contexto.CancellationToken);

            for (int i = 0; i < inicializador.Expressions.Count - 1; i++)
            {
                var expresionActual = inicializador.Expressions[i];
                var siguienteExpresion = inicializador.Expressions[i + 1];

                // Buscar la coma entre las expresiones
                var tokenComa = expresionActual.GetLastToken().GetNextToken();
                while (tokenComa.Span.End <= siguienteExpresion.Span.Start && !tokenComa.IsKind(SyntaxKind.CommaToken))
                {
                    tokenComa = tokenComa.GetNextToken();
                }

                if (tokenComa.IsKind(SyntaxKind.CommaToken))
                {
                    ValidarEspacioDespuesComa(contexto, tokenComa, textoFuente, ReglaLlamada);
                }
            }
        }

        private static void ValidarEspaciosEnListaExpresiones(SyntaxNodeAnalysisContext contexto, SeparatedSyntaxList<ExpressionSyntax> expresiones)
        {
            if (expresiones.Count <= 1)
                return;

            var textoFuente = contexto.Node.SyntaxTree.GetText(contexto.CancellationToken);

            for (int i = 0; i < expresiones.SeparatorCount; i++)
            {
                var separador = expresiones.GetSeparator(i);
                if (separador.IsKind(SyntaxKind.CommaToken))
                {
                    ValidarEspacioDespuesComa(contexto, separador, textoFuente, ReglaLlamada);
                }
            }
        }

        #endregion

        #region Validación de Espacios

        private static void ValidarEspacioDespuesComa(SyntaxNodeAnalysisContext contexto, SyntaxToken tokenComa, 
            SourceText textoFuente, DiagnosticDescriptor regla)
        {
            var posicionComa = tokenComa.Span.End;
            
            // Verificar si hay salto de línea después de la coma
            if (HaySaltoLineaDespuesComa(tokenComa, textoFuente))
                return; // Los saltos de línea están permitidos

            // Verificar si hay espacio después de la coma
            if (posicionComa < textoFuente.Length)
            {
                var caracterSiguiente = textoFuente[posicionComa];
                
                // Si no hay espacio inmediatamente después de la coma
                if (caracterSiguiente != ' ')
                {
                    var diagnostico = Diagnostic.Create(
                        regla,
                        tokenComa.GetLocation());

                    contexto.ReportDiagnostic(diagnostico);
                }
                // Si hay espacio, verificar que no sean múltiples espacios
                else if (HayMultiplesEspacios(textoFuente, posicionComa))
                {
                    // Opcional: reportar múltiples espacios como warning menor
                    // Por ahora solo validamos que haya al menos un espacio
                }
            }
        }

        private static bool HaySaltoLineaDespuesComa(SyntaxToken tokenComa, SourceText textoFuente)
        {
            var posicionFinal = tokenComa.Span.End;
            
            // Buscar caracteres después de la coma hasta encontrar contenido no blanco
            for (int i = posicionFinal; i < textoFuente.Length; i++)
            {
                var caracter = textoFuente[i];
                
                if (caracter == '\r' || caracter == '\n')
                    return true;
                
                if (caracter != ' ' && caracter != '\t')
                    return false;
            }
            
            return false;
        }

        private static bool HayMultiplesEspacios(SourceText textoFuente, int posicionInicio)
        {
            int contadorEspacios = 0;
            
            for (int i = posicionInicio; i < textoFuente.Length; i++)
            {
                var caracter = textoFuente[i];
                
                if (caracter == ' ')
                    contadorEspacios++;
                else
                    break;
            }
            
            return contadorEspacios > 1;
        }

        #endregion
    }
}
