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
/// Code fix provider that replaces SaveChangesAsync/SaveChanges with SaveChangesResultUnitAsync
/// or SaveChangesResultAsync depending on whether the return value is used.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseSaveChangesResultCodeFixProvider))]
[Shared]
public sealed class UseSaveChangesResultCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.UseSaveChangesResult.Id);

    public override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the method name identifier node
        var node = root.FindNode(diagnosticSpan);
        if (node is not IdentifierNameSyntax identifierNode)
            return;

        // For sync SaveChanges(), only offer the fix when it's a standalone expression statement.
        // The code fix converts the method to async Task.
        // Skip the fix when:
        //  - The call is not a standalone expression statement (assignments, returns, etc.)
        //  - The containing method has a non-void return type (async int is not valid C#)
        if (identifierNode.Identifier.Text == "SaveChanges")
        {
            var invocation = identifierNode.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocation?.FirstAncestorOrSelf<ExpressionStatementSyntax>() is null)
                return;

            var containingMethod = identifierNode.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (containingMethod is null)
                return;

            var isVoid = containingMethod.ReturnType is PredefinedTypeSyntax predefined
                         && predefined.Keyword.IsKind(SyntaxKind.VoidKeyword);
            if (!isVoid)
                return;
        }

        // Determine replacement: SaveChangesResultUnitAsync (standalone) vs SaveChangesResultAsync (value used)
        var replacementName = GetReplacementMethodName(identifierNode);
        var title = $"Use {replacementName}";

        context.RegisterCodeFix(
            CodeAction.Create(
                title: title,
                createChangedDocument: c => ApplyFixAsync(context.Document, identifierNode, replacementName, c),
                equivalenceKey: title),
            diagnostic);
    }

    /// <summary>
    /// Determines whether to use SaveChangesResultUnitAsync or SaveChangesResultAsync.
    /// If the return value of SaveChangesAsync is used (assignment, return, condition, etc.),
    /// SaveChangesResultAsync (returning Result&lt;int&gt;) is the correct replacement.
    /// Walks up through chained method calls (e.g. .ConfigureAwait(false)) to find the
    /// outermost invocation before checking whether the value is discarded.
    /// </summary>
    private static string GetReplacementMethodName(IdentifierNameSyntax identifierNode)
    {
        if (identifierNode.Identifier.Text != "SaveChangesAsync")
            return "SaveChangesResultUnitAsync";

        // Walk up: Identifier → MemberAccess → Invocation → possibly chained calls → possibly Await → check parent
        var invocation = identifierNode.FirstAncestorOrSelf<InvocationExpressionSyntax>();
        if (invocation is null)
            return "SaveChangesResultUnitAsync";

        // Walk up through chained method calls (e.g. .ConfigureAwait(false))
        // Pattern: InvocationExpression is the expression of a MemberAccessExpression,
        //          which is the expression of another InvocationExpression
        SyntaxNode current = invocation;
        while (current.Parent is MemberAccessExpressionSyntax memberAccess &&
               memberAccess.Parent is InvocationExpressionSyntax outerInvocation &&
               memberAccess.Expression == current)
        {
            current = outerInvocation;
        }

        // The effective node is the await expression if present, otherwise the outermost invocation
        SyntaxNode effectiveNode = current.Parent is AwaitExpressionSyntax awaitExpr
            ? awaitExpr
            : current;

        // If the effective expression is a direct child of an ExpressionStatement, the value is discarded
        return effectiveNode.Parent is ExpressionStatementSyntax
            ? "SaveChangesResultUnitAsync"
            : "SaveChangesResultAsync";
    }

    private static async Task<Document> ApplyFixAsync(
        Document document,
        IdentifierNameSyntax identifierNode,
        string replacementName,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        var newIdentifier = SyntaxFactory.IdentifierName(replacementName)
            .WithTriviaFrom(identifierNode);

        var newRoot = root.ReplaceNode(identifierNode, newIdentifier);

        // If the original call was synchronous SaveChanges(), we need to add await and make the method async
        if (identifierNode.Identifier.Text == "SaveChanges")
            newRoot = AddAwaitAndMakeAsync(newRoot, identifierNode.SpanStart);

        // Ensure 'using Trellis.EntityFrameworkCore;' is present (replacement methods are extension methods)
        newRoot = AddUsingIfMissing(newRoot, "Trellis.EntityFrameworkCore");

        return document.WithSyntaxRoot(newRoot);
    }

    private static SyntaxNode AddUsingIfMissing(SyntaxNode root, string namespaceName)
    {
        if (root is not CompilationUnitSyntax compilationUnit)
            return root;

        // Check top-level usings
        if (compilationUnit.Usings.Any(u => u.Name?.ToString() == namespaceName))
            return root;

        // Check usings inside namespace declarations
        foreach (var member in compilationUnit.Members)
        {
            var namespaceUsings = member switch
            {
                NamespaceDeclarationSyntax ns => ns.Usings,
                FileScopedNamespaceDeclarationSyntax fs => fs.Usings,
                _ => default
            };

            if (namespaceUsings.Any(u => u.Name?.ToString() == namespaceName))
                return root;
        }

        var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(namespaceName));

        // Match line endings from the last existing using directive
        var lastUsing = compilationUnit.Usings.LastOrDefault();
        usingDirective = lastUsing is not null
            ? usingDirective.WithTrailingTrivia(lastUsing.GetTrailingTrivia())
            : usingDirective.WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);

        return compilationUnit.AddUsings(usingDirective);
    }

    private static SyntaxNode AddAwaitAndMakeAsync(SyntaxNode root, int originalSpanStart)
    {
        // Find the invocation expression that contains our replaced identifier
        var newIdentifier = root.FindToken(originalSpanStart).Parent;
        var invocation = newIdentifier?.FirstAncestorOrSelf<InvocationExpressionSyntax>();
        if (invocation is null)
            return root;

        // Find the containing expression statement
        var expressionStatement = invocation.FirstAncestorOrSelf<ExpressionStatementSyntax>();
        if (expressionStatement is null)
            return root;

        // Wrap the invocation in an await expression
        var awaitExpression = SyntaxFactory.AwaitExpression(invocation.WithoutLeadingTrivia())
            .WithLeadingTrivia(invocation.GetLeadingTrivia());
        var newStatement = expressionStatement.WithExpression(awaitExpression);
        root = root.ReplaceNode(expressionStatement, newStatement);

        // Find the containing method and make it async with Task return type
        var methodDecl = root.FindToken(originalSpanStart).Parent?
            .FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (methodDecl is null)
            return root;

        // Skip if already async
        if (methodDecl.Modifiers.Any(SyntaxKind.AsyncKeyword))
            return root;

        var newMethod = methodDecl
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.AsyncKeyword).WithTrailingTrivia(SyntaxFactory.Space));

        // Change void return type to Task
        if (methodDecl.ReturnType is PredefinedTypeSyntax predefined &&
            predefined.Keyword.IsKind(SyntaxKind.VoidKeyword))
        {
            var taskType = SyntaxFactory.ParseTypeName("Task").WithTriviaFrom(methodDecl.ReturnType);
            newMethod = newMethod.WithReturnType(taskType);
        }

        root = root.ReplaceNode(methodDecl, newMethod);

        // Add 'using System.Threading.Tasks;' if not already present
        root = AddUsingIfMissing(root, "System.Threading.Tasks");

        return root;
    }
}
