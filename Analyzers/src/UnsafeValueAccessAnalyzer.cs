namespace FunctionalDdd.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects unsafe access to Result.Value, Result.Error, and Maybe.Value
/// without proper null/state checks.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnsafeValueAccessAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [
            DiagnosticDescriptors.UnsafeResultValueAccess,
            DiagnosticDescriptors.UnsafeResultErrorAccess,
            DiagnosticDescriptors.UnsafeMaybeValueAccess,
        ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        var memberAccess = (MemberAccessExpressionSyntax)context.Node;
        var memberName = memberAccess.Name.Identifier.Text;

        // We're looking for .Value or .Error access
        if (memberName is not ("Value" or "Error"))
            return;

        var typeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression);
        var type = typeInfo.Type;

        if (type == null)
            return;

        // Check for Result<T>.Value or Result<T>.Error
        if (IsResultType(type))
        {
            if (memberName == "Value" && !IsGuardedBySuccessCheck(memberAccess, context.SemanticModel))
            {
                // Skip invocation patterns like TryCreate().Value - they're handled by FDDD007
                if (memberAccess.Expression is InvocationExpressionSyntax)
                    return;

                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.UnsafeResultValueAccess,
                    memberAccess.Name.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
            else if (memberName == "Error" && !IsGuardedByFailureCheck(memberAccess, context.SemanticModel))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.UnsafeResultErrorAccess,
                    memberAccess.Name.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
        // Check for Maybe<T>.Value
        else if (IsMaybeType(type) && memberName == "Value")
        {
            if (!IsGuardedByHasValueCheck(memberAccess, context.SemanticModel))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.UnsafeMaybeValueAccess,
                    memberAccess.Name.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsResultType(ITypeSymbol typeSymbol) =>
        typeSymbol is INamedTypeSymbol namedType &&
        namedType.Name == "Result" &&
        namedType.ContainingNamespace?.ToDisplayString() == "FunctionalDdd" &&
        namedType.TypeArguments.Length == 1;

    private static bool IsMaybeType(ITypeSymbol typeSymbol) =>
        typeSymbol is INamedTypeSymbol namedType &&
        namedType.Name == "Maybe" &&
        namedType.ContainingNamespace?.ToDisplayString() == "FunctionalDdd" &&
        namedType.TypeArguments.Length == 1;

    private static bool IsGuardedBySuccessCheck(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel) =>
        IsGuardedByCheck(memberAccess, semanticModel, "IsSuccess", true) ||
        IsGuardedByCheck(memberAccess, semanticModel, "IsFailure", false) ||
        IsInsideTryGetValueBlock(memberAccess, semanticModel, "TryGetValue") ||
        IsInsideMatchOrSwitch(memberAccess);

    private static bool IsGuardedByFailureCheck(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel) =>
        IsGuardedByCheck(memberAccess, semanticModel, "IsFailure", true) ||
        IsGuardedByCheck(memberAccess, semanticModel, "IsSuccess", false) ||
        IsInsideTryGetValueBlock(memberAccess, semanticModel, "TryGetError") ||
        IsInsideMatchOrSwitch(memberAccess);

    private static bool IsGuardedByHasValueCheck(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel) =>
        IsGuardedByCheck(memberAccess, semanticModel, "HasValue", true) ||
        IsGuardedByCheck(memberAccess, semanticModel, "HasNoValue", false) ||
        IsInsideTryGetValueBlock(memberAccess, semanticModel, "TryGetValue") ||
        IsInsideMatchOrSwitch(memberAccess);

    private static bool IsGuardedByCheck(
        MemberAccessExpressionSyntax memberAccess,
        SemanticModel semanticModel,
        string checkPropertyName,
        bool expectedValue)
    {
        // Walk up to find enclosing if statement or conditional expression
        var current = memberAccess.Parent;
        while (current != null)
        {
            // Check for if statement
            if (current is IfStatementSyntax ifStatement)
            {
                // Check for negated condition first: !result.Property
                // In then branch, property is false; in else branch, property is true
                if (ifStatement.Condition is PrefixUnaryExpressionSyntax { Operand: MemberAccessExpressionSyntax negatedMemberAccess } prefixUnary &&
                    prefixUnary.IsKind(SyntaxKind.LogicalNotExpression) &&
                    negatedMemberAccess.Name.Identifier.Text == checkPropertyName &&
                    AreSameVariable(negatedMemberAccess.Expression, memberAccess.Expression, semanticModel))
                {
                    if (!expectedValue && IsInThenBranch(memberAccess, ifStatement))
                        return true;
                    if (expectedValue && IsInElseBranch(memberAccess, ifStatement))
                        return true;
                }

                // Check for simple property condition: result.Property
                // In then branch, property is true; in else branch, property is false
                if (ifStatement.Condition is MemberAccessExpressionSyntax conditionMemberAccess &&
                    conditionMemberAccess.Name.Identifier.Text == checkPropertyName &&
                    AreSameVariable(conditionMemberAccess.Expression, memberAccess.Expression, semanticModel))
                {
                    if (expectedValue && IsInThenBranch(memberAccess, ifStatement))
                        return true;
                    if (!expectedValue && IsInElseBranch(memberAccess, ifStatement))
                        return true;
                }

                // Check for equality comparison: result.Property == true/false
                if (IsEqualityCheckingProperty(ifStatement.Condition, memberAccess.Expression, checkPropertyName, expectedValue, semanticModel, out var matchesThenBranch))
                {
                    if (matchesThenBranch && IsInThenBranch(memberAccess, ifStatement))
                        return true;
                    if (!matchesThenBranch && IsInElseBranch(memberAccess, ifStatement))
                        return true;
                }
            }

            // Check for conditional access ?.Value pattern (which is safe)
            if (current is ConditionalAccessExpressionSyntax)
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    private static bool IsEqualityCheckingProperty(
        ExpressionSyntax condition,
        ExpressionSyntax targetExpression,
        string propertyName,
        bool expectedValue,
        SemanticModel semanticModel,
        out bool matchesThenBranch)
    {
        matchesThenBranch = false;

        if (condition is not BinaryExpressionSyntax binaryExpression)
            return false;

        if (!binaryExpression.IsKind(SyntaxKind.EqualsExpression) &&
            !binaryExpression.IsKind(SyntaxKind.NotEqualsExpression))
            return false;

        var left = binaryExpression.Left;
        var right = binaryExpression.Right;

        if (left is MemberAccessExpressionSyntax leftMemberAccess &&
            leftMemberAccess.Name.Identifier.Text == propertyName &&
            right is LiteralExpressionSyntax literal &&
            AreSameVariable(leftMemberAccess.Expression, targetExpression, semanticModel))
        {
            var literalValue = literal.IsKind(SyntaxKind.TrueLiteralExpression);
            var isEquals = binaryExpression.IsKind(SyntaxKind.EqualsExpression);
            // In then branch: property equals literalValue if ==, or !literalValue if !=
            var propertyValueInThenBranch = isEquals ? literalValue : !literalValue;

            if (propertyValueInThenBranch == expectedValue)
            {
                matchesThenBranch = true;
                return true;
            }
            else
            {
                matchesThenBranch = false;
                return true;
            }
        }

        return false;
    }

    private static bool IsConditionCheckingProperty(
        ExpressionSyntax condition,
        ExpressionSyntax targetExpression,
        string propertyName,
        bool expectedValue,
        SemanticModel semanticModel)
    {
        // Handle simple property access: result.IsSuccess
        if (condition is MemberAccessExpressionSyntax conditionMemberAccess &&
            conditionMemberAccess.Name.Identifier.Text == propertyName)
        {
            return AreSameVariable(conditionMemberAccess.Expression, targetExpression, semanticModel) && expectedValue;
        }

        // Handle negation: !result.IsSuccess
        if (condition is PrefixUnaryExpressionSyntax { Operand: MemberAccessExpressionSyntax negatedMemberAccess } prefixUnary &&
            prefixUnary.IsKind(SyntaxKind.LogicalNotExpression) &&
            negatedMemberAccess.Name.Identifier.Text == propertyName)
        {
            return AreSameVariable(negatedMemberAccess.Expression, targetExpression, semanticModel) && !expectedValue;
        }

        // Handle equality: result.IsSuccess == true
        if (condition is BinaryExpressionSyntax binaryExpression)
        {
            if (binaryExpression.IsKind(SyntaxKind.EqualsExpression) ||
                binaryExpression.IsKind(SyntaxKind.NotEqualsExpression))
            {
                var left = binaryExpression.Left;
                var right = binaryExpression.Right;

                if (left is MemberAccessExpressionSyntax leftMemberAccess &&
                    leftMemberAccess.Name.Identifier.Text == propertyName &&
                    right is LiteralExpressionSyntax literal)
                {
                    var literalValue = literal.IsKind(SyntaxKind.TrueLiteralExpression);
                    var isEquals = binaryExpression.IsKind(SyntaxKind.EqualsExpression);
                    var effectiveValue = isEquals ? literalValue : !literalValue;

                    return AreSameVariable(leftMemberAccess.Expression, targetExpression, semanticModel) &&
                           effectiveValue == expectedValue;
                }
            }
        }

        return false;
    }

    private static bool AreSameVariable(ExpressionSyntax expr1, ExpressionSyntax expr2, SemanticModel semanticModel)
    {
        var symbol1 = semanticModel.GetSymbolInfo(expr1).Symbol;
        var symbol2 = semanticModel.GetSymbolInfo(expr2).Symbol;

        if (symbol1 == null || symbol2 == null)
            return false;

        return SymbolEqualityComparer.Default.Equals(symbol1, symbol2);
    }

    private static bool IsInThenBranch(SyntaxNode node, IfStatementSyntax ifStatement) =>
        ifStatement.Statement.Contains(node);

    private static bool IsInElseBranch(SyntaxNode node, IfStatementSyntax ifStatement) =>
        ifStatement.Else?.Statement.Contains(node) ?? false;

    private static bool IsInsideTryGetValueBlock(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel, string tryMethodName)
    {
        // Look for pattern: if (result.TryGetValue(out var value)) { ... }
        var current = memberAccess.Parent;
        while (current != null)
        {
            if (current is IfStatementSyntax { Condition: InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax methodAccess } } ifStatement &&
                methodAccess.Name.Identifier.Text == tryMethodName)
            {
                // Verify it's the same variable
                if (AreSameVariable(methodAccess.Expression, memberAccess.Expression, semanticModel))
                {
                    return IsInThenBranch(memberAccess, ifStatement);
                }
            }

            current = current.Parent;
        }

        return false;
    }

    private static bool IsInsideMatchOrSwitch(MemberAccessExpressionSyntax memberAccess)
    {
        // Look for usage inside Match, MatchError, Switch, or SwitchError lambda
        var current = memberAccess.Parent;
        while (current != null)
        {
            if (current is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax methodAccess })
            {
                var methodName = methodAccess.Name.Identifier.Text;
                if (methodName is "Match" or "MatchAsync" or "MatchError" or "MatchErrorAsync" or
                    "Switch" or "SwitchAsync" or "SwitchError" or "SwitchErrorAsync")
                {
                    return true;
                }
            }

            // Also allow inside lambdas passed to Bind, Map, Tap, etc.
            // since these are within the success track
            if (current is LambdaExpressionSyntax { Parent: ArgumentSyntax { Parent.Parent: InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax parentMethodAccess } } })
            {
                var parentMethodName = parentMethodAccess.Name.Identifier.Text;
                // These methods are only called on success, so accessing .Value is safe
                if (parentMethodName is "Bind" or "BindAsync" or "Map" or "MapAsync" or
                    "Tap" or "TapAsync" or "Ensure" or "EnsureAsync")
                {
                    return true;
                }
            }

            current = current.Parent;
        }

        return false;
    }
}