namespace FunctionalDdd;

using System.Linq.Expressions;

/// <summary>
/// A specification that combines two specifications with AND or OR logic.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
internal sealed class CompositeSpecification<T> : ISpecification<T> where T : class
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;

    public CompositeSpecification(
        Expression<Func<T, bool>>? criteria,
        ISpecification<T> left,
        ISpecification<T> right)
    {
        Criteria = criteria;
        _left = left;
        _right = right;
    }

    public Expression<Func<T, bool>>? Criteria { get; }

    public IReadOnlyList<Expression<Func<T, object>>> Includes =>
        [.. _left.Includes, .. _right.Includes];

    public IReadOnlyList<string> IncludeStrings =>
        [.. _left.IncludeStrings, .. _right.IncludeStrings];

    public IReadOnlyList<Expression<Func<T, object>>> OrderBy =>
        _left.OrderBy.Count > 0 ? _left.OrderBy : _right.OrderBy;

    public IReadOnlyList<Expression<Func<T, object>>> OrderByDescending =>
        _left.OrderByDescending.Count > 0 ? _left.OrderByDescending : _right.OrderByDescending;

    public int? Skip => _left.Skip ?? _right.Skip;

    public int? Take => _left.Take ?? _right.Take;

    public bool AsNoTracking => _left.AsNoTracking || _right.AsNoTracking;

    public bool AsSplitQuery => _left.AsSplitQuery || _right.AsSplitQuery;
}
