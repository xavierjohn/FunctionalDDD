namespace Trellis.EntityFrameworkCore;

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// An <see cref="ExpressionVisitor"/> that rewrites <see cref="Maybe{T}"/> property accesses
/// in LINQ expression trees to use the underlying EF Core storage member via <see cref="EF.Property{TProperty}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Because <see cref="MaybeConvention"/> ignores <see cref="Maybe{T}"/> CLR properties and maps
/// only the generated <c>_camelCase</c> backing field, EF Core cannot translate direct LINQ
/// references to <see cref="Maybe{T}"/> properties. This visitor transparently rewrites such
/// references so that specifications, inline LINQ, and generic repository patterns work without
/// requiring explicit <c>WhereHasValue</c> / <c>WhereLessThan</c> extension methods.
/// </para>
/// <para>Supported patterns:</para>
/// <list type="table">
/// <listheader><term>Source expression</term><description>Rewritten to</description></listheader>
/// <item><term><c>entity.Phone</c></term><description><c>EF.Property&lt;T?&gt;(entity, "_phone")</c></description></item>
/// <item><term><c>entity.Phone.HasValue</c></term><description><c>EF.Property&lt;T?&gt;(entity, "_phone") != null</c></description></item>
/// <item><term><c>entity.Phone.HasNoValue</c></term><description><c>EF.Property&lt;T?&gt;(entity, "_phone") == null</c></description></item>
/// <item><term><c>entity.Phone.Value</c></term><description><c>EF.Property&lt;T?&gt;(entity, "_phone")</c> (strip accessor)</description></item>
/// <item><term><c>entity.Phone.GetValueOrDefault(d)</c></term><description><c>EF.Property&lt;T?&gt;(entity, "_phone") ?? d</c></description></item>
/// </list>
/// <para>
/// <b>Limitation — comparing <c>Maybe&lt;T&gt;</c> to a captured Maybe value.</b> EF Core extracts
/// closed-expression operands (including <c>Maybe&lt;T&gt;.None</c> and <c>Maybe.From(value)</c>)
/// to <c>QueryParameterExpression</c>s during expression-tree funcletization, which runs
/// **before** <c>IQueryExpressionInterceptor.QueryCompilationStarting</c>. By the time this
/// rewriter sees the operand the syntactic distinction is lost. <see cref="VisitBinary"/>
/// conservatively converts any unrecognized <see cref="Maybe{T}"/>-typed operand to a typed
/// null constant so that <c>c.Phone == Maybe&lt;T&gt;.None</c> remains valid. As a consequence,
/// <c>c.Phone == Maybe.From(value)</c> silently miss-queries to <c>_phone IS NULL</c>. Use
/// <c>MaybeQueryableExtensions.WhereEquals(c => c.Phone, value)</c> for value comparisons.
/// A future fix could intercept earlier via <c>IEvaluatableExpressionFilterPlugin</c> to
/// prevent funcletization of <c>Maybe</c>-bearing nodes; this is tracked as a follow-up.
/// </para>
/// <para>
/// <b>Limitation — bare <see cref="Maybe{T}"/> projection.</b> Rewriting bare <c>entity.Phone</c>
/// to <c>EF.Property&lt;T?&gt;(entity, "_phone")</c> changes the lambda return type from
/// <see cref="Maybe{T}"/> to <c>T?</c>. Inside <c>.Select(c => c.Phone)</c> projections, the
/// return-type mismatch causes EF Core to throw a translation error. Project
/// <c>c.Phone.GetValueOrDefault(default)</c> instead, or materialize the entity and read
/// <c>Phone</c> client-side.
/// </para>
/// </remarks>
internal sealed class MaybeExpressionRewriter : ExpressionVisitor
{
    private static readonly Type s_maybeOpenGeneric = typeof(Maybe<>);

    private static readonly MethodInfo s_efPropertyMethod =
        typeof(EF).GetMethod(nameof(EF.Property))!;

    /// <summary>
    /// Rewrites an expression tree, replacing all <see cref="Maybe{T}"/> property accesses
    /// with their EF Core storage member equivalents.
    /// </summary>
    public static Expression Rewrite(Expression expression) =>
        new MaybeExpressionRewriter().Visit(expression);

    /// <summary>
    /// Intercepts member access expressions to detect <see cref="Maybe{T}"/> patterns:
    /// <c>entity.Phone</c>, <c>entity.Phone.HasValue</c>, <c>entity.Phone.HasNoValue</c>, <c>entity.Phone.Value</c>.
    /// </summary>
    protected override Expression VisitMember(MemberExpression node)
    {
        // Pattern: entity.MaybeProperty.HasValue → EF.Property != null
        // Pattern: entity.MaybeProperty.HasNoValue → EF.Property == null
        // Pattern: entity.MaybeProperty.Value → EF.Property (strip accessor)
        if (node.Expression is MemberExpression innerMember
            && innerMember.Member is PropertyInfo innerProp
            && IsMaybeType(innerProp.PropertyType))
        {
            var efPropertyAccess = BuildEfPropertyAccess(innerMember);
            if (efPropertyAccess is null)
                return base.VisitMember(node);

            return node.Member.Name switch
            {
                nameof(Maybe<object>.HasValue) => Expression.NotEqual(
                    efPropertyAccess,
                    Expression.Constant(null, efPropertyAccess.Type)),

                nameof(Maybe<object>.HasNoValue) => Expression.Equal(
                    efPropertyAccess,
                    Expression.Constant(null, efPropertyAccess.Type)),

                "Value" => efPropertyAccess,

                _ => base.VisitMember(node)
            };
        }

        // Pattern: entity.MaybeProperty (bare access — rewrite to EF.Property)
        if (node.Member is PropertyInfo prop && IsMaybeType(prop.PropertyType))
        {
            var efPropertyAccess = BuildEfPropertyAccess(node);
            if (efPropertyAccess is not null)
                return efPropertyAccess;
        }

        return base.VisitMember(node);
    }

    /// <summary>
    /// Intercepts method calls to detect <see cref="Maybe{T}.GetValueOrDefault(T)"/>
    /// and rewrite to <c>EF.Property ?? defaultValue</c>, and <see cref="Maybe{T}.HasValueWhere(Func{T, bool})"/>
    /// and rewrite to <c>EF.Property != null AND predicate-body-with-parameter-replaced</c>.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // Pattern: entity.MaybeProperty.GetValueOrDefault(defaultValue)
        //       → EF.Property<T?>(entity, "_field") ?? defaultValue
        if (node.Method.Name == nameof(Maybe<object>.GetValueOrDefault)
            && node.Object is MemberExpression memberExpr
            && memberExpr.Member is PropertyInfo prop
            && IsMaybeType(prop.PropertyType))
        {
            var efPropertyAccess = BuildEfPropertyAccess(memberExpr);
            if (efPropertyAccess is not null && node.Arguments.Count == 1)
            {
                var defaultValue = Visit(node.Arguments[0]);
                return Expression.Coalesce(efPropertyAccess, defaultValue);
            }
        }

        // Pattern: entity.MaybeProperty.HasValueWhere(t => predicateBody(t))
        //       → EF.Property<T?>(entity, "_field") != null AND predicateBody(replacement)
        //  where replacement is `EF.Property.Value` for value-type T (unwrapping Nullable<T>)
        //  or `EF.Property` cast to T for reference-type T.
        //
        // Only inline LambdaExpression arguments are supported. Captured delegate variables
        // and method-group conversions appear as ConstantExpression / opaque shapes in the
        // outer expression tree — the rewriter cannot inspect their bodies and falls
        // through, letting EF Core's query translator report the failure.
        if (node.Method.Name == nameof(Maybe<object>.HasValueWhere)
            && node.Object is MemberExpression isSomeMember
            && isSomeMember.Member is PropertyInfo isSomeProp
            && IsMaybeType(isSomeProp.PropertyType))
        {
            var efPropertyAccess = BuildEfPropertyAccess(isSomeMember);
            if (efPropertyAccess is not null && node.Arguments.Count == 1)
            {
                // The C# compiler emits inline `Func<T,bool>` lambdas as LambdaExpression
                // nodes when the surrounding outer lambda is an Expression<>. The Quote
                // branch covers cases where the compiler additionally wraps the inner
                // lambda in a UnaryExpression with NodeType=Quote.
                var lambda = node.Arguments[0] switch
                {
                    LambdaExpression lam => lam,
                    UnaryExpression { NodeType: ExpressionType.Quote, Operand: LambdaExpression lam } => lam,
                    _ => null
                };

                if (lambda is { Parameters.Count: 1 })
                {
                    var param = lambda.Parameters[0];
                    var innerType = isSomeProp.PropertyType.GetGenericArguments()[0];

                    // Storage member type is Nullable<T> for value types, T for reference types.
                    // Substitute the inner lambda's parameter with the storage access so the
                    // predicate body operates on the same expression EF will translate.
                    Expression replacement;
                    if (innerType.IsValueType)
                    {
                        // Storage is Nullable<T>; .Value unwraps to T. The leading IS NOT NULL
                        // check, combined with SQL three-valued logic, ensures we never
                        // materialize the .Value on a NULL row.
                        replacement = Expression.Property(efPropertyAccess, "Value");
                    }
                    else
                    {
                        // Storage is T?. The convert is a no-op when the types already match,
                        // skipped to avoid emitting unnecessary nodes that some EF translators
                        // are sensitive to.
                        replacement = efPropertyAccess.Type == innerType
                            ? efPropertyAccess
                            : Expression.Convert(efPropertyAccess, innerType);
                    }

                    var rewrittenBody = new ParameterReplacer(param, replacement).Visit(lambda.Body)!;

                    // Recurse so nested Maybe access inside the predicate body
                    // (e.g., `t => o.OtherMaybe.HasValue`) is also rewritten.
                    rewrittenBody = Visit(rewrittenBody);

                    var notNullCheck = Expression.NotEqual(
                        efPropertyAccess,
                        Expression.Constant(null, efPropertyAccess.Type));

                    return Expression.AndAlso(notNullCheck, rewrittenBody);
                }
            }
        }

        return base.VisitMethodCall(node);
    }

    /// <summary>
    /// Intercepts binary expressions to handle <see cref="Maybe{T}"/> operator overloads
    /// (== and !=) by rewriting the <see cref="Maybe{T}"/> operand.
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression node)
    {
        // For == and != with Maybe<T> custom operators, EF Core won't recognize them.
        // After visiting children (which may rewrite Maybe members), rebuild the binary
        // if the original used a custom operator method on Maybe<T>.
        if (node.Method is not null && node.Method.DeclaringType is { } declaring
            && declaring.IsGenericType && declaring.GetGenericTypeDefinition() == s_maybeOpenGeneric)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            // Rewrite Maybe<T>.None / default(Maybe<T>) operands to typed null
            left = RewriteMaybeDefaultToNull(left, node.Left);
            right = RewriteMaybeDefaultToNull(right, node.Right);

            // Rebuild as a simple comparison without the custom operator method
            return node.NodeType switch
            {
                ExpressionType.Equal => Expression.Equal(left, right),
                ExpressionType.NotEqual => Expression.NotEqual(left, right),
                _ => node.Update(left, node.Conversion, right)
            };
        }

        // For all other binary expressions, visit children first.
        // If visiting changed operand types (e.g., Maybe<T>.Value rewritten from T to T?),
        // rebuild the binary without the original method to let the runtime infer the correct operator.
        var visitedLeft = Visit(node.Left);
        var visitedRight = Visit(node.Right);

        if (visitedLeft.Type != node.Left.Type || visitedRight.Type != node.Right.Type)
        {
            // Align types — if one side became nullable, lift the other to match
            if (visitedLeft.Type != visitedRight.Type)
            {
                var targetType = Nullable.GetUnderlyingType(visitedLeft.Type) is not null ? visitedLeft.Type
                    : Nullable.GetUnderlyingType(visitedRight.Type) is not null ? visitedRight.Type
                    : visitedLeft.Type;

                if (visitedLeft.Type != targetType)
                    visitedLeft = Expression.Convert(visitedLeft, targetType);
                if (visitedRight.Type != targetType)
                    visitedRight = Expression.Convert(visitedRight, targetType);
            }

            return Expression.MakeBinary(node.NodeType, visitedLeft, visitedRight);
        }

        return node.Update(visitedLeft, node.Conversion, visitedRight);
    }

    private static MethodCallExpression? BuildEfPropertyAccess(MemberExpression maybeMemberExpr)
    {
        if (maybeMemberExpr.Member is not PropertyInfo maybeProp)
            return null;

        if (!IsMaybeType(maybeProp.PropertyType))
            return null;

        // The entity expression (e.g., the parameter 'order' in 'order.SubmittedAt')
        var entityExpr = maybeMemberExpr.Expression;
        if (entityExpr is null)
            return null;

        var innerType = maybeProp.PropertyType.GetGenericArguments()[0];
        var storeType = innerType.IsValueType
            ? typeof(Nullable<>).MakeGenericType(innerType)
            : innerType;

        var storageMemberName = MaybeFieldNaming.ToStorageMemberName(maybeProp.Name);

        var genericMethod = s_efPropertyMethod.MakeGenericMethod(storeType);
        return Expression.Call(
            genericMethod,
            Expression.Convert(entityExpr, typeof(object)),
            Expression.Constant(storageMemberName));
    }

    private static bool IsMaybeType(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == s_maybeOpenGeneric;

    /// <summary>
    /// Rewrites <c>Maybe&lt;T&gt;.None</c> or <c>default(Maybe&lt;T&gt;)</c> operands to a typed null
    /// constant so that binary comparisons have compatible operand types.
    /// </summary>
    private static Expression RewriteMaybeDefaultToNull(Expression visited, Expression original)
    {
        // If the visited expression was already rewritten to an EF.Property call, keep it.
        if (visited != original && visited.NodeType == ExpressionType.Call)
            return visited;

        if (!IsMaybeType(original.Type))
            return visited;

        var innerType = original.Type.GetGenericArguments()[0];
        var storeType = innerType.IsValueType
            ? typeof(Nullable<>).MakeGenericType(innerType)
            : innerType;

        return Expression.Constant(null, storeType);
    }

    /// <summary>
    /// Substitutes a single <see cref="ParameterExpression"/> with a replacement expression
    /// throughout an expression tree. Used to inline an <c>HasValueWhere</c> predicate's lambda
    /// parameter with the storage-member access expression so the predicate body operates
    /// on the same expression EF Core will translate.
    /// </summary>
    /// <remarks>
    /// Parameter identity is compared by reference because each <see cref="ParameterExpression"/>
    /// is a distinct instance even when names collide. Nested lambdas inside the body keep
    /// their own distinct parameter instances and are not affected by this visitor.
    /// </remarks>
    private sealed class ParameterReplacer(ParameterExpression target, Expression replacement) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == target ? replacement : node;
    }
}