namespace FunctionalDdd.Asp.Serialization;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using FunctionalDdd;

/// <summary>
/// JSON converter for ScalarValueObject-derived types.
/// Serializes as primitive value, validates during deserialization.
/// </summary>
/// <typeparam name="TValueObject">The value object type.</typeparam>
/// <typeparam name="TPrimitive">The underlying primitive type.</typeparam>
/// <remarks>
/// <para>
/// This converter enables transparent JSON serialization for value objects:
/// <list type="bullet">
/// <item><strong>Serialization:</strong> Writes the primitive value directly (e.g., <c>"user@example.com"</c>)</item>
/// <item><strong>Deserialization:</strong> Reads the primitive and calls <c>TryCreate</c> for validation</item>
/// </list>
/// </para>
/// <para>
/// Validation errors during deserialization throw <see cref="JsonException"/> with the error message,
/// which ASP.NET Core handles and returns as a 400 Bad Request response.
/// </para>
/// </remarks>
public class ScalarValueObjectJsonConverter<TValueObject, TPrimitive> : JsonConverter<TValueObject>
    where TValueObject : IScalarValueObject<TValueObject, TPrimitive>
    where TPrimitive : IComparable
{
    /// <summary>
    /// Reads a value object from JSON by deserializing the primitive and calling TryCreate.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The deserialized value object.</returns>
    /// <exception cref="JsonException">Thrown when the value is null or validation fails.</exception>
#pragma warning disable IL2026 // RequiresUnreferencedCode
#pragma warning disable IL3050 // RequiresDynamicCode
    public override TValueObject? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        // Handle null token explicitly
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        var primitiveValue = JsonSerializer.Deserialize<TPrimitive>(ref reader, options);

        if (primitiveValue is null)
            throw new JsonException($"Cannot deserialize null to {typeof(TValueObject).Name}");

        // Direct call to TryCreate - no reflection needed
        var result = TValueObject.TryCreate(primitiveValue);

        if (result.IsSuccess)
            return result.Value;

        var errorMessage = result.Error is ValidationError ve
            ? string.Join(", ", ve.FieldErrors.SelectMany(fe => fe.Details))
            : result.Error.Detail;

        throw new JsonException(errorMessage);
    }

    /// <summary>
    /// Writes a value object to JSON by serializing its primitive value.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The value object to serialize.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(
        Utf8JsonWriter writer,
        TValueObject value,
        JsonSerializerOptions options) => JsonSerializer.Serialize(writer, value.Value, options);
#pragma warning restore IL3050
#pragma warning restore IL2026
}
