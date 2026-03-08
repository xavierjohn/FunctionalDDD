namespace Trellis.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects direct SaveChangesAsync/SaveChanges calls on DbContext
/// and suggests using SaveChangesResultAsync/SaveChangesResultUnitAsync instead.
/// Only activates when the compilation references the Trellis.EntityFrameworkCore assembly.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseSaveChangesResultAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> TargetMethodNames =
        ["SaveChangesAsync", "SaveChanges"];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticDescriptors.UseSaveChangesResult];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            // Only activate when the compilation references Trellis.EntityFrameworkCore
            // by checking for the well-known DbContextExtensions type
            var dbContextExtensionsType = compilationContext.Compilation.GetTypeByMetadataName(
                "Trellis.EntityFrameworkCore.DbContextExtensions");
            if (dbContextExtensionsType is null)
                return;

            // Get the DbContext type symbol
            var dbContextType = compilationContext.Compilation.GetTypeByMetadataName(
                "Microsoft.EntityFrameworkCore.DbContext");
            if (dbContextType is null)
                return;

            compilationContext.RegisterSyntaxNodeAction(
                ctx => AnalyzeInvocation(ctx, dbContextType),
                SyntaxKind.InvocationExpression);
        });
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, INamedTypeSymbol dbContextType)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Get method name from member access (e.g., _dbContext.SaveChangesAsync)
        string methodName;
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            methodName = memberAccess.Name.Identifier.Text;
        else if (invocation.Expression is IdentifierNameSyntax identifier)
            // Handles unqualified calls like SaveChangesAsync() from within a DbContext subclass
            methodName = identifier.Identifier.Text;
        else
            return;

        if (!TargetMethodNames.Contains(methodName))
            return;

        // Get the method symbol
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return;

        // Check if the method's containing type is or inherits from DbContext
        if (!InheritsFromOrEquals(methodSymbol.ContainingType, dbContextType))
            return;

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.UseSaveChangesResult,
            invocation.Expression is MemberAccessExpressionSyntax ma
                ? ma.Name.GetLocation()
                : invocation.Expression.GetLocation(),
            methodName);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool InheritsFromOrEquals(INamedTypeSymbol? type, INamedTypeSymbol baseType)
    {
        while (type is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(type, baseType))
                return true;
            type = type.BaseType;
        }

        return false;
    }
}