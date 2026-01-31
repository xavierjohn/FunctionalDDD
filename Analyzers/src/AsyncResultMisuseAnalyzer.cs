namespace FunctionalDdd.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects blocking or incorrect access to Task&lt;Result&lt;T&gt;&gt; or ValueTask&lt;Result&lt;T&gt;&gt;.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncResultMisuseAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.AsyncResultMisuse);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        var memberAccess = (MemberAccessExpressionSyntax)context.Node;

        // Check if accessing .Result or .Wait() on a Task/ValueTask
        var memberName = memberAccess.Name.Identifier.Text;
        if (memberName is not ("Result" or "Wait"))
            return;

        var expressionType = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
        if (expressionType == null)
            return;

        // Check if it's Task<Result<T>> or ValueTask<Result<T>>
        if (IsAsyncResultType(expressionType, out var resultInnerType))
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.AsyncResultMisuse,
                memberAccess.Name.GetLocation(),
                resultInnerType);

            context.ReportDiagnostic(diagnostic);
        }
    }

    // Check if type is Task<Result<T>> or ValueTask<Result<T>>
    private static bool IsAsyncResultType(ITypeSymbol typeSymbol, out string? resultInnerType)
    {
        resultInnerType = null;

        if (typeSymbol is not INamedTypeSymbol { TypeArguments.Length: 1 } namedType)
            return false;

        // Check if it's Task<T> or ValueTask<T> from System.Threading.Tasks
        var isTaskType = (namedType.Name is "Task" or "ValueTask") &&
                         namedType.ContainingNamespace?.ToDisplayString() == "System.Threading.Tasks";

        if (!isTaskType)
            return false;

        // Check if the type argument is Result<T> from FunctionalDdd
        var typeArgument = namedType.TypeArguments[0];
        if (typeArgument is INamedTypeSymbol { Name: "Result", TypeArguments.Length: 1 } resultType &&
            resultType.ContainingNamespace?.ToDisplayString() == "FunctionalDdd")
        {
            resultInnerType = resultType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            return true;
        }

        return false;
    }
}
