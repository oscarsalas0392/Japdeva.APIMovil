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
    public class DetectarEspaciosInnecesarios : DiagnosticAnalyzer
    {
        public const string DiagnosticIdLineasVacias = "JAPDEVA051";
        public const string DiagnosticIdEspaciosFinales = "JAPDEVA052";
        public const string DiagnosticIdEspaciosExcesivos = "JAPDEVA053";

        private const string TituloLineasVacias = "Líneas en blanco excesivas";
        private const string TituloEspaciosFinales = "Espacios en blanco al final de línea";
        private const string TituloEspaciosExcesivos = "Espacios excesivos entre elementos";

        private const string FormatoMensajeLineasVacias = "Se detectaron {0} líneas en blanco consecutivas. Máximo permitido: 2 líneas (1 línea vacía para separar secciones de código)";
        private const string FormatoMensajeEspaciosFinales = "Se detectaron espacios en blanco al final de la línea {0}. Remover espacios trailing";
        private const string FormatoMensajeEspaciosExcesivos = "Se detectaron {0} espacios consecutivos innecesarios. Usar un solo espacio";

        private const string DescripcionLineasVacias = "Se permite una línea vacía para separar bloques de código y antes de documentación XML. Más de 2 líneas consecutivas son excesivas y deben reducirse.";
        private const string DescripcionEspaciosFinales = "Los espacios en blanco al final de las líneas deben ser removidos para evitar inconsistencias y problemas en control de versiones.";
        private const string DescripcionEspaciosExcesivos = "Se debe usar un solo espacio entre elementos del código para mantener consistencia visual.";

        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor ReglaLineasVacias = new DiagnosticDescriptor(
            DiagnosticIdLineasVacias,
            TituloLineasVacias,
            FormatoMensajeLineasVacias,
            Categoria,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: DescripcionLineasVacias);

        private static readonly DiagnosticDescriptor ReglaEspaciosFinales = new DiagnosticDescriptor(
            DiagnosticIdEspaciosFinales,
            TituloEspaciosFinales,
            FormatoMensajeEspaciosFinales,
            Categoria,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: DescripcionEspaciosFinales);

        private static readonly DiagnosticDescriptor ReglaEspaciosExcesivos = new DiagnosticDescriptor(
            DiagnosticIdEspaciosExcesivos,
            TituloEspaciosExcesivos,
            FormatoMensajeEspaciosExcesivos,
            Categoria,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: DescripcionEspaciosExcesivos);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(ReglaLineasVacias, ReglaEspaciosFinales, ReglaEspaciosExcesivos);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxTreeAction(AnalizarArbolSintaxis);
        }

        private static void AnalizarArbolSintaxis(SyntaxTreeAnalysisContext contexto)
        {
            var raiz = contexto.Tree.GetCompilationUnitRoot(contexto.CancellationToken);
            var textoCompleto = raiz.GetText();
            
            AnalizarLineasVaciasAlInicio(contexto, textoCompleto);
            AnalizarEspaciosFinalesDeLinea(contexto, textoCompleto);
            AnalizarEspaciosExcesivosEnCodigo(contexto, raiz);
        }

        private static void AnalizarLineasVaciasAlInicio(SyntaxTreeAnalysisContext contexto, SourceText textoCompleto)
        {
            var lineas = textoCompleto.Lines;
            if (!lineas.Any()) return;

            var lineasVaciasAlInicio = 0;
            
            foreach (var linea in lineas)
            {
                var textoLinea = textoCompleto.ToString(linea.Span);
                
                // Si la línea está vacía o solo tiene espacios en blanco
                if (string.IsNullOrWhiteSpace(textoLinea))
                {
                    lineasVaciasAlInicio++;
                }
                else
                {
                    // Encontramos contenido, paramos de contar
                    break;
                }
            }

            // Solo reportar si hay más de 1 línea vacía al inicio
            if (lineasVaciasAlInicio > 1)
            {
                var ubicacion = Location.Create(
                    contexto.Tree,
                    TextSpan.FromBounds(0, lineas[lineasVaciasAlInicio - 1].End));

                var diagnostico = Diagnostic.Create(
                    ReglaLineasVacias,
                    ubicacion,
                    lineasVaciasAlInicio);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static void AnalizarEspaciosFinalesDeLinea(SyntaxTreeAnalysisContext contexto, SourceText textoCompleto)
        {
            var lineas = textoCompleto.Lines;

            for (int i = 0; i < lineas.Count; i++)
            {
                var linea = lineas[i];
                var textoLinea = textoCompleto.ToString(linea.Span);

                // Verificar si la línea termina con espacios (pero no está completamente vacía)
                if (!string.IsNullOrEmpty(textoLinea) && textoLinea.Length > 0)
                {
                    var ultimoCaracterNoEspacio = textoLinea.TrimEnd().Length;
                    var espaciosFinales = textoLinea.Length - ultimoCaracterNoEspacio;

                    if (espaciosFinales > 0)
                    {
                        var inicioEspacios = linea.Start + ultimoCaracterNoEspacio;
                        var ubicacion = Location.Create(
                            contexto.Tree,
                            TextSpan.FromBounds(inicioEspacios, linea.End));

                        var diagnostico = Diagnostic.Create(
                            ReglaEspaciosFinales,
                            ubicacion,
                            i + 1); // Número de línea (1-indexed)

                        contexto.ReportDiagnostic(diagnostico);
                    }
                }
            }
        }

        private static void AnalizarEspaciosExcesivosEnCodigo(SyntaxTreeAnalysisContext contexto, SyntaxNode raiz)
        {
            // Analizar líneas vacías múltiples en el cuerpo del código
            AnalizarLineasVaciasExcesivasEnCuerpo(contexto, raiz);
            
            // Analizar tokens para encontrar espacios excesivos entre elementos
            var tokens = raiz.DescendantTokens(descendIntoTrivia: true);
            
            SyntaxToken? tokenAnterior = null;

            foreach (var token in tokens)
            {
                if (tokenAnterior.HasValue)
                {
                    var espaciosEntre = AnalizarEspaciosEntreTokens(tokenAnterior.Value, token);
                    
                    if (espaciosEntre > 1)
                    {
                        // Solo reportar espacios excesivos en contextos específicos
                        if (DeberiaReportarEspaciosExcesivos(tokenAnterior.Value, token))
                        {
                            var inicioEspacios = tokenAnterior.Value.Span.End;
                            var finEspacios = token.Span.Start;
                            
                            var ubicacion = Location.Create(
                                contexto.Tree,
                                TextSpan.FromBounds(inicioEspacios, finEspacios));

                            var diagnostico = Diagnostic.Create(
                                ReglaEspaciosExcesivos,
                                ubicacion,
                                espaciosEntre);

                            contexto.ReportDiagnostic(diagnostico);
                        }
                    }
                }
                
                tokenAnterior = token;
            }
        }

        private static void AnalizarLineasVaciasExcesivasEnCuerpo(SyntaxTreeAnalysisContext contexto, SyntaxNode raiz)
        {
            var textoCompleto = raiz.GetText();
            var lineas = textoCompleto.Lines;

            for (int i = 0; i < lineas.Count - 1; i++)
            {
                var lineaActual = textoCompleto.ToString(lineas[i].Span);
                var lineaSiguiente = i + 1 < lineas.Count ? textoCompleto.ToString(lineas[i + 1].Span) : "";
                
                // Si encontramos una línea vacía, verificar si es apropiada
                if (string.IsNullOrWhiteSpace(lineaActual) && !string.IsNullOrWhiteSpace(lineaSiguiente))
                {
                    // Verificar si la línea siguiente es documentación XML
                    var lineaSiguienteTrimmed = lineaSiguiente.Trim();
                    var esDocumentacionXML = lineaSiguienteTrimmed.StartsWith("/// <summary>") ||
                                           lineaSiguienteTrimmed.StartsWith("///<summary>") ||
                                           lineaSiguienteTrimmed.StartsWith("/// <param") ||
                                           lineaSiguienteTrimmed.StartsWith("/// <returns>");
                    
                    if (esDocumentacionXML)
                    {
                        // Permitir una línea vacía antes de documentación XML
                        continue;
                    }
                    
                    // Verificar si es antes de una declaración de miembro importante
                    var esDeclaracionImportante = lineaSiguienteTrimmed.Contains("public ") ||
                                                 lineaSiguienteTrimmed.Contains("private ") ||
                                                 lineaSiguienteTrimmed.Contains("protected ") ||
                                                 lineaSiguienteTrimmed.Contains("internal ") ||
                                                 lineaSiguienteTrimmed.StartsWith("namespace ") ||
                                                 lineaSiguienteTrimmed.StartsWith("class ") ||
                                                 lineaSiguienteTrimmed.StartsWith("interface ") ||
                                                 lineaSiguienteTrimmed.StartsWith("struct ") ||
                                                 lineaSiguienteTrimmed.StartsWith("enum ");
                    
                    if (esDeclaracionImportante)
                    {
                        // Permitir una línea vacía antes de declaraciones importantes
                        continue;
                    }
                }
                
                // Detectar múltiples líneas vacías consecutivas
                if (string.IsNullOrWhiteSpace(lineaActual))
                {
                    int lineasVaciasConsecutivas = 1;
                    int j = i + 1;
                    
                    while (j < lineas.Count && string.IsNullOrWhiteSpace(textoCompleto.ToString(lineas[j].Span)))
                    {
                        lineasVaciasConsecutivas++;
                        j++;
                    }
                    
                    // Si hay más de 2 líneas vacías consecutivas, reportar
                    if (lineasVaciasConsecutivas > 2)
                    {
                        var ubicacion = Location.Create(
                            contexto.Tree,
                            TextSpan.FromBounds(lineas[i].Start, lineas[j - 1].End));

                        var diagnostico = Diagnostic.Create(
                            ReglaLineasVacias,
                            ubicacion,
                            lineasVaciasConsecutivas);

                        contexto.ReportDiagnostic(diagnostico);
                    }
                    
                    i = j - 1; // Saltar las líneas vacías ya analizadas
                }
            }
        }

        private static int AnalizarEspaciosEntreTokens(SyntaxToken tokenAnterior, SyntaxToken tokenSiguiente)
        {
            var textoEntre = tokenAnterior.TrailingTrivia.ToString() + tokenSiguiente.LeadingTrivia.ToString();
            
            // Contar solo espacios (no incluir saltos de línea, tabs, etc.)
            var espacios = 0;
            var soloEspacios = true;
            
            foreach (char c in textoEntre)
            {
                if (c == ' ')
                {
                    espacios++;
                }
                else if (c != '\t' && c != '\r' && c != '\n')
                {
                    soloEspacios = false;
                    break;
                }
                else if (c == '\r' || c == '\n')
                {
                    // Si hay salto de línea, no contar como espacios excesivos
                    return 0;
                }
            }
            
            return soloEspacios ? espacios : 0;
        }

        private static bool DeberiaReportarEspaciosExcesivos(SyntaxToken tokenAnterior, SyntaxToken tokenSiguiente)
        {
            // Solo reportar espacios excesivos en contextos donde normalmente esperamos un espacio
            var tiposQueDeberianTenerUnEspacio = new[]
            {
                // Después de palabras clave
                SyntaxKind.PublicKeyword, SyntaxKind.PrivateKeyword, SyntaxKind.ProtectedKeyword,
                SyntaxKind.InternalKeyword, SyntaxKind.StaticKeyword, SyntaxKind.AbstractKeyword,
                SyntaxKind.VirtualKeyword, SyntaxKind.OverrideKeyword, SyntaxKind.SealedKeyword,
                SyntaxKind.ClassKeyword, SyntaxKind.InterfaceKeyword, SyntaxKind.StructKeyword,
                SyntaxKind.EnumKeyword, SyntaxKind.NamespaceKeyword, SyntaxKind.UsingKeyword,
                
                // Operadores que suelen tener espacios
                SyntaxKind.EqualsToken, SyntaxKind.PlusToken, SyntaxKind.MinusToken,
                SyntaxKind.AsteriskToken, SyntaxKind.SlashToken, SyntaxKind.PercentToken,
                
                // Después de comas
                SyntaxKind.CommaToken
            };

            return tiposQueDeberianTenerUnEspacio.Contains(tokenAnterior.Kind()) ||
                   tiposQueDeberianTenerUnEspacio.Contains(tokenSiguiente.Kind());
        }
    }
}
