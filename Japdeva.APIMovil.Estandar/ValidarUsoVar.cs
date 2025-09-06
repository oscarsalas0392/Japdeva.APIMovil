using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarUsoVar : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA043";
        private const string Titulo = "Uso de 'var' no permitido para tipos explícitos";
        private const string FormatoMensaje = "No se debe usar 'var' para el tipo '{0}'. Use el tipo explícito en su lugar";
        private const string Descripcion = "Para mejorar la legibilidad del código, no se debe usar 'var' cuando el tipo es obvio como int, string, double, decimal, DateTime, char, bool, etc.";
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0007",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        // Tipos para los cuales no se debe usar 'var'
        private static readonly ImmutableHashSet<string> TiposProhibidos = ImmutableHashSet.Create(
            // Tipos primitivos básicos
            "int", "System.Int32",
            "string", "System.String", 
            "double", "System.Double",
            "decimal", "System.Decimal",
            "char", "System.Char",
            "bool", "System.Boolean",
            "byte", "System.Byte",
            "sbyte", "System.SByte",
            "short", "System.Int16",
            "ushort", "System.UInt16",
            "uint", "System.UInt32",
            "long", "System.Int64",
            "ulong", "System.UInt64",
            "float", "System.Single",
            
            // Tipos de fecha y hora
            "DateTime", "System.DateTime",
            "DateOnly", "System.DateOnly",
            "TimeOnly", "System.TimeOnly",
            "TimeSpan", "System.TimeSpan",
            "DateTimeOffset", "System.DateTimeOffset",
            
            // Tipos comunes del framework
            "Guid", "System.Guid",
            "object", "System.Object",
            
            // Tipos nullable de primitivos
            "int?", "System.Nullable<System.Int32>", "System.Int32?",
            "string?", "System.String?",
            "double?", "System.Nullable<System.Double>", "System.Double?",
            "decimal?", "System.Nullable<System.Decimal>", "System.Decimal?",
            "char?", "System.Nullable<System.Char>", "System.Char?",
            "bool?", "System.Nullable<System.Boolean>", "System.Boolean?",
            "DateTime?", "System.Nullable<System.DateTime>", "System.DateTime?",
            "DateOnly?", "System.Nullable<System.DateOnly>", "System.DateOnly?",
            "TimeOnly?", "System.Nullable<System.TimeOnly>", "System.TimeOnly?",
            "Guid?", "System.Nullable<System.Guid>", "System.Guid?"
        );

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarDeclaracionVariable, SyntaxKind.VariableDeclaration);
        }

        private static void AnalizarDeclaracionVariable(SyntaxNodeAnalysisContext contexto)
        {
            var declaracion = (VariableDeclarationSyntax)contexto.Node;
            
            // Verificar si usa 'var'
            if (!declaracion.Type.IsVar)
                return;

            // Obtener información semántica para determinar el tipo real
            var modeloSemantico = contexto.SemanticModel;
            var tipoInfo = modeloSemantico.GetTypeInfo(declaracion.Type);
            
            if (tipoInfo.Type == null)
                return;

            var tipoCompleto = tipoInfo.Type.ToDisplayString();
            var tipoSimple = tipoInfo.Type.Name;
            var tipoConNamespace = $"{tipoInfo.Type.ContainingNamespace}.{tipoInfo.Type.Name}";

            // Verificar si el tipo está en la lista de tipos prohibidos para 'var'
            if (EsTipoProhibidoParaVar(tipoCompleto, tipoSimple, tipoConNamespace))
            {
                // Obtener el nombre del tipo para mostrar en el mensaje
                var nombreTipoParaMensaje = ObtenerNombreTipoLegible(tipoInfo.Type);
                
                var diagnostico = Diagnostic.Create(
                    Regla,
                    declaracion.Type.GetLocation(),
                    nombreTipoParaMensaje);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static bool EsTipoProhibidoParaVar(string tipoCompleto, string tipoSimple, string tipoConNamespace)
        {
            // Verificar diferentes formas del tipo
            return TiposProhibidos.Contains(tipoCompleto) ||
                   TiposProhibidos.Contains(tipoSimple) ||
                   TiposProhibidos.Contains(tipoConNamespace) ||
                   EsTipoArrayDeTipoProhibido(tipoCompleto) ||
                   EsListaDeTipoProhibido(tipoCompleto);
        }

        private static bool EsTipoArrayDeTipoProhibido(string tipoCompleto)
        {
            // Verificar arrays de tipos prohibidos: int[], string[], etc.
            if (tipoCompleto.EndsWith("[]"))
            {
                var tipoElemento = tipoCompleto.Substring(0, tipoCompleto.Length - 2);
                return TiposProhibidos.Contains(tipoElemento);
            }
            return false;
        }

        private static bool EsListaDeTipoProhibido(string tipoCompleto)
        {
            // Verificar listas de tipos prohibidos: List<int>, List<string>, etc.
            if (tipoCompleto.StartsWith("System.Collections.Generic.List<") && tipoCompleto.EndsWith(">"))
            {
                var inicio = "System.Collections.Generic.List<".Length;
                var tipoElemento = tipoCompleto.Substring(inicio, tipoCompleto.Length - inicio - 1);
                return TiposProhibidos.Contains(tipoElemento);
            }
            return false;
        }

        private static string ObtenerNombreTipoLegible(ITypeSymbol tipo)
        {
            // Convertir el tipo a una representación legible para el mensaje
            var nombreCompleto = tipo.ToDisplayString();
            
            // Mapear algunos tipos comunes a nombres más simples
            switch (nombreCompleto)
            {
                case "System.Int32": return "int";
                case "System.String": return "string";
                case "System.Double": return "double";
                case "System.Decimal": return "decimal";
                case "System.Char": return "char";
                case "System.Boolean": return "bool";
                case "System.DateTime": return "DateTime";
                case "System.DateOnly": return "DateOnly";
                case "System.TimeOnly": return "TimeOnly";
                case "System.Guid": return "Guid";
                case "System.Object": return "object";
                case "System.Single": return "float";
                case "System.Byte": return "byte";
                case "System.Int16": return "short";
                case "System.Int64": return "long";
                default:
                    // Para arrays y otros tipos, usar el nombre completo simplificado
                    return nombreCompleto
                        .Replace("System.Collections.Generic.List<", "List<")
                        .Replace("System.", "")
                        .Replace("Int32", "int")
                        .Replace("String", "string")
                        .Replace("Double", "double")
                        .Replace("Boolean", "bool");
            }
        }
    }
}
