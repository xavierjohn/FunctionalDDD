namespace FunctionalDdd.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects empty or missing error messages in Error factory methods.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EmptyErrorMessageAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> ErrorFactoryMethods =
        ["Validation", "NotFound", "Unauthorized", "Forbidden", "Conflict", "Unexpected"];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticDescriptors.EmptyErrorMessage];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check for Error.Validation(...), Error.NotFound(...), etc.
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        var methodName = memberAccess.Name.Identifier.Text;
        if (!ErrorFactoryMethods.Contains(methodName))
            return;

        // Check if it's accessing the Error type
        if (memberAccess.Expression is not IdentifierNameSyntax identifier ||
            identifier.Identifier.Text != "Error")
            return;

        // Verify it's from FunctionalDdd
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return;

        if (methodSymbol.ContainingType?.ContainingNamespace?.ToDisplayString() != "FunctionalDdd")
            return;

        // Check the first argument (message)
        var arguments = invocation.ArgumentList.Arguments;
        if (arguments.Count == 0)
        {
            // No arguments - might be using default or named parameters
            // Check if there's a message parameter
            if (HasMessageParameter(methodSymbol) && !HasMessageArgument(arguments))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.EmptyErrorMessage,
                    invocation.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }

            return;
        }

        // Check if first positional argument is the message
        var firstArg = arguments[0];
        if (firstArg.NameColon != null && firstArg.NameColon.Name.Identifier.Text != "message")
        {
            // Named argument but not "message" - check if message is provided elsewhere
            if (!HasMessageArgument(arguments))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.EmptyErrorMessage,
                    invocation.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }

            return;
        }

        // Check if the message argument is empty
        if (IsEmptyOrWhitespaceString(firstArg.Expression, context.SemanticModel))
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.EmptyErrorMessage,
                firstArg.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool HasMessageParameter(IMethodSymbol methodSymbol) =>
        methodSymbol.Parameters.Any(p => p.Name is "message" or "detail");

    private static bool HasMessageArgument(SeparatedSyntaxList<ArgumentSyntax> arguments) =>
        arguments.Any(a =>
            a.NameColon?.Name.Identifier.Text is "message" or "detail" &&
            a.Expression is LiteralExpressionSyntax literal &&
            !string.IsNullOrWhiteSpace(literal.Token.ValueText));

    private static bool IsEmptyOrWhitespaceString(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        // Check for empty string literal ""
        if (expression is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.StringLiteralExpression } literal)
        {
            var value = literal.Token.ValueText;
            return string.IsNullOrWhiteSpace(value);
        }

        // Check for string.Empty
        if (expression is MemberAccessExpressionSyntax { Name.Identifier.Text: "Empty" } memberAccess)
        {
            var typeInfo = semanticModel.GetTypeInfo(memberAccess.Expression);
            return typeInfo.Type?.SpecialType == SpecialType.System_String;
        }

        return false;
    }
}
