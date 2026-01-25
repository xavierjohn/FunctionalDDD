namespace FunctionalDdd.Asp.Validation;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Factory for creating validating JSON converters for <see cref="IScalarValue{TSelf, TPrimitive}"/> types.
/// </summary>
/// <remarks>
/// <para>
/// This factory is registered with <see cref="JsonSerializerOptions"/> and automatically
/// creates <see cref="ValidatingJsonConverter{TValueObject, TPrimitive}"/> instances
/// for any type implementing <see cref="IScalarValue{TSelf, TPrimitive}"/>.
/// </para>
/// <para>
/// Unlike the exception-throwing approach, this factory creates converters that collect
/// validation errors in <see cref="ValidationErrorsContext"/> for comprehensive error reporting.
/// </para>
/// </remarks>
public sealed class ValidatingJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// Determines whether this factory can create a converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to check.</param>
    /// <returns><c>true</c> if the type implements <see cref="IScalarValue{TSelf, TPrimitive}"/>.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "Value object types are preserved by JSON serialization infrastructure")]
    public override bool CanConvert(Type typeToConvert) =>
        ScalarValueTypeHelper.IsScalarValueObject(typeToConvert);

    /// <summary>
    /// Creates a validating converter for the specified value object type.
    /// </summary>
    /// <param name="typeToConvert">The value object type.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>A validating JSON converter for the value object type.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "Value object types are preserved by JSON serialization infrastructure")]
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Value object types are preserved by JSON serialization infrastructure")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JsonConverterFactory is not compatible with Native AOT")]
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var primitiveType = ScalarValueTypeHelper.GetPrimitiveType(typeToConvert);
        return primitiveType is null
            ? null
            : ScalarValueTypeHelper.CreateGenericInstance<JsonConverter>(
                typeof(ValidatingJsonConverter<,>),
                typeToConvert,
                primitiveType);
    }
}
