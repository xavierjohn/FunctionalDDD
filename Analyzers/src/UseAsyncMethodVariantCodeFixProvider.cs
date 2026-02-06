namespace FunctionalDdd.Analyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Code fix provider that replaces sync method with async variant when lambda is async.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseAsyncMethodVariantCodeFixProvider))]
[Shared]
public sealed class UseAsyncMethodVariantCodeFixProvider : CodeFixProvider
{
    private const string Title = "Use async method variant";

    private static readonly ImmutableDictionary<string, string> SyncToAsyncMethods =
        ImmutableDictionary<string, string>.Empty
            .Add("Map", "MapAsync")
            .Add("Bind", "BindAsync")
            .Add("Tap", "TapAsync")
            .Add("Ensure", "EnsureAsync")
            .Add("TapOnFailure", "TapOnFailureAsync");

    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.UseAsyncMethodVariant.Id);

    public override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the method name identifier
        var node = root.FindNode(diagnosticSpan);
        var identifierName = node as IdentifierNameSyntax;
        if (identifierName == null)
            return;

        var methodName = identifierName.Identifier.Text;
        if (!SyncToAsyncMethods.TryGetValue(methodName, out var asyncVariant))
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Replace '{methodName}' with '{asyncVariant}'",
                createChangedDocument: c => ReplaceWithAsyncVariantAsync(context.Document, identifierName, asyncVariant, c),
                equivalenceKey: Title),
            diagnostic);
    }

    private static async Task<Document> ReplaceWithAsyncVariantAsync(
        Document document,
        IdentifierNameSyntax identifierName,
        string asyncVariant,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        var newIdentifier = SyntaxFactory.IdentifierName(asyncVariant)
            .WithTriviaFrom(identifierName);

        var newRoot = root.ReplaceNode(identifierName, newIdentifier);
        return document.WithSyntaxRoot(newRoot);
    }
}