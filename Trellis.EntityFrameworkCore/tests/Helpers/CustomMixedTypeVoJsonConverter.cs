namespace Trellis.EntityFrameworkCore.Tests.Helpers;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// User-written custom <see cref="JsonConverter{T}"/> for <see cref="TestMixedTypeVo"/>.
/// Demonstrates the language-level escape hatch when a composite VO's interior holds
/// shapes that <c>CompositeValueObjectJsonConverter&lt;T&gt;</c> does not support
/// (<c>Maybe&lt;TPrimitive&gt;</c>, arrays).
/// <para>
/// <strong>This is NOT the recommended approach.</strong> The recommended path —
/// documented in cookbook Recipe 13 §"Supported property shapes inside a composite VO"
/// and Recipe 14 — is to keep the composite VO clean as a domain type and declare a
/// wire-shape DTO with nullable transports at the controller/endpoint seam. Writing a
/// custom converter is a last-resort escape hatch only when the DTO indirection is
/// genuinely unacceptable; it adds per-VO code, bypasses framework validation
/// integration, and locks future framework improvements out of the wire path.
/// </para>
/// <para>
/// This converter exists in the test suite to prove the escape hatch works end-to-end
/// and to document the shape of such a converter for the small minority of services
/// that need it. Most services should use the DTO seam.
/// </para>
/// </summary>
public sealed class CustomMixedTypeVoJsonConverter : JsonConverter<TestMixedTypeVo>
{
    public override void Write(Utf8JsonWriter writer, TestMixedTypeVo value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteStartObject();

        writer.WriteString("status", value.Status.Value);

        if (value.Count.HasValue)
            writer.WriteNumber("count", value.Count.Value);
        else
            writer.WriteNull("count");

        if (value.Label.HasValue)
            writer.WriteString("label", value.Label.Value);
        else
            writer.WriteNull("label");

        if (value.Snapshots.HasValue)
        {
            writer.WritePropertyName("snapshots");
            writer.WriteStartArray();
            foreach (var dt in value.Snapshots.Value)
                writer.WriteStringValue(dt);
            writer.WriteEndArray();
        }
        else
        {
            writer.WriteNull("snapshots");
        }

        writer.WriteEndObject();
    }

    public override TestMixedTypeVo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of object.");

        string? status = null;
        var count = Maybe<int>.None;
        var label = Maybe<string>.None;
        var snapshots = Maybe<DateTime[]>.None;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;

            var name = reader.GetString();
            reader.Read();

            switch (name)
            {
                case "status":
                    status = reader.GetString();
                    break;
                case "count":
                    count = reader.TokenType == JsonTokenType.Null
                        ? Maybe<int>.None
                        : Maybe.From(reader.GetInt32());
                    break;
                case "label":
                    if (reader.TokenType == JsonTokenType.Null)
                    {
                        label = Maybe<string>.None;
                    }
                    else
                    {
                        var s = reader.GetString();
                        label = s is null ? Maybe<string>.None : Maybe.From(s);
                    }

                    break;
                case "snapshots":
                    if (reader.TokenType == JsonTokenType.Null)
                    {
                        snapshots = Maybe<DateTime[]>.None;
                    }
                    else if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        var list = new List<DateTime>();
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                            list.Add(reader.GetDateTime());
                        snapshots = Maybe.From(list.ToArray());
                    }
                    else
                    {
                        // Any other token (string, number, object, …) is invalid for
                        // 'snapshots'. Throw rather than silently leaving the reader at
                        // a non-PropertyName position, which would cause the outer loop
                        // to start treating that token's children as top-level properties.
                        throw new JsonException(
                            $"Property 'snapshots' expects a JSON array or null; got token {reader.TokenType}.");
                    }

                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        if (status is null)
            throw new JsonException("Missing required property 'status'.");

        var result = TestMixedTypeVo.TryCreate(status, count, label, snapshots);
        if (!result.TryGetValue(out var vo, out var error))
            throw new JsonException(error.GetDisplayMessage());
        return vo;
    }
}
