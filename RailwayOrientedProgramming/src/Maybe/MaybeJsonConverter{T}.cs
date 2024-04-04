namespace FunctionalDdd;
using System;
using System.Text.Json.Serialization;
using System.Text.Json;

public class MaybeJsonConverter<T>
    : JsonConverter<Maybe<T>>
    where T : notnull
{
    public override Maybe<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return Maybe.None<T>();

        // Deserialize the value of type T
        T? value = JsonSerializer.Deserialize<T>(ref reader, options);
        if (value is null)
            return Maybe.None<T>();

        return Maybe.From(value);
    }

    public override void Write(Utf8JsonWriter writer, Maybe<T> value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            JsonSerializer.Serialize(writer, value.Value, options);
        else
            writer.WriteNullValue();
    }
}