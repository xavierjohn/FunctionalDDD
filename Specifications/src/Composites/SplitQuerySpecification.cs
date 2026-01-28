namespace FunctionalDdd;

using System.Linq.Expressions;

/// <summary>
/// A specification that enables split queries.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
internal sealed class SplitQuerySpecification<T> : ISpecification<T> where T : class
{
    private readonly ISpecification<T> _inner;

    public SplitQuerySpecification(ISpecification<T> inner) => _inner = inner;

    public Expression<Func<T, bool>>? Criteria => _inner.Criteria;

    public IReadOnlyList<Expression<Func<T, object>>> Includes => _inner.Includes;

    public IReadOnlyList<string> IncludeStrings => _inner.IncludeStrings;

    public IReadOnlyList<Expression<Func<T, object>>> OrderBy => _inner.OrderBy;

    public IReadOnlyList<Expression<Func<T, object>>> OrderByDescending => _inner.OrderByDescending;

    public int? Skip => _inner.Skip;

    public int? Take => _inner.Take;

    public bool AsNoTracking => _inner.AsNoTracking;

    public bool AsSplitQuery => true;
}
