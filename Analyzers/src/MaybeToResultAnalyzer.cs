namespace FunctionalDdd.Analyzers;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects calls to Maybe&lt;T&gt;.ToResult() without providing an error parameter.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MaybeToResultAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.MaybeToResultWithoutError);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check if it's a member access (e.g., maybe.ToResult())
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        // Check if the method is ToResult
        if (memberAccess.Name.Identifier.Text != "ToResult")
            return;

        var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null)
            return;

        // Check if it's called on Maybe<T>
        var receiverType = methodSymbol.ReceiverType;
        if (!IsMaybeType(receiverType, out var maybeInnerType))
            return;

        // Check if the method has zero parameters (no error provided)
        if (methodSymbol.Parameters.Length == 0)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.MaybeToResultWithoutError,
                memberAccess.Name.GetLocation(),
                maybeInnerType);

            context.ReportDiagnostic(diagnostic);
        }
    }

    // Check if type is Maybe<T> from FunctionalDdd
    private static bool IsMaybeType(ITypeSymbol? typeSymbol, out string? innerType)
    {
        innerType = null;

        if (typeSymbol is not INamedTypeSymbol namedType)
            return false;

        if (namedType.Name != "Maybe" ||
            namedType.ContainingNamespace?.ToDisplayString() != "FunctionalDdd" ||
            namedType.TypeArguments.Length != 1)
            return false;

        innerType = namedType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        return true;
    }
}
