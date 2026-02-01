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
/// Code fix provider that wraps unsafe Result.Value, Result.Error, or Maybe.Value access
/// in appropriate guard statements (if checks).
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddResultGuardCodeFixProvider))]
[Shared]
public sealed class AddResultGuardCodeFixProvider : CodeFixProvider
{
    private const string TitleValue = "Add 'if (result.IsSuccess)' guard";
    private const string TitleError = "Add 'if (result.IsFailure)' guard";
    private const string TitleMaybe = "Add 'if (maybe.HasValue)' guard";

    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(
            DiagnosticDescriptors.UnsafeResultValueAccess.Id,
            DiagnosticDescriptors.UnsafeResultErrorAccess.Id,
            DiagnosticDescriptors.UnsafeMaybeValueAccess.Id);

    public override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the member access (.Value or .Error)
        // The diagnostic is on the identifier, so we need to get the parent MemberAccessExpression
        var node = root.FindNode(diagnosticSpan);
        var memberAccess = node.Parent as MemberAccessExpressionSyntax ?? node as MemberAccessExpressionSyntax;
        if (memberAccess == null)
            return;

        // Determine the guard type based on diagnostic ID
        var (title, guardProperty, guardValue) = diagnostic.Id switch
        {
            "FDDD003" => (TitleValue, "IsSuccess", true),   // Result.Value
            "FDDD004" => (TitleError, "IsFailure", true),   // Result.Error
            "FDDD005" => (TitleMaybe, "HasValue", true),    // Maybe.Value
            _ => (null, null, false)
        };

        if (title == null || guardProperty == null)
            return;

        // Register the code fix
        context.RegisterCodeFix(
            CodeAction.Create(
                title: title,
                createChangedDocument: c => AddGuardAsync(
                    context.Document,
                    memberAccess,
                    guardProperty,
                    c),
                equivalenceKey: title),
            diagnostic);
    }

    private static async Task<Document> AddGuardAsync(
        Document document,
        MemberAccessExpressionSyntax memberAccess,
        string guardProperty,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        // Find the statement containing the member access
        var statement = memberAccess.FirstAncestorOrSelf<StatementSyntax>();
        if (statement == null)
            return document;

        // Get the expression being accessed (e.g., "result" from "result.Error")
        var resultExpression = memberAccess.Expression;

        // Create the guard condition: result.IsSuccess or result.IsFailure or maybe.HasValue
        var guardCondition = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            resultExpression,
            SyntaxFactory.IdentifierName(guardProperty));

        // Create the if statement wrapping the original statement
        var ifStatement = SyntaxFactory.IfStatement(
            guardCondition,
            statement.WithoutLeadingTrivia())
            .WithLeadingTrivia(statement.GetLeadingTrivia())
            .WithTrailingTrivia(statement.GetTrailingTrivia())
            .WithAdditionalAnnotations(Microsoft.CodeAnalysis.Formatting.Formatter.Annotation);

        // Replace the original statement with the if statement
        var newRoot = root.ReplaceNode(statement, ifStatement);
        
        // Format the document to fix indentation
        var formattedRoot = Microsoft.CodeAnalysis.Formatting.Formatter.Format(
            newRoot, 
            Microsoft.CodeAnalysis.Formatting.Formatter.Annotation, 
            document.Project.Solution.Workspace,
            cancellationToken: cancellationToken);
        return document.WithSyntaxRoot(formattedRoot);
    }
}
