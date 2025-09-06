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
    public class DetectarCamposNoUtilizados : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA038";
        private const string Titulo = "Campo o constante no utilizado";
        private const string FormatoMensaje = "El {0} '{1}' está declarado pero nunca se utiliza";
        private const string Descripcion = "Los campos y constantes que se declaran pero nunca se utilizan deben ser removidos para mantener el código limpio y reducir la complejidad.";
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

        public override void Initialize(AnalysisContext contexto)
        {
            contexto.EnableConcurrentExecution();
            contexto.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            contexto.RegisterSyntaxNodeAction(AnalizarClase, SyntaxKind.ClassDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarStruct, SyntaxKind.StructDeclaration);
            contexto.RegisterSyntaxNodeAction(AnalizarRecord, SyntaxKind.RecordDeclaration);
        }

        private static void AnalizarClase(SyntaxNodeAnalysisContext contexto)
        {
            var clase = (ClassDeclarationSyntax)contexto.Node;
            AnalizarTipoParaCampos(contexto, clase, clase.Members);
        }

        private static void AnalizarStruct(SyntaxNodeAnalysisContext contexto)
        {
            var estructura = (StructDeclarationSyntax)contexto.Node;
            AnalizarTipoParaCampos(contexto, estructura, estructura.Members);
        }

        private static void AnalizarRecord(SyntaxNodeAnalysisContext contexto)
        {
            var record = (RecordDeclarationSyntax)contexto.Node;
            AnalizarTipoParaCampos(contexto, record, record.Members);
        }

        private static void AnalizarTipoParaCampos(SyntaxNodeAnalysisContext contexto, SyntaxNode tipo, SyntaxList<MemberDeclarationSyntax> miembros)
        {
            // Recopilar todos los campos y constantes declarados
            var camposDeclarados = new Dictionary<string, (MemberDeclarationSyntax declaracion, string tipoMiembro)>();

            foreach (var miembro in miembros)
            {
                if (miembro is FieldDeclarationSyntax campo)
                {
                    foreach (var variable in campo.Declaration.Variables)
                    {
                        var nombre = variable.Identifier.ValueText;
                        var tipoMiembro = campo.Modifiers.Any(SyntaxKind.ConstKeyword) ? "constante" : "campo";
                        camposDeclarados[nombre] = (campo, tipoMiembro);
                    }
                }
            }

            if (!camposDeclarados.Any())
                return;

            // Buscar usages de los campos en todo el tipo
            var camposUtilizados = new HashSet<string>();

            // Analizar todos los nodos del tipo para encontrar referencias
            var todosLosNodos = tipo.DescendantNodes();

            foreach (var nodo in todosLosNodos)
            {
                if (nodo is IdentifierNameSyntax identificador)
                {
                    var nombre = identificador.Identifier.ValueText;
                    if (camposDeclarados.ContainsKey(nombre))
                    {
                        // Verificar que no sea la declaración misma
                        var padreVariable = identificador.Ancestors().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
                        if (padreVariable == null || padreVariable.Identifier.ValueText != nombre)
                        {
                            camposUtilizados.Add(nombre);
                        }
                    }
                }
                else if (nodo is MemberAccessExpressionSyntax accesoMiembro)
                {
                    var nombre = accesoMiembro.Name.Identifier.ValueText;
                    if (camposDeclarados.ContainsKey(nombre))
                    {
                        camposUtilizados.Add(nombre);
                    }
                }
            }

            // Reportar campos no utilizados
            foreach (var kvp in camposDeclarados)
            {
                var nombreCampo = kvp.Key;
                var (declaracion, tipoMiembro) = kvp.Value;

                if (!camposUtilizados.Contains(nombreCampo))
                {
                    // Excluir campos que pueden ser utilizados por serialización o frameworks
                    if (EsCampoEspecial(declaracion as FieldDeclarationSyntax))
                        continue;

                    var diagnostic = Diagnostic.Create(
                        Regla,
                        declaracion.GetLocation(),
                        tipoMiembro,
                        nombreCampo);

                    contexto.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static bool EsCampoEspecial(FieldDeclarationSyntax campo)
        {
            if (campo == null)
                return false;

            // Excluir campos públicos (pueden ser utilizados externamente)
            if (campo.Modifiers.Any(SyntaxKind.PublicKeyword))
                return true;

            // Excluir campos con atributos (pueden ser utilizados por frameworks)
            if (campo.AttributeLists.Any())
                return true;

            // Excluir campos protected (pueden ser utilizados por clases derivadas)
            if (campo.Modifiers.Any(SyntaxKind.ProtectedKeyword))
                return true;

            return false;
        }
    }
}
