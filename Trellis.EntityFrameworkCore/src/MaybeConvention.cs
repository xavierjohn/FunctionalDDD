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
    private static readonly Type s_maybeOpenGenericType = typeof(Maybe<>);

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

            var maybeProperties = entityType.ClrType
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(IsMaybeProperty)
                .ToList();

            foreach (var clrProperty in maybeProperties)
            {
                var propertyName = clrProperty.Name;
                var innerType = clrProperty.PropertyType.GetGenericArguments()[0];
                var backingFieldName = ToBackingFieldName(propertyName);

                // Always ignore the Maybe<T> CLR property — EF Core cannot map structs as nullable
                entityType.Builder.Ignore(propertyName);

                // Verify the backing field exists (source generator should have created it)
                var backingField = entityType.ClrType.GetField(
                    backingFieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic);

                if (backingField is null)
                    continue; // No backing field — nothing to map

                // Check if already configured explicitly
                var existingBackingProp = entityType.FindProperty(backingFieldName);
                if (existingBackingProp is not null)
                    continue;

                // Determine the nullable type for the backing field
                var nullableType = innerType.IsValueType
                    ? typeof(Nullable<>).MakeGenericType(innerType)
                    : innerType;

                // Map the backing field as a nullable property
                var propertyBuilder = entityType.Builder.Property(nullableType, backingFieldName);
                if (propertyBuilder is null)
                    continue;

                propertyBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);
                propertyBuilder.IsRequired(false);
                propertyBuilder.HasAnnotation(RelationalAnnotationNames.ColumnName, propertyName);
            }
        }
    }

    private static bool IsMaybeProperty(PropertyInfo property)
    {
        var type = property.PropertyType;
        return type.IsGenericType
               && type.GetGenericTypeDefinition() == s_maybeOpenGenericType;
    }

    private static string ToBackingFieldName(string propertyName) =>
        propertyName.Length == 1
            ? $"_{char.ToLowerInvariant(propertyName[0])}"
            : $"_{char.ToLowerInvariant(propertyName[0])}{propertyName[1..]}";
}