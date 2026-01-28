namespace FunctionalDdd;

using System.Linq.Expressions;

/// <summary>
/// A specification that adds ordering to another specification.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
internal sealed class OrderedSpecification<T> : ISpecification<T> where T : class
{
    private readonly ISpecification<T> _inner;
    private readonly Expression<Func<T, object>> _orderExpression;
    private readonly bool _ascending;

    public OrderedSpecification(ISpecification<T> inner, Expression<Func<T, object>> orderExpression, bool ascending)
    {
        _inner = inner;
        _orderExpression = orderExpression;
        _ascending = ascending;
    }

    public Expression<Func<T, bool>>? Criteria => _inner.Criteria;

    public IReadOnlyList<Expression<Func<T, object>>> Includes => _inner.Includes;

    public IReadOnlyList<string> IncludeStrings => _inner.IncludeStrings;

    public IReadOnlyList<Expression<Func<T, object>>> OrderBy =>
        _ascending ? [.. _inner.OrderBy, _orderExpression] : _inner.OrderBy;

    public IReadOnlyList<Expression<Func<T, object>>> OrderByDescending =>
        !_ascending ? [.. _inner.OrderByDescending, _orderExpression] : _inner.OrderByDescending;

    public int? Skip => _inner.Skip;

    public int? Take => _inner.Take;

    public bool AsNoTracking => _inner.AsNoTracking;

    public bool AsSplitQuery => _inner.AsSplitQuery;
}
