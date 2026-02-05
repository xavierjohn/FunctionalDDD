namespace FunctionalDdd;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// JSON converter for <see cref="EnumValueObject{TSelf}"/> types.
/// Serializes to the string Name and deserializes from string name.
/// </summary>
/// <typeparam name="TEnumValueObject">The enum value object type to convert.</typeparam>
/// <example>
/// <code><![CDATA[
/// [JsonConverter(typeof(EnumValueObjectJsonConverter<OrderState>))]
/// public class OrderState : EnumValueObject<OrderState>
/// {
///     public static readonly OrderState Draft = new("Draft");
///     public static readonly OrderState Confirmed = new("Confirmed");
///     
///     private OrderState(string name) : base(name) { }
/// }
/// 
/// // Serialization
/// var json = JsonSerializer.Serialize(OrderState.Draft);  // "Draft"
/// 
/// // Deserialization
/// var state = JsonSerializer.Deserialize<OrderState>("\"Draft\"");
/// ]]></code>
/// </example>
public sealed class EnumValueObjectJsonConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnumValueObject> : JsonConverter<TEnumValueObject>
    where TEnumValueObject : EnumValueObject<TEnumValueObject>
{
    /// <inheritdoc />
    public override TEnumValueObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.String => ReadFromString(ref reader),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unexpected token type '{reader.TokenType}' when parsing {typeof(TEnumValueObject).Name}. Expected String.")
        };

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TEnumValueObject value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.Name);
    }

    private static TEnumValueObject ReadFromString(ref Utf8JsonReader reader)
    {
        var name = reader.GetString();
        var result = EnumValueObject<TEnumValueObject>.TryFromName(name);

        if (result.IsFailure)
            throw new JsonException($"Invalid {typeof(TEnumValueObject).Name} value: '{name}'. {result.Error.Detail}");

        return result.Value;
    }
}

/// <summary>
/// JSON converter factory that automatically creates <see cref="EnumValueObjectJsonConverter{TEnumValueObject}"/>
/// for any <see cref="EnumValueObject{TSelf}"/> derived type.
/// </summary>
/// <example>
/// <code><![CDATA[
/// var options = new JsonSerializerOptions
/// {
///     Converters = { new EnumValueObjectJsonConverterFactory() }
/// };
/// 
/// var json = JsonSerializer.Serialize(new Order { State = OrderState.Draft }, options);
/// ]]></code>
/// </example>
public sealed class EnumValueObjectJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        for (var baseType = typeToConvert.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(EnumValueObject<>))
            {
                return true;
            }
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
        var converterType = typeof(EnumValueObjectJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
#pragma warning restore IL2071
#pragma warning restore IL2070
#pragma warning restore IL2055
#pragma warning restore IL3050
}
