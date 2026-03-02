namespace Trellis.EntityFrameworkCore;

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Extension methods for <see cref="EntityTypeBuilder{TEntity}"/> that simplify
/// mapping <see cref="Maybe{T}"/> domain properties to nullable database columns.
/// </summary>
/// <remarks>
/// <para>
/// EF Core cannot make a <c>readonly struct</c> property nullable directly.
/// <see cref="Maybe{T}"/> is a struct, so <c>IsRequired(false)</c> is rejected.
/// The workaround is to back the <see cref="Maybe{T}"/> property with a private
/// nullable field of the inner type and let EF Core map the field instead.
/// </para>
/// <para>
/// These extension methods automate that pattern. The entity must declare a
/// private nullable backing field following the <c>_camelCase</c> naming convention:
/// </para>
/// <code>
/// public class Customer
/// {
///     public CustomerId Id { get; set; } = null!;
///
///     private PhoneNumber? _phone;
///     public Maybe&lt;PhoneNumber&gt; Phone
///     {
///         get =&gt; _phone is not null ? Maybe.From(_phone) : Maybe.None&lt;PhoneNumber&gt;();
///         set =&gt; _phone = value.HasValue ? value.Value : null;
///     }
/// }
/// </code>
/// <para>
/// Then in <c>OnModelCreating</c>:
/// </para>
/// <code>
/// builder.MaybeProperty(c =&gt; c.Phone);
/// </code>
/// </remarks>
public static class MaybePropertyExtensions
{
    /// <summary>
    /// Configures a <see cref="Maybe{T}"/> property by mapping its private nullable
    /// backing field to a nullable database column. The <see cref="Maybe{T}"/> property
    /// itself is ignored by EF Core.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TInner">The type wrapped in <see cref="Maybe{T}"/>. Supports both reference types and value types.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="propertyExpression">
    /// An expression selecting the <see cref="Maybe{T}"/> property (e.g., <c>c =&gt; c.Phone</c>).
    /// </param>
    /// <returns>
    /// A <see cref="PropertyBuilder"/> for the backing field, allowing further column configuration
    /// (e.g., <c>.HasMaxLength(20)</c>).
    /// </returns>
    /// <remarks>
    /// <para>
    /// The entity must declare a private nullable field following the <c>_camelCase</c> convention.
    /// For a property named <c>Phone</c>, the backing field must be <c>_phone</c>.
    /// </para>
    /// <para>
    /// The backing field's type must be the nullable inner type:
    /// <list type="bullet">
    /// <item><c>Maybe&lt;PhoneNumber&gt;</c> → <c>PhoneNumber? _phone</c> (reference type, naturally nullable)</item>
    /// <item><c>Maybe&lt;DateTime&gt;</c> → <c>DateTime? _submittedAt</c> (value type, <c>Nullable&lt;DateTime&gt;</c>)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Value converters for Trellis scalar value objects are automatically provided by
    /// <see cref="ModelConfigurationBuilderExtensions.ApplyTrellisConventions"/>. No additional
    /// <c>HasConversion</c> call is needed for types like <c>PhoneNumber</c> or <c>EmailAddress</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Entity
    /// public class Customer
    /// {
    ///     public CustomerId Id { get; set; } = null!;
    ///
    ///     private PhoneNumber? _phone;
    ///     public Maybe&lt;PhoneNumber&gt; Phone
    ///     {
    ///         get =&gt; _phone is not null ? Maybe.From(_phone) : Maybe.None&lt;PhoneNumber&gt;();
    ///         set =&gt; _phone = value.HasValue ? value.Value : null;
    ///     }
    ///
    ///     private DateTime? _submittedAt;
    ///     public Maybe&lt;DateTime&gt; SubmittedAt
    ///     {
    ///         get =&gt; _submittedAt.HasValue ? Maybe.From(_submittedAt.Value) : Maybe.None&lt;DateTime&gt;();
    ///         set =&gt; _submittedAt = value.HasValue ? value.Value : null;
    ///     }
    /// }
    ///
    /// // Configuration
    /// builder.MaybeProperty(c =&gt; c.Phone);
    /// builder.MaybeProperty(c =&gt; c.SubmittedAt);
    /// </code>
    /// </example>
    public static PropertyBuilder MaybeProperty<TEntity, TInner>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, Maybe<TInner>>> propertyExpression)
        where TEntity : class
        where TInner : notnull
    {
        var propertyInfo = GetPropertyInfo(propertyExpression);
        var backingFieldName = ToBackingFieldName(propertyInfo.Name);
        var innerType = typeof(TInner);
        var nullableType = innerType.IsValueType
            ? typeof(Nullable<>).MakeGenericType(innerType)
            : innerType;

        ValidateBackingField(
            typeof(TEntity), propertyInfo.Name, backingFieldName, nullableType,
            nameof(propertyExpression));

        builder.Ignore(propertyInfo.Name);

        return builder.Property(nullableType, backingFieldName)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .IsRequired(false);
    }

    private static void ValidateBackingField(
        Type entityType, string propertyName, string backingFieldName, Type expectedType,
        string paramName)
    {
        var field = entityType.GetField(
            backingFieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (field is null)
            throw new ArgumentException(
                $"Entity '{entityType.Name}' does not declare a private backing field '{backingFieldName}' " +
                $"for Maybe<> property '{propertyName}'. " +
                $"Declare: private {FormatTypeName(expectedType)} {backingFieldName};",
                paramName);

        if (field.FieldType != expectedType)
            throw new ArgumentException(
                $"Backing field '{entityType.Name}.{backingFieldName}' is of type '{FormatTypeName(field.FieldType)}' " +
                $"but expected '{FormatTypeName(expectedType)}'. " +
                $"Change the field to: private {FormatTypeName(expectedType)} {backingFieldName};",
                paramName);
    }

    private static string FormatTypeName(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return $"{type.GetGenericArguments()[0].Name}?";

        return type.Name;
    }

    private static PropertyInfo GetPropertyInfo<TEntity, TInner>(
        Expression<Func<TEntity, Maybe<TInner>>> expression) where TInner : notnull
    {
        if (expression.Body is MemberExpression { Member: PropertyInfo propertyInfo })
            return propertyInfo;

        throw new ArgumentException(
            "Expression must be a simple property access (e.g., c => c.Phone).",
            nameof(expression));
    }

    private static string ToBackingFieldName(string propertyName) =>
        $"_{char.ToLowerInvariant(propertyName[0])}{propertyName[1..]}";
}
