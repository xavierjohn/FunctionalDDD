namespace FunctionalDdd.Testing;

/// <summary>
/// Extension methods for testing specifications without a database.
/// </summary>
public static class SpecificationTestExtensions
{
    /// <summary>
    /// Tests if an entity matches the specification criteria.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to test.</param>
    /// <param name="entity">The entity to test against.</param>
    /// <returns>True if the entity satisfies the specification; otherwise false.</returns>
    /// <example>
    /// <code>
    /// var spec = new ActiveOrdersSpec();
    /// var order = Order.Create(...);
    /// 
    /// spec.IsSatisfiedBy(order).Should().BeTrue();
    /// </code>
    /// </example>
    public static bool IsSatisfiedBy<T>(
        this ISpecification<T> spec,
        T entity) where T : class =>
        spec.Criteria?.Compile().Invoke(entity) ?? true;

    /// <summary>
    /// Tests if all entities in a collection match the specification criteria.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to test.</param>
    /// <param name="entities">The entities to test against.</param>
    /// <returns>True if all entities satisfy the specification; otherwise false.</returns>
    public static bool IsSatisfiedByAll<T>(
        this ISpecification<T> spec,
        IEnumerable<T> entities) where T : class
    {
        var compiled = spec.Criteria?.Compile();
        if (compiled is null)
            return true;

        return entities.All(compiled);
    }

    /// <summary>
    /// Tests if any entity in a collection matches the specification criteria.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to test.</param>
    /// <param name="entities">The entities to test against.</param>
    /// <returns>True if any entity satisfies the specification; otherwise false.</returns>
    public static bool IsSatisfiedByAny<T>(
        this ISpecification<T> spec,
        IEnumerable<T> entities) where T : class
    {
        var compiled = spec.Criteria?.Compile();
        if (compiled is null)
            return true;

        return entities.Any(compiled);
    }

    /// <summary>
    /// Filters a collection of entities by the specification criteria.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to apply.</param>
    /// <param name="entities">The entities to filter.</param>
    /// <returns>The entities that satisfy the specification.</returns>
    public static IEnumerable<T> Filter<T>(
        this ISpecification<T> spec,
        IEnumerable<T> entities) where T : class
    {
        var compiled = spec.Criteria?.Compile();
        if (compiled is null)
            return entities;

        return entities.Where(compiled);
    }

    /// <summary>
    /// Counts entities in a collection that match the specification criteria.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to apply.</param>
    /// <param name="entities">The entities to count.</param>
    /// <returns>The count of entities that satisfy the specification.</returns>
    public static int Count<T>(
        this ISpecification<T> spec,
        IEnumerable<T> entities) where T : class
    {
        var compiled = spec.Criteria?.Compile();
        if (compiled is null)
            return entities.Count();

        return entities.Count(compiled);
    }
}
