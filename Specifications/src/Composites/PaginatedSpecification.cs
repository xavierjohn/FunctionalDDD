namespace FunctionalDdd;

using System.Linq.Expressions;

/// <summary>
/// A specification that adds pagination to another specification.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
internal sealed class PaginatedSpecification<T> : ISpecification<T> where T : class
{
    private readonly ISpecification<T> _inner;

    public PaginatedSpecification(ISpecification<T> inner, int skip, int take)
    {
        _inner = inner;
        Skip = skip;
        Take = take;
    }

    public Expression<Func<T, bool>>? Criteria => _inner.Criteria;

    public IReadOnlyList<Expression<Func<T, object>>> Includes => _inner.Includes;

    public IReadOnlyList<string> IncludeStrings => _inner.IncludeStrings;

    public IReadOnlyList<Expression<Func<T, object>>> OrderBy => _inner.OrderBy;

    public IReadOnlyList<Expression<Func<T, object>>> OrderByDescending => _inner.OrderByDescending;

    public int? Skip { get; }

    public int? Take { get; }

    public bool AsNoTracking => _inner.AsNoTracking;

    public bool AsSplitQuery => _inner.AsSplitQuery;
}
