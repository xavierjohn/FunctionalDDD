namespace Trellis.Core.Tests;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

/// <summary>
/// Pins the fail-fast behavior of <see cref="ResultRequiresExplicitHttpMappingConverter"/>:
/// direct JSON serialization / deserialization of <see cref="Result{TValue}"/> throws an
/// actionable <see cref="InvalidOperationException"/> instead of silently producing a struct
/// dump. The intended path is <c>.ToHttpResponse()</c> (Trellis.Asp) — proven by checking
/// the message wording — and consumers who legitimately want raw JSON of a Result can
/// override via <c>JsonSerializerOptions.Converters</c>.
/// </summary>
public class ResultJsonFailFastTests
{
    [Fact]
    public void Serialize_Result_throws_with_actionable_message()
    {
        var result = Result.Ok(42);

        var act = () => JsonSerializer.Serialize(result);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Result<Int32>*cannot be serialized*ToHttpResponse*");
    }

    [Fact]
    public void Deserialize_Result_throws_with_actionable_message()
    {
        var act = () => JsonSerializer.Deserialize<Result<int>>("{\"IsSuccess\":true,\"Value\":42}");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Result<Int32>*cannot be deserialized*ToHttpResponse*");
    }

    [Fact]
    public void Serialize_failure_Result_also_throws_with_actionable_message()
    {
        var result = Result.Fail<int>(new Error.NotFound(new ResourceRef("Order", "42")));

        var act = () => JsonSerializer.Serialize(result);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Result<Int32>*cannot be serialized*Match*TryGetValue*");
    }

    [Fact]
    public void Consumer_override_in_options_takes_precedence_over_default_throwing_converter()
    {
        // Legitimate escape hatch: consumer who really wants to serialize Result<T> (logging,
        // IPC, storage) registers a JsonConverter<Result<T>> in JsonSerializerOptions.Converters.
        // STJ's converter resolution preferences the option-registered converter over the
        // type's [JsonConverter] attribute, so the throwing default is bypassed.
        var result = Result.Ok(42);

        var json = JsonSerializer.Serialize(result, s_optionsWithRawConverter);

        json.Should().Be("\"OK:42\"");
    }

    private static readonly JsonSerializerOptions s_optionsWithRawConverter = new()
    {
        Converters = { new RawResultConverter() },
    };

    [Fact]
    public void Serializing_a_DTO_containing_Result_property_also_throws()
    {
        // The default object formatter recursively serializes properties — the throw fires on
        // the nested Result<T> property exactly the same as a top-level Result<T>.
        var dto = new DtoCarryingResult(Result.Ok(42));

        var act = () => JsonSerializer.Serialize(dto);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Result<Int32>*ToHttpResponse*");
    }

    private sealed record DtoCarryingResult(Result<int> Inner);

    private sealed class RawResultConverter : JsonConverter<Result<int>>
    {
        public override Result<int> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotImplementedException();

        public override void Write(Utf8JsonWriter writer, Result<int> value, JsonSerializerOptions options)
        {
            value.TryGetValue(out var v);
            writer.WriteStringValue($"OK:{v}");
        }
    }
}
