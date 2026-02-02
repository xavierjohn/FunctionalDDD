namespace FunctionalDdd.Analyzers;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects when .Value is accessed on a TryCreate() result for types
/// implementing IScalarValue, suggesting to use Create() instead for clearer intent.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TryCreateValueAccessAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticDescriptors.UseCreateInsteadOfTryCreateValue];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        var memberAccess = (MemberAccessExpressionSyntax)context.Node;

        // Check if accessing .Value
        if (memberAccess.Name.Identifier.Text != "Value")
            return;

        // Check if the expression is a direct invocation to TryCreate
        if (memberAccess.Expression is not InvocationExpressionSyntax invocation)
            return;

        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return;

        // Check if the method is TryCreate
        if (methodSymbol.Name != "TryCreate")
            return;

        // Check the return type is Result<T>
        var returnType = methodSymbol.ReturnType;
        if (returnType is not INamedTypeSymbol { Name: "Result", TypeArguments.Length: 1 } resultType)
            return;

        // Get the type being created (the T in Result<T>)
        var containingType = resultType.TypeArguments[0] as INamedTypeSymbol;
        if (containingType == null)
            return;

        // Simplified check: If it's a static method named TryCreate that returns Result<T>,
        // and there's a corresponding Create method, suggest using Create instead.
        // This works for both IScalarValue types and any similar pattern.
        if (!methodSymbol.IsStatic)
            return;

        if (!HasCreateMethod(containingType))
            return;

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.UseCreateInsteadOfTryCreateValue,
            memberAccess.GetLocation(),
            containingType.Name);

        context.ReportDiagnostic(diagnostic);
    }

    // Check if the type implements IScalarValue<TSelf, TPrimitive>
    // Note: We check for the interface name and structure, not the namespace,
    // so this works for user-defined types in any namespace
    private static bool ImplementsIScalarValue(INamedTypeSymbol typeSymbol)
    {
        // Check the type itself and all its base types
        var currentType = typeSymbol;
        while (currentType != null)
        {
            if (currentType.AllInterfaces.Any(i =>
                i.Name == "IScalarValue" &&
                i.TypeArguments.Length == 2 &&
                i.ContainingNamespace?.ToDisplayString() == "FunctionalDdd"))
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }

    // Verify the type has a static Create method (checks the type and its base types)
    private static bool HasCreateMethod(INamedTypeSymbol typeSymbol)
    {
        var currentType = typeSymbol;
        while (currentType != null)
        {
            if (currentType.GetMembers("Create")
                .OfType<IMethodSymbol>()
                .Any(m => m.IsStatic))
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }
}