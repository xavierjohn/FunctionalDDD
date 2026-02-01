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
/// Code fix provider that replaces Map with Bind when the lambda returns a Result.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseBindInsteadOfMapCodeFixProvider))]
[Shared]
public sealed class UseBindInsteadOfMapCodeFixProvider : CodeFixProvider
{
    private const string Title = "Replace Map with Bind";

    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.UseBindInsteadOfMap.Id);

    public override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the Map/MapAsync identifier
        var identifierNode = root.FindNode(diagnosticSpan) as IdentifierNameSyntax;
        if (identifierNode == null)
            return;

        // Register the code fix
        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: c => ReplaceMapWithBindAsync(context.Document, identifierNode, c),
                equivalenceKey: Title),
            diagnostic);
    }

    private static async Task<Document> ReplaceMapWithBindAsync(
        Document document,
        IdentifierNameSyntax mapIdentifier,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        // Determine the replacement method name
        var currentName = mapIdentifier.Identifier.Text;
        var newName = currentName switch
        {
            "Map" => "Bind",
            "MapAsync" => "BindAsync",
            _ => currentName
        };

        // Create the new identifier
        var newIdentifier = SyntaxFactory.IdentifierName(newName)
            .WithTriviaFrom(mapIdentifier);

        // Replace the node
        var newRoot = root.ReplaceNode(mapIdentifier, newIdentifier);
        return document.WithSyntaxRoot(newRoot);
    }
}
