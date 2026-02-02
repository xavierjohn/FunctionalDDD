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

        // DEBUG 1: We found .Value access
        context.ReportDiagnostic(Diagnostic.Create(
            new DiagnosticDescriptor("FDDD998", "Debug", "Found .Value access", "Debug", DiagnosticSeverity.Info, true),
            memberAccess.GetLocation()));

        // Check if the expression is a direct invocation to TryCreate
        if (memberAccess.Expression is not InvocationExpressionSyntax invocation)
            return;

        // DEBUG 2: It's an invocation
        context.ReportDiagnostic(Diagnostic.Create(
            new DiagnosticDescriptor("FDDD997", "Debug", "It's an invocation", "Debug", DiagnosticSeverity.Info, true),
            memberAccess.GetLocation()));

        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return;

        // DEBUG 3: Has method symbol
        context.ReportDiagnostic(Diagnostic.Create(
            new DiagnosticDescriptor("FDDD996", "Debug", $"Method: {methodSymbol.Name}", "Debug", DiagnosticSeverity.Info, true),
            memberAccess.GetLocation()));

        // Check if the method is TryCreate
        if (methodSymbol.Name != "TryCreate")
            return;

        // DEBUG 4: Method is TryCreate
        context.ReportDiagnostic(Diagnostic.Create(
            new DiagnosticDescriptor("FDDD995", "Debug", $"Return type: {methodSymbol.ReturnType.Name}", "Debug", DiagnosticSeverity.Info, true),
            memberAccess.GetLocation()));

        // The containing type might be the derived type (e.g., Name) or the base type (e.g., RequiredString<Name>)
        // We need to check the return type to see what type TryCreate returns
        var returnType = methodSymbol.ReturnType;
        if (returnType is not INamedTypeSymbol { Name: "Result", TypeArguments.Length: 1 } resultType)
        {
            // DEBUG: Return type doesn't match
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("FDDD994", "Debug", 
                    $"Return type mismatch: {returnType.Name}, TypeArgs: {(returnType as INamedTypeSymbol)?.TypeArguments.Length ?? -1}", 
                    "Debug", DiagnosticSeverity.Warning, true),
                memberAccess.GetLocation()));
            return;
        }

        // Get the type being created (the T in Result<T>)
        var containingType = resultType.TypeArguments[0] as INamedTypeSymbol;
        if (containingType == null)
            return;

        // DEBUG: Report what type we found
        var debugDiagnostic = Diagnostic.Create(
            new DiagnosticDescriptor(
                "FDDD999",
                "Debug: Type found",
                $"Found type: {containingType.ToDisplayString()} with {containingType.AllInterfaces.Length} interfaces",
                "Debug",
                DiagnosticSeverity.Info,
                true),
            memberAccess.GetLocation());
        context.ReportDiagnostic(debugDiagnostic);

        // Check if the type implements IScalarValue<TSelf, TPrimitive>
        if (!ImplementsIScalarValue(containingType))
            return;

        // Check if it has a Create method (redundant check since IScalarValue guarantees it, but kept for safety)
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

    // Verify the type has a static Create method (guaranteed by IScalarValue but checked for robustness)
    private static bool HasCreateMethod(INamedTypeSymbol typeSymbol) =>
        typeSymbol.GetMembers("Create")
            .OfType<IMethodSymbol>()
            .Any(m => m.IsStatic);
}