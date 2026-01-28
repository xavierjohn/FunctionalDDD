namespace FunctionalDdd;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Extension methods for DbSet to work with specifications and Result pattern.
/// </summary>
public static class DbSetExtensions
{
    /// <summary>
    /// Applies the specification and returns all matching entities.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="dbSet">The DbSet to query.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A list of matching entities.</returns>
    public static async Task<List<T>> ToListAsync<T>(
        this DbSet<T> dbSet,
        ISpecification<T> specification,
        CancellationToken ct = default) where T : class =>
        await SpecificationEvaluator
            .Apply(specification, dbSet)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    /// <summary>
    /// Applies the specification and returns the first match or NotFoundError.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="dbSet">The DbSet to query.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="entityName">Optional entity name for error messages.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A Result containing the entity or NotFound error.</returns>
    public static async Task<Result<T>> FirstOrNotFoundAsync<T>(
        this DbSet<T> dbSet,
        ISpecification<T> specification,
        string? entityName = null,
        CancellationToken ct = default) where T : class
    {
        var entity = await SpecificationEvaluator
            .Apply(specification, dbSet)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        return entity is not null
            ? entity
            : Error.NotFound($"{entityName ?? typeof(T).Name} not found");
    }

    /// <summary>
    /// Applies the specification and returns the single match or appropriate error.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="dbSet">The DbSet to query.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="entityName">Optional entity name for error messages.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A Result containing the single entity, NotFound error, or Conflict error.</returns>
    public static async Task<Result<T>> SingleOrNotFoundAsync<T>(
        this DbSet<T> dbSet,
        ISpecification<T> specification,
        string? entityName = null,
        CancellationToken ct = default) where T : class
    {
        var entities = await SpecificationEvaluator
            .Apply(specification, dbSet)
            .Take(2)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return entities.Count switch
        {
            0 => Error.NotFound($"{entityName ?? typeof(T).Name} not found"),
            1 => entities[0],
            _ => Error.Conflict($"Multiple {entityName ?? typeof(T).Name} entities found")
        };
    }

    /// <summary>
    /// Checks if any entity matches the specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="dbSet">The DbSet to query.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if any entity matches; otherwise false.</returns>
    public static async Task<bool> AnyAsync<T>(
        this DbSet<T> dbSet,
        ISpecification<T> specification,
        CancellationToken ct = default) where T : class =>
        await SpecificationEvaluator
            .Apply(specification, dbSet)
            .AnyAsync(ct)
            .ConfigureAwait(false);

    /// <summary>
    /// Returns the count of entities matching the specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="dbSet">The DbSet to query.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The count of matching entities.</returns>
    public static async Task<int> CountAsync<T>(
        this DbSet<T> dbSet,
        ISpecification<T> specification,
        CancellationToken ct = default) where T : class =>
        await SpecificationEvaluator
            .Apply(specification, dbSet)
            .CountAsync(ct)
            .ConfigureAwait(false);
}
