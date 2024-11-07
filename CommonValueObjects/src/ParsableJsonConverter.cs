namespace FunctionalDdd;

using System.Text.Json.Serialization;

using System.Text.Json;

public class ParsableJsonConverter<T> :
    JsonConverter<T> where T : IParsable<T>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => T.Parse(reader.GetString()!, default);

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
