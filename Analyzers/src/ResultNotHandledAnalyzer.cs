namespace FunctionalDdd.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects when Result return values are not handled.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ResultNotHandledAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ResultNotHandled);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeExpressionStatement, SyntaxKind.ExpressionStatement);
    }

    private static void AnalyzeExpressionStatement(SyntaxNodeAnalysisContext context)
    {
        var expressionStatement = (ExpressionStatementSyntax)context.Node;
        var expression = expressionStatement.Expression;

        // Check for method invocations that return Result
        if (expression is InvocationExpressionSyntax invocation)
        {
            AnalyzeInvocation(context, invocation);
        }
        // Check for await expressions
        else if (expression is AwaitExpressionSyntax awaitExpression &&
                 awaitExpression.Expression is InvocationExpressionSyntax awaitedInvocation)
        {
            AnalyzeInvocation(context, awaitedInvocation);
        }
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
    {
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return;

        var returnType = methodSymbol.ReturnType;

        // Unwrap Task<T> or ValueTask<T>
        if (IsTaskType(returnType) && returnType is INamedTypeSymbol namedType && namedType.TypeArguments.Length == 1)
        {
            returnType = namedType.TypeArguments[0];
        }

        // Check if the return type is Result<T>
        if (!IsResultType(returnType))
            return;

        // Get the method name for the diagnostic message
        var methodName = GetMethodName(invocation, methodSymbol);

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.ResultNotHandled,
            invocation.GetLocation(),
            methodName);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsResultType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedType)
            return false;

        // Check for Result<T> from FunctionalDdd namespace
        return namedType.Name == "Result" &&
               namedType.ContainingNamespace?.ToDisplayString() == "FunctionalDdd" &&
               namedType.TypeArguments.Length == 1;
    }

    private static bool IsTaskType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedType)
            return false;

        var fullName = namedType.ToDisplayString();
        return fullName.StartsWith("System.Threading.Tasks.Task<", System.StringComparison.Ordinal) ||
               fullName.StartsWith("System.Threading.Tasks.ValueTask<", System.StringComparison.Ordinal);
    }

    private static string GetMethodName(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol)
    {
        // For extension methods, try to get a more descriptive name
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.Name.Identifier.Text;
        }

        return methodSymbol.Name;
    }
}
