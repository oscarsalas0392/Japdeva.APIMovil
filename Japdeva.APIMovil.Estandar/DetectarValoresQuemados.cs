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
        private const string FormatoMensaje = "El valor '{0}' est� quemado en el c�digo y deber�a ser una constante o readonly";
        private const string Descripcion = "Los valores literales que se repiten o son significativos deben extraerse como constantes o campos readonly para mejorar el mantenimiento del c�digo.";
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

                    if (EstaEnAtributoAuthorizeRoles(literal))
                        continue;

                    // Verificar si este literal es parte de una expresión unaria (como -10)
                    var padreUnario = literal.Parent as PrefixUnaryExpressionSyntax;
                    if (padreUnario != null && padreUnario.IsKind(SyntaxKind.UnaryMinusExpression))
                    {
                        // Si es parte de una expresión unaria, omitir el literal individual
                        continue;
                    }

                    // Excluir literales que están siendo asignados a variables locales
                    if (EstaEnAsignacionVariable(literal))
                        continue;

                    valorLiteral = ObtenerValorLiteral(literal);
                    ubicacion = literal.GetLocation();
                }
                // Manejar expresiones unarias como -10
                else if (descendiente is PrefixUnaryExpressionSyntax unary && unary.IsKind(SyntaxKind.UnaryMinusExpression))
                {
                    if (EstaEnAtributoRoute(unary))
                        continue;

                    // Verificar si esta expresión unaria está siendo asignada a una variable local
                    if (EstaEnAsignacionVariableUnaria(unary))
                        continue;

                    valorLiteral = ObtenerValorLiteral(unary);
                    ubicacion = unary.GetLocation();
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
                return literal.ToString(); // Usar ToString() en lugar de ValueText para mantener el formato completo
            }

            // Manejar expresiones unarias como -10
            if (expresion is PrefixUnaryExpressionSyntax unary && unary.IsKind(SyntaxKind.UnaryMinusExpression))
            {
                return expresion.ToString(); // Retorna "-10" completo
            }

            return expresion.ToString();
        }

        private static bool DeberiaSerConstante(string valorLiteral, List<LiteralInfo> ubicaciones, HashSet<string> constantesExistentes)
        {
            // Ya existe como constante
            if (constantesExistentes.Contains(valorLiteral))
                return false;

            // Excluir valores que realmente no necesitan ser constantes
            if (EsValorExcluido(valorLiteral))
                return false;

            // Todos los demás valores literales deben ser detectados como hardcodeados
            return true;
        }

        private static bool EsValorExcluido(string valorLiteral)
        {
            // Solo excluir valores que realmente no tienen sentido como constantes
            var valoresExcluidos = new HashSet<string>
            {
                "null",
                "\"\"",  // cadena vacía
                "\" \""  // cadena con solo espacio
            };

            if (valoresExcluidos.Contains(valorLiteral))
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

            // Cadenas de configuraci�n
            if (valorLiteral.Contains("ConnectionString") || valorLiteral.Contains("AppSettings") ||
                valorLiteral.Contains("Config") || valorLiteral.Contains("Setting"))
                return true;

            // Mensajes de error espec�ficos
            if (valorLiteral.Length > 20 && (valorLiteral.Contains("Error") ||
                valorLiteral.Contains("Exception") || valorLiteral.Contains("Invalid")))
                return true;


            // Formatos de fecha/hora
            if (Regex.IsMatch(valorLiteral, @"[yMdHmsf]{2,}"))
                return true;

            // N�meros con significado especial (c�digos, IDs, etc.)
            if (Regex.IsMatch(valorLiteral, @"^\d{3,}$"))
                return true;

            // Cadenas con patrones especiales (emails, tel�fonos, etc.)
            if (valorLiteral.Contains("@") || Regex.IsMatch(valorLiteral, @"\d{3}-\d{3}-\d{4}"))
                return true;

            // Nombres de claves o identificadores
            if (valorLiteral.Length > 10 && Regex.IsMatch(valorLiteral, @"^[A-Z][a-zA-Z]+$"))
                return true;

            return false;
        }

        private static bool EstaEnAsignacionVariableUnaria(PrefixUnaryExpressionSyntax unary)
        {
            // Las strings siempre deben ser detectadas, incluso si están en variables
            // (pero las expresiones unarias no pueden ser strings, así que esto no aplica)

            // Buscar si esta expresión unaria está en una asignación de variable local
            var variableDeclaration = unary.Ancestors().OfType<VariableDeclarationSyntax>().FirstOrDefault();
            if (variableDeclaration != null)
            {
                // Verificar si es una variable local (no un campo de clase)
                var localDeclaration = variableDeclaration.Ancestors().OfType<LocalDeclarationStatementSyntax>().FirstOrDefault();
                if (localDeclaration != null)
                {
                    return true; // Es una asignación a variable local, excluir
                }
            }

            // Buscar si está en una asignación simple (variable = valor)
            var assignment = unary.Ancestors().OfType<AssignmentExpressionSyntax>().FirstOrDefault();
            if (assignment != null && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                return true; // Es una asignación simple, excluir
            }

            return false; // No está en asignación de variable, debe ser detectado
        }

        private static bool EstaEnAsignacionVariable(LiteralExpressionSyntax literal)
        {
            // Las strings siempre deben ser detectadas, incluso si están en variables
            if (literal.Token.IsKind(SyntaxKind.StringLiteralToken))
                return false;

            // Buscar si este literal está en una asignación de variable local
            var variableDeclaration = literal.Ancestors().OfType<VariableDeclarationSyntax>().FirstOrDefault();
            if (variableDeclaration != null)
            {
                // Verificar si es una variable local (no un campo de clase)
                var localDeclaration = variableDeclaration.Ancestors().OfType<LocalDeclarationStatementSyntax>().FirstOrDefault();
                if (localDeclaration != null)
                {
                    return true; // Es una asignación a variable local, excluir (excepto strings que ya se manejaron arriba)
                }
            }

            // Buscar si está en una asignación simple (variable = valor)
            var assignment = literal.Ancestors().OfType<AssignmentExpressionSyntax>().FirstOrDefault();
            if (assignment != null && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                return true; // Es una asignación simple, excluir (excepto strings)
            }

            return false; // No está en asignación de variable, debe ser detectado
        }

        private static bool EstaEnAtributoRoute(SyntaxNode nodo)
        {
            var atributo = nodo.Ancestors().OfType<AttributeSyntax>().FirstOrDefault();
            if (atributo == null)
                return false;

            var nombre = atributo.Name.ToString();
            return nombre.IndexOf("Route", System.StringComparison.OrdinalIgnoreCase) != -1;
        }

        private static bool EstaEnAtributoAuthorizeRoles(SyntaxNode nodo)
        {
            var atributo = nodo.Ancestors().OfType<AttributeSyntax>().FirstOrDefault();
            if (atributo == null)
                return false;

            var nombre = atributo.Name.ToString();
            if (nombre.IndexOf("Authorize", System.StringComparison.OrdinalIgnoreCase) == -1)
                return false;

            // Verificar si el literal está en un argumento "Roles"
            var argumento = nodo.Ancestors().OfType<AttributeArgumentSyntax>().FirstOrDefault();
            if (argumento?.NameEquals?.Name?.ToString() == "Roles")
                return true;

            // También verificar si está en una lista de argumentos donde se usa Roles = "..."
            var listaArgumentos = nodo.Ancestors().OfType<AttributeArgumentListSyntax>().FirstOrDefault();
            if (listaArgumentos != null)
            {
                foreach (var arg in listaArgumentos.Arguments)
                {
                    if (arg.NameEquals?.Name?.ToString() == "Roles")
                    {
                        // Verificar si nuestro nodo está dentro de este argumento
                        if (arg.Expression.Span.Contains(nodo.Span))
                            return true;
                    }
                }
            }

            return false;
        }

        private class LiteralInfo
        {
            public string Valor { get; set; }
            public Location Ubicacion { get; set; }
        }
    }
}

