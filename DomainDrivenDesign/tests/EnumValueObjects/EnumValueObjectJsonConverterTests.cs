namespace DomainDrivenDesign.Tests.EnumValueObjects;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Tests for <see cref="EnumValueObjectJsonConverter{TEnumValueObject}"/> and <see cref="EnumValueObjectJsonConverterFactory"/>.
/// </summary>
public class EnumValueObjectJsonConverterTests
{
    #region Test Enum Value Objects

    [JsonConverter(typeof(EnumValueObjectJsonConverter<Priority>))]
    internal class Priority : EnumValueObject<Priority>
    {
        public static readonly Priority Low = new("Low");
        public static readonly Priority Medium = new("Medium");
        public static readonly Priority High = new("High");
        public static readonly Priority Critical = new("Critical");

        private Priority(string name) : base(name) { }
    }

    // Enum value object without JsonConverter attribute for testing factory
    internal class Status : EnumValueObject<Status>
    {
        public static readonly Status Active = new("Active");
        public static readonly Status Inactive = new("Inactive");

        private Status(string name) : base(name) { }
    }

    internal record TestDto(Priority Priority, string Description);

    internal record TestDtoWithNullable(Priority? Priority, string Description);

    private static readonly JsonSerializerOptions s_factoryOptions = new()
    {
        Converters = { new EnumValueObjectJsonConverterFactory() }
    };

    #endregion

    #region Serialization Tests

    [Fact]
    public void Serialize_EnumValueObject_WritesNameAsString()
    {
        var priority = Priority.High;

        var json = JsonSerializer.Serialize(priority);

        json.Should().Be("\"High\"");
    }

    [Fact]
    public void Serialize_EnumValueObjectInObject_WritesNameAsString()
    {
        var dto = new TestDto(Priority.Critical, "Urgent task");

        var json = JsonSerializer.Serialize(dto);

        json.Should().Contain("\"Priority\":\"Critical\"");
    }

    [Fact]
    public void Serialize_NullEnumValueObject_WritesNull()
    {
        var dto = new TestDtoWithNullable(null, "No priority");

        var json = JsonSerializer.Serialize(dto);

        json.Should().Contain("\"Priority\":null");
    }

    #endregion

    #region Deserialization from String Tests

    [Fact]
    public void Deserialize_FromString_ReturnsCorrectMember()
    {
        var json = "\"Medium\"";

        var result = JsonSerializer.Deserialize<Priority>(json);

        result.Should().Be(Priority.Medium);
    }

    [Fact]
    public void Deserialize_FromStringCaseInsensitive_ReturnsCorrectMember()
    {
        var json = "\"CRITICAL\"";

        var result = JsonSerializer.Deserialize<Priority>(json);

        result.Should().Be(Priority.Critical);
    }

    [Fact]
    public void Deserialize_FromInvalidString_ThrowsJsonException()
    {
        var json = "\"Unknown\"";

        var act = () => JsonSerializer.Deserialize<Priority>(json);

        act.Should().Throw<JsonException>()
            .WithMessage("*Unknown*not a valid Priority*");
    }

    [Fact]
    public void Deserialize_FromNull_ReturnsNull()
    {
        var json = "{\"Priority\":null,\"Description\":\"Test\"}";

        var result = JsonSerializer.Deserialize<TestDtoWithNullable>(json);

        result.Should().NotBeNull();
        result!.Priority.Should().BeNull();
    }

    [Fact]
    public void Deserialize_ObjectWithEnumValueObject_Works()
    {
        var json = "{\"Priority\":\"Low\",\"Description\":\"Test task\"}";

        var result = JsonSerializer.Deserialize<TestDto>(json);

        result.Should().NotBeNull();
        result!.Priority.Should().Be(Priority.Low);
        result.Description.Should().Be("Test task");
    }

    #endregion

    #region Factory Tests

    [Fact]
    public void Factory_CanConvert_ReturnsTrueForEnumValueObject()
    {
        var factory = new EnumValueObjectJsonConverterFactory();

        factory.CanConvert(typeof(Priority)).Should().BeTrue();
        factory.CanConvert(typeof(Status)).Should().BeTrue();
    }

    [Fact]
    public void Factory_CanConvert_ReturnsFalseForNonEnumValueObject()
    {
        var factory = new EnumValueObjectJsonConverterFactory();

        factory.CanConvert(typeof(string)).Should().BeFalse();
        factory.CanConvert(typeof(int)).Should().BeFalse();
        factory.CanConvert(typeof(TestDto)).Should().BeFalse();
    }

    [Fact]
    public void Factory_Serialize_Works()
    {
        var status = Status.Active;

        var json = JsonSerializer.Serialize(status, s_factoryOptions);

        json.Should().Be("\"Active\"");
    }

    [Fact]
    public void Factory_Deserialize_Works()
    {
        var json = "\"Inactive\"";

        var result = JsonSerializer.Deserialize<Status>(json, s_factoryOptions);

        result.Should().Be(Status.Inactive);
    }

    #endregion

    #region Error Cases

    [Fact]
    public void Deserialize_FromNumber_ThrowsJsonException()
    {
        var json = "1";

        var act = () => JsonSerializer.Deserialize<Priority>(json);

        act.Should().Throw<JsonException>()
            .WithMessage("*Unexpected token type*Number*Expected String*");
    }

    [Fact]
    public void Deserialize_FromObject_ThrowsJsonException()
    {
        var json = "{}";

        var act = () => JsonSerializer.Deserialize<Priority>(json);

        act.Should().Throw<JsonException>();
    }

    #endregion

    #region Round-Trip Tests

    [Fact]
    public void RoundTrip_PreservesValue()
    {
        var original = Priority.High;

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Priority>(json);

        deserialized.Should().Be(original);
    }

    [Fact]
    public void RoundTrip_WithDto_PreservesAllProperties()
    {
        var original = new TestDto(Priority.Medium, "Important task");

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<TestDto>(json);

        deserialized.Should().BeEquivalentTo(original);
    }

    #endregion
}
