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
    public class ValidarNomenclaturaParametros : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA035";

        private const string Titulo = "Par�metro de m�todo debe usar camelCase";
        private const string FormatoMensaje = "El par�metro '{0}' debe seguir la convenci�n camelCase (ejemplo: '{1}')";
        private const string Descripcion = "Los par�metros de m�todos, constructores y propiedades deben usar la convenci�n camelCase (primera letra min�scula, palabras siguientes con primera letra may�scula) para mantener la consistencia del c�digo.";
        private const string Categoria = "Naming";

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
            contexto.RegisterSyntaxNodeAction(AnalizarPropiedad, SyntaxKind.PropertyDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarIndexer, SyntaxKind.IndexerDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarDelegate, SyntaxKind.DelegateDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarFuncionLocal, SyntaxKind.LocalFunctionStatement);
        }

        private static void AnalizarMetodo(SyntaxNodeAnalysisContext contexto)
        {
            var metodo = (MethodDeclarationSyntax)contexto.Node;
            ValidarParametros(contexto, metodo.ParameterList, "m�todo", metodo.Identifier.ValueText);
        }

        private static void AnalizarConstructor(SyntaxNodeAnalysisContext contexto)
        {
            var constructor = (ConstructorDeclarationSyntax)contexto.Node;
            ValidarParametros(contexto, constructor.ParameterList, "constructor", constructor.Identifier.ValueText);
        }

        private static void AnalizarPropiedad(SyntaxNodeAnalysisContext contexto)
        {
            var propiedad = (PropertyDeclarationSyntax)contexto.Node;

            // Validar par�metros de accessors (get/set con par�metros)
            if (propiedad.AccessorList != null)
            {
                foreach (var accessor in propiedad.AccessorList.Accessors)
                {
                    // AccessorDeclarationSyntax no tiene una propiedad ParameterList.
                    // Por lo tanto, eliminamos la validaci�n de par�metros para los accessors.
                    // Si se necesita validar algo espec�fico, se debe implementar de otra manera.
                }
            }
        }

        private static void AnalizarIndexer(SyntaxNodeAnalysisContext contexto)
        {
            var indexer = (IndexerDeclarationSyntax)contexto.Node;

            // Convert BracketedParameterListSyntax to a compatible ParameterListSyntax
            var parameterList = SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(indexer.ParameterList.Parameters));

            ValidarParametros(contexto, parameterList, "indexer", "this");
        }

        private static void AnalizarDelegate(SyntaxNodeAnalysisContext contexto)
        {
            var delegateDeclaration = (DelegateDeclarationSyntax)contexto.Node;
            ValidarParametros(contexto, delegateDeclaration.ParameterList, "delegate", delegateDeclaration.Identifier.ValueText);
        }

        private static void AnalizarFuncionLocal(SyntaxNodeAnalysisContext contexto)
        {
            var funcionLocal = (LocalFunctionStatementSyntax)contexto.Node;
            ValidarParametros(contexto, funcionLocal.ParameterList, "funci�n local", funcionLocal.Identifier.ValueText);
        }

        private static void ValidarParametros(SyntaxNodeAnalysisContext contexto, ParameterListSyntax parametros, string tipoMiembro, string nombreMiembro)
        {
            if (parametros == null)
                return;

            foreach (var parametro in parametros.Parameters)
            {
                var nombreParametro = parametro.Identifier.ValueText;

                if (!EsParametroEspecial(nombreParametro, parametro, tipoMiembro, nombreMiembro) && 
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

        private static bool EsParametroEspecial(string nombreParametro, ParameterSyntax parametro, string tipoMiembro, string nombreMiembro)
        {
            // Excluir parámetros que comienzan con underscore (descartables)
            if (nombreParametro.StartsWith("_"))
                return true;

            // Excluir parámetros comunes que ya están en camelCase correcto
            string[] parametrosComunes = new[]
            {
                "configuracion", "logger", "context", "contexto", "services", "servicios",
                "builder", "app", "options", "configuration", "cancellationToken"
            };
            
            if (parametrosComunes.Contains(nombreParametro))
                return true;

            // Excluir parámetros comunes de una sola letra (por convención)
            if (nombreParametro.Length == 1)
            {
                var letrasComunes = new[] { "i", "j", "k", "x", "y", "z", "n", "m", "t" };
                if (letrasComunes.Contains(nombreParametro.ToLower()))
                    return true;
            }

            // Excluir par�metros con atributos especiales
            if (parametro.AttributeLists.Any())
            {
                foreach (var listaAtributos in parametro.AttributeLists)
                {
                    foreach (var atributo in listaAtributos.Attributes)
                    {
                        var nombreAtributo = atributo.Name.ToString();
                        
                        var atributosEspeciales = new[]
                        {
                            "FromRoute", "FromQuery", "FromBody", "FromHeader", "FromForm",
                            "FromServices", "CallerMemberName", "CallerFilePath", "CallerLineNumber",
                            "Optional", "ParamArray", "Out", "Ref", "In"
                        };

                        if (atributosEspeciales.Any(a => 
                            nombreAtributo.IndexOf(a, System.StringComparison.OrdinalIgnoreCase) != -1))
                        {
                            return true;
                        }
                    }
                }
            }

            // Excluir par�metros espec�ficos por tipo de miembro
            switch (tipoMiembro)
            {
                case "m�todo":
                    // Excluir m�todos de test
                    if (EsMetodoTest(nombreMiembro))
                        return true;
                    break;

                case "constructor":
                    // Los constructores pueden tener par�metros especiales
                    break;

                case "delegate":
                    // Los delegates pueden tener convenciones especiales
                    if (nombreParametro.Equals("sender", System.StringComparison.OrdinalIgnoreCase) ||
                        nombreParametro.Equals("e", System.StringComparison.OrdinalIgnoreCase))
                        return true;
                    break;
            }

            // Excluir par�metros con modificadores especiales
            if (parametro.Modifiers.Any(m => 
                m.IsKind(SyntaxKind.RefKeyword) || 
                m.IsKind(SyntaxKind.OutKeyword) || 
                m.IsKind(SyntaxKind.InKeyword) ||
                m.IsKind(SyntaxKind.ParamsKeyword)))
            {
                // Para estos casos, ser m�s flexible
                if (nombreParametro.Length <= 3)
                    return true;
            }

            // Excluir par�metros comunes en controllers
            var parametrosControllerComunes = new[]
            {
                "id", "Id", "ID", "cancellationToken", "httpContext"
            };

            if (parametrosControllerComunes.Contains(nombreParametro))
                return true;

            return false;
        }

        private static bool EsMetodoTest(string nombreMetodo)
        {
            var patronesTest = new[]
            {
                "Test", "Should", "When", "Given", "Arrange", "Act", "Assert",
                "Fact", "Theory", "TestCase", "Setup", "TearDown"
            };

            return patronesTest.Any(patron => 
                nombreMetodo.IndexOf(patron, System.StringComparison.OrdinalIgnoreCase) != -1);
        }

        private static bool EsNombreCamelCase(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return false;

            // Patr�n para camelCase:
            // - Empieza con letra min�scula
            // - Puede contener letras, n�meros
            // - Las palabras siguientes empiezan con may�scula
            var patron = @"^[a-z][a-zA-Z0-9]*$";
            
            if (!Regex.IsMatch(nombre, patron))
                return false;

            // Verificaciones adicionales para mejor camelCase
            // No debe ser todo min�sculas cuando tiene m�ltiples palabras evidentes
            if (nombre.Length > 15 && nombre.ToLowerInvariant() == nombre)
            {
                // Si es muy largo y todo min�sculas, probablemente no es camelCase correcto
                return false;
            }

            return true;
        }

        private static string ConvertirACamelCase(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return nombre;

            // Si ya est� en formato correcto, no cambiar
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

            // Convertir kebab-case a camelCase
            if (resultado.Contains("-"))
            {
                var partes = resultado.Split('-');
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

            // Limpiar caracteres especiales al inicio
            resultado = Regex.Replace(resultado, @"^[^a-zA-Z]+", "");
            
            // Si queda vac�o, usar nombre por defecto
            if (string.IsNullOrEmpty(resultado))
                return "parameter";

            // Asegurar que empiece con min�scula
            if (char.IsUpper(resultado[0]))
            {
                resultado = char.ToLowerInvariant(resultado[0]) + resultado.Substring(1);
            }

            return resultado;
        }
    }
}


