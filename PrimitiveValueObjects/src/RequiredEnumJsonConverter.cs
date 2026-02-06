namespace FunctionalDdd.PrimitiveValueObjects;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// JSON converter for <see cref="RequiredEnum{TSelf}"/> types.
/// Serializes to the string Name and deserializes from string name.
/// </summary>
/// <typeparam name="TRequiredEnum">The enum value object type to convert.</typeparam>
/// <example>
/// <code><![CDATA[
/// [JsonConverter(typeof(RequiredEnumJsonConverter<OrderState>))]
/// public partial class OrderState : RequiredEnum<OrderState>
/// {
///     public static readonly OrderState Draft = new();
///     public static readonly OrderState Confirmed = new();
/// }
/// 
/// // Serialization
/// var json = JsonSerializer.Serialize(OrderState.Draft);  // "Draft"
/// 
/// // Deserialization
/// var state = JsonSerializer.Deserialize<OrderState>("\"Draft\"");
/// ]]></code>
/// </example>
public sealed class RequiredEnumJsonConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TRequiredEnum> : JsonConverter<TRequiredEnum>
    where TRequiredEnum : RequiredEnum<TRequiredEnum>
{
    /// <inheritdoc />
    public override TRequiredEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.String => ReadFromString(ref reader),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unexpected token type '{reader.TokenType}' when parsing {typeof(TRequiredEnum).Name}. Expected String.")
        };

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TRequiredEnum value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.Name);
    }

    private static TRequiredEnum ReadFromString(ref Utf8JsonReader reader)
    {
        var name = reader.GetString();
        var result = RequiredEnum<TRequiredEnum>.TryFromName(name);

        if (result.IsFailure)
            throw new JsonException($"Invalid {typeof(TRequiredEnum).Name} value: '{name}'. {result.Error.Detail}");

        return result.Value;
    }
}

/// <summary>
/// JSON converter factory that automatically creates <see cref="RequiredEnumJsonConverter{TRequiredEnum}"/>
/// for any <see cref="RequiredEnum{TSelf}"/> derived type.
/// </summary>
/// <example>
/// <code><![CDATA[
/// var options = new JsonSerializerOptions
/// {
///     Converters = { new RequiredEnumJsonConverterFactory() }
/// };
/// 
/// var json = JsonSerializer.Serialize(new Order { State = OrderState.Draft }, options);
/// ]]></code>
/// </example>
public sealed class RequiredEnumJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        for (var baseType = typeToConvert.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(RequiredEnum<>))
                return true;
        }

        return false;
    }

    /// <inheritdoc />
#pragma warning disable IL3050 // MakeGenericType is not AOT compatible
#pragma warning disable IL2055 // MakeGenericType may not work with trimming
#pragma warning disable IL2070 // CreateInstance may not work with trimming
#pragma warning disable IL2071 // Generic argument does not satisfy DynamicallyAccessedMembers
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(RequiredEnumJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
#pragma warning restore IL2071
#pragma warning restore IL2070
#pragma warning restore IL2055
#pragma warning restore IL3050
}
