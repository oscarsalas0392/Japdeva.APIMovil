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
    public class LimitarLineasCodigo : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA001";
        private const string Titulo = "La clase excede el límite de 150 líneas";
        private const string FormatoMensaje = "La clase '{0}' tiene {1} líneas efectivas, que excede el límite de 150 líneas";
        private const string Descripcion = "Las clases no deben exceder las 150 líneas de código (excluyendo comentarios, usings, espacios en blanco y atributos).";
        private const string Categoria = "Maintainability";
        private const int LineasMaximas = 150;

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            Titulo,
            FormatoMensaje,
            Categoria,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Descripcion,
            helpLinkUri: "https://docs.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarDeclaracionClase, SyntaxKind.ClassDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarDeclaracionStruct, SyntaxKind.StructDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarDeclaracionRecord, SyntaxKind.RecordDeclaration);
        }

        private static void AnalizarDeclaracionClase(SyntaxNodeAnalysisContext contexto)
        {
            var declaracionClase = (ClassDeclarationSyntax)contexto.Node;
            AnalizarDeclaracionTipo(contexto, declaracionClase, declaracionClase.Identifier);
        }

        private static void AnalizarDeclaracionStruct(SyntaxNodeAnalysisContext contexto)
        {
            var declaracionStruct = (StructDeclarationSyntax)contexto.Node;
            AnalizarDeclaracionTipo(contexto, declaracionStruct, declaracionStruct.Identifier);
        }

        private static void AnalizarDeclaracionRecord(SyntaxNodeAnalysisContext contexto)
        {
            var declaracionRecord = (RecordDeclarationSyntax)contexto.Node;
            AnalizarDeclaracionTipo(contexto, declaracionRecord, declaracionRecord.Identifier);
        }

        private static void AnalizarDeclaracionTipo(SyntaxNodeAnalysisContext contexto, SyntaxNode declaracionTipo, SyntaxToken identificador)
        {
            var lineasEfectivas = ContarLineasEfectivas(declaracionTipo);

            if (lineasEfectivas > LineasMaximas)
            {
                var diagnostico = Diagnostic.Create(
                    Regla,
                    identificador.GetLocation(),
                    identificador.ValueText,
                    lineasEfectivas);

                contexto.ReportDiagnostic(diagnostico);
            }
        }

        private static int ContarLineasEfectivas(SyntaxNode declaracionTipo)
        {
            var textoFuente = declaracionTipo.GetText();
            var lineas = textoFuente.Lines;
            var lineasEfectivas = 0;

            foreach (var linea in lineas)
            {
                var textoLinea = linea.ToString().Trim();

                // Excluir líneas vacías
                if (string.IsNullOrWhiteSpace(textoLinea))
                    continue;

                // Excluir comentarios de línea completa
                if (textoLinea.StartsWith("//"))
                    continue;

                // Excluir comentarios de bloque que ocupan línea completa
                if (textoLinea.StartsWith("/*") || textoLinea.StartsWith("*") || textoLinea.Equals("*/"))
                    continue;

                // Excluir documentación XML
                if (textoLinea.StartsWith("///"))
                    continue;

                // Excluir usings (aunque normalmente están fuera del tipo)
                if (textoLinea.StartsWith("using ") && textoLinea.EndsWith(";"))
                    continue;

                // Excluir líneas que solo contienen llaves de apertura/cierre
                if (textoLinea == "{" || textoLinea == "}")
                    continue;

                // Excluir atributos que ocupan línea completa
                if (textoLinea.StartsWith("[") && textoLinea.EndsWith("]"))
                    continue;

                // Excluir directivas del preprocesador
                if (textoLinea.StartsWith("#"))
                    continue;

                lineasEfectivas++;
            }

            return lineasEfectivas;
        }
    }
}
