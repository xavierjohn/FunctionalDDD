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

        if (argumentType == null)
            return;

        if (IsResultType(argumentType))
        {
            var innerType = GetResultInnerType(argumentType);
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.ResultDoubleWrapping,
                argument.GetLocation(),
                innerType ?? "T");

            context.ReportDiagnostic(diagnostic);
        }
    }

    // Check if type is Result<Result<T>>
    private static bool IsDoubleWrappedResult(ITypeSymbol typeSymbol, out string? innerType)
    {
        innerType = null;

        if (!IsResultType(typeSymbol))
            return false;

        var outerResultType = GetResultInnerType(typeSymbol);
        if (outerResultType == null)
            return false;

        if (typeSymbol is INamedTypeSymbol namedType && namedType.TypeArguments.Length == 1)
        {
            var innerTypeSymbol = namedType.TypeArguments[0];
            if (IsResultType(innerTypeSymbol))
            {
                innerType = GetResultInnerType(innerTypeSymbol) ?? "T";
                return true;
            }
        }

        return false;
    }

    // Check if type is Result<T> from FunctionalDdd
    private static bool IsResultType(ITypeSymbol typeSymbol) =>
        typeSymbol is INamedTypeSymbol namedType &&
        namedType.Name == "Result" &&
        namedType.ContainingNamespace?.ToDisplayString() == "FunctionalDdd" &&
        namedType.TypeArguments.Length == 1;

    // Get the T from Result<T>
    private static string? GetResultInnerType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedType && namedType.TypeArguments.Length == 1)
            return namedType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        return null;
    }

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
