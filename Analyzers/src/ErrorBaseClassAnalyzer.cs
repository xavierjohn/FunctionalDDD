namespace FunctionalDdd.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects direct instantiation of the Error base class instead of using specific error types.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ErrorBaseClassAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticDescriptors.UseSpecificErrorType];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
    }

    private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        var objectCreation = (ObjectCreationExpressionSyntax)context.Node;

        var typeInfo = context.SemanticModel.GetTypeInfo(objectCreation);
        if (typeInfo.Type == null)
            return;

        // Check if it's creating the Error base class directly
        if (IsErrorBaseClass(typeInfo.Type))
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.UseSpecificErrorType,
                objectCreation.Type.GetLocation());

            context.ReportDiagnostic(diagnostic);
        }
    }

    // Check if type is the Error base class from FunctionalDdd (not a derived type)
    private static bool IsErrorBaseClass(ITypeSymbol typeSymbol)
    {
        if (typeSymbol.Name != "Error")
            return false;

        if (typeSymbol.ContainingNamespace?.ToDisplayString() != "FunctionalDdd")
            return false;

        // Check if it's exactly the Error class, not a derived type
        // The base Error class is just "Error", derived types have different names
        // (ValidationError, NotFoundError, etc.)
        if (typeSymbol is INamedTypeSymbol namedType)
        {
            // If the type name is exactly "Error" and it's from FunctionalDdd namespace,
            // it's the base class
            return true;
        }

        return false;
    }
}