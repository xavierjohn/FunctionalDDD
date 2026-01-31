namespace FunctionalDdd;

using Ardalis.Specification;

/// <summary>
/// Result-returning extensions for Ardalis.Specification's IReadRepositoryBase.
/// Provides seamless integration with Railway Oriented Programming patterns for read-only repositories.
/// </summary>
public static class ReadRepositoryResultExtensions
{
    /// <summary>
    /// Returns the first entity matching the specification, or a NotFoundError if none exists.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The read repository.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="entityName">Optional entity name for error messages (defaults to type name).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing the entity or a NotFoundError.</returns>
    public static async Task<Result<T>> FirstOrNotFoundAsync<T>(
        this IReadRepositoryBase<T> repository,
        ISpecification<T> specification,
        string? entityName = null,
        CancellationToken ct = default) where T : class
    {
        var entity = await repository.FirstOrDefaultAsync(specification, ct).ConfigureAwait(false);

        return entity is not null
            ? entity
            : Error.NotFound($"{entityName ?? typeof(T).Name} not found");
    }

    /// <summary>
    /// Returns the single entity matching the specification, or an appropriate error.
    /// Returns NotFoundError if no entity exists, ConflictError if multiple entities exist.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The read repository.</param>
    /// <param name="specification">The single-result specification to apply.</param>
    /// <param name="entityName">Optional entity name for error messages (defaults to type name).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing the single entity, NotFoundError, or ConflictError.</returns>
    public static async Task<Result<T>> SingleOrNotFoundAsync<T>(
        this IReadRepositoryBase<T> repository,
        ISingleResultSpecification<T> specification,
        string? entityName = null,
        CancellationToken ct = default) where T : class
    {
        var name = entityName ?? typeof(T).Name;

        try
        {
            var entity = await repository.SingleOrDefaultAsync(specification, ct).ConfigureAwait(false);

            return entity is not null
                ? entity
                : Error.NotFound($"{name} not found");
        }
        catch (InvalidOperationException)
        {
            return Error.Conflict($"Multiple {name} entities found");
        }
    }

    /// <summary>
    /// Checks if any entity matches the specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The read repository.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if any entity matches; otherwise false.</returns>
    public static async Task<bool> AnyAsync<T>(
        this IReadRepositoryBase<T> repository,
        ISpecification<T> specification,
        CancellationToken ct = default) where T : class =>
        await repository.AnyAsync(specification, ct).ConfigureAwait(false);

    /// <summary>
    /// Returns all entities matching the specification.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The read repository.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of matching entities.</returns>
    public static async Task<List<T>> ToListAsync<T>(
        this IReadRepositoryBase<T> repository,
        ISpecification<T> specification,
        CancellationToken ct = default) where T : class =>
        await repository.ListAsync(specification, ct).ConfigureAwait(false);
}