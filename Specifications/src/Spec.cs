namespace FunctionalDdd;

using System.Linq.Expressions;

/// <summary>
/// Creates a specification from an inline expression without subclassing.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <example>
/// <code>
/// var spec = Spec.For&lt;Order&gt;(o => o.Total > 100);
/// </code>
/// </example>
#pragma warning disable CA1000 // Do not declare static members on generic types - intentional API design
public sealed class Spec<T> : Specification<T> where T : class
{
    private Spec() { }

    private Spec(Expression<Func<T, bool>> criteria) : base(criteria) { }

    /// <summary>
    /// Creates a specification that matches all entities.
    /// </summary>
    public static Spec<T> All => new();

    /// <summary>
    /// Creates a specification with the given criteria.
    /// </summary>
    /// <param name="criteria">The filter expression.</param>
    /// <returns>A new specification with the given criteria.</returns>
    public static Spec<T> Where(Expression<Func<T, bool>> criteria) => new(criteria);
}
#pragma warning restore CA1000

/// <summary>
/// Static factory for creating inline specifications.
/// </summary>
public static class Spec
{
    /// <summary>
    /// Creates a specification for the given entity type with criteria.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="criteria">The filter expression.</param>
    /// <returns>A new specification with the given criteria.</returns>
    public static Spec<T> For<T>(Expression<Func<T, bool>> criteria) where T : class
        => Spec<T>.Where(criteria);

    /// <summary>
    /// Creates a specification that matches all entities of the given type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <returns>A specification that matches all entities.</returns>
    public static Spec<T> All<T>() where T : class
        => Spec<T>.All;
}
