namespace FunctionalDdd;

using System.Linq.Expressions;

/// <summary>
/// Base class for creating type-safe, composable query specifications.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <example>
/// <code>
/// public sealed class ActiveOrdersSpec : Specification&lt;Order&gt;
/// {
///     public ActiveOrdersSpec() 
///         : base(o => o.Status == OrderStatus.Active) { }
/// }
/// </code>
/// </example>
public abstract class Specification<T> : ISpecification<T> where T : class
{
    private readonly List<Expression<Func<T, object>>> _includes = [];
    private readonly List<string> _includeStrings = [];
    private readonly List<Expression<Func<T, object>>> _orderBy = [];
    private readonly List<Expression<Func<T, object>>> _orderByDescending = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Specification{T}"/> class with no criteria.
    /// </summary>
    protected Specification() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Specification{T}"/> class with the specified criteria.
    /// </summary>
    /// <param name="criteria">The filter expression.</param>
    protected Specification(Expression<Func<T, bool>> criteria)
        => Criteria = criteria;

    /// <inheritdoc />
    public Expression<Func<T, bool>>? Criteria { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes;

    /// <inheritdoc />
    public IReadOnlyList<string> IncludeStrings => _includeStrings;

    /// <inheritdoc />
    public IReadOnlyList<Expression<Func<T, object>>> OrderBy => _orderBy;

    /// <inheritdoc />
    public IReadOnlyList<Expression<Func<T, object>>> OrderByDescending => _orderByDescending;

    /// <inheritdoc />
    public int? Skip { get; private set; }

    /// <inheritdoc />
    public int? Take { get; private set; }

    /// <inheritdoc />
    public bool AsNoTracking { get; private set; }

    /// <inheritdoc />
    public bool AsSplitQuery { get; private set; }

    /// <summary>
    /// Adds a navigation property to eagerly load.
    /// </summary>
    /// <param name="includeExpression">The navigation property expression.</param>
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
        => _includes.Add(includeExpression);

    /// <summary>
    /// Adds a nested navigation property path to eagerly load.
    /// </summary>
    /// <param name="includeString">The navigation property path (e.g., "Lines.Product").</param>
    protected void AddInclude(string includeString)
        => _includeStrings.Add(includeString);

    /// <summary>
    /// Adds ascending ordering.
    /// </summary>
    /// <param name="orderByExpression">The property expression to order by.</param>
    protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
        => _orderBy.Add(orderByExpression);

    /// <summary>
    /// Adds descending ordering.
    /// </summary>
    /// <param name="orderByDescExpression">The property expression to order by descending.</param>
    protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        => _orderByDescending.Add(orderByDescExpression);

    /// <summary>
    /// Enables pagination.
    /// </summary>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The maximum number of records to return.</param>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    /// <summary>
    /// Disables EF Core change tracking for read-only queries.
    /// </summary>
    protected void ApplyAsNoTracking()
        => AsNoTracking = true;

    /// <summary>
    /// Splits queries to avoid cartesian explosion with multiple includes.
    /// </summary>
    protected void ApplyAsSplitQuery()
        => AsSplitQuery = true;
}
