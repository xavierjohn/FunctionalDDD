namespace FunctionalDdd.Asp.Validation;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A JSON converter for <see cref="Maybe{TValue}"/> properties where <typeparamref name="TValue"/>
/// implements <see cref="IScalarValue{TSelf, TPrimitive}"/>.
/// </summary>
/// <typeparam name="TValue">The scalar value object type.</typeparam>
/// <typeparam name="TPrimitive">The underlying primitive type.</typeparam>
/// <remarks>
/// <para>
/// This converter enables DTOs to use <see cref="Maybe{TValue}"/> for optional value object properties:
/// </para>
/// <list type="bullet">
/// <item><c>null</c> JSON value → <c>Maybe.None&lt;TValue&gt;()</c> — no error, value was not provided</item>
/// <item>Valid JSON value → <c>Maybe.From(validated)</c> — value provided and valid</item>
/// <item>Invalid JSON value → validation error collected, continues deserialization</item>
/// </list>
/// <para>
/// Unlike <see cref="ValidatingJsonConverter{TValue, TPrimitive}"/>, this converter treats
/// JSON <c>null</c> as a valid "not provided" signal rather than an error.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public record RegisterUserDto
/// {
///     public EmailAddress Email { get; init; } = null!;       // required
///     public Maybe&lt;FirstName&gt; FirstName { get; init; }  // optional — null = None
/// }
/// </code>
/// </example>
public sealed class MaybeScalarValueJsonConverter<TValue, TPrimitive> : JsonConverter<Maybe<TValue>>
    where TValue : class, IScalarValue<TValue, TPrimitive>
    where TPrimitive : IComparable
{
    /// <inheritdoc />
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "TPrimitive type parameter is preserved by JSON serialization infrastructure")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JSON deserialization of primitive types is compatible with AOT")]
    public override Maybe<TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // null JSON value means "not provided" — return None, no error
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        // Deserialize the primitive value
        var primitiveValue = JsonSerializer.Deserialize<TPrimitive>(ref reader, options);

        if (primitiveValue is null)
        {
            var fieldName = ValidationErrorsContext.CurrentPropertyName ?? GetDefaultFieldName();
            ValidationErrorsContext.AddError(fieldName, $"Cannot deserialize null to {typeof(TValue).Name}");
            return default;
        }

        // Determine the field name for error reporting
        var propertyName = ValidationErrorsContext.CurrentPropertyName ?? GetDefaultFieldName();

        // Use TryCreate to validate
        var result = TValue.TryCreate(primitiveValue, propertyName);

        if (result.IsSuccess)
            return Maybe.From(result.Value);

        // Collect validation error
        if (result.Error is ValidationError validationError)
            ValidationErrorsContext.AddError(validationError);
        else
            ValidationErrorsContext.AddError(propertyName, result.Error.Detail);

        // Return None to allow deserialization to continue
        return default;
    }

    /// <inheritdoc />
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "TPrimitive type parameter is preserved by JSON serialization infrastructure")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JSON serialization of primitive types is compatible with AOT")]
    public override void Write(Utf8JsonWriter writer, Maybe<TValue> value, JsonSerializerOptions options)
    {
        if (value.HasNoValue)
        {
            writer.WriteNullValue();
            return;
        }

        WritePrimitiveValue(writer, value.Value.Value);
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
                writer.WriteStringValue(value?.ToString());
                break;
        }
    }

    private static string GetDefaultFieldName()
    {
        var name = typeof(TValue).Name;
        return name.Length > 0 && char.IsUpper(name[0])
            ? char.ToLowerInvariant(name[0]) + name.Substring(1)
            : name;
    }
}
