namespace FunctionalDdd.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

/// <summary>
/// Analyzer that detects manual combination of multiple Result.IsSuccess checks
/// and suggests using Result.Combine() instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseResultCombineAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticDescriptors.UseResultCombine];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterOperationAction(AnalyzeConditional, OperationKind.Conditional);
    }

    private static void AnalyzeConditional(OperationAnalysisContext context)
    {
        var conditional = (IConditionalOperation)context.Operation;

        // Check if the condition is a binary AND operation
        if (conditional.Condition is not IBinaryOperation binaryOp)
            return;

        if (binaryOp.OperatorKind != BinaryOperatorKind.ConditionalAnd)
            return;

        // Count IsSuccess checks in the condition
        var isSuccessCount = CountIsSuccessChecks(binaryOp);

        // If there are 2 or more IsSuccess checks, suggest using Combine
        if (isSuccessCount >= 2)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.UseResultCombine,
                conditional.Syntax.GetLocation());

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static int CountIsSuccessChecks(IOperation operation)
    {
        var count = 0;

        if (operation is IBinaryOperation binaryOp)
        {
            // Recursively count in left and right operands
            count += CountIsSuccessChecks(binaryOp.LeftOperand);
            count += CountIsSuccessChecks(binaryOp.RightOperand);
        }
        else if (operation is IPropertyReferenceOperation propertyRef)
        {
            // Check if it's accessing IsSuccess on a Result
            if (propertyRef.Property.Name == "IsSuccess" &&
                propertyRef.Property.ContainingType.IsResultType())
            {
                count++;
            }
        }

        return count;
    }
}