namespace FunctionalDdd;

using System.Linq.Expressions;

/// <summary>
/// A specification that adds an include to another specification.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
internal sealed class IncludeSpecification<T> : ISpecification<T> where T : class
{
    private readonly ISpecification<T> _inner;
    private readonly Expression<Func<T, object>>? _include;
    private readonly string? _includeString;

    public IncludeSpecification(ISpecification<T> inner, Expression<Func<T, object>> include)
    {
        _inner = inner;
        _include = include;
    }

    public IncludeSpecification(ISpecification<T> inner, string includeString)
    {
        _inner = inner;
        _includeString = includeString;
    }

    public Expression<Func<T, bool>>? Criteria => _inner.Criteria;

    public IReadOnlyList<Expression<Func<T, object>>> Includes =>
        _include is not null ? [.. _inner.Includes, _include] : _inner.Includes;

    public IReadOnlyList<string> IncludeStrings =>
        _includeString is not null ? [.. _inner.IncludeStrings, _includeString] : _inner.IncludeStrings;

    public IReadOnlyList<Expression<Func<T, object>>> OrderBy => _inner.OrderBy;

    public IReadOnlyList<Expression<Func<T, object>>> OrderByDescending => _inner.OrderByDescending;

    public int? Skip => _inner.Skip;

    public int? Take => _inner.Take;

    public bool AsNoTracking => _inner.AsNoTracking;

    public bool AsSplitQuery => _inner.AsSplitQuery;
}
