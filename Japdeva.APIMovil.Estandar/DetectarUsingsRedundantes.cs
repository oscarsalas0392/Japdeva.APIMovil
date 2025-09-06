using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DetectarUsingsRedundantes : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA039";
        private const string Titulo = "Using redundante con global usings implícitos";
        private const string FormatoMensaje = "El using '{0}' es redundante porque ya está incluido por los global usings implícitos";
        private const string Descripcion = "Los usings que ya están incluidos por global usings implícitos deben ser removidos para evitar redundancia y mantener el código limpio.";
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        // Lista de global usings implícitos para proyectos ASP.NET Core/.NET 9+
        private static readonly HashSet<string> GlobalUsingsImplicitos = new HashSet<string>
        {
            "Microsoft.AspNetCore.Builder",
            "Microsoft.AspNetCore.Hosting",
            "Microsoft.AspNetCore.Http",
            "Microsoft.AspNetCore.Routing",
            "Microsoft.Extensions.Configuration",
            "Microsoft.Extensions.DependencyInjection",
            "Microsoft.Extensions.Hosting",
            "Microsoft.Extensions.Logging",
            "System",
            "System.Collections.Generic",
            "System.IO",
            "System.Linq",
            "System.Net.Http",
            "System.Net.Http.Json",
            "System.Threading",
            "System.Threading.Tasks"
        };

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarUnidadCompilacion, SyntaxKind.CompilationUnit);
        }

        private static void AnalizarUnidadCompilacion(SyntaxNodeAnalysisContext contexto)
        {
            var unidadCompilacion = (CompilationUnitSyntax)contexto.Node;

            // Solo analizar si el proyecto tiene ImplicitUsings habilitado
            if (!TieneImplicitUsingsHabilitado(contexto))
                return;

            // Obtener todos los usings del archivo
            var usings = unidadCompilacion.Usings;
            if (!usings.Any())
                return;

            // Verificar cada using
            foreach (var usingDirective in usings)
            {
                var nombreUsing = ObtenerNombreUsing(usingDirective);
                
                // Verificar si es redundante debido a global usings implícitos
                if (GlobalUsingsImplicitos.Contains(nombreUsing))
                {
                    var diagnostico = Diagnostic.Create(
                        Regla,
                        usingDirective.GetLocation(),
                        nombreUsing);

                    contexto.ReportDiagnostic(diagnostico);
                }
            }
        }

        private static bool TieneImplicitUsingsHabilitado(SyntaxNodeAnalysisContext contexto)
        {
            // Verificar si el proyecto tiene ImplicitUsings habilitado
            // Esto se puede hacer revisando si existen archivos GlobalUsings.g.cs
            // o verificando opciones de compilación
            
            try
            {
                // Buscar archivos que contengan global usings generados automáticamente
                var syntaxTrees = contexto.Compilation.SyntaxTrees;
                return syntaxTrees.Any(tree => 
                    tree.FilePath.Contains("GlobalUsings.g.cs") ||
                    tree.GetRoot().DescendantNodes()
                        .OfType<UsingDirectiveSyntax>()
                        .Any(u => u.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword)));
            }
            catch
            {
                // Si no se puede determinar, asumir que sí tiene ImplicitUsings
                return true;
            }
        }

        private static string ObtenerNombreUsing(UsingDirectiveSyntax usingDirective)
        {
            return usingDirective.Name?.ToString() ?? string.Empty;
        }
    }
}
