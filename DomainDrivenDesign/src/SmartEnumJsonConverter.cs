namespace FunctionalDdd;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// JSON converter for <see cref="SmartEnum{TSelf}"/> types.
/// Serializes smart enums to their string name and deserializes from either name or integer value.
/// </summary>
/// <typeparam name="TSmartEnum">The smart enum type to convert.</typeparam>
/// <remarks>
/// <para>
/// Serialization behavior:
/// <list type="bullet">
/// <item>Smart enums are serialized as their string <see cref="SmartEnum{TSelf}.Name"/></item>
/// <item>This produces human-readable JSON output</item>
/// </list>
/// </para>
/// <para>
/// Deserialization behavior:
/// <list type="bullet">
/// <item>String values are matched by name (case-insensitive)</item>
/// <item>Integer values are matched by value</item>
/// <item>Null values deserialize to null (for nullable properties)</item>
/// <item>Invalid values throw <see cref="JsonException"/></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Apply the converter to a smart enum:
/// <code><![CDATA[
/// [JsonConverter(typeof(SmartEnumJsonConverter<OrderStatus>))]
/// public class OrderStatus : SmartEnum<OrderStatus>
/// {
///     public static readonly OrderStatus Pending = new(1, "Pending");
///     public static readonly OrderStatus Shipped = new(2, "Shipped");
///     
///     private OrderStatus(int value, string name) : base(value, name) { }
/// }
/// 
/// // Serialization
/// var json = JsonSerializer.Serialize(OrderStatus.Pending);  // "Pending"
/// 
/// // Deserialization from string
/// var status1 = JsonSerializer.Deserialize<OrderStatus>("\"Pending\"");  // OrderStatus.Pending
/// 
/// // Deserialization from int
/// var status2 = JsonSerializer.Deserialize<OrderStatus>("1");  // OrderStatus.Pending
/// ]]></code>
/// </example>
public sealed class SmartEnumJsonConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TSmartEnum> : JsonConverter<TSmartEnum>
    where TSmartEnum : SmartEnum<TSmartEnum>
{
    /// <inheritdoc />
    public override TSmartEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.String => ReadFromString(ref reader),
            JsonTokenType.Number => ReadFromNumber(ref reader),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unexpected token type '{reader.TokenType}' when parsing {typeof(TSmartEnum).Name}. Expected String or Number.")
        };

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TSmartEnum value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.Name);
    }

    private static TSmartEnum ReadFromString(ref Utf8JsonReader reader)
    {
        var name = reader.GetString();
        var result = SmartEnum<TSmartEnum>.TryFromName(name);

        if (result.IsFailure)
            throw new JsonException($"Invalid {typeof(TSmartEnum).Name} value: '{name}'. {result.Error.Detail}");

        return result.Value;
    }

    private static TSmartEnum ReadFromNumber(ref Utf8JsonReader reader)
    {
        var value = reader.GetInt32();
        var result = SmartEnum<TSmartEnum>.TryFromValue(value);

        if (result.IsFailure)
            throw new JsonException($"Invalid {typeof(TSmartEnum).Name} value: {value}. {result.Error.Detail}");

        return result.Value;
    }
}

/// <summary>
/// JSON converter factory that automatically creates <see cref="SmartEnumJsonConverter{TSmartEnum}"/>
/// for any <see cref="SmartEnum{TSelf}"/> derived type.
/// </summary>
/// <remarks>
/// <para>
/// Register this factory with <see cref="JsonSerializerOptions"/> to automatically handle
/// all smart enum types without explicit converter attributes on each type.
/// </para>
/// </remarks>
/// <example>
/// Register the factory globally:
/// <code><![CDATA[
/// var options = new JsonSerializerOptions
/// {
///     Converters = { new SmartEnumJsonConverterFactory() }
/// };
/// 
/// // Now all SmartEnum types are automatically serialized/deserialized
/// var json = JsonSerializer.Serialize(new Order { Status = OrderStatus.Pending }, options);
/// ]]></code>
/// </example>
/// <example>
/// Register in ASP.NET Core:
/// <code><![CDATA[
/// builder.Services.AddControllers()
///     .AddJsonOptions(options =>
///     {
///         options.JsonSerializerOptions.Converters.Add(new SmartEnumJsonConverterFactory());
///     });
/// ]]></code>
/// </example>
public sealed class SmartEnumJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        // Check if the type derives from SmartEnum<T> where T is the type itself
        var baseType = typeToConvert.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(SmartEnum<>))
                return true;

            baseType = baseType.BaseType;
        }

        return false;
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// <strong>AOT Compatibility Note:</strong> This method uses <c>MakeGenericType</c> and <c>Activator.CreateInstance</c>,
    /// which are not compatible with Native AOT compilation. For AOT scenarios, apply 
    /// <see cref="SmartEnumJsonConverter{TSmartEnum}"/> directly to each smart enum type using the
    /// <c>[JsonConverter(typeof(SmartEnumJsonConverter&lt;YourSmartEnum&gt;))]</c> attribute.
    /// </para>
    /// </remarks>
#pragma warning disable IL3050 // MakeGenericType is not AOT compatible
#pragma warning disable IL2055 // MakeGenericType may not work with trimming
#pragma warning disable IL2070 // CreateInstance may not work with trimming
#pragma warning disable IL2071 // Generic argument does not satisfy DynamicallyAccessedMembers
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(SmartEnumJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
#pragma warning restore IL2071
#pragma warning restore IL2070
#pragma warning restore IL2055
#pragma warning restore IL3050
}
