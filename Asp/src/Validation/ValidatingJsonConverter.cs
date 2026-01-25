namespace FunctionalDdd.Asp.Validation;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A JSON converter for value objects that implement <see cref="IScalarValue{TSelf, TPrimitive}"/>.
/// This converter collects validation errors instead of throwing exceptions,
/// enabling comprehensive validation error responses.
/// </summary>
/// <typeparam name="TValueObject">The type of the value object to convert.</typeparam>
/// <typeparam name="TPrimitive">The underlying primitive type.</typeparam>
/// <remarks>
/// <para>
/// This converter enables the pattern where DTOs can contain value objects directly,
/// and all validation errors are collected during deserialization rather than failing
/// on the first invalid value.
/// </para>
/// <para>
/// When a value fails validation:
/// <list type="bullet">
/// <item>The error is added to <see cref="ValidationErrorsContext"/></item>
/// <item>A default value (null) is returned</item>
/// <item>Deserialization continues to collect additional errors</item>
/// </list>
/// </para>
/// <para>
/// After deserialization, use <see cref="ScalarValueValidationFilter"/> to check for errors
/// and return appropriate 400 Bad Request responses.
/// </para>
/// </remarks>
public sealed class ValidatingJsonConverter<TValueObject, TPrimitive> : JsonConverter<TValueObject?>
    where TValueObject : class, IScalarValue<TValueObject, TPrimitive>
    where TPrimitive : IComparable
{
    /// <inheritdoc />
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "TPrimitive type parameter is preserved by JSON serialization infrastructure")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JSON deserialization of primitive types is compatible with AOT")]
    public override TValueObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Handle null JSON values
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        // Deserialize the primitive value
        var primitiveValue = JsonSerializer.Deserialize<TPrimitive>(ref reader, options);

        if (primitiveValue is null)
        {
            // Collect error for null primitive
            var fieldName = ValidationErrorsContext.CurrentPropertyName ?? GetDefaultFieldName(typeToConvert);
            ValidationErrorsContext.AddError(fieldName, $"Cannot deserialize null to {typeof(TValueObject).Name}");
            return null;
        }

        // Determine the field name for error reporting
        // Priority: 1) Context property name (set by wrapper), 2) Type name as fallback
        var propertyName = ValidationErrorsContext.CurrentPropertyName ?? GetDefaultFieldName(typeToConvert);

        // Use TryCreate to validate - direct call via static abstract interface member
        // Pass the property name so validation errors have the correct field name
        var result = TValueObject.TryCreate(primitiveValue, propertyName);

        if (result.IsSuccess)
            return result.Value;

        // Collect validation error
        if (result.Error is ValidationError validationError)
        {
            ValidationErrorsContext.AddError(validationError);
        }
        else
        {
            ValidationErrorsContext.AddError(propertyName, result.Error.Detail);
        }

        // Return null to allow deserialization to continue
        // The ScalarValueValidationFilter will check ValidationErrorsContext for errors
        return null;
    }

    /// <inheritdoc />
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "TPrimitive type parameter is preserved by JSON serialization infrastructure")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JSON serialization of primitive types is compatible with AOT")]
    public override void Write(Utf8JsonWriter writer, TValueObject? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        // Write primitive values directly to avoid requiring type info in source-generated contexts
        WritePrimitiveValue(writer, value.Value);
    }

    private static void WritePrimitiveValue(Utf8JsonWriter writer, TPrimitive value)
    {
        switch (value)
        {
            case string s:
                writer.WriteStringValue(s);
                break;
            case Guid g:
                writer.WriteStringValue(g);
                break;
            case int i:
                writer.WriteNumberValue(i);
                break;
            case long l:
                writer.WriteNumberValue(l);
                break;
            case double d:
                writer.WriteNumberValue(d);
                break;
            case float f:
                writer.WriteNumberValue(f);
                break;
            case decimal m:
                writer.WriteNumberValue(m);
                break;
            case bool b:
                writer.WriteBooleanValue(b);
                break;
            case DateTime dt:
                writer.WriteStringValue(dt);
                break;
            case DateTimeOffset dto:
                writer.WriteStringValue(dto);
                break;
            case DateOnly date:
                writer.WriteStringValue(date.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
                break;
            case TimeOnly time:
                writer.WriteStringValue(time.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
                break;
            default:
                // Fallback for other types - convert to string
                writer.WriteStringValue(value?.ToString());
                break;
        }
    }

    private static string GetDefaultFieldName(Type type) =>
        type.Name.Length > 0 && char.IsUpper(type.Name[0])
            ? char.ToLowerInvariant(type.Name[0]) + type.Name.Substring(1)
            : type.Name;
}
