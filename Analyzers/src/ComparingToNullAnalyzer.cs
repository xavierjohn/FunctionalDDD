namespace FunctionalDdd.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects comparing Result or Maybe to null.
/// These are structs and cannot be null - use IsSuccess/IsFailure or HasValue/HasNoValue instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ComparingToNullAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticDescriptors.ComparingToNull];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.EqualsExpression);
        context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.NotEqualsExpression);
        context.RegisterSyntaxNodeAction(AnalyzeIsPattern, SyntaxKind.IsPatternExpression);
    }

    private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        var binaryExpression = (BinaryExpressionSyntax)context.Node;

        // Check if one side is null
        var expression = GetNonNullSide(binaryExpression);
        if (expression == null)
            return;

        // Check if the other side is Result or Maybe
        var typeInfo = context.SemanticModel.GetTypeInfo(expression);
        var (typeName, propertyToUse) = GetTypeAndProperty(typeInfo.Type);

        if (typeName != null && propertyToUse != null)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.ComparingToNull,
                binaryExpression.GetLocation(),
                typeName,
                propertyToUse);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeIsPattern(SyntaxNodeAnalysisContext context)
    {
        var isPattern = (IsPatternExpressionSyntax)context.Node;

        // Check for "x is null" or "x is not null"
        var isNullPattern = isPattern.Pattern is ConstantPatternSyntax constantPattern &&
            constantPattern.Expression.IsKind(SyntaxKind.NullLiteralExpression);

        var isNotNullPattern = isPattern.Pattern is UnaryPatternSyntax unaryPattern &&
            unaryPattern.IsKind(SyntaxKind.NotPattern) &&
            unaryPattern.Pattern is ConstantPatternSyntax innerConstant &&
            innerConstant.Expression.IsKind(SyntaxKind.NullLiteralExpression);

        if (!isNullPattern && !isNotNullPattern)
            return;

        // Check if the expression is Result or Maybe
        var typeInfo = context.SemanticModel.GetTypeInfo(isPattern.Expression);
        var (typeName, propertyToUse) = GetTypeAndProperty(typeInfo.Type);

        if (typeName != null && propertyToUse != null)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.ComparingToNull,
                isPattern.GetLocation(),
                typeName,
                propertyToUse);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static ExpressionSyntax? GetNonNullSide(BinaryExpressionSyntax binary)
    {
        if (binary.Right.IsKind(SyntaxKind.NullLiteralExpression))
            return binary.Left;

        if (binary.Left.IsKind(SyntaxKind.NullLiteralExpression))
            return binary.Right;

        return null;
    }

    private static (string? typeName, string? propertyToUse) GetTypeAndProperty(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol == null)
            return (null, null);

        if (typeSymbol.IsResultType())
            return ("Result", "IsSuccess or IsFailure");

        if (typeSymbol.IsMaybeType())
            return ("Maybe", "HasValue or HasNoValue");

        return (null, null);
    }
}