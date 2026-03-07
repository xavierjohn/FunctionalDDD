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
/// Code fix provider that replaces SaveChangesAsync/SaveChanges with SaveChangesResultUnitAsync.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseSaveChangesResultCodeFixProvider))]
[Shared]
public sealed class UseSaveChangesResultCodeFixProvider : CodeFixProvider
{
    private const string Title = "Use SaveChangesResultUnitAsync";

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
        // When used in assignments, returns, etc., the sync-to-async conversion is too complex
        // for an automatic fix — the developer must refactor manually.
        if (identifierNode.Identifier.Text == "SaveChanges")
        {
            var invocation = identifierNode.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocation?.FirstAncestorOrSelf<ExpressionStatementSyntax>() is null)
                return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: c => ReplaceSaveChangesAsync(context.Document, identifierNode, c),
                equivalenceKey: Title),
            diagnostic);
    }

    private static async Task<Document> ReplaceSaveChangesAsync(
        Document document,
        IdentifierNameSyntax identifierNode,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        var newIdentifier = SyntaxFactory.IdentifierName("SaveChangesResultUnitAsync")
            .WithTriviaFrom(identifierNode);

        var newRoot = root.ReplaceNode(identifierNode, newIdentifier);

        // If the original call was synchronous SaveChanges(), we need to add await and make the method async
        if (identifierNode.Identifier.Text == "SaveChanges")
            newRoot = AddAwaitAndMakeAsync(newRoot, identifierNode.SpanStart);

        return document.WithSyntaxRoot(newRoot);
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

        return root.ReplaceNode(methodDecl, newMethod);
    }
}
