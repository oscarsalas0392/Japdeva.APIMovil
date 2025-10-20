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
            // Agregar análisis de métodos para detectar valores quemados dentro de métodos
            contexto.RegisterSyntaxNodeAction(AnalizarMetodo, SyntaxKind.MethodDeclaration);
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
            var recordDeclaracion = (RecordDeclarationSyntax)contexto.Node;
            AnalizarTipoParaVariablesQuemadas(contexto, recordDeclaracion);
        }

        private static void AnalizarMetodo(SyntaxNodeAnalysisContext contexto)
        {
            var metodoDeclaracion = (MethodDeclarationSyntax)contexto.Node;
            
            // Verificar si el método está en una clase que tiene atributo [Table] - si lo tiene, excluir todo el método
            var claseContenedora = metodoDeclaracion.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (claseContenedora != null && TieneAtributoTable(claseContenedora))
                return;

            var valoresLiterales = new Dictionary<string, List<LiteralInfo>>();
            var constantesExistentes = new HashSet<string>();

            // Recopilar constantes existentes de la clase contenedora
            if (claseContenedora != null)
            {
                RecopilarConstantesExistentes(claseContenedora, constantesExistentes);
            }
            
            // Recopilar todos los valores literales en el método
            RecopilarValoresLiterales(metodoDeclaracion, valoresLiterales);

            // Analizar y reportar valores quemados
            foreach (var valor in valoresLiterales)
            {
                var valorLiteral = valor.Key;
                var ubicaciones = valor.Value;

                // Para métodos, verificar constantes existentes de la clase
                if (DeberiaSerConstanteEnMetodo(valorLiteral, ubicaciones, constantesExistentes))
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

        private static bool DeberiaSerConstanteEnMetodo(string valorLiteral, List<LiteralInfo> ubicaciones, HashSet<string> constantesExistentes)
        {
            // Ya existe como constante en la clase
            if (constantesExistentes.Contains(valorLiteral))
                return false;

            // Si un valor "comúnmente aceptable" ya tiene una constante disponible,
            // entonces SÍ debe ser detectado como hardcodeado para forzar el uso de la constante
            if (EsValorExcluidoPeroTieneConstante(valorLiteral, constantesExistentes))
                return true;

            // Excluir valores que realmente no necesitan ser constantes
            if (EsValorExcluido(valorLiteral))
                return false;

            // Todos los demás valores literales en métodos deben ser detectados como hardcodeados
            return true;
        }

        private static void AnalizarTipoParaVariablesQuemadas(SyntaxNodeAnalysisContext contexto, TypeDeclarationSyntax tipoDeclaracion)
        {
            // Verificar si la clase tiene atributo [Table] - si lo tiene, excluir toda la clase
            if (TieneAtributoTable(tipoDeclaracion))
                return;

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
                    // Solo los campos const deben excluirse de la detección
                    // Los campos readonly SÍ deben ser detectados como valores quemados
                    var esConstante = campo.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword));

                    if (esConstante)
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

                    // Excluir literales que están dentro de atributos HTTP de ASP.NET Core
                    if (EstaEnAtributoHttp(literal))
                        continue;

                    // Excluir literales que están dentro de un atributo Table
                    if (EstaEnAtributoTable(literal))
                        continue;

                    // Excluir literales que están dentro de un atributo Column
                    if (EstaEnAtributoColumn(literal))
                        continue;

                    // Verificar si este literal es parte de una expresión unaria (como -10)
                    var padreUnario = literal.Parent as PrefixUnaryExpressionSyntax;
                    if (padreUnario != null && padreUnario.IsKind(SyntaxKind.UnaryMinusExpression))
                    {
                        // Si es parte de una expresión unaria, omitir el literal individual
                        continue;
                    }

                    // Excluir literales que están en inicializadores de propiedades (valores por defecto)
                    if (EstaEnInicializadorPropiedad(literal))
                        continue;

                    // Excluir inicializadores de campos con valor 0 (práctica común para contadores)
                    if (EstaEnInicializadorCampoConCero(literal))
                        continue;

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

                    // Excluir expresiones unarias que están dentro de atributos HTTP de ASP.NET Core
                    if (EstaEnAtributoHttp(unary))
                        continue;

                    // Excluir expresiones unarias que están dentro de un atributo Table
                    if (EstaEnAtributoTable(unary))
                        continue;

                    // Excluir expresiones unarias que están dentro de un atributo Column
                    if (EstaEnAtributoColumn(unary))
                        continue;

                    // Excluir expresiones unarias que están en inicializadores de propiedades (valores por defecto)
                    if (EstaEnInicializadorPropiedadUnaria(unary))
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

                        // Excluir contenido de cadenas interpoladas que están dentro de atributos HTTP de ASP.NET Core
                        if (EstaEnAtributoHttp(contenido))
                            continue;

                        // Excluir contenido de cadenas interpoladas que están dentro de un atributo Table
                        if (EstaEnAtributoTable(contenido))
                            continue;

                        // Excluir contenido de cadenas interpoladas que están dentro de un atributo Column
                        if (EstaEnAtributoColumn(contenido))
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

            // LÓGICA MEJORADA: Si un valor "comúnmente aceptable" ya tiene una constante disponible,
            // entonces SÍ debe ser detectado como hardcodeado para forzar el uso de la constante
            if (EsValorExcluidoPeroTieneConstante(valorLiteral, constantesExistentes))
                return true;

            // Excluir valores que realmente no necesitan ser constantes
            if (EsValorExcluido(valorLiteral))
                return false;

            // Todos los demás valores literales deben ser detectados como hardcodeados
            return true;
        }

        private static bool EsValorExcluidoPeroTieneConstante(string valorLiteral, HashSet<string> constantesExistentes)
        {
            // Si el valor literal es "comúnmente aceptable" pero existe una constante con ese valor,
            // entonces debe ser detectado para forzar el uso de la constante
            var valoresComunes = new HashSet<string> { "0", "1", "-1", "true", "false" };
            
            return valoresComunes.Contains(valorLiteral) && constantesExistentes.Contains(valorLiteral);
        }

        private static bool EsValorExcluido(string valorLiteral)
        {
            // Solo excluir valores que realmente no tienen sentido como constantes
            var valoresExcluidos = new HashSet<string>
            {
                "null",
                "\"\"",  // cadena vacía
                "\" \""  // cadena con solo espacio
                // REMOVIDO: "0" - ahora SÍ debe ser detectado como hardcodeado
                // REMOVIDO: "false" y "true" - estos SÍ deben ser detectados como hardcodeados
                // Los números y booleanos hardcodeados deben ser constantes para mayor claridad
            };

            if (valoresExcluidos.Contains(valorLiteral))
                return true;

            // NO excluir números enteros como 0, 1, 2, 3, etc. - estos DEBEN ser detectados
            // NO excluir booleanos true/false - estos también DEBEN ser detectados
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
            // YA NO EXCLUIR variables locales - los literales en variables locales también deben ser constantes
            // Las declaraciones como "int valor = -1;" DEBEN ser detectadas como hardcodeadas
            
            // Buscar si está en una asignación simple (variable = valor) pero NO en declaración inicial
            var assignment = unary.Ancestors().OfType<AssignmentExpressionSyntax>().FirstOrDefault();
            if (assignment != null && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                // Verificar que no esté en una declaración de variable (eso debe detectarse)
                var enDeclaracion = assignment.Ancestors().OfType<VariableDeclarationSyntax>().Any();
                if (!enDeclaracion)
                {
                    return true; // Es una reasignación simple, excluir
                }
            }

            return false; // No está en asignación excluible, debe ser detectado
        }

        private static bool EstaEnAsignacionVariable(LiteralExpressionSyntax literal)
        {
            // Las strings siempre deben ser detectadas, incluso si están en variables
            if (literal.Token.IsKind(SyntaxKind.StringLiteralToken))
                return false;

            // NO excluir si está en un inicializador de objeto (como { Id = 1 })
            var inicializadorObjeto = literal.Ancestors().OfType<InitializerExpressionSyntax>().FirstOrDefault();
            if (inicializadorObjeto != null)
            {
                return false; // Debe ser detectado aunque esté en inicializador
            }

            // YA NO EXCLUIR variables locales - los literales en variables locales también deben ser constantes
            // Las declaraciones como "int pagina = 1;" DEBEN ser detectadas como hardcodeadas
            
            // Buscar si está en una asignación simple (variable = valor) pero NO en declaración inicial
            var assignment = literal.Ancestors().OfType<AssignmentExpressionSyntax>().FirstOrDefault();
            if (assignment != null && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                // Verificar que no esté en una declaración de variable (eso debe detectarse)
                var enDeclaracion = assignment.Ancestors().OfType<VariableDeclarationSyntax>().Any();
                if (!enDeclaracion)
                {
                    // NO excluir si el literal está en una expresión aritmética (como + 1, - 2, * 3, etc.)
                    var enExpresionAritmetica = literal.Ancestors().OfType<BinaryExpressionSyntax>().Any(binary =>
                        binary.IsKind(SyntaxKind.AddExpression) ||
                        binary.IsKind(SyntaxKind.SubtractExpression) ||
                        binary.IsKind(SyntaxKind.MultiplyExpression) ||
                        binary.IsKind(SyntaxKind.DivideExpression) ||
                        binary.IsKind(SyntaxKind.ModuloExpression));
                    
                    if (enExpresionAritmetica)
                    {
                        return false; // Es aritmética, DEBE ser detectado como hardcodeado
                    }

                    // Solo excluir si NO está dentro de un inicializador de objeto
                    var enInicializador = assignment.Ancestors().OfType<InitializerExpressionSyntax>().Any();
                    if (!enInicializador)
                    {
                        return true; // Es una reasignación simple fuera de inicializador, excluir
                    }
                }
            }

            return false; // No está en asignación excluible, debe ser detectado
        }

        private static bool EstaEnAtributoRoute(SyntaxNode nodo)
        {
            var atributo = nodo.Ancestors().OfType<AttributeSyntax>().FirstOrDefault();
            if (atributo == null)
                return false;

            var nombre = atributo.Name.ToString();
            return nombre.IndexOf("Route", System.StringComparison.OrdinalIgnoreCase) != -1;
        }

        private static bool EstaEnAtributoHttp(SyntaxNode nodo)
        {
            var atributo = nodo.Ancestors().OfType<AttributeSyntax>().FirstOrDefault();
            if (atributo == null)
                return false;

            var nombre = atributo.Name.ToString();
            return nombre.IndexOf("HttpGet", System.StringComparison.OrdinalIgnoreCase) != -1 ||
                   nombre.IndexOf("HttpPost", System.StringComparison.OrdinalIgnoreCase) != -1 ||
                   nombre.IndexOf("HttpPut", System.StringComparison.OrdinalIgnoreCase) != -1 ||
                   nombre.IndexOf("HttpDelete", System.StringComparison.OrdinalIgnoreCase) != -1 ||
                   nombre.IndexOf("HttpPatch", System.StringComparison.OrdinalIgnoreCase) != -1 ||
                   nombre.IndexOf("HttpHead", System.StringComparison.OrdinalIgnoreCase) != -1 ||
                   nombre.IndexOf("HttpOptions", System.StringComparison.OrdinalIgnoreCase) != -1;
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

        private static bool EstaEnAtributoTable(SyntaxNode nodo)
        {
            var atributo = nodo.Ancestors().OfType<AttributeSyntax>().FirstOrDefault();
            if (atributo == null)
                return false;

            var nombre = atributo.Name.ToString();
            return nombre.IndexOf("Table", System.StringComparison.OrdinalIgnoreCase) != -1;
        }

        private static bool EstaEnAtributoColumn(SyntaxNode nodo)
        {
            var atributo = nodo.Ancestors().OfType<AttributeSyntax>().FirstOrDefault();
            if (atributo == null)
                return false;

            var nombre = atributo.Name.ToString();
            return nombre.IndexOf("Column", System.StringComparison.OrdinalIgnoreCase) != -1;
        }

        private static bool TieneAtributoTable(TypeDeclarationSyntax tipoDeclaracion)
        {
            foreach (var listaAtributos in tipoDeclaracion.AttributeLists)
            {
                foreach (var atributo in listaAtributos.Attributes)
                {
                    var nombre = atributo.Name.ToString();
                    if (nombre.IndexOf("Table", System.StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool EstaEnInicializadorPropiedad(LiteralExpressionSyntax literal)
        {
            // Verificar si el literal está en un inicializador de propiedad (= valor)
            var equalsValueClause = literal.Ancestors().OfType<EqualsValueClauseSyntax>().FirstOrDefault();
            if (equalsValueClause != null)
            {
                // Verificar si está en una declaración de propiedad
                var propertyDeclaration = equalsValueClause.Ancestors().OfType<PropertyDeclarationSyntax>().FirstOrDefault();
                if (propertyDeclaration != null)
                {
                    return true; // Es un valor por defecto de propiedad, excluir
                }
            }

            return false; // No está en inicializador de propiedad, debe ser detectado
        }

        private static bool EstaEnInicializadorPropiedadUnaria(PrefixUnaryExpressionSyntax unary)
        {
            // Verificar si la expresión unaria está en un inicializador de propiedad (= valor)
            var equalsValueClause = unary.Ancestors().OfType<EqualsValueClauseSyntax>().FirstOrDefault();
            if (equalsValueClause != null)
            {
                // Verificar si está en una declaración de propiedad
                var propertyDeclaration = equalsValueClause.Ancestors().OfType<PropertyDeclarationSyntax>().FirstOrDefault();
                if (propertyDeclaration != null)
                {
                    return true; // Es un valor por defecto de propiedad, excluir
                }
            }

            return false; // No está en inicializador de propiedad, debe ser detectado
        }

        private static bool EstaEnInicializadorCampoConCero(LiteralExpressionSyntax literal)
        {
            // Solo aplicar esta exclusión para el valor "0"
            if (literal.ToString() != "0")
                return false;

            // Verificar si el literal está en un inicializador de campo (= valor)
            var equalsValueClause = literal.Ancestors().OfType<EqualsValueClauseSyntax>().FirstOrDefault();
            if (equalsValueClause != null)
            {
                // Verificar si está en una declaración de campo
                var fieldDeclaration = equalsValueClause.Ancestors().OfType<FieldDeclarationSyntax>().FirstOrDefault();
                if (fieldDeclaration != null)
                {
                    // Excluir solo campos privados con valor 0 (práctica común para contadores)
                    var esPrivado = fieldDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword));
                    if (esPrivado)
                    {
                        return true; // Es un campo privado inicializado con 0, excluir
                    }
                }
            }

            return false; // No es un campo privado con 0, debe ser detectado
        }

        private class LiteralInfo
        {
            public string Valor { get; set; }
            public Location Ubicacion { get; set; }
        }
    }
}

