namespace Trellis.Analyzers;

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// TRLS023: Warns when <c>HttpResponseOptionsBuilder&lt;T&gt;.CreatedAtRoute(...)</c> is invoked
/// on a versioned controller (one with <c>[ApiVersion(...)]</c>) without an <c>"api-version"</c>
/// entry in the supplied route-values dictionary. The resulting <c>Location</c> header omits
/// the version under query/header API versioning and 404s on dereference. The fix migrates the
/// call to <c>CreatedAtVersionedRoute(...)</c> from <c>Trellis.Asp.ApiVersioning</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CreatedAtRouteMissingApiVersionAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.MissingApiVersionRouteValue);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var inv = (InvocationExpressionSyntax)context.Node;

        // Quick syntactic filter: must look like `something.CreatedAtRoute(routeName, routeValues)`.
        if (inv.Expression is not MemberAccessExpressionSyntax mae) return;
        if (mae.Name.Identifier.Text != "CreatedAtRoute") return;
        if (inv.ArgumentList.Arguments.Count != 2) return;

        // Resolve the symbol to confirm it's HttpResponseOptionsBuilder<T>.CreatedAtRoute on Trellis.Asp.
        var methodSymbol = context.SemanticModel.GetSymbolInfo(inv).Symbol as IMethodSymbol;
        if (methodSymbol is null) return;
        var containingType = methodSymbol.ContainingType;
        if (containingType is null) return;
        if (containingType.Name != "HttpResponseOptionsBuilder") return;
        if (containingType.ContainingNamespace?.ToDisplayString() != "Trellis.Asp") return;

        // Only fire inside a class that's api-versioned (has [ApiVersion]) and not [ApiVersionNeutral].
        var classDecl = inv.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (classDecl is null) return;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
        if (classSymbol is null) return;
        if (!HasAttribute(classSymbol, "ApiVersionAttribute", "Asp.Versioning")) return;
        if (HasAttribute(classSymbol, "ApiVersionNeutralAttribute", "Asp.Versioning")) return;

        // Only fire when the route-values argument is a recognizable dictionary literal.
        // Bail to false-negative (no warning) for non-literal forms like
        // `c => myComputedDictionary` — we can't tell what's inside.
        var routeValuesArg = inv.ArgumentList.Arguments[1];
        var literal = ExtractRouteValueDictionaryInitializer(routeValuesArg);
        if (literal is null) return;

        if (RouteValueDictionaryContainsApiVersionKey(literal))
            return;

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.MissingApiVersionRouteValue,
            inv.GetLocation()));
    }

    private static bool HasAttribute(INamedTypeSymbol type, string attributeTypeName, string attributeNamespace)
    {
        // [ApiVersion] / [ApiVersionNeutral] are declared with Inherited = false, so look only at
        // the immediate type. Walking the base-type chain would treat derived controllers without
        // their own [ApiVersion] as versioned, which would produce a false positive that API
        // Versioning itself doesn't accept.
        foreach (var attr in type.GetAttributes())
        {
            var ac = attr.AttributeClass;
            if (ac?.Name == attributeTypeName &&
                ac.ContainingNamespace?.ToDisplayString() == attributeNamespace)
            {
                return true;
            }
        }

        return false;
    }

    private static InitializerExpressionSyntax? ExtractRouteValueDictionaryInitializer(ArgumentSyntax arg)
    {
        // Lambda body shapes we recognize:
        //   c => new RouteValueDictionary { ... }
        //   c => new RouteValueDictionary(new { ... })
        //   c => { return new RouteValueDictionary { ... }; }
        if (arg.Expression is not LambdaExpressionSyntax lambda) return null;

        ExpressionSyntax? bodyExpr = lambda.Body switch
        {
            ExpressionSyntax e => e,
            BlockSyntax block => block.Statements
                .OfType<ReturnStatementSyntax>()
                .Select(r => r.Expression!)
                .FirstOrDefault(),
            _ => null,
        };

        if (bodyExpr is BaseObjectCreationExpressionSyntax oce && oce.Initializer is { } init)
            return init;

        return null;
    }

    private static bool RouteValueDictionaryContainsApiVersionKey(InitializerExpressionSyntax init)
    {
        // RouteValueDictionary uses case-insensitive key comparison at runtime. Match the same
        // semantics here so that {"API-VERSION"], ["Api-Version"], etc. are accepted as equivalent
        // to "api-version" — otherwise the analyzer fires on a literal that's already correct.
        foreach (var expr in init.Expressions)
        {
            // `["api-version"] = x` — collection-initializer assignment with bracket key.
            if (expr is AssignmentExpressionSyntax ae &&
                ae.Left is ImplicitElementAccessSyntax iea)
            {
                var key = TryGetStringLiteralKey(iea.ArgumentList.Arguments);
                if (key is not null && string.Equals(key, "api-version", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            // `{ "api-version", x }` — pre-C#6 collection-initializer-style dictionary entry.
            else if (expr is InitializerExpressionSyntax pair &&
                     pair.Expressions.FirstOrDefault() is LiteralExpressionSyntax litKey &&
                     string.Equals(litKey.Token.ValueText, "api-version", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string? TryGetStringLiteralKey(SeparatedSyntaxList<ArgumentSyntax> args)
    {
        if (args.Count == 0) return null;
        if (args[0].Expression is LiteralExpressionSyntax lit && lit.Token.IsKind(SyntaxKind.StringLiteralToken))
            return lit.Token.ValueText;
        return null;
    }
}
