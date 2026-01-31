namespace FunctionalDdd.Analyzers;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects Result&lt;Result&lt;T&gt;&gt; double wrapping in type declarations,
/// variable assignments, and factory method calls.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ResultDoubleWrappingAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ResultDoubleWrapping);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, 
            SyntaxKind.VariableDeclaration,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.Parameter);

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        ITypeSymbol? typeSymbol = context.Node switch
        {
            VariableDeclarationSyntax variable => context.SemanticModel.GetTypeInfo(variable.Type).Type,
            PropertyDeclarationSyntax property => context.SemanticModel.GetTypeInfo(property.Type).Type,
            MethodDeclarationSyntax method => context.SemanticModel.GetTypeInfo(method.ReturnType).Type,
            ParameterSyntax parameter => context.SemanticModel.GetTypeInfo(parameter.Type!).Type,
            _ => null
        };

        if (typeSymbol == null)
            return;

        if (IsDoubleWrappedResult(typeSymbol, out var innerType))
        {
            var location = context.Node switch
            {
                VariableDeclarationSyntax v => v.Type.GetLocation(),
                PropertyDeclarationSyntax p => p.Type.GetLocation(),
                MethodDeclarationSyntax m => m.ReturnType.GetLocation(),
                ParameterSyntax param => param.Type!.GetLocation(),
                _ => context.Node.GetLocation()
            };

            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.ResultDoubleWrapping,
                location,
                innerType);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check for Result.Success(result) or Result.Failure(result)
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null)
            return;

        // Check if it's Result.Success or Result.Failure
        if (!IsResultFactoryMethod(methodSymbol))
            return;

        // Check if the argument is already a Result
        if (invocation.ArgumentList.Arguments.Count != 1)
            return;

        var argument = invocation.ArgumentList.Arguments[0];
        var argumentType = context.SemanticModel.GetTypeInfo(argument.Expression).Type;

        if (argumentType is INamedTypeSymbol { TypeArguments.Length: 1 } resultType &&
            IsResultType(argumentType))
        {
            var innerType = resultType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.ResultDoubleWrapping,
                argument.GetLocation(),
                innerType);

            context.ReportDiagnostic(diagnostic);
        }
    }

    // Check if type is Result<Result<T>>
    private static bool IsDoubleWrappedResult(ITypeSymbol typeSymbol, out string? innerType)
    {
        innerType = null;

        // Check if outer type is Result<T> with exactly 1 type argument
        if (typeSymbol is not INamedTypeSymbol { Name: "Result", TypeArguments.Length: 1 } outerResult ||
            outerResult.ContainingNamespace?.ToDisplayString() != "FunctionalDdd")
            return false;

        // Check if inner type is also Result<T>
        var innerTypeSymbol = outerResult.TypeArguments[0];
        if (innerTypeSymbol is INamedTypeSymbol { Name: "Result", TypeArguments.Length: 1 } innerResult &&
            innerResult.ContainingNamespace?.ToDisplayString() == "FunctionalDdd")
        {
            innerType = innerResult.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            return true;
        }

        return false;
    }

    // Check if type is Result<T> from FunctionalDdd
    private static bool IsResultType(ITypeSymbol typeSymbol) =>
        typeSymbol is INamedTypeSymbol { Name: "Result", TypeArguments.Length: 1 } namedType &&
        namedType.ContainingNamespace?.ToDisplayString() == "FunctionalDdd";

    // Check if method is Result.Success or Result.Failure
    private static bool IsResultFactoryMethod(IMethodSymbol methodSymbol)
    {
        if (methodSymbol.ContainingType?.Name != "Result")
            return false;

        if (methodSymbol.ContainingType.ContainingNamespace?.ToDisplayString() != "FunctionalDdd")
            return false;

        return methodSymbol.Name is "Success" or "Failure";
    }
}
