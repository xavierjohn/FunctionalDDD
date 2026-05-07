namespace Trellis.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects unsafe access to <c>Maybe&lt;T&gt;.Value</c> without proper
/// presence checks. The corresponding rules for <c>Result&lt;T&gt;.Value</c>
/// and <c>Result&lt;T&gt;.Error</c> were removed from the current API: <c>Value</c> no longer
/// exists on <c>Result&lt;T&gt;</c>, and <c>Error</c> is now nullable so NRT handles
/// unsafe access at the language level.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnsafeValueAccessAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticDescriptors.UnsafeMaybeValueAccess];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        var memberAccess = (MemberAccessExpressionSyntax)context.Node;

        if (memberAccess.Name.Identifier.Text != "Value")
            return;

        var typeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression);
        var type = typeInfo.Type;

        if (type is null || !type.IsMaybeType())
            return;

        if (IsGuardedByHasValueCheck(memberAccess, context.SemanticModel))
            return;

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.UnsafeMaybeValueAccess,
            memberAccess.Name.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsGuardedByHasValueCheck(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel) =>
        IsGuardedByCheck(memberAccess, semanticModel, "HasValue", true) ||
        IsGuardedByCheck(memberAccess, semanticModel, "HasNoValue", false) ||
        IsGuardedByShortCircuitAnd(memberAccess, semanticModel) ||
        IsGuardedByPriorAssignment(memberAccess, semanticModel) ||
        IsInsideTryGetValueBlock(memberAccess, semanticModel, "TryGetValue") ||
        IsInsideTrackSafeLambda(memberAccess, semanticModel);

    private static bool IsGuardedByCheck(
        MemberAccessExpressionSyntax memberAccess,
        SemanticModel semanticModel,
        string checkPropertyName,
        bool expectedValue)
    {
        var current = memberAccess.Parent;
        while (current != null)
        {
            if (current is IfStatementSyntax ifStatement)
            {
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

                if (ifStatement.Condition is MemberAccessExpressionSyntax conditionMemberAccess &&
                    conditionMemberAccess.Name.Identifier.Text == checkPropertyName &&
                    AreSameVariable(conditionMemberAccess.Expression, memberAccess.Expression, semanticModel))
                {
                    if (expectedValue && IsInThenBranch(memberAccess, ifStatement))
                        return true;
                    if (!expectedValue && IsInElseBranch(memberAccess, ifStatement))
                        return true;
                }

                if (IsEqualityCheckingProperty(ifStatement.Condition, memberAccess.Expression, checkPropertyName, expectedValue, semanticModel, out var matchesThenBranch))
                {
                    if (matchesThenBranch && IsInThenBranch(memberAccess, ifStatement))
                        return true;
                    if (!matchesThenBranch && IsInElseBranch(memberAccess, ifStatement))
                        return true;
                }
            }

            if (current is ConditionalExpressionSyntax conditionalExpression)
            {
                if (conditionalExpression.Condition is PrefixUnaryExpressionSyntax { Operand: MemberAccessExpressionSyntax negatedTernaryMemberAccess } ternaryPrefixUnary &&
                    ternaryPrefixUnary.IsKind(SyntaxKind.LogicalNotExpression) &&
                    negatedTernaryMemberAccess.Name.Identifier.Text == checkPropertyName &&
                    AreSameVariable(negatedTernaryMemberAccess.Expression, memberAccess.Expression, semanticModel))
                {
                    if (!expectedValue && IsInWhenTrueBranch(memberAccess, conditionalExpression))
                        return true;
                    if (expectedValue && IsInWhenFalseBranch(memberAccess, conditionalExpression))
                        return true;
                }

                if (conditionalExpression.Condition is MemberAccessExpressionSyntax ternaryConditionMemberAccess &&
                    ternaryConditionMemberAccess.Name.Identifier.Text == checkPropertyName &&
                    AreSameVariable(ternaryConditionMemberAccess.Expression, memberAccess.Expression, semanticModel))
                {
                    if (expectedValue && IsInWhenTrueBranch(memberAccess, conditionalExpression))
                        return true;
                    if (!expectedValue && IsInWhenFalseBranch(memberAccess, conditionalExpression))
                        return true;
                }

                if (IsEqualityCheckingProperty(conditionalExpression.Condition, memberAccess.Expression, checkPropertyName, expectedValue, semanticModel, out var ternaryMatchesTrueBranch))
                {
                    if (ternaryMatchesTrueBranch && IsInWhenTrueBranch(memberAccess, conditionalExpression))
                        return true;
                    if (!ternaryMatchesTrueBranch && IsInWhenFalseBranch(memberAccess, conditionalExpression))
                        return true;
                }
            }

            // Conditional access ?.Value pattern is safe.
            if (current is ConditionalAccessExpressionSyntax)
                return true;

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
            var propertyValueInThenBranch = isEquals ? literalValue : !literalValue;

            matchesThenBranch = propertyValueInThenBranch == expectedValue;
            return true;
        }

        if (right is MemberAccessExpressionSyntax rightMemberAccess &&
            rightMemberAccess.Name.Identifier.Text == propertyName &&
            left is LiteralExpressionSyntax literalLeft &&
            AreSameVariable(rightMemberAccess.Expression, targetExpression, semanticModel))
        {
            var literalValue = literalLeft.IsKind(SyntaxKind.TrueLiteralExpression);
            var isEquals = binaryExpression.IsKind(SyntaxKind.EqualsExpression);
            var propertyValueInThenBranch = isEquals ? literalValue : !literalValue;

            matchesThenBranch = propertyValueInThenBranch == expectedValue;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns <see langword="true"/> if two syntax expressions refer to the same logical
    /// receiver. Only stable receiver forms are considered comparable: identifiers,
    /// member-access chains, <c>this</c>, and <c>base</c>. For instance members accessed
    /// through a chain of receivers, comparison is structural — each chain segment must match
    /// in both the terminal symbol and the recursive receiver. Implicit and explicit
    /// <c>this</c> are treated as equivalent (an unqualified instance-member access and the
    /// same member explicitly qualified with <c>this.</c> name the same thing). Static
    /// members and locals/parameters compare by symbol identity alone. Any other receiver
    /// shape (invocation, element access, conditional access, cast, etc.) is conservatively
    /// rejected because it cannot be structurally compared without evaluating runtime state.
    /// </summary>
    private static bool AreSameVariable(ExpressionSyntax expr1, ExpressionSyntax expr2, SemanticModel semanticModel)
    {
        while (expr1 is ParenthesizedExpressionSyntax p1)
            expr1 = p1.Expression;
        while (expr2 is ParenthesizedExpressionSyntax p2)
            expr2 = p2.Expression;

        // Reject any receiver shape we cannot structurally compare.
        if (!IsStableReceiverShape(expr1) || !IsStableReceiverShape(expr2))
            return false;

        var symbol1 = semanticModel.GetSymbolInfo(expr1).Symbol;
        var symbol2 = semanticModel.GetSymbolInfo(expr2).Symbol;

        if (symbol1 == null || symbol2 == null)
            return false;

        if (!SymbolEqualityComparer.Default.Equals(symbol1, symbol2))
            return false;

        // Static members, locals, parameters, type names — symbol identity is sufficient.
        // The receivers (if any) cannot disambiguate them further.
        if (symbol1.IsStatic ||
            symbol1 is ILocalSymbol or IParameterSymbol or ITypeSymbol or INamespaceSymbol)
        {
            return true;
        }

        // Instance member: the same symbol on different receivers refers to different state.
        // Walk the receiver chains, treating implicit `this` (no receiver) and explicit
        // `this`/`base` as equivalent.
        var receiver1 = expr1 is MemberAccessExpressionSyntax m1 ? m1.Expression : null;
        var receiver2 = expr2 is MemberAccessExpressionSyntax m2 ? m2.Expression : null;

        var isThis1 = receiver1 is null or ThisExpressionSyntax or BaseExpressionSyntax;
        var isThis2 = receiver2 is null or ThisExpressionSyntax or BaseExpressionSyntax;

        if (isThis1 && isThis2)
            return true;

        if (isThis1 != isThis2)
            return false;

        return AreSameVariable(receiver1!, receiver2!, semanticModel);
    }

    private static bool IsStableReceiverShape(ExpressionSyntax expr) =>
        expr is IdentifierNameSyntax
            or MemberAccessExpressionSyntax
            or ThisExpressionSyntax
            or BaseExpressionSyntax;

    private static bool IsInThenBranch(SyntaxNode node, IfStatementSyntax ifStatement) =>
        ifStatement.Statement.Contains(node);

    private static bool IsInElseBranch(SyntaxNode node, IfStatementSyntax ifStatement) =>
        ifStatement.Else?.Statement.Contains(node) ?? false;

    private static bool IsInWhenTrueBranch(SyntaxNode node, ConditionalExpressionSyntax conditionalExpression) =>
        conditionalExpression.WhenTrue.Contains(node);

    private static bool IsInWhenFalseBranch(SyntaxNode node, ConditionalExpressionSyntax conditionalExpression) =>
        conditionalExpression.WhenFalse.Contains(node);

    private static bool IsInsideTryGetValueBlock(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel, string tryMethodName)
    {
        var current = memberAccess.Parent;
        while (current != null)
        {
            if (current is IfStatementSyntax ifStatement)
            {
                if (ifStatement.Condition is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax methodAccess } &&
                    methodAccess.Name.Identifier.Text == tryMethodName &&
                    AreSameVariable(methodAccess.Expression, memberAccess.Expression, semanticModel))
                {
                    return IsInThenBranch(memberAccess, ifStatement);
                }

                if (ifStatement.Condition is PrefixUnaryExpressionSyntax { Operand: InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax negatedMethodAccess } } prefixUnary &&
                    prefixUnary.IsKind(SyntaxKind.LogicalNotExpression) &&
                    negatedMethodAccess.Name.Identifier.Text == tryMethodName &&
                    AreSameVariable(negatedMethodAccess.Expression, memberAccess.Expression, semanticModel))
                {
                    return IsInElseBranch(memberAccess, ifStatement);
                }
            }

            current = current.Parent;
        }

        return false;
    }

    private static bool IsInsideTrackSafeLambda(
        MemberAccessExpressionSyntax memberAccess,
        SemanticModel semanticModel)
    {
        if (memberAccess.FirstAncestorOrSelf<LambdaExpressionSyntax>() is not { Parent: ArgumentSyntax argument } ||
            argument.Parent?.Parent is not InvocationExpressionSyntax invocation ||
            invocation.Expression is not MemberAccessExpressionSyntax methodAccess ||
            !AreSameVariable(methodAccess.Expression, memberAccess.Expression, semanticModel))
            return false;

        if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol ||
            !methodSymbol.IsTrellisExtensionMethod())
            return false;

        var parameter = GetArgumentParameter(methodSymbol, argument);
        if (parameter == null)
            return false;

        return IsSafeLambdaParameter(methodSymbol.Name, parameter.Name);
    }

    private static IParameterSymbol? GetArgumentParameter(IMethodSymbol methodSymbol, ArgumentSyntax argument)
    {
        if (argument.NameColon is { } nameColon)
            return methodSymbol.Parameters.FirstOrDefault(parameter => parameter.Name == nameColon.Name.Identifier.Text);

        if (argument.Parent is not BaseArgumentListSyntax argumentList)
            return null;

        var argumentIndex = argumentList.Arguments.IndexOf(argument);
        return argumentIndex >= 0 && argumentIndex < methodSymbol.Parameters.Length
            ? methodSymbol.Parameters[argumentIndex]
            : null;
    }

    /// <summary>
    /// Lambda parameters whose body is only invoked on the present-value branch of a
    /// <c>Maybe&lt;T&gt;</c> chain. Inside such bodies, accessing <c>.Value</c> on the
    /// receiver is safe because the API itself has already discriminated.
    /// </summary>
    private static bool IsSafeLambdaParameter(string methodName, string parameterName) =>
        methodName switch
        {
            "Bind" or "BindAsync" or "Map" or "MapAsync" or "Tap" or "TapAsync" or "Ensure" or "EnsureAsync" => true,
            "Match" or "MatchAsync" or "Switch" or "SwitchAsync" => parameterName is "onSome",
            _ => false,
        };

    /// <summary>
    /// Recognizes: x = Maybe&lt;T&gt;.From(...); followed by x.Value in the same block.
    /// Only suppresses when T is a non-nullable value type (where From() can never return None).
    /// </summary>
    private static bool IsGuardedByPriorAssignment(
        MemberAccessExpressionSyntax memberAccess,
        SemanticModel semanticModel)
    {
        var containingStatement = memberAccess.FirstAncestorOrSelf<StatementSyntax>();
        if (containingStatement?.Parent is not BlockSyntax block)
            return false;

        var memberAccessIndex = block.Statements.IndexOf(containingStatement);
        if (memberAccessIndex < 0)
            return false;

        for (var i = memberAccessIndex - 1; i >= 0; i--)
        {
            if (block.Statements[i] is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment })
                continue;

            if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
                continue;

            if (!AreSameVariable(assignment.Left, memberAccess.Expression, semanticModel))
                continue;

            if (assignment.Right is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax methodAccess } ||
                methodAccess.Name.Identifier.Text != "From")
                return false;

            if (semanticModel.GetSymbolInfo(assignment.Right).Symbol is not IMethodSymbol methodSymbol)
                return false;

            var containingType = methodSymbol.ContainingType;
            if (containingType?.Name is not "Maybe" ||
                containingType.ContainingNamespace?.ToDisplayString() is not "Trellis")
                return false;

            var maybeType = semanticModel.GetTypeInfo(memberAccess.Expression).Type;
            if (maybeType is not INamedTypeSymbol { TypeArguments.Length: 1 } namedType)
                return false;

            var innerType = namedType.TypeArguments[0];
            if (innerType.IsValueType && innerType.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T
                && !HasReassignmentBetween(block, i + 1, memberAccessIndex, memberAccess.Expression, semanticModel))
                return true;
        }

        return false;
    }

    private static bool HasReassignmentBetween(
        BlockSyntax block,
        int startExclusive,
        int endExclusive,
        ExpressionSyntax targetExpression,
        SemanticModel semanticModel)
    {
        for (var j = startExclusive; j < endExclusive; j++)
        {
            if (block.Statements[j] is ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment } &&
                assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
                AreSameVariable(assignment.Left, targetExpression, semanticModel))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Recognizes <c>x.HasValue &amp;&amp; ... &amp;&amp; x.Value</c> in a left-to-right
    /// short-circuit chain. Common in expression trees (specifications) and any multi-clause
    /// boolean filter.
    /// </summary>
    /// <remarks>
    /// C# left-associates <c>a &amp;&amp; b &amp;&amp; c</c> as <c>(a &amp;&amp; b) &amp;&amp; c</c>,
    /// so the immediate left operand of the outermost <c>&amp;&amp;</c> is itself a binary
    /// expression for any 3+ clause shape. To recognize the guard, recurse through nested
    /// <c>&amp;&amp;</c> operators on the left side looking for a matching <c>HasValue</c>
    /// access on the same receiver. <c>||</c>, <c>!</c>, ternary, and other operators stop the
    /// recursion because they break the short-circuit guarantee.
    /// </remarks>
    private static bool IsGuardedByShortCircuitAnd(
        MemberAccessExpressionSyntax memberAccess,
        SemanticModel semanticModel)
    {
        var current = memberAccess.Parent;
        while (current != null)
        {
            if (current is BinaryExpressionSyntax binaryExpression &&
                binaryExpression.IsKind(SyntaxKind.LogicalAndExpression) &&
                binaryExpression.Right.Contains(memberAccess) &&
                ContainsHasValueGuard(binaryExpression.Left, memberAccess.Expression, semanticModel))
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="expr"/> is, or contains within a
    /// connected <c>&amp;&amp;</c> subtree (with parentheses transparent), a <c>HasValue</c>
    /// member access on the same receiver as <paramref name="targetReceiver"/>. Recursion stops
    /// at non-<c>&amp;&amp;</c> boundaries so <c>||</c>, <c>!</c>, ternary, and other operators
    /// do not falsely satisfy the guard.
    /// </summary>
    private static bool ContainsHasValueGuard(
        ExpressionSyntax expr,
        ExpressionSyntax targetReceiver,
        SemanticModel semanticModel)
    {
        // Parentheses are transparent for short-circuit semantics.
        while (expr is ParenthesizedExpressionSyntax paren)
            expr = paren.Expression;

        if (expr is MemberAccessExpressionSyntax member &&
            member.Name.Identifier.Text == "HasValue" &&
            AreSameVariable(member.Expression, targetReceiver, semanticModel))
        {
            return true;
        }

        if (expr is BinaryExpressionSyntax binExpr &&
            binExpr.IsKind(SyntaxKind.LogicalAndExpression))
        {
            return ContainsHasValueGuard(binExpr.Left, targetReceiver, semanticModel)
                || ContainsHasValueGuard(binExpr.Right, targetReceiver, semanticModel);
        }

        return false;
    }
}