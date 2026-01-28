namespace FunctionalDdd;

using System.Linq.Expressions;

/// <summary>
/// A specification that negates another specification.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
internal sealed class NegatedSpecification<T> : ISpecification<T> where T : class
{
    private readonly ISpecification<T> _inner;

    public NegatedSpecification(ISpecification<T> inner)
    {
        _inner = inner;
        Criteria = inner.Criteria.Negate();
    }

    public Expression<Func<T, bool>>? Criteria { get; }

    public IReadOnlyList<Expression<Func<T, object>>> Includes => _inner.Includes;

    public IReadOnlyList<string> IncludeStrings => _inner.IncludeStrings;

    public IReadOnlyList<Expression<Func<T, object>>> OrderBy => _inner.OrderBy;

    public IReadOnlyList<Expression<Func<T, object>>> OrderByDescending => _inner.OrderByDescending;

    public int? Skip => _inner.Skip;

    public int? Take => _inner.Take;

    public bool AsNoTracking => _inner.AsNoTracking;

    public bool AsSplitQuery => _inner.AsSplitQuery;
}
