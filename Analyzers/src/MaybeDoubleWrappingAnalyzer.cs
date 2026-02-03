namespace FunctionalDdd.Analyzers;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects Maybe&lt;Maybe&lt;T&gt;&gt; double wrapping in type declarations.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MaybeDoubleWrappingAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticDescriptors.MaybeDoubleWrapping];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration,
            SyntaxKind.VariableDeclaration,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.Parameter);
    }

    private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        ITypeSymbol? typeSymbol = context.Node switch
        {
            VariableDeclarationSyntax variable => context.SemanticModel.GetTypeInfo(variable.Type).Type,
            PropertyDeclarationSyntax property => context.SemanticModel.GetTypeInfo(property.Type).Type,
            MethodDeclarationSyntax method => context.SemanticModel.GetTypeInfo(method.ReturnType).Type,
            ParameterSyntax parameter when parameter.Type != null => context.SemanticModel.GetTypeInfo(parameter.Type).Type,
            _ => null
        };

        if (typeSymbol == null)
            return;

        if (IsDoubleWrappedMaybe(typeSymbol, out var innerType))
        {
            var location = context.Node switch
            {
                VariableDeclarationSyntax v => v.Type.GetLocation(),
                PropertyDeclarationSyntax p => p.Type.GetLocation(),
                MethodDeclarationSyntax m => m.ReturnType.GetLocation(),
                ParameterSyntax param when param.Type != null => param.Type.GetLocation(),
                _ => context.Node.GetLocation()
            };

            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.MaybeDoubleWrapping,
                location,
                innerType);

            context.ReportDiagnostic(diagnostic);
        }
    }

    // Check if type is Maybe<Maybe<T>>
    private static bool IsDoubleWrappedMaybe(ITypeSymbol typeSymbol, out string? innerType)
    {
        innerType = null;

        // Check if outer type is Maybe<T> with exactly 1 type argument
        if (typeSymbol is not INamedTypeSymbol { Name: "Maybe", TypeArguments.Length: 1 } outerMaybe ||
            outerMaybe.ContainingNamespace?.ToDisplayString() != "FunctionalDdd")
            return false;

        // Check if inner type is also Maybe<T>
        var innerTypeSymbol = outerMaybe.TypeArguments[0];
        if (innerTypeSymbol is INamedTypeSymbol { Name: "Maybe", TypeArguments.Length: 1 } innerMaybe &&
            innerMaybe.ContainingNamespace?.ToDisplayString() == "FunctionalDdd")
        {
            innerType = innerMaybe.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            return true;
        }

        return false;
    }
}