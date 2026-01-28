namespace FunctionalDdd;

using System.Linq.Expressions;

/// <summary>
/// Extension methods for combining expressions.
/// </summary>
internal static class ExpressionExtensions
{
    /// <summary>
    /// Combines two expressions with AND logic.
    /// </summary>
    /// <typeparam name="T">The parameter type.</typeparam>
    /// <param name="left">The left expression.</param>
    /// <param name="right">The right expression.</param>
    /// <returns>A combined expression with AND logic.</returns>
    public static Expression<Func<T, bool>>? AndAlso<T>(
        this Expression<Func<T, bool>>? left,
        Expression<Func<T, bool>>? right)
    {
        if (left is null)
            return right;
        if (right is null)
            return left;

        var parameter = Expression.Parameter(typeof(T));
        var combined = Expression.AndAlso(
            Expression.Invoke(left, parameter),
            Expression.Invoke(right, parameter));

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    /// <summary>
    /// Combines two expressions with OR logic.
    /// </summary>
    /// <typeparam name="T">The parameter type.</typeparam>
    /// <param name="left">The left expression.</param>
    /// <param name="right">The right expression.</param>
    /// <returns>A combined expression with OR logic.</returns>
    public static Expression<Func<T, bool>>? OrElse<T>(
        this Expression<Func<T, bool>>? left,
        Expression<Func<T, bool>>? right)
    {
        if (left is null)
            return right;
        if (right is null)
            return left;

        var parameter = Expression.Parameter(typeof(T));
        var combined = Expression.OrElse(
            Expression.Invoke(left, parameter),
            Expression.Invoke(right, parameter));

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    /// <summary>
    /// Negates an expression.
    /// </summary>
    /// <typeparam name="T">The parameter type.</typeparam>
    /// <param name="expression">The expression to negate.</param>
    /// <returns>The negated expression.</returns>
    public static Expression<Func<T, bool>>? Negate<T>(
        this Expression<Func<T, bool>>? expression)
    {
        if (expression is null)
            return null;

        var parameter = Expression.Parameter(typeof(T));
        var negated = Expression.Not(Expression.Invoke(expression, parameter));

        return Expression.Lambda<Func<T, bool>>(negated, parameter);
    }
}
