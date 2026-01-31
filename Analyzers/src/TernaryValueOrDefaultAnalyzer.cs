namespace FunctionalDdd.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects the pattern "result.IsSuccess ? result.Value : defaultValue"
/// and suggests using GetValueOrDefault() or Match() instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TernaryValueOrDefaultAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticDescriptors.UseFunctionalValueOrDefault];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeConditionalExpression, SyntaxKind.ConditionalExpression);
    }

    private static void AnalyzeConditionalExpression(SyntaxNodeAnalysisContext context)
    {
        var conditional = (ConditionalExpressionSyntax)context.Node;

        // Check if the condition is accessing IsSuccess on a Result
        if (conditional.Condition is not MemberAccessExpressionSyntax conditionMemberAccess)
            return;

        if (conditionMemberAccess.Name.Identifier.Text != "IsSuccess")
            return;

        // Get the type being accessed
        var conditionType = context.SemanticModel.GetTypeInfo(conditionMemberAccess.Expression).Type;
        if (!IsResultType(conditionType))
            return;

        // Check if whenTrue is accessing .Value on the same result
        if (conditional.WhenTrue is MemberAccessExpressionSyntax whenTrueMemberAccess)
        {
            if (whenTrueMemberAccess.Name.Identifier.Text == "Value")
            {
                // Check if it's the same expression
                if (AreEquivalentExpressions(conditionMemberAccess.Expression, whenTrueMemberAccess.Expression))
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.UseFunctionalValueOrDefault,
                        conditional.GetLocation());

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    // Check if two expressions are equivalent (simplified comparison)
    private static bool AreEquivalentExpressions(ExpressionSyntax expr1, ExpressionSyntax expr2) =>
        expr1.ToString() == expr2.ToString();

    // Check if type is Result<T> from FunctionalDdd
    private static bool IsResultType(ITypeSymbol? typeSymbol) =>
        typeSymbol is INamedTypeSymbol namedType &&
        namedType.Name == "Result" &&
        namedType.ContainingNamespace?.ToDisplayString() == "FunctionalDdd" &&
        namedType.TypeArguments.Length == 1;
}