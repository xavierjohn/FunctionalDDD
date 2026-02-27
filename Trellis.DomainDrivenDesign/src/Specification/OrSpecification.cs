namespace Trellis;

using System.Linq.Expressions;

/// <summary>
/// Combines two specifications using logical OR.
/// At least one specification must be satisfied for this specification to be satisfied.
/// </summary>
/// <typeparam name="T">The type of entity this specification applies to.</typeparam>
internal sealed class OrSpecification<T>(Specification<T> left, Specification<T> right)
    : Specification<T>
{
    /// <inheritdoc />
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = left.ToExpression();
        var rightExpr = right.ToExpression();
        var param = Expression.Parameter(typeof(T));
        var body = Expression.OrElse(
            Expression.Invoke(leftExpr, param),
            Expression.Invoke(rightExpr, param));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}
