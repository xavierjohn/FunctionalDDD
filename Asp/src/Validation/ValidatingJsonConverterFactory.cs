namespace FunctionalDdd;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A JSON converter factory that creates <see cref="ValidatingJsonConverter{T}"/> instances
/// for types implementing <see cref="ITryCreatable{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// This factory enables automatic validation during JSON deserialization for all value objects
/// that implement <see cref="ITryCreatable{T}"/>. Register this factory with
/// <see cref="JsonSerializerOptions.Converters"/> to enable validation for DTOs containing value objects.
/// </para>
/// <para>
/// The factory creates appropriate converters for both reference types and value types (structs).
/// </para>
/// </remarks>
/// <example>
/// Configuring in ASP.NET Core:
/// <code>
/// builder.Services.Configure&lt;JsonOptions&gt;(options =>
/// {
///     options.SerializerOptions.Converters.Add(new ValidatingJsonConverterFactory());
/// });
/// </code>
/// </example>
public sealed class ValidatingJsonConverterFactory : JsonConverterFactory
{
    private static readonly Type s_iTryCreatableType = typeof(ITryCreatable<>);

    /// <summary>
    /// Determines whether the factory can convert the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to check.</param>
    /// <returns><c>true</c> if the type implements <see cref="ITryCreatable{T}"/>; otherwise, <c>false</c>.</returns>
    [UnconditionalSuppressMessage("AOT", "IL2070:RequiresDynamicCode",
        Justification = "GetInterfaces is safe here as we're checking for a known interface type.")]
    public override bool CanConvert(Type typeToConvert)
    {
        // Check if it's a nullable value type
        var underlyingType = Nullable.GetUnderlyingType(typeToConvert);
        var typeToCheck = underlyingType ?? typeToConvert;

        // Check if the type implements ITryCreatable<T> where T is itself
        return typeToCheck
            .GetInterfaces()
            .Any(i => i.IsGenericType &&
                     i.GetGenericTypeDefinition() == s_iTryCreatableType &&
                     i.GetGenericArguments()[0] == typeToCheck);
    }

    /// <summary>
    /// Creates a converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type for which to create a converter.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>A <see cref="JsonConverter"/> instance for the specified type.</returns>
    [UnconditionalSuppressMessage("AOT", "IL2070:RequiresDynamicCode",
        Justification = "The factory only creates converters for ITryCreatable types which are known at compile time.")]
    [UnconditionalSuppressMessage("AOT", "IL2071:RequiresDynamicCode",
        Justification = "The converter types have parameterless constructors.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
        Justification = "The factory is used with types that are already referenced and instantiated elsewhere.")]
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        // Handle nullable value types
        var underlyingType = Nullable.GetUnderlyingType(typeToConvert);
        var actualType = underlyingType ?? typeToConvert;

        // Determine if it's a value type (struct) or reference type (class)
        Type converterType;
        if (actualType.IsValueType)
        {
            // Use the struct converter for value types
            converterType = typeof(ValidatingStructJsonConverter<>).MakeGenericType(actualType);
        }
        else
        {
            // Use the class converter for reference types
            converterType = typeof(ValidatingJsonConverter<>).MakeGenericType(actualType);
        }

        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}
