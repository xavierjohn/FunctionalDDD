namespace Trellis.Analyzers;

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
/// Code fix for TRLS023: rewrites <c>CreatedAtRoute(routeName, routeValues)</c> to
/// <c>CreatedAtVersionedRoute(routeName, routeValues)</c>. The latter is provided by
/// the <c>Trellis.Asp.ApiVersioning</c> package and injects the api-version route value
/// at request time.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CreatedAtRouteMissingApiVersionCodeFixProvider))]
[Shared]
public sealed class CreatedAtRouteMissingApiVersionCodeFixProvider : CodeFixProvider
{
    private const string Title = "Use CreatedAtVersionedRoute";

    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.MissingApiVersionRouteValue.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan);
        var invocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();
        if (invocation is null) return;
        if (invocation.Expression is not MemberAccessExpressionSyntax mae) return;
        if (mae.Name.Identifier.Text != "CreatedAtRoute") return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: c => RewriteAsync(context.Document, mae, c),
                equivalenceKey: Title),
            diagnostic);
    }

    private static async Task<Document> RewriteAsync(
        Document document,
        MemberAccessExpressionSyntax memberAccess,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null) return document;

        var newName = SyntaxFactory.IdentifierName("CreatedAtVersionedRoute")
            .WithTriviaFrom(memberAccess.Name);
        var newMemberAccess = memberAccess.WithName(newName);
        var newRoot = root.ReplaceNode(memberAccess, newMemberAccess);
        return document.WithSyntaxRoot(newRoot);
    }
}
