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

        // 1. Rename the member: CreatedAtRoute → CreatedAtVersionedRoute.
        var newName = SyntaxFactory.IdentifierName("CreatedAtVersionedRoute")
            .WithTriviaFrom(memberAccess.Name);
        var newMemberAccess = memberAccess.WithName(newName);
        var newRoot = root.ReplaceNode(memberAccess, newMemberAccess);

        // 2. Ensure `using Trellis.Asp.ApiVersioning;` is in scope. Without this the rewritten
        //    extension call won't resolve and the code-fix produces uncompilable code. We don't
        //    detect the package reference here (analyzers can't observe project metadata
        //    reliably), so we always add the using if missing — if the package isn't referenced,
        //    the user gets a clear "missing reference" build error pointing to the new namespace
        //    rather than a confusing "method not found" error.
        if (newRoot is CompilationUnitSyntax cu && !HasUsing(cu, ApiVersioningNamespace))
        {
            cu = AddUsing(cu, ApiVersioningNamespace);
            return document.WithSyntaxRoot(cu);
        }

        return document.WithSyntaxRoot(newRoot);
    }

    private const string ApiVersioningNamespace = "Trellis.Asp.ApiVersioning";

    /// <summary>
    /// Adds <paramref name="namespaceName"/> as a <c>using</c> directive in the same scope where
    /// existing usings live. The repo convention is file-scoped namespaces with usings *inside*
    /// the namespace block, so adding to <see cref="CompilationUnitSyntax.Usings"/> would place
    /// the new using above the namespace declaration — out of order with the existing usings,
    /// and potentially flagged by IDE0065 (using directive placement).
    /// </summary>
    private static CompilationUnitSyntax AddUsing(CompilationUnitSyntax cu, string namespaceName)
    {
        // Locate where existing usings live, so we add the new using to the same scope.
        var (fileScopedNs, blockScopedNs) = FindNamespaceWithUsings(cu);

        if (fileScopedNs is not null)
        {
            var trailing = fileScopedNs.Usings.Count > 0
                ? fileScopedNs.Usings[fileScopedNs.Usings.Count - 1].GetTrailingTrivia()
                : SyntaxFactory.TriviaList(SyntaxFactory.ElasticCarriageReturnLineFeed);

            var directive = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(namespaceName))
                .WithTrailingTrivia(trailing);
            var updated = fileScopedNs.AddUsings(directive);
            return cu.ReplaceNode(fileScopedNs, updated);
        }

        if (blockScopedNs is not null)
        {
            var trailing = blockScopedNs.Usings.Count > 0
                ? blockScopedNs.Usings[blockScopedNs.Usings.Count - 1].GetTrailingTrivia()
                : SyntaxFactory.TriviaList(SyntaxFactory.ElasticCarriageReturnLineFeed);

            var directive = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(namespaceName))
                .WithTrailingTrivia(trailing);
            var updated = blockScopedNs.AddUsings(directive);
            return cu.ReplaceNode(blockScopedNs, updated);
        }

        // No namespace usings — fall back to top-level usings.
        var topTrailing = cu.Usings.Count > 0
            ? cu.Usings[cu.Usings.Count - 1].GetTrailingTrivia()
            : SyntaxFactory.TriviaList(SyntaxFactory.ElasticCarriageReturnLineFeed);

        var topDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(namespaceName))
            .WithTrailingTrivia(topTrailing);
        return cu.AddUsings(topDirective);
    }

    private static (FileScopedNamespaceDeclarationSyntax? FileScoped, NamespaceDeclarationSyntax? BlockScoped)
        FindNamespaceWithUsings(CompilationUnitSyntax cu)
    {
        // Prefer a namespace that already declares usings (matches the existing scope). If none
        // does, fall back to the first namespace declaration found (still keeps usings consistent
        // with the file's namespace style).
        FileScopedNamespaceDeclarationSyntax? firstFileScoped = null;
        NamespaceDeclarationSyntax? firstBlockScoped = null;

        foreach (var member in cu.Members)
        {
            switch (member)
            {
                case FileScopedNamespaceDeclarationSyntax fs:
                    firstFileScoped ??= fs;
                    if (fs.Usings.Count > 0)
                        return (fs, null);
                    break;
                case NamespaceDeclarationSyntax ns:
                    firstBlockScoped ??= ns;
                    if (ns.Usings.Count > 0)
                        return (null, ns);
                    break;
            }
        }

        // No namespace had usings. If the file uses namespace declarations at all, prefer adding
        // there (consistent style); otherwise the caller falls back to top-level.
        if (cu.Usings.Count > 0)
            return (null, null);

        return (firstFileScoped, firstBlockScoped);
    }

    private static bool HasUsing(CompilationUnitSyntax cu, string namespaceName)
    {
        // Top-level usings (file-scoped or block-scoped) before any namespace declaration.
        foreach (var u in cu.Usings)
        {
            if (u.Name?.ToString() == namespaceName)
                return true;
        }

        // Usings inside namespace declarations. The repo convention is file-scoped namespaces
        // with usings *inside* the namespace block:
        //
        //     namespace Trellis.Foo;
        //     using Bar;
        //
        // so cu.Usings is often empty even when `using Trellis.Asp.ApiVersioning;` is already
        // in scope. Walk both NamespaceDeclarationSyntax and FileScopedNamespaceDeclarationSyntax.
        foreach (var member in cu.Members)
        {
            var nsUsings = member switch
            {
                NamespaceDeclarationSyntax ns => ns.Usings,
                FileScopedNamespaceDeclarationSyntax fs => fs.Usings,
                _ => default,
            };

            foreach (var u in nsUsings)
            {
                if (u.Name?.ToString() == namespaceName)
                    return true;
            }
        }

        return false;
    }
}
