using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DetectarCodigoComentado : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA040";
        private const string Titulo = "Código comentado detectado";
        private const string FormatoMensaje = "Se encontró código comentado que debería ser eliminado: '{0}'";
        private const string Descripcion = "El código comentado debe ser eliminado del código fuente. El control de versiones mantiene el historial de cambios.";
        private const string Categoria = "Maintainability";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/maintainability",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxTreeAction(AnalizarArbolSintaxis);
        }

        private static void AnalizarArbolSintaxis(SyntaxTreeAnalysisContext contexto)
        {
            var raiz = contexto.Tree.GetRoot();
            
            // Analizar comentarios de una línea (//)
            var comentariosLinea = raiz.DescendantTrivia()
                .Where(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia));

            foreach (var comentario in comentariosLinea)
            {
                AnalizarComentario(contexto, comentario);
            }

            // Analizar comentarios de múltiples líneas (/* */)
            var comentariosBloque = raiz.DescendantTrivia()
                .Where(trivia => trivia.IsKind(SyntaxKind.MultiLineCommentTrivia));

            foreach (var comentario in comentariosBloque)
            {
                AnalizarComentario(contexto, comentario);
            }
        }

        private static void AnalizarComentario(SyntaxTreeAnalysisContext contexto, SyntaxTrivia comentario)
        {
            var textoComentario = comentario.ToString().Trim();
            
            // Remover los marcadores de comentario para analizar el contenido
            var contenido = LimpiarComentario(textoComentario);
            
            // Verificar si parece código comentado
            if (EsCodigoComentado(contenido))
            {
                var ubicacion = Location.Create(contexto.Tree, comentario.Span);
                var textoPrevisualizacion = contenido.Length > 50 
                    ? contenido.Substring(0, 47) + "..."
                    : contenido;

                var diagnostico = Diagnostic.Create(
                    Regla,
                    ubicacion,
                    textoPrevisualizacion);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static string LimpiarComentario(string comentario)
        {
            // Remover marcadores de comentarios
            if (comentario.StartsWith("//"))
            {
                comentario = comentario.Substring(2);
            }
            else if (comentario.StartsWith("/*") && comentario.EndsWith("*/"))
            {
                comentario = comentario.Substring(2, comentario.Length - 4);
            }

            return comentario.Trim();
        }

        private static bool EsCodigoComentado(string contenido)
        {
            // Filtrar comentarios legítimos que no son código
            if (EsComentarioLegitimo(contenido))
                return false;

            // Patrones que indican código C# comentado
            var patronesCodigo = new[]
            {
                // Declaraciones de variables
                @"^\s*(var|int|string|bool|double|float|decimal|long|short|byte|char|object)\s+\w+\s*[=;]",
                
                // Declaraciones de métodos
                @"^\s*(public|private|protected|internal|static).*\s+\w+\s*\([^)]*\)\s*[{;]",
                
                // Llamadas a métodos con paréntesis
                @"\w+\s*\([^)]*\)\s*[;.]",
                
                // Estructuras de control
                @"^\s*(if|else|for|foreach|while|do|switch|case|try|catch|finally)\s*[\(\{]",
                
                // Declaraciones de clases, interfaces, etc.
                @"^\s*(class|interface|struct|enum|namespace)\s+\w+",
                
                // Using statements
                @"^\s*using\s+[\w\.]+\s*;",
                
                // Return statements
                @"^\s*return\s+[^;]+;",
                
                // Asignaciones
                @"\w+\s*[=]\s*[^;]+;",
                
                // Propiedades
                @"^\s*(public|private|protected|internal).*\s+\w+\s*\{\s*(get|set)",
                
                // Atributos
                @"^\s*\[\w+.*\]",
                
                // Declaraciones de campos
                @"^\s*(public|private|protected|internal|static|readonly|const)\s+\w+\s+\w+",
                
                // Comentarios XML de documentación (también código comentado)
                @"^\s*//\s*</?[a-zA-Z]+.*>",
                
                // Líneas con punto y coma al final (típico de C#)
                @".+;\s*$",
                
                // LINQ queries
                @"\.(Where|Select|OrderBy|GroupBy|FirstOrDefault|SingleOrDefault|ToList|ToArray)\s*\(",
                
                // Constructores
                @"^\s*(public|private|protected|internal)\s+\w+\s*\([^)]*\)\s*[:{]"
            };

            // Si coincide con cualquier patrón de código, es probablemente código comentado
            foreach (var patron in patronesCodigo)
            {
                if (Regex.IsMatch(contenido, patron, RegexOptions.IgnoreCase | RegexOptions.Multiline))
                {
                    return true;
                }
            }

            // Verificar si contiene múltiples indicadores de código
            var indicadoresCodigo = 0;
            
            if (contenido.Contains(';')) indicadoresCodigo++;
            if (contenido.Contains('{') || contenido.Contains('}')) indicadoresCodigo++;
            if (contenido.Contains('(') && contenido.Contains(')')) indicadoresCodigo++;
            if (Regex.IsMatch(contenido, @"\w+\s*=\s*\w+")) indicadoresCodigo++;
            if (contenido.Contains("new ")) indicadoresCodigo++;
            if (contenido.Contains("=>")) indicadoresCodigo++;

            return indicadoresCodigo >= 2;
        }

        private static bool EsComentarioLegitimo(string contenido)
        {
            // Lista de patrones que indican comentarios legítimos, no código
            var patronesComentarios = new[]
            {
                // Comentarios TODO, FIXME, NOTE, etc.
                @"^\s*(TODO|FIXME|NOTE|HACK|BUG|REVIEW):",
                
                // Comentarios explicativos comunes
                @"^\s*(This|The|A|An|We|I|You|It|Returns?|Represents?|Indicates?|Checks?|Gets?|Sets?)",
                
                // Comentarios de documentación
                @"^\s*<summary>|</summary>|<param|</param>|<returns>|</returns>|<remarks>|</remarks>",
                
                // URLs o referencias
                @"https?://|www\.|ftp://",
                
                // Comentarios muy cortos (probablemente explicativos)
                @"^\s*[A-Z][a-z\s]{1,30}\.?\s*$",
                
                // Comentarios que terminan con signos de pregunta o exclamación
                @"[!?]\s*$"
            };

            foreach (var patron in patronesComentarios)
            {
                if (Regex.IsMatch(contenido, patron, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            // Si es muy corto y no contiene símbolos de código, probablemente es legítimo
            if (contenido.Length < 20 && !contenido.Contains(';') && !contenido.Contains('{'))
            {
                return true;
            }

            return false;
        }
    }
}
