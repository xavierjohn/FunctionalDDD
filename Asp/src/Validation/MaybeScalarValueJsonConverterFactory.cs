namespace FunctionalDdd.Asp.Validation;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Factory for creating <see cref="MaybeScalarValueJsonConverter{TValue, TPrimitive}"/> instances
/// for <see cref="Maybe{T}"/> properties where T implements <see cref="IScalarValue{TSelf, TPrimitive}"/>.
/// </summary>
/// <remarks>
/// <para>
/// This factory is registered with <see cref="JsonSerializerOptions"/> and automatically
/// creates converters for <see cref="Maybe{T}"/> types wrapping scalar value objects.
/// </para>
/// <para>
/// It complements <see cref="ValidatingJsonConverterFactory"/> which handles direct
/// <see cref="IScalarValue{TSelf, TPrimitive}"/> properties.
/// </para>
/// </remarks>
public sealed class MaybeScalarValueJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// Determines whether this factory can create a converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to check.</param>
    /// <returns><c>true</c> if the type is <see cref="Maybe{T}"/> where T implements <see cref="IScalarValue{TSelf, TPrimitive}"/>.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "Value object types are preserved by JSON serialization infrastructure")]
    public override bool CanConvert(Type typeToConvert) =>
        ScalarValueTypeHelper.IsMaybeScalarValue(typeToConvert);

    /// <summary>
    /// Creates a converter for the specified <see cref="Maybe{T}"/> type.
    /// </summary>
    /// <param name="typeToConvert">The <see cref="Maybe{T}"/> type.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>A <see cref="MaybeScalarValueJsonConverter{TValue, TPrimitive}"/> for the type.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "Value object types are preserved by JSON serialization infrastructure")]
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Value object types are preserved by JSON serialization infrastructure")]
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Inner type of Maybe<T> is preserved by JSON serialization infrastructure")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JsonConverterFactory is not compatible with Native AOT")]
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var innerType = ScalarValueTypeHelper.GetMaybeInnerType(typeToConvert);
        if (innerType is null)
            return null;

        var primitiveType = ScalarValueTypeHelper.GetPrimitiveType(innerType);
        return primitiveType is null
            ? null
            : ScalarValueTypeHelper.CreateGenericInstance<JsonConverter>(
                typeof(MaybeScalarValueJsonConverter<,>),
                innerType,
                primitiveType);
    }
}
