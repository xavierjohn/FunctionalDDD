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
        ImmutableArray.Create(DiagnosticDescriptors.MaybeDoubleWrapping);

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
            ParameterSyntax parameter => context.SemanticModel.GetTypeInfo(parameter.Type!).Type,
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
                ParameterSyntax param => param.Type!.GetLocation(),
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

        if (!IsMaybeType(typeSymbol))
            return false;

        if (typeSymbol is INamedTypeSymbol namedType && namedType.TypeArguments.Length == 1)
        {
            var innerTypeSymbol = namedType.TypeArguments[0];
            if (IsMaybeType(innerTypeSymbol))
            {
                innerType = GetMaybeInnerType(innerTypeSymbol) ?? "T";
                return true;
            }
        }

        return false;
    }

    // Check if type is Maybe<T> from FunctionalDdd
    private static bool IsMaybeType(ITypeSymbol typeSymbol) =>
        typeSymbol is INamedTypeSymbol namedType &&
        namedType.Name == "Maybe" &&
        namedType.ContainingNamespace?.ToDisplayString() == "FunctionalDdd" &&
        namedType.TypeArguments.Length == 1;

    // Get the T from Maybe<T>
    private static string? GetMaybeInnerType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedType && namedType.TypeArguments.Length == 1)
            return namedType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        return null;
    }
}
