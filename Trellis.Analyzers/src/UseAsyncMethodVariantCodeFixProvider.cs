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

        var node = root.FindNode(diagnosticSpan);
        if (node is not IdentifierNameSyntax identifierName)
            return;

        var methodName = identifierName.Identifier.Text;
        if (!SyncToAsyncMethods.TryGetValue(methodName, out var asyncVariant))
            return;

        var invocationToAwait = FindInvocationToAwait(identifierName);
        if (invocationToAwait is null ||
            HasNonAsyncAnonymousFunctionArgument(invocationToAwait) ||
            !CanAwaitInvocationInPlace(invocationToAwait))
            return;

        var enclosingFunction = FindEnclosingFunction(identifierName);
        if (enclosingFunction is null || !CanApplyFix(enclosingFunction, invocationToAwait))
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Replace '{methodName}' with '{asyncVariant}' and await the result",
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

        var currentNode = root.FindNode(identifierName.Span);
        if (currentNode is not IdentifierNameSyntax currentIdentifier)
            return document;

        var invocationToAwait = FindInvocationToAwait(currentIdentifier);
        if (invocationToAwait is null ||
            HasNonAsyncAnonymousFunctionArgument(invocationToAwait) ||
            !CanAwaitInvocationInPlace(invocationToAwait))
            return document;

        var enclosingFunction = FindEnclosingFunction(currentIdentifier);
        if (enclosingFunction is null || !CanApplyFix(enclosingFunction, invocationToAwait))
            return document;

        var newIdentifier = SyntaxFactory.IdentifierName(asyncVariant)
            .WithTriviaFrom(currentIdentifier);

        var renamedInvocation = invocationToAwait.ReplaceNode(currentIdentifier, newIdentifier);
        ExpressionSyntax replacementExpression = invocationToAwait.Parent is AwaitExpressionSyntax
            ? renamedInvocation
            : SyntaxFactory.AwaitExpression(renamedInvocation.WithoutLeadingTrivia())
                .WithLeadingTrivia(invocationToAwait.GetLeadingTrivia());

        var updatedFunction = enclosingFunction.ReplaceNode(invocationToAwait, replacementExpression);
        updatedFunction = AddAsyncModifierIfNeeded(updatedFunction);

        var newRoot = root.ReplaceNode(enclosingFunction, updatedFunction);
        return document.WithSyntaxRoot(newRoot);
    }

    private static InvocationExpressionSyntax? FindInvocationToAwait(IdentifierNameSyntax identifierName)
    {
        var invocation = identifierName.FirstAncestorOrSelf<InvocationExpressionSyntax>();
        if (invocation is null)
            return null;

        return invocation.Parent is MemberAccessExpressionSyntax memberAccess && memberAccess.Expression == invocation
            ? null
            : invocation;
    }

    private static bool HasNonAsyncAnonymousFunctionArgument(InvocationExpressionSyntax invocation) =>
        invocation.ArgumentList.Arguments.Any(argument =>
            (argument.Expression is LambdaExpressionSyntax { Modifiers: var lambdaModifiers } &&
             !lambdaModifiers.Any(SyntaxKind.AsyncKeyword)) ||
            (argument.Expression is AnonymousMethodExpressionSyntax { Modifiers: var anonymousModifiers } &&
             !anonymousModifiers.Any(SyntaxKind.AsyncKeyword)));

    private static bool CanAwaitInvocationInPlace(InvocationExpressionSyntax invocation)
    {
        SyntaxNode current = invocation;
        while (true)
        {
            if (current.Parent is ParenthesizedExpressionSyntax parenthesized && parenthesized.Expression == current)
            {
                current = parenthesized;
                continue;
            }

            if (current.Parent is AwaitExpressionSyntax awaitExpression && awaitExpression.Expression == current)
            {
                current = awaitExpression;
                continue;
            }

            break;
        }

        return current.Parent switch
        {
            EqualsValueClauseSyntax equalsValue when equalsValue.Value == current => IsSafeVarDeclaration(equalsValue),
            ReturnStatementSyntax returnStatement when returnStatement.Expression == current => true,
            ArrowExpressionClauseSyntax arrowExpression when arrowExpression.Expression == current => true,
            ExpressionStatementSyntax expressionStatement when expressionStatement.Expression == current => true,
            _ => false
        };
    }

    private static bool IsSafeVarDeclaration(EqualsValueClauseSyntax equalsValue) =>
        equalsValue.Parent is VariableDeclaratorSyntax variable &&
        variable.Parent is VariableDeclarationSyntax declaration &&
        declaration.Type.IsVar &&
        !IsVariableReferencedAfterDeclaration(variable);

    private static bool IsVariableReferencedAfterDeclaration(VariableDeclaratorSyntax variable)
    {
        var enclosingFunction = FindEnclosingFunction(variable);
        if (enclosingFunction is null)
            return true;

        var variableName = variable.Identifier.ValueText;
        return enclosingFunction.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Any(identifier => identifier.SpanStart > variable.Span.End &&
                               identifier.Identifier.ValueText == variableName);
    }

    private static SyntaxNode? FindEnclosingFunction(SyntaxNode node) =>
        node.Ancestors().FirstOrDefault(a =>
            a is MethodDeclarationSyntax or
                LocalFunctionStatementSyntax or
                LambdaExpressionSyntax or
                AnonymousMethodExpressionSyntax);

    private static bool CanApplyFix(SyntaxNode enclosingFunction, InvocationExpressionSyntax invocationToAwait) =>
        enclosingFunction switch
        {
            MethodDeclarationSyntax method => method.Modifiers.Any(SyntaxKind.AsyncKeyword)
                ? !IsDirectReturnLikeContext(invocationToAwait)
                : IsTaskLikeReturnType(method.ReturnType) &&
                  !HasByRefParameter(method.ParameterList) &&
                  !HasUnsafeReturnExpression(method, invocationToAwait),
            LocalFunctionStatementSyntax localFunction => localFunction.Modifiers.Any(SyntaxKind.AsyncKeyword)
                ? !IsDirectReturnLikeContext(invocationToAwait)
                : IsTaskLikeReturnType(localFunction.ReturnType) &&
                  !HasByRefParameter(localFunction.ParameterList) &&
                  !HasUnsafeReturnExpression(localFunction, invocationToAwait),
            LambdaExpressionSyntax lambda => lambda.Modifiers.Any(SyntaxKind.AsyncKeyword) &&
                                             !IsDirectReturnLikeContext(invocationToAwait),
            AnonymousMethodExpressionSyntax anonymousMethod => anonymousMethod.Modifiers.Any(SyntaxKind.AsyncKeyword) &&
                                                               !IsDirectReturnLikeContext(invocationToAwait),
            _ => false
        };

    private static bool IsDirectReturnLikeContext(InvocationExpressionSyntax invocation)
    {
        SyntaxNode current = invocation;
        while (current.Parent is ParenthesizedExpressionSyntax parenthesized && parenthesized.Expression == current)
        {
            current = parenthesized;
        }

        return current.Parent switch
        {
            ReturnStatementSyntax returnStatement when returnStatement.Expression == current => true,
            ArrowExpressionClauseSyntax arrowExpression when arrowExpression.Expression == current => true,
            _ => false
        };
    }

    private static bool IsTaskLikeReturnType(TypeSyntax returnType)
    {
        var returnTypeName = returnType switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            GenericNameSyntax generic => generic.Identifier.Text,
            QualifiedNameSyntax qualified => qualified.Right.Identifier.Text,
            AliasQualifiedNameSyntax aliasQualified => aliasQualified.Name.Identifier.Text,
            _ => null
        };

        return returnTypeName is "Task" or "ValueTask";
    }

    private static bool HasByRefParameter(ParameterListSyntax parameterList) =>
        parameterList.Parameters.Any(parameter =>
            parameter.Modifiers.Any(SyntaxKind.RefKeyword) ||
            parameter.Modifiers.Any(SyntaxKind.OutKeyword) ||
            parameter.Modifiers.Any(SyntaxKind.InKeyword));

    private static bool HasUnsafeReturnExpression(SyntaxNode enclosingFunction, InvocationExpressionSyntax invocationToAwait)
    {
        return enclosingFunction.DescendantNodes(ShouldDescendInto)
            .OfType<ReturnStatementSyntax>()
            .Any(returnStatement =>
                returnStatement.Expression != null && !IsDirectReturnExpression(returnStatement.Expression, invocationToAwait));

        static bool IsDirectReturnExpression(ExpressionSyntax expression, InvocationExpressionSyntax invocation)
        {
            while (expression is ParenthesizedExpressionSyntax parenthesized)
            {
                expression = parenthesized.Expression;
            }

            return expression.Span == invocation.Span;
        }

        bool ShouldDescendInto(SyntaxNode node) =>
            node == enclosingFunction ||
            node is not MethodDeclarationSyntax and
                not LocalFunctionStatementSyntax and
                not LambdaExpressionSyntax and
                not AnonymousMethodExpressionSyntax;
    }

    private static SyntaxNode AddAsyncModifierIfNeeded(SyntaxNode enclosingFunction)
    {
        var asyncToken = SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
            .WithTrailingTrivia(SyntaxFactory.Space);

        return enclosingFunction switch
        {
            MethodDeclarationSyntax method when !method.Modifiers.Any(SyntaxKind.AsyncKeyword) => method.AddModifiers(asyncToken),
            LocalFunctionStatementSyntax localFunction when !localFunction.Modifiers.Any(SyntaxKind.AsyncKeyword) => localFunction.AddModifiers(asyncToken),
            _ => enclosingFunction
        };
    }
}