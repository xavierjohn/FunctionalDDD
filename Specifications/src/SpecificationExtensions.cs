namespace FunctionalDdd;

using System.Linq.Expressions;

/// <summary>
/// Extension methods for composing specifications.
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Combines two specifications with AND logic.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new specification combining both with AND logic.</returns>
    public static ISpecification<T> And<T>(
        this ISpecification<T> left,
        ISpecification<T> right) where T : class =>
        new CompositeSpecification<T>(
            left.Criteria.AndAlso(right.Criteria),
            left,
            right);

    /// <summary>
    /// Combines two specifications with OR logic.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new specification combining both with OR logic.</returns>
    public static ISpecification<T> Or<T>(
        this ISpecification<T> left,
        ISpecification<T> right) where T : class =>
        new CompositeSpecification<T>(
            left.Criteria.OrElse(right.Criteria),
            left,
            right);

    /// <summary>
    /// Negates a specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to negate.</param>
    /// <returns>A new specification with negated criteria.</returns>
    public static ISpecification<T> Not<T>(
        this ISpecification<T> spec) where T : class =>
        new NegatedSpecification<T>(spec);

    /// <summary>
    /// Adds a navigation property to eagerly load.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to extend.</param>
    /// <param name="include">The navigation property expression.</param>
    /// <returns>A new specification with the include added.</returns>
    public static ISpecification<T> Include<T>(
        this ISpecification<T> spec,
        Expression<Func<T, object>> include) where T : class =>
        new IncludeSpecification<T>(spec, include);

    /// <summary>
    /// Adds a string-based navigation property path to eagerly load.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to extend.</param>
    /// <param name="includeString">The navigation property path (e.g., "Lines.Product").</param>
    /// <returns>A new specification with the include added.</returns>
    public static ISpecification<T> Include<T>(
        this ISpecification<T> spec,
        string includeString) where T : class =>
        new IncludeSpecification<T>(spec, includeString);

    /// <summary>
    /// Adds ascending ordering to a specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to extend.</param>
    /// <param name="orderBy">The property expression to order by.</param>
    /// <returns>A new specification with ordering added.</returns>
    public static ISpecification<T> OrderBy<T>(
        this ISpecification<T> spec,
        Expression<Func<T, object>> orderBy) where T : class =>
        new OrderedSpecification<T>(spec, orderBy, ascending: true);

    /// <summary>
    /// Adds descending ordering to a specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to extend.</param>
    /// <param name="orderBy">The property expression to order by descending.</param>
    /// <returns>A new specification with ordering added.</returns>
    public static ISpecification<T> OrderByDescending<T>(
        this ISpecification<T> spec,
        Expression<Func<T, object>> orderBy) where T : class =>
        new OrderedSpecification<T>(spec, orderBy, ascending: false);

    /// <summary>
    /// Adds pagination to a specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to extend.</param>
    /// <param name="pageNumber">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A new specification with pagination added.</returns>
    public static ISpecification<T> Paginate<T>(
        this ISpecification<T> spec,
        int pageNumber,
        int pageSize) where T : class =>
        new PaginatedSpecification<T>(spec, (pageNumber - 1) * pageSize, pageSize);

    /// <summary>
    /// Disables change tracking for read-only queries.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to extend.</param>
    /// <returns>A new specification with change tracking disabled.</returns>
    public static ISpecification<T> AsNoTracking<T>(
        this ISpecification<T> spec) where T : class =>
        new NoTrackingSpecification<T>(spec);

    /// <summary>
    /// Enables split queries to avoid cartesian explosion with multiple includes.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to extend.</param>
    /// <returns>A new specification with split queries enabled.</returns>
    public static ISpecification<T> AsSplitQuery<T>(
        this ISpecification<T> spec) where T : class =>
        new SplitQuerySpecification<T>(spec);
}
