namespace FunctionalDdd.Analyzers;

using System.Collections.Immutable;
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

        if (typeSymbol.IsDoubleWrappedMaybe(out var innerType))
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
}