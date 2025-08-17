using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DetectarVariablesQuemadas : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA005";
        private const string Titulo = "Valor quemado detectado";
        private const string FormatoMensaje = "El valor '{0}' está quemado en el código y debería ser una constante o readonly";
        private const string Descripcion = "Los valores literales que se repiten o son significativos deben extraerse como constantes o campos readonly para mejorar el mantenimiento del código.";
        private const string Categoria = "Maintainability";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1802");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarClase, SyntaxKind.ClassDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarStruct, SyntaxKind.StructDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarRecord, SyntaxKind.RecordDeclaration);
        }

        private static void AnalizarClase(SyntaxNodeAnalysisContext contexto)
        {
            var clase = (ClassDeclarationSyntax)contexto.Node;
            AnalizarTipoParaVariablesQuemadas(contexto, clase);
        }

        private static void AnalizarStruct(SyntaxNodeAnalysisContext contexto)
        {
            var estructura = (StructDeclarationSyntax)contexto.Node;
            AnalizarTipoParaVariablesQuemadas(contexto, estructura);
        }

        private static void AnalizarRecord(SyntaxNodeAnalysisContext contexto)
        {
            var record = (RecordDeclarationSyntax)contexto.Node;
            AnalizarTipoParaVariablesQuemadas(contexto, record);
        }

        private static void AnalizarTipoParaVariablesQuemadas(SyntaxNodeAnalysisContext contexto, TypeDeclarationSyntax tipoDeclaracion)
        {
            var valoresLiterales = new Dictionary<string, List<LiteralInfo>>();
            var constantesExistentes = new HashSet<string>();

            // Recopilar constantes y readonly existentes
            RecopilarConstantesExistentes(tipoDeclaracion, constantesExistentes);

            // Recopilar todos los valores literales en el tipo
            RecopilarValoresLiterales(tipoDeclaracion, valoresLiterales);

            // Analizar y reportar valores quemados
            foreach (var valor in valoresLiterales)
            {
                var valorLiteral = valor.Key;
                var ubicaciones = valor.Value;

                if (DeberiaSerConstante(valorLiteral, ubicaciones, constantesExistentes))
                {
                    foreach (var ubicacion in ubicaciones)
                    {
                        var diagnostico = Diagnostic.Create(
                            Regla,
                            ubicacion.Ubicacion,
                            valorLiteral);

                        contexto.ReportDiagnostic(diagnostico);
                    }
                }
            }
        }

        private static void RecopilarConstantesExistentes(TypeDeclarationSyntax tipoDeclaracion, HashSet<string> constantesExistentes)
        {
            foreach (var miembro in tipoDeclaracion.Members)
            {
                if (miembro is FieldDeclarationSyntax campo)
                {
                    // Verificar si es const o readonly
                    var esConstante = campo.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword));
                    var esReadonly = campo.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));

                    if (esConstante || esReadonly)
                    {
                        foreach (var variable in campo.Declaration.Variables)
                        {
                            if (variable.Initializer?.Value != null)
                            {
                                var valorConstante = ObtenerValorLiteral(variable.Initializer.Value);
                                if (!string.IsNullOrEmpty(valorConstante))
                                {
                                    constantesExistentes.Add(valorConstante);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void RecopilarValoresLiterales(SyntaxNode nodo, Dictionary<string, List<LiteralInfo>> valoresLiterales)
        {
            foreach (var descendiente in nodo.DescendantNodes())
            {
                string valorLiteral = null;
                Location ubicacion = null;

                // Excluir literales que están dentro de un atributo Route
                if (descendiente is LiteralExpressionSyntax literal)
                {
                    if (EstaEnAtributoRoute(literal))
                        continue;

                    valorLiteral = ObtenerValorLiteral(literal);
                    ubicacion = literal.GetLocation();
                }
                else if (descendiente is InterpolatedStringExpressionSyntax cadenaInterpolada)
                {
                    foreach (var contenido in cadenaInterpolada.Contents.OfType<InterpolatedStringTextSyntax>())
                    {
                        if (EstaEnAtributoRoute(contenido))
                            continue;

                        var textoParcial = contenido.TextToken.ValueText;
                        if (!string.IsNullOrWhiteSpace(textoParcial) && textoParcial.Length > 3)
                        {
                            valorLiteral = $"\"{textoParcial}\"";
                            ubicacion = contenido.GetLocation();
                            AgregarValorLiteral(valoresLiterales, valorLiteral, ubicacion);
                        }
                    }
                    continue;
                }

                if (!string.IsNullOrEmpty(valorLiteral) && ubicacion != null)
                {
                    AgregarValorLiteral(valoresLiterales, valorLiteral, ubicacion);
                }
            }
        }
        private static void AgregarValorLiteral(Dictionary<string, List<LiteralInfo>> valoresLiterales, string valorLiteral, Location ubicacion)
        {
            if (!valoresLiterales.ContainsKey(valorLiteral))
            {
                valoresLiterales[valorLiteral] = new List<LiteralInfo>();
            }

            valoresLiterales[valorLiteral].Add(new LiteralInfo
            {
                Valor = valorLiteral,
                Ubicacion = ubicacion
            });
        }

        private static string ObtenerValorLiteral(ExpressionSyntax expresion)
        {
            if (expresion is LiteralExpressionSyntax literal)
            {
                return literal.Token.ValueText;
            }

            return expresion.ToString();
        }

        private static bool DeberiaSerConstante(string valorLiteral, List<LiteralInfo> ubicaciones, HashSet<string> constantesExistentes)
        {
            // Ya existe como constante
            if (constantesExistentes.Contains(valorLiteral))
                return false;

            // Excluir valores comunes que no necesitan ser constantes
            if (EsValorExcluido(valorLiteral))
                return false;

            // Si se repite más de una vez, debería ser constante
            if (ubicaciones.Count > 1)
                return true;

            // Valores únicos pero significativos que deberían ser constantes
            if (EsValorSignificativo(valorLiteral))
                return true;

            return false;
        }

        private static bool EsValorExcluido(string valorLiteral)
        {
            // Excluir valores comunes
            var valoresComunes = new HashSet<string>
            {
                "0", "1", "2", "3", "4", "5", "10", "-1",
                "true", "false", "null",
                "\"\"", "\" \"", "\"\""
            };

            if (valoresComunes.Contains(valorLiteral))
                return true;

            // Excluir cadenas muy cortas (menos de 4 caracteres sin comillas)
            if (valorLiteral.StartsWith("\"") && valorLiteral.EndsWith("\""))
            {
                var contenidoCadena = valorLiteral.Substring(1, valorLiteral.Length - 2);
                if (contenidoCadena.Length < 4)
                    return true;
            }

            // Excluir números decimales simples
            if (Regex.IsMatch(valorLiteral, @"^\d+\.\d{1,2}$"))
                return true;

            return false;
        }

        private static bool EsValorSignificativo(string valorLiteral)
        {
            // URLs, conexiones, rutas
            if (valorLiteral.Contains("http") || valorLiteral.Contains("https") ||
                valorLiteral.Contains("ftp") || valorLiteral.Contains("://"))
                return true;

            // Rutas de archivos
            if (valorLiteral.Contains("\\") || valorLiteral.Contains("/"))
                return true;

            // Cadenas de configuración
            if (valorLiteral.Contains("ConnectionString") || valorLiteral.Contains("AppSettings") ||
                valorLiteral.Contains("Config") || valorLiteral.Contains("Setting"))
                return true;

            // Mensajes de error específicos
            if (valorLiteral.Length > 20 && (valorLiteral.Contains("Error") || 
                valorLiteral.Contains("Exception") || valorLiteral.Contains("Invalid")))
                return true;


            // Formatos de fecha/hora
            if (Regex.IsMatch(valorLiteral, @"[yMdHmsf]{2,}"))
                return true;

            // Números con significado especial (códigos, IDs, etc.)
            if (Regex.IsMatch(valorLiteral, @"^\d{3,}$"))
                return true;

            // Cadenas con patrones especiales (emails, teléfonos, etc.)
            if (valorLiteral.Contains("@") || Regex.IsMatch(valorLiteral, @"\d{3}-\d{3}-\d{4}"))
                return true;

            // Nombres de claves o identificadores
            if (valorLiteral.Length > 10 && Regex.IsMatch(valorLiteral, @"^[A-Z][a-zA-Z]+$"))
                return true;

            return false;
        }

        private static bool EstaEnAtributoRoute(SyntaxNode nodo)
        {
            var atributo = nodo.Ancestors().OfType<AttributeSyntax>().FirstOrDefault();
            if (atributo == null)
                return false;

            var nombre = atributo.Name.ToString();
            return nombre.IndexOf("Route", System.StringComparison.OrdinalIgnoreCase) != -1;
        }

        private class LiteralInfo
        {
            public string Valor { get; set; }
            public Location Ubicacion { get; set; }
        }
    }
}

