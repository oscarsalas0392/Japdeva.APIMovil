using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    /// <summary>
    /// Analizador que valida que los using statements estén ordenados correctamente:
    /// 1. System.*
    /// 2. Microsoft.*
    /// 3. Librerías de terceros
    /// 4. Namespaces del proyecto (Japdeva.*)
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarOrdenUsings : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA017";

        private const string Titulo = "Using statements deben estar ordenados correctamente";
        private const string FormatoMensaje = "El using '{0}' no está en el orden correcto. Orden esperado: System.*, Microsoft.*, terceros, Japdeva.*";
        private const string Descripcion = "Los using statements deben estar ordenados en grupos específicos: System.*, Microsoft.*, librerías de terceros, namespaces del proyecto (Japdeva.*). Esto mejora la legibilidad y organización del código.";
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0065");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarUnidadCompilacion, SyntaxKind.CompilationUnit);
        }

        private static void AnalizarUnidadCompilacion(SyntaxNodeAnalysisContext contexto)
        {
            var unidadCompilacion = (CompilationUnitSyntax)contexto.Node;

            // Obtener todos los using statements
            var usings = unidadCompilacion.Usings.ToList();
            if (usings.Count <= 1)
                return; // No hay nada que validar si hay 0 o 1 using

            // Validar el orden de los usings
            ValidarOrdenDeUsings(contexto, usings);
        }

        private static void ValidarOrdenDeUsings(SyntaxNodeAnalysisContext contexto, System.Collections.Generic.List<UsingDirectiveSyntax> usings)
        {
            // Crear una lista ordenada esperada para comparar
            var usingsSortedCorrectly = usings.OrderBy(u => ObtenerGrupoUsing(ObtenerNombreNamespace(u)))
                                              .ThenBy(u => ObtenerNombreNamespace(u))
                                              .ToList();

            // Comparar cada using con su posición esperada
            for (int i = 0; i < usings.Count; i++)
            {
                string nombreActual = ObtenerNombreNamespace(usings[i]);
                string nombreEsperado = ObtenerNombreNamespace(usingsSortedCorrectly[i]);

                if (nombreActual != nombreEsperado)
                {
                    var diagnostico = Diagnostic.Create(
                        Regla,
                        usings[i].GetLocation(),
                        nombreActual);

                    contexto.ReportDiagnostic(diagnostico);
                }
            }
        }

        /// <summary>
        /// Obtiene el nombre del namespace del using statement.
        /// </summary>
        /// <param name="usingStatement">Using statement a evaluar.</param>
        /// <returns>Nombre del namespace.</returns>
        private static string ObtenerNombreNamespace(UsingDirectiveSyntax usingStatement)
        {
            return usingStatement.Name?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Determina el grupo al que pertenece el using statement.
        /// </summary>
        /// <param name="nombreNamespace">Nombre del namespace.</param>
        /// <returns>Número del grupo (0=System, 1=Microsoft, 2=Terceros, 3=Japdeva).</returns>
        private static int ObtenerGrupoUsing(string nombreNamespace)
        {
            if (string.IsNullOrEmpty(nombreNamespace))
                return 2; // Terceros por defecto

            // Grupo 0: System.*
            if (nombreNamespace.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                return 0;

            // Grupo 1: Microsoft.*
            if (nombreNamespace.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase))
                return 1;

            // Grupo 3: Japdeva.* (proyecto local)
            if (nombreNamespace.StartsWith("Japdeva", StringComparison.OrdinalIgnoreCase))
                return 3;

            // Grupo 2: Terceros (todo lo demás)
            return 2;
        }
    }
}
