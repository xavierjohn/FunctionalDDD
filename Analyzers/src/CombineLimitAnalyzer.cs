namespace FunctionalDdd.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects Combine chains exceeding the maximum supported tuple size (9 elements).
/// When a Combine call would produce a 10+ element tuple, reports FDDD019 with guidance
/// to refactor into logical sub-groups.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CombineLimitAnalyzer : DiagnosticAnalyzer
{
    private const int MaxTupleElements = 9;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticDescriptors.CombineChainTooLong];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check if this is a .Combine() or .CombineAsync() call
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        var methodName = memberAccess.Name.Identifier.Text;
        if (methodName is not ("Combine" or "CombineAsync"))
            return;

        // Only analyze the outermost Combine in a chain to avoid duplicate reports.
        // Inner Combine calls are receivers of outer ones — skip them.
        if (IsPartOfOuterCombineChain(invocation))
            return;

        // Count chain depth syntactically to determine the would-be tuple size
        var elementCount = CountCombineChainElements(invocation);
        if (elementCount <= MaxTupleElements)
            return;

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.CombineChainTooLong,
            memberAccess.Name.GetLocation(),
            elementCount);

        context.ReportDiagnostic(diagnostic);
    }

    /// <summary>
    /// Checks if this invocation is the receiver of another .Combine() call,
    /// meaning it's an inner link in a larger chain and shouldn't be analyzed independently.
    /// </summary>
    private static bool IsPartOfOuterCombineChain(InvocationExpressionSyntax invocation) =>
        invocation.Parent is MemberAccessExpressionSyntax parentMemberAccess &&
        parentMemberAccess.Name.Identifier.Text is "Combine" or "CombineAsync" &&
        parentMemberAccess.Parent is InvocationExpressionSyntax;

    /// <summary>
    /// Counts the total number of elements being combined by walking the Combine chain syntactically.
    /// For example, <c>r1.Combine(r2).Combine(r3)</c> = 3 elements.
    /// </summary>
    private static int CountCombineChainElements(InvocationExpressionSyntax invocation)
    {
        var combineCount = 0;
        var current = invocation;

        while (true)
        {
            if (current.Expression is not MemberAccessExpressionSyntax ma)
                break;

            if (ma.Name.Identifier.Text is not ("Combine" or "CombineAsync"))
                break;

            combineCount++;

            if (ma.Expression is InvocationExpressionSyntax receiver)
                current = receiver;
            else
                break;
        }

        // +1 for the initial Result<T> at the start of the chain
        return combineCount + 1;
    }
}
