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

        if (typeSymbol is not INamedTypeSymbol namedType)
            return false;

        // Check if it's Task<T> or ValueTask<T>
        var isTask = namedType.Name == "Task" &&
                     namedType.ContainingNamespace?.ToDisplayString() == "System.Threading.Tasks";

        var isValueTask = namedType.Name == "ValueTask" &&
                          namedType.ContainingNamespace?.ToDisplayString() == "System.Threading.Tasks";

        if (!isTask && !isValueTask)
            return false;

        // Check if the type argument is Result<T>
        if (namedType.TypeArguments.Length != 1)
            return false;

        var typeArgument = namedType.TypeArguments[0];
        if (IsResultType(typeArgument))
        {
            resultInnerType = GetResultInnerType(typeArgument) ?? "T";
            return true;
        }

        return false;
    }

    // Check if type is Result<T> from FunctionalDdd
    private static bool IsResultType(ITypeSymbol typeSymbol) =>
        typeSymbol is INamedTypeSymbol namedType &&
        namedType.Name == "Result" &&
        namedType.ContainingNamespace?.ToDisplayString() == "FunctionalDdd" &&
        namedType.TypeArguments.Length == 1;

    // Get the T from Result<T>
    private static string? GetResultInnerType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedType && namedType.TypeArguments.Length == 1)
            return namedType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        return null;
    }
}
