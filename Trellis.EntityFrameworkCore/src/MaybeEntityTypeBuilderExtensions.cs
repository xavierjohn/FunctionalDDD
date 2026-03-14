namespace Trellis.EntityFrameworkCore;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Entity type builder helpers that resolve <see cref="Maybe{T}"/> properties to their mapped backing fields.
/// </summary>
public static class MaybeEntityTypeBuilderExtensions
{
    /// <summary>
    /// Creates an index using CLR property selectors while automatically translating any <see cref="Maybe{T}"/>
    /// properties to their field-only EF Core mappings.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entityTypeBuilder">The entity type builder.</param>
    /// <param name="propertySelector">
    /// A single property selector or anonymous object of property selectors.
    /// Example: <c>e =&gt; e.Phone</c> or <c>e =&gt; new { e.Status, e.SubmittedAt }</c>.
    /// </param>
    /// <returns>The index builder for additional configuration.</returns>
    public static IndexBuilder<TEntity> HasTrellisIndex<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        Expression<Func<TEntity, object?>> propertySelector)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entityTypeBuilder);
        ArgumentNullException.ThrowIfNull(propertySelector);

        var propertyNames = MaybePropertyResolver.ResolveMappedPropertyNames(propertySelector);
        return entityTypeBuilder.HasIndex([.. propertyNames]);
    }
}