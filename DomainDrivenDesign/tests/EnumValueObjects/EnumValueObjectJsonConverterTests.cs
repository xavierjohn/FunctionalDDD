namespace DomainDrivenDesign.Tests.EnumValueObjects;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Tests for <see cref="EnumValueObjectJsonConverter{TEnumValueObject}"/> and <see cref="EnumValueObjectJsonConverterFactory"/>.
/// </summary>
public class EnumValueObjectJsonConverterTests
{
    #region Test Enum value objects

    [JsonConverter(typeof(EnumValueObjectJsonConverter<Priority>))]
    internal class Priority : EnumValueObject<Priority>
    {
        public static readonly Priority Low = new(1, "Low");
        public static readonly Priority Medium = new(2, "Medium");
        public static readonly Priority High = new(3, "High");
        public static readonly Priority Critical = new(4, "Critical");

        private Priority(int value, string name) : base(value, name) { }
    }

    // Enum value object without JsonConverter attribute for testing factory
    internal class Status : EnumValueObject<Status>
    {
        public static readonly Status Active = new(1, "Active");
        public static readonly Status Inactive = new(2, "Inactive");

        private Status(int value, string name) : base(value, name) { }
    }

    internal record TestDto(Priority Priority, string Description);

    internal record TestDtoWithNullable(Priority? Priority, string Description);

    // Cached options for tests that need the factory
    private static readonly JsonSerializerOptions s_factoryOptions = new()
    {
        Converters = { new EnumValueObjectJsonConverterFactory() }
    };

    #endregion

    #region Serialization Tests

    [Fact]
    public void Serialize_EnumValueObject_WritesNameAsString()
    {
        // Arrange
        var priority = Priority.High;

        // Act
        var json = JsonSerializer.Serialize(priority);

        // Assert
        json.Should().Be("\"High\"");
    }

    [Fact]
    public void Serialize_EnumValueObjectInObject_WritesNameAsString()
    {
        // Arrange
        var dto = new TestDto(Priority.Critical, "Urgent task");

        // Act
        var json = JsonSerializer.Serialize(dto);

        // Assert
        json.Should().Contain("\"Priority\":\"Critical\"");
    }

    [Fact]
    public void Serialize_NullEnumValueObject_WritesNull()
    {
        // Arrange
        var dto = new TestDtoWithNullable(null, "No priority");

        // Act
        var json = JsonSerializer.Serialize(dto);

        // Assert
        json.Should().Contain("\"Priority\":null");
    }

    #endregion

    #region Deserialization from String Tests

    [Fact]
    public void Deserialize_FromString_ReturnsCorrectMember()
    {
        // Arrange
        var json = "\"Medium\"";

        // Act
        var result = JsonSerializer.Deserialize<Priority>(json);

        // Assert
        result.Should().Be(Priority.Medium);
    }

    [Fact]
    public void Deserialize_FromStringCaseInsensitive_ReturnsCorrectMember()
    {
        // Arrange
        var json = "\"LOW\"";

        // Act
        var result = JsonSerializer.Deserialize<Priority>(json);

        // Assert
        result.Should().Be(Priority.Low);
    }

    [Fact]
    public void Deserialize_ObjectWithEnumValueObject_ReturnsCorrectDto()
    {
        // Arrange
        var json = """{"Priority":"High","Description":"Important task"}""";

        // Act
        var result = JsonSerializer.Deserialize<TestDto>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Priority.Should().Be(Priority.High);
        result.Description.Should().Be("Important task");
    }

    [Fact]
    public void Deserialize_InvalidString_ThrowsJsonException()
    {
        // Arrange
        var json = "\"Unknown\"";

        // Act
        var act = () => JsonSerializer.Deserialize<Priority>(json);

        // Assert
        act.Should().Throw<JsonException>()
            .WithMessage("*Unknown*");
    }

    #endregion

    #region Deserialization from Number Tests

    [Fact]
    public void Deserialize_FromNumber_ReturnsCorrectMember()
    {
        // Arrange
        var json = "2";

        // Act
        var result = JsonSerializer.Deserialize<Priority>(json);

        // Assert
        result.Should().Be(Priority.Medium);
    }

    [Fact]
    public void Deserialize_ObjectWithEnumValueObjectAsNumber_ReturnsCorrectDto()
    {
        // Arrange
        var json = """{"Priority":4,"Description":"Critical issue"}""";

        // Act
        var result = JsonSerializer.Deserialize<TestDto>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Priority.Should().Be(Priority.Critical);
    }

    [Fact]
    public void Deserialize_InvalidNumber_ThrowsJsonException()
    {
        // Arrange
        var json = "999";

        // Act
        var act = () => JsonSerializer.Deserialize<Priority>(json);

        // Assert
        act.Should().Throw<JsonException>()
            .WithMessage("*999*");
    }

    #endregion

    #region Null Handling Tests

    [Fact]
    public void Deserialize_Null_ReturnsNull()
    {
        // Arrange
        var json = "null";

        // Act
        var result = JsonSerializer.Deserialize<Priority?>(json);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_ObjectWithNullEnumValueObject_ReturnsNullProperty()
    {
        // Arrange
        var json = """{"Priority":null,"Description":"No priority set"}""";

        // Act
        var result = JsonSerializer.Deserialize<TestDtoWithNullable>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Priority.Should().BeNull();
    }

    #endregion

    #region Invalid Token Type Tests

    [Fact]
    public void Deserialize_InvalidTokenType_ThrowsJsonException()
    {
        // Arrange
        var json = "true";  // Boolean is not valid

        // Act
        var act = () => JsonSerializer.Deserialize<Priority>(json);

        // Assert
        act.Should().Throw<JsonException>()
            .WithMessage("*Unexpected token type*");
    }

    [Fact]
    public void Deserialize_ObjectTokenType_ThrowsJsonException()
    {
        // Arrange
        var json = "{}";  // Object is not valid

        // Act
        var act = () => JsonSerializer.Deserialize<Priority>(json);

        // Assert
        act.Should().Throw<JsonException>()
            .WithMessage("*Unexpected token type*");
    }

    [Fact]
    public void Deserialize_ArrayTokenType_ThrowsJsonException()
    {
        // Arrange
        var json = "[]";  // Array is not valid

        // Act
        var act = () => JsonSerializer.Deserialize<Priority>(json);

        // Assert
        act.Should().Throw<JsonException>()
            .WithMessage("*Unexpected token type*");
    }

    #endregion

    #region Converter Factory Tests

    [Fact]
    public void ConverterFactory_CanConvert_ReturnsTrueForEnumValueObject()
    {
        // Arrange
        var factory = new EnumValueObjectJsonConverterFactory();

        // Act & Assert
        factory.CanConvert(typeof(Priority)).Should().BeTrue();
        factory.CanConvert(typeof(Status)).Should().BeTrue();
    }

    [Fact]
    public void ConverterFactory_CanConvert_ReturnsFalseForNonEnumValueObject()
    {
        // Arrange
        var factory = new EnumValueObjectJsonConverterFactory();

        // Act & Assert
        factory.CanConvert(typeof(string)).Should().BeFalse();
        factory.CanConvert(typeof(int)).Should().BeFalse();
        factory.CanConvert(typeof(DayOfWeek)).Should().BeFalse();  // Regular enum
    }

    [Fact]
    public void ConverterFactory_SerializesWithoutAttribute()
    {
        // Act
        var json = JsonSerializer.Serialize(Status.Active, s_factoryOptions);

        // Assert
        json.Should().Be("\"Active\"");
    }

    [Fact]
    public void ConverterFactory_DeserializesWithoutAttribute()
    {
        // Arrange
        var json = "\"Inactive\"";

        // Act
        var result = JsonSerializer.Deserialize<Status>(json, s_factoryOptions);

        // Assert
        result.Should().Be(Status.Inactive);
    }

    #endregion

    #region Round-Trip Tests

    [Fact]
    public void RoundTrip_EnumValueObject_PreservesValue()
    {
        // Arrange
        var original = Priority.Critical;

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Priority>(json);

        // Assert
        deserialized.Should().Be(original);
    }

    [Fact]
    public void RoundTrip_ObjectWithEnumValueObject_PreservesValue()
    {
        // Arrange
        var original = new TestDto(Priority.Low, "Test description");

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<TestDto>(json);

        // Assert
        deserialized.Should().BeEquivalentTo(original);
    }

    #endregion
}
