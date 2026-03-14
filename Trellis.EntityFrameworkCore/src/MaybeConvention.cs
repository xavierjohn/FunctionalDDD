namespace Trellis.EntityFrameworkCore;

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
/// Convention that automatically maps <see cref="Maybe{T}"/> properties by discovering their
/// source-generated private nullable backing fields (<c>_camelCase</c>) and configuring
/// EF Core to use the backing field as a nullable column.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Maybe{T}"/> is a <c>readonly struct</c>, which means EF Core cannot mark it
/// as nullable directly (<c>IsRequired(false)</c> throws <c>InvalidOperationException</c>).
/// This convention works with the <c>MaybePartialPropertyGenerator</c> source generator,
/// which emits private nullable backing fields for <c>partial Maybe&lt;T&gt;</c> properties.
/// </para>
/// <para>
/// For each CLR property of type <c>Maybe&lt;T&gt;</c> found on an entity type, this convention:
/// </para>
/// <list type="number">
/// <item>Ignores the <c>Maybe&lt;T&gt;</c> CLR property (EF Core cannot map structs as nullable)</item>
/// <item>Maps the private <c>_camelCase</c> backing field as an EF property</item>
/// <item>Marks the backing field as optional (<c>IsRequired(false)</c>)</item>
/// <item>Configures field-only access mode</item>
/// <item>Sets the column name to the original property name (e.g., <c>Phone</c> instead of <c>_phone</c>)</item>
/// </list>
/// <para>
/// User code with the source generator:
/// </para>
/// <code>
/// public partial class Customer
/// {
///     public CustomerId Id { get; set; } = null!;
///     public partial Maybe&lt;PhoneNumber&gt; Phone { get; set; }
/// }
/// </code>
/// <para>
/// No <c>MaybeProperty()</c> call is needed in <c>OnModelCreating</c> — the convention handles
/// everything automatically.
/// </para>
/// </remarks>
internal sealed class MaybeConvention : IModelFinalizingConvention
{
    /// <summary>
    /// After the model is built, discovers all <see cref="Maybe{T}"/> CLR properties on entity types
    /// and configures their backing fields as nullable database columns.
    /// </summary>
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes().ToList())
        {
            if (entityType.ClrType is null)
                continue;

            foreach (var maybeProperty in MaybePropertyResolver.GetMaybeProperties(entityType.ClrType))
            {
                // Always ignore the Maybe<T> CLR property — EF Core cannot map structs as nullable
                entityType.Builder.Ignore(maybeProperty.PropertyName);

                // Verify the backing field exists (source generator should have created it)
                var backingField = entityType.ClrType.GetField(
                    maybeProperty.BackingFieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic);

                if (backingField is null)
                    continue; // No backing field — nothing to map

                // Reuse an existing property if earlier model-building steps created it (for example via HasIndex).
                var existingBackingProp = entityType.FindProperty(maybeProperty.BackingFieldName);

                // Map or fetch the backing field as a nullable property.
                var propertyBuilder = existingBackingProp?.Builder
                    ?? entityType.Builder.Property(maybeProperty.StoreType, maybeProperty.BackingFieldName);
                if (propertyBuilder is null)
                    continue;

                propertyBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);
                propertyBuilder.IsRequired(false);
                propertyBuilder.HasAnnotation(RelationalAnnotationNames.ColumnName, maybeProperty.PropertyName);
            }
        }
    }
}