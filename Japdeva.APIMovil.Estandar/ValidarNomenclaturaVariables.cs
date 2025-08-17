using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarNomenclaturaVariables : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA008";

        private const string Titulo = "Variable debe usar camelCase";
        private const string FormatoMensaje = "La variable '{0}' debe seguir la convención camelCase (ejemplo: '{1}')";
        private const string Descripcion = "Las variables locales y parámetros de métodos deben usar la convención camelCase (primera letra minúscula, palabras siguientes con primera letra mayúscula) para mantener la consistencia del código.";
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/standard/design-guidelines/capitalization-conventions");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarMetodo, SyntaxKind.MethodDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarConstructor, SyntaxKind.ConstructorDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarAccesor, SyntaxKind.GetAccessorDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarAccesor, SyntaxKind.SetAccessorDeclaration);
        }

        private static void AnalizarMetodo(SyntaxNodeAnalysisContext contexto)
        {
            var metodo = (MethodDeclarationSyntax)contexto.Node;

            // Validar parámetros del método
            ValidarParametros(contexto, metodo.ParameterList);

            // Validar variables locales en el cuerpo del método
            if (metodo.Body != null)
            {
                ValidarVariablesLocales(contexto, metodo.Body);
            }
            else if (metodo.ExpressionBody != null)
            {
                // Para métodos con expression body, no hay variables locales que validar
                return;
            }
        }

        private static void AnalizarConstructor(SyntaxNodeAnalysisContext contexto)
        {
            var constructor = (ConstructorDeclarationSyntax)contexto.Node;

            // Validar parámetros del constructor
            ValidarParametros(contexto, constructor.ParameterList);

            // Validar variables locales en el cuerpo del constructor
            if (constructor.Body != null)
            {
                ValidarVariablesLocales(contexto, constructor.Body);
            }
        }

        private static void AnalizarAccesor(SyntaxNodeAnalysisContext contexto)
        {
            var accesor = (AccessorDeclarationSyntax)contexto.Node;

            // Validar variables locales en el cuerpo del accesor
            if (accesor.Body != null)
            {
                ValidarVariablesLocales(contexto, accesor.Body);
            }
        }

        private static void ValidarParametros(SyntaxNodeAnalysisContext contexto, ParameterListSyntax parametros)
        {
            if (parametros == null)
                return;

            foreach (var parametro in parametros.Parameters)
            {
                var nombreParametro = parametro.Identifier.ValueText;

                if (!EsParametroEspecial(nombreParametro, parametro) && 
                    !EsNombreCamelCase(nombreParametro))
                {
                    var nombreSugerido = ConvertirACamelCase(nombreParametro);
                    
                    var diagnostico = Diagnostic.Create(
                        Regla,
                        parametro.Identifier.GetLocation(),
                        nombreParametro,
                        nombreSugerido);

                    contexto.ReportDiagnostic(diagnostico);
                }
            }
        }

        private static void ValidarVariablesLocales(SyntaxNodeAnalysisContext contexto, SyntaxNode nodo)
        {
            foreach (var descendiente in nodo.DescendantNodes())
            {
                switch (descendiente)
                {
                    case LocalDeclarationStatementSyntax declaracionLocal:
                        ValidarDeclaracionLocal(contexto, declaracionLocal);
                        break;

                    case ForEachStatementSyntax forEachStatement:
                        ValidarVariableForEach(contexto, forEachStatement);
                        break;

                    case ForStatementSyntax forStatement:
                        ValidarVariablesFor(contexto, forStatement);
                        break;

                    case UsingStatementSyntax usingStatement:
                        ValidarVariablesUsing(contexto, usingStatement);
                        break;

                    case TryStatementSyntax tryStatement:
                        ValidarVariablesCatch(contexto, tryStatement);
                        break;
                }
            }
        }

        private static void ValidarDeclaracionLocal(SyntaxNodeAnalysisContext contexto, LocalDeclarationStatementSyntax declaracion)
        {
            foreach (var variable in declaracion.Declaration.Variables)
            {
                var nombreVariable = variable.Identifier.ValueText;

                if (!EsVariableEspecial(nombreVariable, variable) && 
                    !EsNombreCamelCase(nombreVariable))
                {
                    var nombreSugerido = ConvertirACamelCase(nombreVariable);
                    
                    var diagnostico = Diagnostic.Create(
                        Regla,
                        variable.Identifier.GetLocation(),
                        nombreVariable,
                        nombreSugerido);

                    contexto.ReportDiagnostic(diagnostico);
                }
            }
        }

        private static void ValidarVariableForEach(SyntaxNodeAnalysisContext contexto, ForEachStatementSyntax forEachStatement)
        {
            var nombreVariable = forEachStatement.Identifier.ValueText;

            if (!EsVariableEspecialSimple(nombreVariable) && 
                !EsNombreCamelCase(nombreVariable))
            {
                var nombreSugerido = ConvertirACamelCase(nombreVariable);
                
                var diagnostico = Diagnostic.Create(
                    Regla,
                    forEachStatement.Identifier.GetLocation(),
                    nombreVariable,
                    nombreSugerido);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static void ValidarVariablesFor(SyntaxNodeAnalysisContext contexto, ForStatementSyntax forStatement)
        {
            if (forStatement.Declaration != null)
            {
                foreach (var variable in forStatement.Declaration.Variables)
                {
                    var nombreVariable = variable.Identifier.ValueText;

                    if (!EsVariableEspecial(nombreVariable, variable) && 
                        !EsNombreCamelCase(nombreVariable))
                    {
                        var nombreSugerido = ConvertirACamelCase(nombreVariable);
                        
                        var diagnostico = Diagnostic.Create(
                            Regla,
                            variable.Identifier.GetLocation(),
                            nombreVariable,
                            nombreSugerido);

                        contexto.ReportDiagnostic(diagnostico);
                    }
                }
            }
        }

        private static void ValidarVariablesUsing(SyntaxNodeAnalysisContext contexto, UsingStatementSyntax usingStatement)
        {
            if (usingStatement.Declaration != null)
            {
                foreach (var variable in usingStatement.Declaration.Variables)
                {
                    var nombreVariable = variable.Identifier.ValueText;

                    if (!EsVariableEspecial(nombreVariable, variable) && 
                        !EsNombreCamelCase(nombreVariable))
                    {
                        var nombreSugerido = ConvertirACamelCase(nombreVariable);
                        
                        var diagnostico = Diagnostic.Create(
                            Regla,
                            variable.Identifier.GetLocation(),
                            nombreVariable,
                            nombreSugerido);

                        contexto.ReportDiagnostic(diagnostico);
                    }
                }
            }
        }

        private static void ValidarVariablesCatch(SyntaxNodeAnalysisContext contexto, TryStatementSyntax tryStatement)
        {
            foreach (var catchClause in tryStatement.Catches)
            {
                if (catchClause.Declaration?.Identifier != null)
                {
                    var nombreVariable = catchClause.Declaration.Identifier.ValueText;

                    if (!EsVariableEspecialSimple(nombreVariable) && 
                        !EsNombreCamelCase(nombreVariable))
                    {
                        var nombreSugerido = ConvertirACamelCase(nombreVariable);
                        
                        var diagnostico = Diagnostic.Create(
                            Regla,
                            catchClause.Declaration.Identifier.GetLocation(),
                            nombreVariable,
                            nombreSugerido);

                        contexto.ReportDiagnostic(diagnostico);
                    }
                }
            }
        }

        private static bool EsParametroEspecial(string nombreParametro, ParameterSyntax parametro)
        {
            // Excluir parámetros que comienzan con underscore (descartables)
            if (nombreParametro.StartsWith("_"))
                return true;

            // Excluir parámetros comunes de una sola letra (por convención)
            if (nombreParametro.Length == 1 && char.IsLower(nombreParametro[0]))
                return true;

            // Excluir parámetros con atributos especiales
            if (parametro.AttributeLists.Any())
                return true;

            return false;
        }

        private static bool EsVariableEspecial(string nombreVariable, VariableDeclaratorSyntax variable)
        {
            // Excluir variables que comienzan con underscore (descartables)
            if (nombreVariable.StartsWith("_"))
                return true;

            // Excluir variables de una sola letra (contadores, etc.)
            if (nombreVariable.Length == 1 && char.IsLower(nombreVariable[0]))
                return true;

            // Excluir variables comunes para excepciones
            if (nombreVariable.Equals("ex", System.StringComparison.OrdinalIgnoreCase) ||
                nombreVariable.Equals("exception", System.StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        private static bool EsVariableEspecialSimple(string nombreVariable)
        {
            // Excluir variables que comienzan con underscore (descartables)
            if (nombreVariable.StartsWith("_"))
                return true;

            // Excluir variables de una sola letra (contadores, etc.)
            if (nombreVariable.Length == 1 && char.IsLower(nombreVariable[0]))
                return true;

            // Excluir variables comunes para excepciones
            if (nombreVariable.Equals("ex", System.StringComparison.OrdinalIgnoreCase) ||
                nombreVariable.Equals("exception", System.StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        private static bool EsNombreCamelCase(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return false;

            // Patrón para camelCase:
            // - Empieza con letra minúscula
            // - Puede contener letras, números
            // - Las palabras siguientes empiezan con mayúscula
            var patron = @"^[a-z][a-zA-Z0-9]*$";
            
            if (!Regex.IsMatch(nombre, patron))
                return false;

            // Verificar que no sea todo minúsculas con números (ej: "variable1")
            // debe tener al menos una mayúscula después de la primera letra si tiene más de una palabra
            return true;
        }

        private static string ConvertirACamelCase(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return nombre;

            // Si ya está en formato correcto, no cambiar
            if (EsNombreCamelCase(nombre))
                return nombre;

            var resultado = nombre;

            // Convertir PascalCase a camelCase
            if (char.IsUpper(resultado[0]))
            {
                resultado = char.ToLowerInvariant(resultado[0]) + resultado.Substring(1);
            }

            // Convertir snake_case a camelCase
            if (resultado.Contains("_"))
            {
                var partes = resultado.Split('_');
                resultado = partes[0].ToLowerInvariant();
                
                for (int i = 1; i < partes.Length; i++)
                {
                    if (!string.IsNullOrEmpty(partes[i]))
                    {
                        resultado += char.ToUpperInvariant(partes[i][0]) + 
                                   partes[i].Substring(1).ToLowerInvariant();
                    }
                }
            }

            // Convertir SCREAMING_SNAKE_CASE a camelCase
            if (resultado.ToUpperInvariant() == resultado && resultado.Contains("_"))
            {
                var partes = resultado.Split('_');
                resultado = partes[0].ToLowerInvariant();
                
                for (int i = 1; i < partes.Length; i++)
                {
                    if (!string.IsNullOrEmpty(partes[i]))
                    {
                        var parte = partes[i].ToLowerInvariant();
                        resultado += char.ToUpperInvariant(parte[0]) + parte.Substring(1);
                    }
                }
            }

            return resultado;
        }
    }
}
