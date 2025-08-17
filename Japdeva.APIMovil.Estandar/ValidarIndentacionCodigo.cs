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
    public class ValidarIndentacionCodigo : DiagnosticAnalyzer
    {
        public const string DiagnosticIdTabs = "JAPDEVA025";
        public const string DiagnosticIdEspacios = "JAPDEVA026";

        private const string TituloTabs = "No usar tabs para indentación";
        private const string FormatoMensajeTabs = "La línea {0} usa tabs para indentación. Se deben usar espacios (4 espacios por nivel)";
        private const string DescripcionTabs = "El código debe usar espacios para indentación en lugar de caracteres tab para mantener la consistencia visual en diferentes editores.";

        private const string TituloEspacios = "Indentación incorrecta";
        private const string FormatoMensajeEspacios = "La línea {0} tiene indentación incorrecta. Se esperaban {1} espacios pero se encontraron {2}";
        private const string DescripcionEspacios = "El código debe usar 4 espacios por cada nivel de indentación para mantener la consistencia y legibilidad.";

        private const string Categoria = "Style";
        private const int EspaciosPorNivel = 4;

        private static readonly DiagnosticDescriptor ReglaTabs = new DiagnosticDescriptor(
            DiagnosticIdTabs,
            TituloTabs,
            FormatoMensajeTabs,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: DescripcionTabs,
            helpLinkUri: "https://docs.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0055");

        private static readonly DiagnosticDescriptor ReglaEspacios = new DiagnosticDescriptor(
            DiagnosticIdEspacios,
            TituloEspacios,
            FormatoMensajeEspacios,
            Categoria,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: DescripcionEspacios,
            helpLinkUri: "https://docs.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0055");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(ReglaTabs, ReglaEspacios);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxTreeAction(AnalizarArchivoCompleto);
        }

        private static void AnalizarArchivoCompleto(SyntaxTreeAnalysisContext contexto)
        {
            var raiz = contexto.Tree.GetRoot(contexto.CancellationToken);
            var textoFuente = raiz.GetText();

            ValidarIndentacionLineas(contexto, textoFuente);
        }

        private static void ValidarIndentacionLineas(SyntaxTreeAnalysisContext contexto, SourceText textoFuente)
        {
            for (int i = 0; i < textoFuente.Lines.Count; i++)
            {
                var linea = textoFuente.Lines[i];
                var textoLinea = linea.ToString();
                var numeroLinea = i + 1;

                // Saltar líneas vacías
                if (string.IsNullOrWhiteSpace(textoLinea))
                    continue;

                // Verificar uso de tabs
                if (textoLinea.StartsWith("\t") || textoLinea.Contains("\t"))
                {
                    var diagnosticoTabs = Diagnostic.Create(
                        ReglaTabs,
                        Location.Create(contexto.Tree, linea.Span),
                        numeroLinea);

                    contexto.ReportDiagnostic(diagnosticoTabs);
                    continue; // No verificar espacios si hay tabs
                }

                // Verificar indentación con espacios
                ValidarIndentacionEspacios(contexto, linea, textoLinea, numeroLinea);
            }
        }

        private static void ValidarIndentacionEspacios(SyntaxTreeAnalysisContext contexto, TextLine linea, 
            string textoLinea, int numeroLinea)
        {
            // Obtener la cantidad de espacios al inicio de la línea
            int espaciosActuales = 0;
            foreach (char c in textoLinea)
            {
                if (c == ' ')
                    espaciosActuales++;
                else
                    break;
            }

            // Si la línea no tiene contenido después de los espacios, no validar
            var contenidoLinea = textoLinea.TrimStart();
            if (string.IsNullOrEmpty(contenidoLinea))
                return;

            // Calcular el nivel de indentación esperado
            var nivelEsperado = CalcularNivelIndentacionEsperado(contexto, linea, contenidoLinea);
            var espaciosEsperados = nivelEsperado * EspaciosPorNivel;

            // Verificar si la indentación es correcta
            if (espaciosActuales != espaciosEsperados && 
                !EsLineaEspecial(contenidoLinea) &&
                nivelEsperado >= 0) // Solo validar si pudimos calcular el nivel
            {
                var diagnosticoEspacios = Diagnostic.Create(
                    ReglaEspacios,
                    Location.Create(contexto.Tree, linea.Span),
                    numeroLinea,
                    espaciosEsperados,
                    espaciosActuales);

                contexto.ReportDiagnostic(diagnosticoEspacios);
            }
        }

        private static int CalcularNivelIndentacionEsperado(SyntaxTreeAnalysisContext contexto, TextLine linea, string contenidoLinea)
        {
            try
            {
                var posicion = linea.Start;
                var raiz = contexto.Tree.GetRoot(contexto.CancellationToken);
                var nodo = raiz.FindNode(new TextSpan(posicion, 1));

                // Contar la profundidad de anidamiento
                int nivel = 0;
                var nodoActual = nodo;

                while (nodoActual != null && nodoActual != raiz)
                {
                    // Incrementar nivel para ciertos tipos de nodos
                    if (EsNodoQueRequiereIndentacion(nodoActual))
                    {
                        nivel++;
                    }

                    nodoActual = nodoActual.Parent;
                }

                // Ajustar para líneas que cierran bloques
                if (EsLineaCierreBloque(contenidoLinea))
                {
                    nivel = System.Math.Max(0, nivel - 1);
                }

                return nivel;
            }
            catch
            {
                // En caso de error, no validar la indentación
                return -1;
            }
        }

        // Step 3: If your project cannot target .NET 6 or later, you can remove or conditionally handle the usage of 'FileScopedNamespaceDeclarationSyntax'.  
        // Example:  
        private static bool EsNodoQueRequiereIndentacion(SyntaxNode nodo)
        {
            return nodo is ClassDeclarationSyntax ||
                   nodo is MethodDeclarationSyntax ||
                   nodo is PropertyDeclarationSyntax ||
                   nodo is ConstructorDeclarationSyntax ||
                   nodo is NamespaceDeclarationSyntax ||
                   nodo is InterfaceDeclarationSyntax ||
                   nodo is StructDeclarationSyntax ||
                   nodo is RecordDeclarationSyntax ||
                   nodo is EnumDeclarationSyntax ||
                   nodo is BlockSyntax ||
                   nodo is IfStatementSyntax ||
                   nodo is ForStatementSyntax ||
                   nodo is ForEachStatementSyntax ||
                   nodo is WhileStatementSyntax ||
                   nodo is DoStatementSyntax ||
                   nodo is TryStatementSyntax ||
                   nodo is CatchClauseSyntax ||
                   nodo is FinallyClauseSyntax ||
                   nodo is SwitchStatementSyntax ||
                   nodo is UsingStatementSyntax;
        }

        private static bool EsLineaEspecial(string contenidoLinea)
        {
            // Excluir líneas especiales que pueden tener indentación diferente
            return contenidoLinea.StartsWith("//") ||           // Comentarios
                   contenidoLinea.StartsWith("/*") ||           // Inicio comentario bloque
                   contenidoLinea.StartsWith("*") ||            // Comentario bloque
                   contenidoLinea.StartsWith("*/") ||           // Fin comentario bloque
                   contenidoLinea.StartsWith("///") ||          // Documentación XML
                   contenidoLinea.StartsWith("#") ||            // Directivas preprocesador
                   contenidoLinea.StartsWith("using ") ||       // Using statements
                   contenidoLinea.StartsWith("[") ||            // Atributos
                   contenidoLinea.StartsWith("<summary") ||     // Documentación XML
                   contenidoLinea.StartsWith("<param") ||       // Documentación XML
                   contenidoLinea.StartsWith("<returns") ||     // Documentación XML
                   contenidoLinea.StartsWith("</summary") ||    // Documentación XML
                   contenidoLinea.StartsWith("</param") ||      // Documentación XML
                   contenidoLinea.StartsWith("</returns");      // Documentación XML
        }

        private static bool EsLineaCierreBloque(string contenidoLinea)
        {
            var lineaLimpia = contenidoLinea.Trim();
            return lineaLimpia == "}" ||
                   lineaLimpia == "};" ||
                   lineaLimpia.StartsWith("};") ||
                   lineaLimpia.StartsWith("}");
        }
    }
}

