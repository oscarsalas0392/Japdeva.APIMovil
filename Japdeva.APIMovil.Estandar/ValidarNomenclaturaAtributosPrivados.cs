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
    public class ValidarNomenclaturaAtributosPrivados : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA011";

        private const string Titulo = "Atributo privado debe usar _camelCase";
        private const string FormatoMensaje = "El {0} privado '{1}' debe seguir la convención _camelCase (ejemplo: '{2}')";
        private const string Descripcion = "Los campos privados de las clases deben usar la convención _camelCase (guion bajo seguido de camelCase) para distinguirlos claramente de otros miembros y mantener la consistencia del código.";
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/standard/design-guidelines/names-of-type-members#names-of-fields");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarCampo, SyntaxKind.FieldDeclaration);
        }

        private static void AnalizarCampo(SyntaxNodeAnalysisContext contexto)
        {
            var campo = (FieldDeclarationSyntax)contexto.Node;

            // Solo analizar campos privados
            var esPrivado = !campo.Modifiers.Any(m => 
                m.IsKind(SyntaxKind.PublicKeyword) || 
                m.IsKind(SyntaxKind.ProtectedKeyword) || 
                m.IsKind(SyntaxKind.InternalKeyword));
            
            if (!esPrivado)
                return;

            // Excluir constantes (ya tienen su propia regla)
            var esConstante = campo.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword));
            if (esConstante)
                return;

            // Analizar cada variable en la declaración del campo
            foreach (var variable in campo.Declaration.Variables)
            {
                var nombreCampo = variable.Identifier.ValueText;
                ValidarNomenclaturaCampoPrivado(contexto, campo, variable, nombreCampo);
            }
        }

        private static void ValidarNomenclaturaCampoPrivado(SyntaxNodeAnalysisContext contexto, 
            FieldDeclarationSyntax campo, VariableDeclaratorSyntax variable, string nombreCampo)
        {
            if (!EsCampoPrivadoEspecial(nombreCampo, campo, variable) && 
                !EsNombreUnderscoreCamelCase(nombreCampo))
            {
                var nombreSugerido = ConvertirAUnderscoreCamelCase(nombreCampo);
                var tipoMiembro = ObtenerTipoMiembro(campo);
                
                var diagnostico = Diagnostic.Create(
                    Regla,
                    variable.Identifier.GetLocation(),
                    tipoMiembro,
                    nombreCampo,
                    nombreSugerido);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static string ObtenerTipoMiembro(FieldDeclarationSyntax campo)
        {
            var esReadonly = campo.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));
            var esStatic = campo.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

            if (esStatic && esReadonly)
                return "campo static readonly";
            if (esStatic)
                return "campo static";
            if (esReadonly)
                return "campo readonly";
            
            return "campo";
        }

        private static bool EsCampoPrivadoEspecial(string nombreCampo, FieldDeclarationSyntax campo, VariableDeclaratorSyntax variable)
        {
            // Excluir campos que ya siguen la convención correcta
            if (EsNombreUnderscoreCamelCase(nombreCampo))
                return true;

            // Excluir campos con múltiples underscores (posibles constantes privadas)
            if (nombreCampo.StartsWith("__") || nombreCampo.Contains("___"))
                return true;

            // Excluir campos generados por el compilador
            if (nombreCampo.Contains("<") || nombreCampo.Contains(">"))
                return true;

            // Excluir campos backing de eventos (suelen terminar en "Handler" o "Event")
            if (nombreCampo.EndsWith("Handler", System.StringComparison.OrdinalIgnoreCase) ||
                nombreCampo.EndsWith("Event", System.StringComparison.OrdinalIgnoreCase))
                return true;

            // Excluir campos que representan delegados o eventos
            var tipoCampo = campo.Declaration.Type.ToString();
            if (tipoCampo.Contains("Action") || tipoCampo.Contains("Func") || 
                tipoCampo.Contains("EventHandler") || tipoCampo.Contains("Delegate"))
                return true;

            // Excluir campos static readonly que podrían ser configuraciones
            var esStaticReadonly = campo.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)) &&
                                  campo.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));
            if (esStaticReadonly)
                return true;

            return false;
        }

        private static bool EsNombreUnderscoreCamelCase(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return false;

            // Patrón para _camelCase:
            // - Empieza con exactamente un underscore
            // - Seguido de letra minúscula
            // - Puede contener letras, números
            // - Las palabras siguientes empiezan con mayúscula
            var patron = @"^_[a-z][a-zA-Z0-9]*$";
            
            return Regex.IsMatch(nombre, patron);
        }

        private static string ConvertirAUnderscoreCamelCase(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return "_" + nombre;

            // Si ya está en formato correcto, no cambiar
            if (EsNombreUnderscoreCamelCase(nombre))
                return nombre;

            var resultado = nombre;

            // Remover underscores existentes al inicio (excepto si ya es correcto)
            resultado = resultado.TrimStart('_');
            
            // Si está vacío después de remover underscores, usar un nombre por defecto
            if (string.IsNullOrEmpty(resultado))
                return "_field";

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

            // Asegurar que empiece con minúscula si no lo está ya
            if (char.IsUpper(resultado[0]))
            {
                resultado = char.ToLowerInvariant(resultado[0]) + resultado.Substring(1);
            }

            // Agregar underscore al inicio
            return "_" + resultado;
        }
    }
}

