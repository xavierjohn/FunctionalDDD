namespace FunctionalDdd;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Applies specifications to IQueryable for EF Core.
/// </summary>
public static class SpecificationEvaluator
{
    /// <summary>
    /// Applies all specification criteria to the queryable.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="inputQuery">The input queryable.</param>
    /// <returns>The queryable with the specification applied.</returns>
    public static IQueryable<T> Apply<T>(
        ISpecification<T> specification,
        IQueryable<T> inputQuery) where T : class
    {
        var query = inputQuery;

        // Apply criteria
        if (specification.Criteria is not null)
            query = query.Where(specification.Criteria);

        // Apply includes
        query = specification.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        query = specification.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy.Count > 0)
        {
            var orderedQuery = query.OrderBy(specification.OrderBy[0]);
            for (var i = 1; i < specification.OrderBy.Count; i++)
            {
                orderedQuery = orderedQuery.ThenBy(specification.OrderBy[i]);
            }

            query = orderedQuery;
        }

        if (specification.OrderByDescending.Count > 0)
        {
            var orderedQuery = query is IOrderedQueryable<T> oq
                ? oq.ThenByDescending(specification.OrderByDescending[0])
                : query.OrderByDescending(specification.OrderByDescending[0]);

            for (var i = 1; i < specification.OrderByDescending.Count; i++)
            {
                orderedQuery = orderedQuery.ThenByDescending(specification.OrderByDescending[i]);
            }

            query = orderedQuery;
        }

        // Apply paging
        if (specification.Skip.HasValue)
            query = query.Skip(specification.Skip.Value);

        if (specification.Take.HasValue)
            query = query.Take(specification.Take.Value);

        // Apply tracking options
        if (specification.AsNoTracking)
            query = query.AsNoTracking();

        // Note: AsSplitQuery is available in Microsoft.EntityFrameworkCore.Relational
        // Consumers using relational databases should apply it in their repository:
        // if (specification.AsSplitQuery) query = query.AsSplitQuery();

        return query;
    }
}
