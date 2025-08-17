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
    public class DetectarUsingsNoUtilizados : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA002";
        private const string Titulo = "Using no utilizado";
        private const string FormatoMensaje = "El using '{0}' no se está utilizando y debería ser removido";
        private const string Descripcion = "Los usings que no se utilizan en el archivo deben ser removidos para mantener el código limpio y mejorar el rendimiento de compilación.";
        private const string Categoria = "Style";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0005");

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
            var modeloSemantico = contexto.SemanticModel;

            // Obtener todos los usings del archivo
            var usings = unidadCompilacion.Usings;
            if (!usings.Any())
                return;

            // Obtener todos los símbolos utilizados en el archivo
            var simbolosUtilizados = ObtenerSimbolosUtilizados(unidadCompilacion, modeloSemantico);

            // Verificar cada using
            foreach (var usingDirective in usings)
            {
                if (EsUsingNoUtilizado(usingDirective, simbolosUtilizados, modeloSemantico))
                {
                    var nombreUsing = ObtenerNombreUsing(usingDirective);
                    var diagnostico = Diagnostic.Create(
                        Regla,
                        usingDirective.GetLocation(),
                        nombreUsing);

                    contexto.ReportDiagnostic(diagnostico);
                }
            }
        }

        private static HashSet<string> ObtenerSimbolosUtilizados(CompilationUnitSyntax unidadCompilacion, SemanticModel modeloSemantico)
        {
            var simbolosUtilizados = new HashSet<string>();
            var nodos = unidadCompilacion.DescendantNodes();

            foreach (var nodo in nodos)
            {
                // Analizar diferentes tipos de nodos que pueden usar símbolos
                switch (nodo)
                {
                    case IdentifierNameSyntax identificador:
                        AgregarSimbolo(identificador, modeloSemantico, simbolosUtilizados);
                        break;

                    case QualifiedNameSyntax nombreCalificado:
                        AgregarSimbolo(nombreCalificado, modeloSemantico, simbolosUtilizados);
                        break;

                    case GenericNameSyntax nombreGenerico:
                        AgregarSimbolo(nombreGenerico, modeloSemantico, simbolosUtilizados);
                        break;

                    case AttributeSyntax atributo:
                        AgregarSimbolo(atributo.Name, modeloSemantico, simbolosUtilizados);
                        break;

                    case  TypeSyntax tipoNodo when!(tipoNodo.Parent is UsingDirectiveSyntax):
                            AgregarSimbolo(tipoNodo, modeloSemantico, simbolosUtilizados);
                        break;
                }
            }

            return simbolosUtilizados;
        }

        private static void AgregarSimbolo(SyntaxNode nodo, SemanticModel modeloSemantico, HashSet<string> simbolosUtilizados)
        {
            try
            {
                var infoSimbolo = modeloSemantico.GetSymbolInfo(nodo);
                var simbolo = infoSimbolo.Symbol ?? infoSimbolo.CandidateSymbols.FirstOrDefault();

                if (simbolo != null)
                {
                    // Agregar el namespace del símbolo
                    var espacioNombres = ObtenerEspacioNombres(simbolo);
                    if (!string.IsNullOrEmpty(espacioNombres))
                    {
                        simbolosUtilizados.Add(espacioNombres);
                        
                        // También agregar namespaces padre
                        var partesNamespace = espacioNombres.Split('.');
                        var espacioNombresParcial = "";
                        foreach (var parte in partesNamespace)
                        {
                            espacioNombresParcial = string.IsNullOrEmpty(espacioNombresParcial) 
                                ? parte 
                                : $"{espacioNombresParcial}.{parte}";
                            simbolosUtilizados.Add(espacioNombresParcial);
                        }
                    }
                }
            }
            catch
            {
                // Ignorar errores de análisis semántico
            }
        }

        private static string ObtenerEspacioNombres(ISymbol simbolo)
        {
            var espacioNombres = simbolo.ContainingNamespace;
            if (espacioNombres == null || espacioNombres.IsGlobalNamespace)
                return string.Empty;

            return espacioNombres.ToDisplayString();
        }

        private static bool EsUsingNoUtilizado(UsingDirectiveSyntax usingDirective, HashSet<string> simbolosUtilizados, SemanticModel modeloSemantico)
        {
            // Excluir usings especiales
            if (usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
                return false;

            if (usingDirective.Alias != null)
                return false; // No analizar using aliases por ahora

            if (usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
                return false; // No analizar using static por ahora

            var nombreUsing = ObtenerNombreUsing(usingDirective);
            
            // Verificar si el using se está utilizando
            return !simbolosUtilizados.Contains(nombreUsing);
        }

        private static string ObtenerNombreUsing(UsingDirectiveSyntax usingDirective)
        {
            return usingDirective.Name?.ToString() ?? string.Empty;
        }
    }
}
