using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ValidarConsistenciaDocumentacionXML : DiagnosticAnalyzer
{
    public const string DiagnosticIdParametrosFaltantes = "JAPDEVA087";
    public const string DiagnosticIdParametrosIncorrectos = "JAPDEVA088";
    public const string DiagnosticIdRetornoIncorrecto = "JAPDEVA089";
    
    private static readonly DiagnosticDescriptor RuleParametrosFaltantes = new DiagnosticDescriptor(
        DiagnosticIdParametrosFaltantes,
        "Faltan parámetros en la documentación XML",
        "El método '{0}' tiene parámetros que no están documentados en XML: {1}",
        "Documentation",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Todos los parámetros del método deben estar documentados en los comentarios XML.");

    private static readonly DiagnosticDescriptor RuleParametrosIncorrectos = new DiagnosticDescriptor(
        DiagnosticIdParametrosIncorrectos,
        "Parámetros documentados que no existen en el método",
        "El método '{0}' tiene documentación XML para parámetros que no existen: {1}",
        "Documentation",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "La documentación XML no debe incluir parámetros que no existen en la firma del método.");

    private static readonly DiagnosticDescriptor RuleRetornoIncorrecto = new DiagnosticDescriptor(
        DiagnosticIdRetornoIncorrecto,
        "Documentación de retorno inconsistente con el tipo de retorno",
        "El método '{0}' tiene tipo de retorno '{1}' pero la documentación XML indica '{2}'",
        "Documentation",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "La documentación XML del valor de retorno debe ser consistente con el tipo de retorno del método.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
        ImmutableArray.Create(RuleParametrosFaltantes, RuleParametrosIncorrectos, RuleRetornoIncorrecto);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        // Cambiar a análisis de syntax tree completo en lugar de nodos individuales
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
    }

    private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot();
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

        foreach (var method in methods)
        {
            // Solo analizar métodos públicos
            if (!method.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                continue;

            // Obtener el texto fuente completo del archivo
            var sourceText = context.Tree.GetText().ToString();
            
            // Encontrar la posición del método en el archivo
            var methodPosition = method.GetLocation().SourceSpan.Start;
            var methodLine = context.Tree.GetText().Lines.GetLineFromPosition(methodPosition).LineNumber;
            
            // Buscar documentación XML en las líneas anteriores al método
            var xmlDoc = ExtractXmlDocumentationFromSource(sourceText, methodLine);
            
            if (!string.IsNullOrEmpty(xmlDoc))
            {
                ValidateMethodDocumentation(context, method, xmlDoc);
            }
        }
    }

    private static string ExtractXmlDocumentationFromSource(string sourceText, int methodLine)
    {
        var lines = sourceText.Split('\n');
        var xmlLines = new System.Collections.Generic.List<string>();
        
        // Buscar hacia atrás desde la línea del método
        for (int i = methodLine - 1; i >= 0; i--)
        {
            var line = lines[i].Trim();
            
            // Si encontramos una línea que empieza con ///, es documentación XML
            if (line.StartsWith("///"))
            {
                xmlLines.Insert(0, line);
            }
            // Si encontramos una línea que no es documentación XML ni está vacía, paramos
            else if (!string.IsNullOrWhiteSpace(line))
            {
                break;
            }
        }
        
        return string.Join("\n", xmlLines);
    }

    private static void ValidateMethodDocumentation(SyntaxTreeAnalysisContext context, 
        MethodDeclarationSyntax method, string xmlDoc)
    {
        try
        {
            // Obtener parámetros del método
            var methodParams = method.ParameterList.Parameters
                .Select(p => p.Identifier.ValueText)
                .ToList();

            // Obtener parámetros documentados
            var documentedParams = ExtractDocumentedParametersFromText(xmlDoc);

            // Verificar parámetros faltantes
            var missingParams = methodParams.Except(documentedParams).ToList();
            if (missingParams.Any())
            {
                var diagnostic = Diagnostic.Create(RuleParametrosFaltantes,
                    method.Identifier.GetLocation(),
                    method.Identifier.ValueText,
                    string.Join(", ", missingParams));
                context.ReportDiagnostic(diagnostic);
            }

            // Verificar parámetros extra en documentación
            var extraParams = documentedParams.Except(methodParams).ToList();
            if (extraParams.Any())
            {
                var diagnostic = Diagnostic.Create(RuleParametrosIncorrectos,
                    method.Identifier.GetLocation(),
                    method.Identifier.ValueText,
                    string.Join(", ", extraParams));
                context.ReportDiagnostic(diagnostic);
            }

            // Verificar retorno
            ValidateReturnDocumentationFromText(context, method, xmlDoc);
        }
        catch (System.Exception)
        {
            // Si hay error, continuar sin reportar
        }
    }

    private static System.Collections.Generic.List<string> ExtractDocumentedParametersFromText(string xmlDoc)
    {
        var paramNames = new System.Collections.Generic.List<string>();
        var lines = xmlDoc.Split('\n');
        
        foreach (var line in lines)
        {
            // Buscar patrón /// <param name="nombreParametro">
            if (line.Contains("<param name=\""))
            {
                var startIndex = line.IndexOf("<param name=\"") + 13;
                var endIndex = line.IndexOf("\"", startIndex);
                
                if (startIndex > 12 && endIndex > startIndex)
                {
                    var paramName = line.Substring(startIndex, endIndex - startIndex);
                    if (!string.IsNullOrEmpty(paramName))
                    {
                        paramNames.Add(paramName);
                    }
                }
            }
        }
        
        return paramNames;
    }

    private static void ValidateReturnDocumentationFromText(SyntaxTreeAnalysisContext context, 
        MethodDeclarationSyntax method, string xmlDoc)
    {
        try
        {
            var returnType = method.ReturnType.ToString();
            var hasReturnsDoc = xmlDoc.Contains("<returns>");

            // Si el método retorna void o Task, no debería tener documentación de returns
            if ((returnType == "void" || returnType == "Task") && hasReturnsDoc)
            {
                var diagnostic = Diagnostic.Create(RuleRetornoIncorrecto,
                    method.Identifier.GetLocation(),
                    method.Identifier.ValueText,
                    returnType,
                    "documenta un valor de retorno");
                context.ReportDiagnostic(diagnostic);
            }

            // Si el método retorna un valor, debería tener documentación de returns
            if (returnType != "void" && returnType != "Task" && !hasReturnsDoc)
            {
                var diagnostic = Diagnostic.Create(RuleRetornoIncorrecto,
                    method.Identifier.GetLocation(),
                    method.Identifier.ValueText,
                    returnType,
                    "no documenta el valor de retorno");
                context.ReportDiagnostic(diagnostic);
            }
        }
        catch
        {
            // Si hay error, no reportar
        }
    }

    private static DocumentationCommentTriviaSyntax GetXmlDocumentation(MethodDeclarationSyntax method)
    {
        var leadingTrivia = method.GetLeadingTrivia();
        
        // Buscar comentarios de documentación de línea simple
        var singleLineDoc = leadingTrivia
            .Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
            .Select(t => t.GetStructure() as DocumentationCommentTriviaSyntax)
            .FirstOrDefault(d => d != null);
            
        if (singleLineDoc != null)
            return singleLineDoc;
            
        // Buscar comentarios de documentación multilínea
        var multiLineDoc = leadingTrivia
            .Where(t => t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
            .Select(t => t.GetStructure() as DocumentationCommentTriviaSyntax)
            .FirstOrDefault(d => d != null);
            
        return multiLineDoc;
    }

    /// <summary>
    /// Método alternativo para obtener documentación XML como texto plano
    /// </summary>
    

    /// <summary>
    /// Extrae parámetros documentados usando regex simple
    /// </summary>
    private static System.Collections.Generic.List<string> ExtractDocumentedParameters(string xmlDocumentation)
    {
        var paramNames = new System.Collections.Generic.List<string>();
        
        // Buscar patrones como <param name="nombreParametro">
        var lines = xmlDocumentation.Split('\n');
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.Contains("<param name=\""))
            {
                var startIndex = trimmedLine.IndexOf("<param name=\"") + 13; // longitud de "<param name=\""
                var endIndex = trimmedLine.IndexOf("\"", startIndex);
                
                if (startIndex > 12 && endIndex > startIndex)
                {
                    var paramName = trimmedLine.Substring(startIndex, endIndex - startIndex);
                    if (!string.IsNullOrEmpty(paramName))
                    {
                        paramNames.Add(paramName);
                    }
                }
            }
        }

        return paramNames;
    }

    /// <summary>
    /// Valida documentación de retorno usando texto plano
    /// </summary>
    private static void ValidateReturnDocumentationAlternative(SyntaxNodeAnalysisContext context, 
        MethodDeclarationSyntax method, string xmlDocumentation)
    {
        try
        {
            var returnType = method.ReturnType.ToString();
            var hasReturnsDoc = xmlDocumentation.Contains("<returns>");

            // Si el método retorna void o Task, no debería tener documentación de returns
            if ((returnType == "void" || returnType == "Task") && hasReturnsDoc)
            {
                var diagnostic = Diagnostic.Create(RuleRetornoIncorrecto,
                    method.Identifier.GetLocation(),
                    method.Identifier.ValueText,
                    returnType,
                    "documenta un valor de retorno");
                context.ReportDiagnostic(diagnostic);
            }

            // Si el método retorna un valor, debería tener documentación de returns
            if (returnType != "void" && returnType != "Task" && !hasReturnsDoc)
            {
                var diagnostic = Diagnostic.Create(RuleRetornoIncorrecto,
                    method.Identifier.GetLocation(),
                    method.Identifier.ValueText,
                    returnType,
                    "no documenta el valor de retorno");
                context.ReportDiagnostic(diagnostic);
            }
        }
        catch
        {
            // Si hay error en la validación, no reportar para evitar fallar el analizador
        }
    }

    private static System.Collections.Generic.List<string> GetDocumentedParameters(DocumentationCommentTriviaSyntax xmlDoc)
    {
        try
        {
            var paramNodes = xmlDoc.DescendantNodes()
                .OfType<XmlElementSyntax>()
                .Where(x => x.StartTag != null && x.StartTag.Name.ToString() == "param");

            var paramNames = new System.Collections.Generic.List<string>();

            foreach (var paramNode in paramNodes)
            {
                var nameAttr = paramNode.StartTag.Attributes
                    .OfType<XmlNameAttributeSyntax>()
                    .FirstOrDefault();

                if (nameAttr?.Identifier?.Identifier.ValueText != null)
                {
                    paramNames.Add(nameAttr.Identifier.Identifier.ValueText);
                }
            }

            return paramNames;
        }
        catch
        {
            return new System.Collections.Generic.List<string>();
        }
    }

    private static void ValidateReturnDocumentation(SyntaxNodeAnalysisContext context, 
        MethodDeclarationSyntax method, DocumentationCommentTriviaSyntax xmlDoc)
    {
        try
        {
            var returnType = method.ReturnType.ToString();
            var hasReturnsDoc = xmlDoc.DescendantNodes()
                .OfType<XmlElementSyntax>()
                .Any(x => x.StartTag != null && x.StartTag.Name.ToString() == "returns");

            // Si el método retorna void o Task, no debería tener documentación de returns
            if ((returnType == "void" || returnType == "Task") && hasReturnsDoc)
            {
                var diagnostic = Diagnostic.Create(RuleRetornoIncorrecto,
                    method.Identifier.GetLocation(),
                    method.Identifier.ValueText,
                    returnType,
                    "documenta un valor de retorno");
                context.ReportDiagnostic(diagnostic);
            }

            // Si el método retorna un valor, debería tener documentación de returns
            if (returnType != "void" && returnType != "Task" && !hasReturnsDoc)
            {
                var diagnostic = Diagnostic.Create(RuleRetornoIncorrecto,
                    method.Identifier.GetLocation(),
                    method.Identifier.ValueText,
                    returnType,
                    "no documenta el valor de retorno");
                context.ReportDiagnostic(diagnostic);
            }
        }
        catch
        {
            // Si hay error en la validación, no reportar para evitar fallar el analizador
        }
    }
}