using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Japdeva.APIMovil.Estandar
{
    /// <summary>
    /// Analizador que valida el uso obligatorio de 'this.' al acceder a miembros de instancia.
    /// Mejora la legibilidad y claridad del código al distinguir explícitamente entre 
    /// miembros de instancia y variables locales o parámetros.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValidarUsoThis : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JAPDEVA060";
        private const string TITULO = "Uso obligatorio de 'this.' para miembros de instancia";
        private const string MENSAJE = "El acceso al miembro de instancia '{0}' debe usar 'this.' para mayor claridad";
        private const string DESCRIPCION = "Los miembros de instancia deben ser accedidos usando 'this.' para mejorar la legibilidad y evitar ambigüedades.";
        private const string CATEGORIA = "Estilo";
        private const string URL_AYUDA = "https://docs.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0003";

        private static readonly DiagnosticDescriptor Regla = new DiagnosticDescriptor(
            DiagnosticId,
            TITULO,
            MENSAJE,
            CATEGORIA,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: DESCRIPCION,
            helpLinkUri: URL_AYUDA);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(Regla);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeIdentifierName, SyntaxKind.IdentifierName);
        }

        private static void AnalyzeIdentifierName(SyntaxNodeAnalysisContext context)
        {
            var identifierName = (IdentifierNameSyntax)context.Node;
            var simbolo = context.SemanticModel.GetSymbolInfo(identifierName).Symbol;

            if (simbolo == null)
                return;

            // Solo analizar campos, propiedades y métodos de instancia
            if (!EsMiembroDeInstancia(simbolo))
                return;

            // Verificar si ya tiene 'this.'
            if (TieneThis(identifierName))
                return;

            // Excluir casos especiales
            if (DebeExcluirse(identifierName, simbolo, context))
                return;

            // Verificar si hay una variable local o parámetro que podría crear ambigüedad
            if (!HayPotencialAmbiguedad(identifierName, simbolo, context))
                return;

            var diagnostico = Diagnostic.Create(
                Regla,
                identifierName.GetLocation(),
                identifierName.Identifier.ValueText);

            context.ReportDiagnostic(diagnostico);
        }

        private static bool EsMiembroDeInstancia(ISymbol simbolo)
        {
            switch (simbolo.Kind)
            {
                case SymbolKind.Field:
                    var campo = (IFieldSymbol)simbolo;
                    return !campo.IsStatic && !campo.IsConst;

                case SymbolKind.Property:
                    var propiedad = (IPropertySymbol)simbolo;
                    return !propiedad.IsStatic;

                case SymbolKind.Method:
                    var metodo = (IMethodSymbol)simbolo;
                    return !metodo.IsStatic && metodo.MethodKind == MethodKind.Ordinary;

                default:
                    return false;
            }
        }

        private static bool TieneThis(IdentifierNameSyntax identifierName)
        {
            return identifierName.Parent is MemberAccessExpressionSyntax memberAccess &&
                   memberAccess.Expression is ThisExpressionSyntax;
        }

        private static bool DebeExcluirse(IdentifierNameSyntax identifierName, ISymbol simbolo, SyntaxNodeAnalysisContext context)
        {
            // Excluir si está en el lado derecho de una asignación a miembro
            if (EsAsignacionAMiembro(identifierName))
                return true;

            // Excluir si está en una expresión nameof
            if (EstaEnNameof(identifierName))
                return true;

            // Excluir si está en una declaración de constructor
            if (EstaEnConstructor(identifierName) && EsAsignacionDeParametro(identifierName))
                return true;

            // Excluir si el símbolo no pertenece a la clase actual
            var claseActual = ObtenerClaseContenedora(identifierName);
            if (claseActual == null || !SymbolEqualityComparer.Default.Equals(simbolo.ContainingType, context.SemanticModel.GetDeclaredSymbol(claseActual)))
                return true;

            return false;
        }

        private static bool HayPotencialAmbiguedad(IdentifierNameSyntax identifierName, ISymbol simbolo, SyntaxNodeAnalysisContext context)
        {
            // Siempre requerir 'this.' para campos privados para mayor claridad
            if (simbolo.Kind == SymbolKind.Field)
                return true;

            // Para propiedades y métodos, solo si hay ambigüedad real
            var nombreMiembro = simbolo.Name;
            var metodoContenedor = identifierName.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            var constructorContenedor = identifierName.FirstAncestorOrSelf<ConstructorDeclarationSyntax>();
            
            if (metodoContenedor != null)
            {
                return TieneParametroConMismoNombre(metodoContenedor.ParameterList, nombreMiembro) ||
                       TieneVariableLocalConMismoNombre(metodoContenedor, nombreMiembro, identifierName);
            }

            if (constructorContenedor != null)
            {
                return TieneParametroConMismoNombre(constructorContenedor.ParameterList, nombreMiembro);
            }

            return false;
        }

        private static bool EsAsignacionAMiembro(IdentifierNameSyntax identifierName)
        {
            var assignment = identifierName.FirstAncestorOrSelf<AssignmentExpressionSyntax>();
            return assignment != null && assignment.Left == identifierName;
        }

        private static bool EstaEnNameof(IdentifierNameSyntax identifierName)
        {
            var invocation = identifierName.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocation?.Expression is IdentifierNameSyntax identifier)
            {
                return identifier.Identifier.ValueText == "nameof";
            }
            return false;
        }

        private static bool EstaEnConstructor(IdentifierNameSyntax identifierName)
        {
            return identifierName.FirstAncestorOrSelf<ConstructorDeclarationSyntax>() != null;
        }

        private static bool EsAsignacionDeParametro(IdentifierNameSyntax identifierName)
        {
            var assignment = identifierName.FirstAncestorOrSelf<AssignmentExpressionSyntax>();
            return assignment != null && assignment.Left == identifierName;
        }

        private static ClassDeclarationSyntax ObtenerClaseContenedora(IdentifierNameSyntax identifierName)
        {
            return identifierName.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        }

        private static bool TieneParametroConMismoNombre(ParameterListSyntax parametros, string nombreMiembro)
        {
            return parametros?.Parameters.Any(p => p.Identifier.ValueText == nombreMiembro) ?? false;
        }

        private static bool TieneVariableLocalConMismoNombre(MethodDeclarationSyntax metodo, string nombreMiembro, SyntaxNode ubicacionActual)
        {
            if (metodo.Body == null)
                return false;

            var declaracionesVariables = metodo.Body.DescendantNodes()
                .OfType<VariableDeclaratorSyntax>()
                .Where(v => v.Identifier.ValueText == nombreMiembro)
                .Where(v => v.SpanStart < ubicacionActual.SpanStart); // Solo variables declaradas antes

            return declaracionesVariables.Any();
        }
    }
}
