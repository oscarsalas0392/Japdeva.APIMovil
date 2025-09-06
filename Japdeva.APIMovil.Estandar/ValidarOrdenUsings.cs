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
    public class ValidarOrdenUsings : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA017";

        private const string Titulo = "Los using statements deben estar ordenados correctamente";
    private const string FormatoMensaje = "El using '{0}' no está en el orden correcto, los using deben agruparse: System.*, terceros, proyecto local";
        private const string Descripcion = "Los using statements deben estar ordenados en grupos espec�ficos: primero System.*, luego librer�as de terceros, finalmente namespaces del proyecto local. Esto mejora la legibilidad y organizaci�n del c�digo.";
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
            var usings = unidadCompilacion.Usings;
            if (!usings.Any())
                return;

            // Analizar el orden de los usings
            ValidarOrdenDeclaracionesUsing(contexto, usings);
        }

        private static void ValidarOrdenDeclaracionesUsing(SyntaxNodeAnalysisContext contexto, SyntaxList<UsingDirectiveSyntax> usings)
        {
            var usingsCategoritzados = CategorizarUsings(usings, contexto);
            var ordenEsperado = ObtenerOrdenEsperado(usingsCategoritzados);
            
            for (int i = 0; i < usings.Count; i++)
            {
                var usingActual = usings[i];
                var nombreUsing = ObtenerNombreUsing(usingActual);
                
                // Saltar usings especiales
                if (EsUsingEspecial(usingActual))
                    continue;

                var categoriaActual = ObtenerCategoriaUsing(nombreUsing, contexto);
                var posicionEsperada = ObtenerPosicionEsperada(nombreUsing, ordenEsperado, categoriaActual);
                
                if (i < posicionEsperada)
                {
                    var diagnostico = Diagnostic.Create(
                        Regla,
                        usingActual.GetLocation(),
                        nombreUsing);

                    contexto.ReportDiagnostic(diagnostico);
                }
            }
        }

        private static Dictionary<CategoriaUsing, List<UsingInfo>> CategorizarUsings(
            SyntaxList<UsingDirectiveSyntax> usings, SyntaxNodeAnalysisContext contexto)
        {
            var categorias = new Dictionary<CategoriaUsing, List<UsingInfo>>
            {
                { CategoriaUsing.System, new List<UsingInfo>() },
                { CategoriaUsing.Terceros, new List<UsingInfo>() },
                { CategoriaUsing.Proyecto, new List<UsingInfo>() },
                { CategoriaUsing.Especial, new List<UsingInfo>() }
            };

            for (int i = 0; i < usings.Count; i++)
            {
                var usingDirective = usings[i];
                var nombreUsing = ObtenerNombreUsing(usingDirective);
                
                var categoria = EsUsingEspecial(usingDirective) 
                    ? CategoriaUsing.Especial 
                    : ObtenerCategoriaUsing(nombreUsing, contexto);

                categorias[categoria].Add(new UsingInfo
                {
                    Directive = usingDirective,
                    Nombre = nombreUsing,
                    PosicionOriginal = i
                });
            }

            return categorias;
        }

        private static List<UsingInfo> ObtenerOrdenEsperado(Dictionary<CategoriaUsing, List<UsingInfo>> categorias)
        {
            var ordenEsperado = new List<UsingInfo>();

            // Orden: Especiales (global, alias, static), System, Terceros, Proyecto
            ordenEsperado.AddRange(categorias[CategoriaUsing.Especial].OrderBy(u => u.Nombre));
            ordenEsperado.AddRange(categorias[CategoriaUsing.System].OrderBy(u => u.Nombre));
            ordenEsperado.AddRange(categorias[CategoriaUsing.Terceros].OrderBy(u => u.Nombre));
            ordenEsperado.AddRange(categorias[CategoriaUsing.Proyecto].OrderBy(u => u.Nombre));

            return ordenEsperado;
        }

        private static int ObtenerPosicionEsperada(string nombreUsing, List<UsingInfo> ordenEsperado, CategoriaUsing categoria)
        {
            for (int i = 0; i < ordenEsperado.Count; i++)
            {
                if (ordenEsperado[i].Nombre == nombreUsing)
                    return i;
            }
            
            return ordenEsperado.Count;
        }

        private static CategoriaUsing ObtenerCategoriaUsing(string nombreUsing, SyntaxNodeAnalysisContext contexto)
        {
            if (string.IsNullOrEmpty(nombreUsing))
                return CategoriaUsing.Especial;

            // System namespaces
            if (EsNamespaceSystem(nombreUsing))
                return CategoriaUsing.System;

            // Namespaces del proyecto
            if (EsNamespaceProyecto(nombreUsing, contexto))
                return CategoriaUsing.Proyecto;

            // Terceros (todo lo dem�s)
            return CategoriaUsing.Terceros;
        }

        private static bool EsNamespaceSystem(string nombreUsing)
        {
            var namespacesSystem = new[]
            {
                "System", "Microsoft.Extensions", "Microsoft.AspNetCore",
                "Microsoft.EntityFrameworkCore", "Microsoft.Data",
                "Microsoft.VisualStudio", "Microsoft.CodeAnalysis"
            };

            return namespacesSystem.Any(ns => 
                nombreUsing.StartsWith(ns, System.StringComparison.OrdinalIgnoreCase));
        }

        private static bool EsNamespaceProyecto(string nombreUsing, SyntaxNodeAnalysisContext contexto)
        {
            // Obtener el namespace ra�z del proyecto actual
            var namespacesProyecto = ObtenerNamespacesProyecto(contexto);
            
            return namespacesProyecto.Any(ns => 
                nombreUsing.StartsWith(ns, System.StringComparison.OrdinalIgnoreCase));
        }

        private static List<string> ObtenerNamespacesProyecto(SyntaxNodeAnalysisContext contexto)
        {
            var namespacesProyecto = new List<string>();
            
            // Obtener el assembly name como indicador del namespace del proyecto
            var assemblyName = contexto.SemanticModel.Compilation.AssemblyName;
            if (!string.IsNullOrEmpty(assemblyName))
            {
                namespacesProyecto.Add(assemblyName);
            }

            // Agregar namespaces comunes del proyecto basados en la estructura
            var rutaArchivo = contexto.Node.SyntaxTree.FilePath;
            if (!string.IsNullOrEmpty(rutaArchivo))
            {
                // Extraer posibles namespaces del proyecto de la ruta
                var partesRuta = rutaArchivo.Replace('\\', '/').Split('/');
                for (int i = 0; i < partesRuta.Length; i++)
                {
                    var parte = partesRuta[i];
                    
                    // Buscar patrones que indiquen namespace del proyecto
                    if (parte.Contains('.') && !parte.EndsWith(".cs") && !parte.EndsWith(".dll"))
                    {
                        namespacesProyecto.Add(parte);
                        
                        // Tambi�n agregar posibles sub-namespaces
                        var subPartes = parte.Split('.');
                        if (subPartes.Length > 1)
                        {
                            namespacesProyecto.Add(subPartes[0]);
                        }
                    }
                }
            }

            // Namespaces comunes que suelen ser del proyecto
            namespacesProyecto.AddRange(new[]
            {
                "Japdeva", "APIMovil", // Basado en el contexto del proyecto
            });

            return namespacesProyecto.Distinct().ToList();
        }

        private static bool EsUsingEspecial(UsingDirectiveSyntax usingDirective)
        {
            // Global usings
            if (usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
                return true;

            // Using alias
            if (usingDirective.Alias != null)
                return true;

            // Using static
            if (usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
                return true;

            return false;
        }

        private static string ObtenerNombreUsing(UsingDirectiveSyntax usingDirective)
        {
            return usingDirective.Name?.ToString() ?? string.Empty;
        }

        private enum CategoriaUsing
        {
            Especial,   // global, alias, static
            System,     // System.*, Microsoft.*
            Terceros,   // Newtonsoft.*, AutoMapper.*, etc.
            Proyecto    // Namespaces del proyecto actual
        }

        private class UsingInfo
        {
            public UsingDirectiveSyntax Directive { get; set; }
            public string Nombre { get; set; }
            public int PosicionOriginal { get; set; }
        }
    }
}

