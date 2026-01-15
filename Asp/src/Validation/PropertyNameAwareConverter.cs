namespace FunctionalDdd;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A generic wrapper converter that sets the property name in <see cref="ValidationErrorsContext"/>
/// before delegating to an inner converter.
/// </summary>
/// <typeparam name="T">The type being converted.</typeparam>
/// <remarks>
/// <para>
/// This converter enables AOT-compatible property-name-aware validation by:
/// <list type="bullet">
/// <item>Setting <see cref="ValidationErrorsContext.CurrentPropertyName"/> before reading</item>
/// <item>Delegating to the inner converter for actual deserialization</item>
/// <item>Clearing the property name after reading</item>
/// </list>
/// </para>
/// <para>
/// The inner converter (e.g., <see cref="ValidatingJsonConverter{T}"/>) reads from
/// <see cref="ValidationErrorsContext.CurrentPropertyName"/> to determine the field name
/// for validation errors.
/// </para>
/// </remarks>
internal sealed class PropertyNameAwareConverter<T> : JsonConverter<T?>
{
    private readonly JsonConverter<T?> _innerConverter;
    private readonly string _propertyName;

    /// <summary>
    /// Creates a new property-name-aware wrapper converter.
    /// </summary>
    /// <param name="innerConverter">The inner converter to delegate to.</param>
    /// <param name="propertyName">The property name to set in the context during read operations.</param>
    public PropertyNameAwareConverter(JsonConverter<T?> innerConverter, string propertyName)
    {
        _innerConverter = innerConverter;
        _propertyName = propertyName;
    }

    /// <inheritdoc />
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Set the property name in context so the inner converter can use it
        var previousPropertyName = ValidationErrorsContext.CurrentPropertyName;
        ValidationErrorsContext.CurrentPropertyName = _propertyName;
        try
        {
            return _innerConverter.Read(ref reader, typeToConvert, options);
        }
        finally
        {
            // Restore the previous property name (for nested objects)
            ValidationErrorsContext.CurrentPropertyName = previousPropertyName;
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options) =>
        _innerConverter.Write(writer, value, options);
}

/// <summary>
/// Factory methods for creating property-name-aware converters.
/// </summary>
internal static class PropertyNameAwareConverterFactory
{
    /// <summary>
    /// Creates a property-name-aware wrapper for the given converter.
    /// </summary>
    /// <param name="innerConverter">The inner converter to wrap.</param>
    /// <param name="propertyName">The property name to use for validation errors.</param>
    /// <param name="type">The type being converted.</param>
    /// <returns>A wrapped converter that sets the property name in context.</returns>
    public static JsonConverter? Create(JsonConverter innerConverter, string propertyName, Type type)
    {
        // Handle nullable value types
        var underlyingType = Nullable.GetUnderlyingType(type);
        var actualType = underlyingType ?? type;

        // Try AOT-safe path first - check if we have a registered converter
        if (ValidatingConverterRegistry.HasConverter(actualType))
            return CreateFromRegistry(innerConverter, propertyName, actualType);

        // Fallback to reflection-based creation
        return CreateWithReflection(innerConverter, propertyName, type);
    }

#pragma warning disable IL2055, IL2060, IL3050 // Reflection - fallback path
    private static JsonConverter? CreateFromRegistry(JsonConverter innerConverter, string propertyName, Type type)
    {
        // Use the registered converter type to create the wrapper
        var wrapperType = typeof(PropertyNameAwareConverter<>).MakeGenericType(type);
        return Activator.CreateInstance(wrapperType, innerConverter, propertyName) as JsonConverter;
    }

    private static JsonConverter? CreateWithReflection(JsonConverter innerConverter, string propertyName, Type type)
    {
        var wrapperType = typeof(PropertyNameAwareConverter<>).MakeGenericType(type);
        return Activator.CreateInstance(wrapperType, innerConverter, propertyName) as JsonConverter;
    }
#pragma warning restore IL2055, IL2060, IL3050
}
