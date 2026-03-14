namespace Trellis.EntityFrameworkCore;

using System.Linq.Expressions;
using System.Reflection;
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
    /// <exception cref="InvalidOperationException">
    /// Thrown when a <see cref="Maybe{T}"/> selector resolves to a backing field that neither exists on the CLR type
    /// nor is already mapped in the EF model.
    /// </exception>
    public static IndexBuilder<TEntity> HasTrellisIndex<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        Expression<Func<TEntity, object?>> propertySelector)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entityTypeBuilder);
        ArgumentNullException.ThrowIfNull(propertySelector);

        var propertyNames = MaybePropertyResolver.ResolveReferencedProperties(propertySelector)
            .Select(property => ResolveMappedPropertyName(entityTypeBuilder, property))
            .ToArray();

        return entityTypeBuilder.HasIndex([.. propertyNames]);
    }

    private static string ResolveMappedPropertyName<TEntity>(
        EntityTypeBuilder<TEntity> entityTypeBuilder,
        PropertyInfo property)
        where TEntity : class
    {
        if (!MaybePropertyResolver.IsMaybeProperty(property))
            return property.Name;

        var descriptor = MaybePropertyResolver.Describe(property);
        var backingField = entityTypeBuilder.Metadata.ClrType.GetField(
            descriptor.BackingFieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (backingField is not null || entityTypeBuilder.Metadata.FindProperty(descriptor.BackingFieldName) is not null)
            return descriptor.BackingFieldName;

        throw new InvalidOperationException(
            $"Cannot create an index for Maybe<T> property '{descriptor.PropertyName}' on entity '{entityTypeBuilder.Metadata.ClrType.Name}'. " +
            $"Expected backing field '{descriptor.BackingFieldName}' was not found on the CLR type and no mapped EF property with that name exists. " +
            "Declare the property as partial so the Trellis.EntityFrameworkCore.Generator can emit the backing field, or configure the backing field property explicitly before calling HasTrellisIndex.");
    }
}