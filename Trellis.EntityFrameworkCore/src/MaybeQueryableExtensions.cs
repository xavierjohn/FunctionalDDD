namespace Trellis.EntityFrameworkCore;

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// LINQ query extensions for <see cref="Maybe{T}"/> properties backed by private nullable fields.
/// These methods rewrite the expression tree to target the underlying <c>_camelCase</c> backing field
/// via <see cref="EF.Property{TProperty}"/>, enabling EF Core to translate the query to SQL.
/// </summary>
/// <remarks>
/// <para>
/// Because <see cref="MaybePropertyExtensions.MaybeProperty{TEntity,TInner}"/> ignores the
/// <see cref="Maybe{T}"/> CLR property, EF Core cannot translate direct LINQ references to it.
/// These extension methods provide a strongly-typed alternative to raw <c>EF.Property</c> calls.
/// </para>
/// <code>
/// // Instead of:
/// context.Customers.Where(c => EF.Property&lt;PhoneNumber?&gt;(c, "_phone") == null)
///
/// // Use:
/// context.Customers.WhereNone(c => c.Phone)
/// </code>
/// </remarks>
public static class MaybeQueryableExtensions
{
    /// <summary>
    /// Filters the query to entities where the <see cref="Maybe{T}"/> property has no value
    /// (backing field IS NULL).
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TInner">The type wrapped in <see cref="Maybe{T}"/>.</typeparam>
    /// <param name="source">The queryable source.</param>
    /// <param name="propertySelector">
    /// An expression selecting the <see cref="Maybe{T}"/> property (e.g., <c>c =&gt; c.Phone</c>).
    /// </param>
    /// <returns>A filtered queryable where the backing field is NULL.</returns>
    /// <example>
    /// <code>
    /// var customersWithoutPhone = await context.Customers
    ///     .WhereNone(c => c.Phone)
    ///     .ToListAsync(ct);
    /// </code>
    /// </example>
    public static IQueryable<TEntity> WhereNone<TEntity, TInner>(
        this IQueryable<TEntity> source,
        Expression<Func<TEntity, Maybe<TInner>>> propertySelector)
        where TEntity : class
        where TInner : notnull
    {
        var predicate = BuildNullCheck<TEntity, TInner>(propertySelector, isNullCheck: true);
        return source.Where(predicate);
    }

    /// <summary>
    /// Filters the query to entities where the <see cref="Maybe{T}"/> property has a value
    /// (backing field IS NOT NULL).
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TInner">The type wrapped in <see cref="Maybe{T}"/>.</typeparam>
    /// <param name="source">The queryable source.</param>
    /// <param name="propertySelector">
    /// An expression selecting the <see cref="Maybe{T}"/> property (e.g., <c>c =&gt; c.Phone</c>).
    /// </param>
    /// <returns>A filtered queryable where the backing field is NOT NULL.</returns>
    /// <example>
    /// <code>
    /// var customersWithPhone = await context.Customers
    ///     .WhereHasValue(c => c.Phone)
    ///     .ToListAsync(ct);
    /// </code>
    /// </example>
    public static IQueryable<TEntity> WhereHasValue<TEntity, TInner>(
        this IQueryable<TEntity> source,
        Expression<Func<TEntity, Maybe<TInner>>> propertySelector)
        where TEntity : class
        where TInner : notnull
    {
        var predicate = BuildNullCheck<TEntity, TInner>(propertySelector, isNullCheck: false);
        return source.Where(predicate);
    }

    /// <summary>
    /// Filters the query to entities where the <see cref="Maybe{T}"/> property equals the
    /// specified value (backing field = <paramref name="value"/>).
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TInner">The type wrapped in <see cref="Maybe{T}"/>.</typeparam>
    /// <param name="source">The queryable source.</param>
    /// <param name="propertySelector">
    /// An expression selecting the <see cref="Maybe{T}"/> property (e.g., <c>c =&gt; c.Phone</c>).
    /// </param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>A filtered queryable where the backing field equals the value.</returns>
    /// <example>
    /// <code>
    /// var phone = PhoneNumber.Create("+15550100");
    /// var matches = await context.Customers
    ///     .WhereEquals(c => c.Phone, phone)
    ///     .ToListAsync(ct);
    /// </code>
    /// </example>
    public static IQueryable<TEntity> WhereEquals<TEntity, TInner>(
        this IQueryable<TEntity> source,
        Expression<Func<TEntity, Maybe<TInner>>> propertySelector,
        TInner value)
        where TEntity : class
        where TInner : notnull
    {
        var (backingFieldName, nullableType) = ResolveBackingField<TInner>(propertySelector);
        var parameter = propertySelector.Parameters[0];
        var efProperty = BuildEfPropertyAccess(parameter, backingFieldName, nullableType);

        Expression valueExpr = typeof(TInner).IsValueType
            ? Expression.Convert(Expression.Constant(value), nullableType)
            : Expression.Constant(value, nullableType);

        var equals = Expression.Equal(efProperty, valueExpr);
        var lambda = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);

        return source.Where(lambda);
    }

    private static Expression<Func<TEntity, bool>> BuildNullCheck<TEntity, TInner>(
        Expression<Func<TEntity, Maybe<TInner>>> propertySelector,
        bool isNullCheck)
        where TEntity : class
        where TInner : notnull
    {
        var (backingFieldName, nullableType) = ResolveBackingField<TInner>(propertySelector);
        var parameter = propertySelector.Parameters[0];
        var efProperty = BuildEfPropertyAccess(parameter, backingFieldName, nullableType);

        var nullConstant = Expression.Constant(null, nullableType);
        var comparison = isNullCheck
            ? Expression.Equal(efProperty, nullConstant)
            : Expression.NotEqual(efProperty, nullConstant);

        return Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
    }

    private static (string BackingFieldName, Type NullableType) ResolveBackingField<TInner>(
        LambdaExpression propertySelector) where TInner : notnull
    {
        if (propertySelector.Body is not MemberExpression { Member: PropertyInfo property })
            throw new ArgumentException(
                "Expression must be a simple property access (e.g., c => c.Phone).",
                nameof(propertySelector));

        var backingFieldName = $"_{char.ToLowerInvariant(property.Name[0])}{property.Name[1..]}";
        var innerType = typeof(TInner);
        var nullableType = innerType.IsValueType
            ? typeof(Nullable<>).MakeGenericType(innerType)
            : innerType;

        return (backingFieldName, nullableType);
    }

    private static readonly MethodInfo EfPropertyMethodInfo =
        typeof(EF).GetMethod(nameof(EF.Property))!;

    private static MethodCallExpression BuildEfPropertyAccess(
        ParameterExpression parameter, string backingFieldName, Type nullableType)
    {
        var genericMethod = EfPropertyMethodInfo.MakeGenericMethod(nullableType);

        return Expression.Call(
            genericMethod,
            Expression.Convert(parameter, typeof(object)),
            Expression.Constant(backingFieldName));
    }
}
