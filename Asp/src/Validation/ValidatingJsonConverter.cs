namespace FunctionalDdd;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A JSON converter for value objects that implement <see cref="ITryCreatable{TSelf}"/>.
/// This converter collects validation errors instead of throwing exceptions, 
/// enabling comprehensive validation error responses.
/// </summary>
/// <typeparam name="T">The type of the value object to convert. Must implement <see cref="ITryCreatable{T}"/>.</typeparam>
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
/// <item>A default value (null for reference types) is returned</item>
/// <item>Deserialization continues to collect additional errors</item>
/// </list>
/// </para>
/// <para>
/// After deserialization, the caller should check <see cref="ValidationErrorsContext.HasErrors"/>
/// or <see cref="ValidationErrorsContext.GetValidationError"/> to determine if validation failed.
/// </para>
/// </remarks>
/// <example>
/// Using in a DTO with automatic validation:
/// <code>
/// public record CreateUserRequest(
///     FirstName FirstName,      // Automatically validated
///     LastName LastName,        // Automatically validated
///     EmailAddress Email        // Automatically validated
/// );
/// 
/// // In ASP.NET Core with proper configuration:
/// [HttpPost]
/// public ActionResult&lt;User&gt; Register([FromBody] CreateUserRequest request)
/// {
///     // If we get here, all value objects are valid
///     return User.TryCreate(request.FirstName, request.LastName, request.Email)
///         .ToActionResult(this);
/// }
/// </code>
/// </example>
public sealed class ValidatingJsonConverter<T> : JsonConverter<T?> where T : class, ITryCreatable<T>
{
    private readonly string? _propertyName;

    /// <summary>
    /// Creates a new instance of <see cref="ValidatingJsonConverter{T}"/>.
    /// </summary>
    public ValidatingJsonConverter() : this(null)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="ValidatingJsonConverter{T}"/> with a specific property name for error reporting.
    /// </summary>
    /// <param name="propertyName">The property name to use in validation error messages.</param>
    public ValidatingJsonConverter(string? propertyName) =>
        _propertyName = propertyName;

    /// <inheritdoc />
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Handle null JSON values
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        // Read the string value
        var stringValue = reader.GetString();

        // Determine the field name for error reporting
        // Priority: 1) Context property name (set by wrapper), 2) Constructor property name, 3) Type name
        var fieldName = ValidationErrorsContext.CurrentPropertyName
                        ?? _propertyName
                        ?? GetDefaultFieldName(typeToConvert);

        // Use TryCreate to validate
        var result = T.TryCreate(stringValue, fieldName);

        if (result.IsSuccess)
            return result.Value;

        // Collect validation error if we're in a validation context
        if (result.Error is ValidationError validationError)
            ValidationErrorsContext.AddError(validationError);
        else
            ValidationErrorsContext.AddError(fieldName, result.Error.Detail);

        // Return null to allow deserialization to continue
        // The caller will check ValidationErrorsContext for errors
        return null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.ToString());
    }

    private static string GetDefaultFieldName(Type type)
    {
        var name = type.Name;
        if (name.Length == 0)
            return name;
        if (name.Length == 1)
            return name.ToLowerInvariant();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}

/// <summary>
/// A JSON converter for struct value objects that implement <see cref="ITryCreatable{TSelf}"/>.
/// Similar to <see cref="ValidatingJsonConverter{T}"/> but for value types.
/// </summary>
/// <typeparam name="T">The type of the value object to convert. Must be a struct implementing <see cref="ITryCreatable{T}"/>.</typeparam>
public sealed class ValidatingStructJsonConverter<T> : JsonConverter<T?> where T : struct, ITryCreatable<T>
{
    private readonly string? _propertyName;

    /// <summary>
    /// Creates a new instance of <see cref="ValidatingStructJsonConverter{T}"/>.
    /// </summary>
    public ValidatingStructJsonConverter() : this(null)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="ValidatingStructJsonConverter{T}"/> with a specific property name for error reporting.
    /// </summary>
    /// <param name="propertyName">The property name to use in validation error messages.</param>
    public ValidatingStructJsonConverter(string? propertyName) =>
        _propertyName = propertyName;

    /// <inheritdoc />
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Handle null JSON values
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        // Read the string value
        var stringValue = reader.GetString();

        // Determine the field name for error reporting
        // Priority: 1) Context property name (set by wrapper), 2) Constructor property name, 3) Type name
        var fieldName = ValidationErrorsContext.CurrentPropertyName
                        ?? _propertyName
                        ?? GetDefaultFieldName(typeof(T));

        // Use TryCreate to validate
        var result = T.TryCreate(stringValue, fieldName);

        if (result.IsSuccess)
            return result.Value;

        // Collect validation error if we're in a validation context
        if (result.Error is ValidationError validationError)
            ValidationErrorsContext.AddError(validationError);
        else
            ValidationErrorsContext.AddError(fieldName, result.Error.Detail);

        // Return null to allow deserialization to continue
        return null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.ToString());
    }

    private static string GetDefaultFieldName(Type type)
    {
        var name = type.Name;
        if (name.Length == 0)
            return name;
        if (name.Length == 1)
            return name.ToLowerInvariant();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
