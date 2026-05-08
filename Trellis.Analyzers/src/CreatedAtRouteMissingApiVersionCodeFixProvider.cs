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
            // Match the existing usings' trailing newline style (CRLF on Windows-style files,
            // LF on others) so we don't introduce a mixed-line-ending diff.
            var trailingNewline = cu.Usings.Count > 0
                ? cu.Usings[cu.Usings.Count - 1].GetTrailingTrivia()
                : SyntaxFactory.TriviaList(SyntaxFactory.ElasticCarriageReturnLineFeed);

            var usingDirective = SyntaxFactory.UsingDirective(
                    SyntaxFactory.ParseName(ApiVersioningNamespace))
                .WithTrailingTrivia(trailingNewline);
            cu = cu.AddUsings(usingDirective);
            return document.WithSyntaxRoot(cu);
        }

        return document.WithSyntaxRoot(newRoot);
    }

    private const string ApiVersioningNamespace = "Trellis.Asp.ApiVersioning";

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
