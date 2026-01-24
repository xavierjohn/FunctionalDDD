namespace PrimitiveValueObjects.Tests;

using System.Globalization;
using System.Text.Json;

public class PositiveIntTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void Can_create_valid_PositiveInt(int value)
    {
        // Act
        var result = PositiveInt.TryCreate(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Cannot_create_zero_or_negative_PositiveInt(int value)
    {
        // Act
        var result = PositiveInt.TryCreate(value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)result.Error;
        validation.FieldErrors[0].Details[0].Should().Be("Value must be greater than zero.");
    }

    [Fact]
    public void Cannot_create_null_PositiveInt()
    {
        // Act
        var result = PositiveInt.TryCreate((int?)null);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validation = (ValidationError)result.Error;
        validation.FieldErrors[0].Details[0].Should().Be("Value is required.");
    }

    [Fact]
    public void One_property_returns_one()
    {
        // Act
        var one = PositiveInt.One;

        // Assert
        one.Value.Should().Be(1);
    }

    [Fact]
    public void Two_PositiveInt_with_same_value_should_be_equal()
    {
        // Arrange
        var a = PositiveInt.TryCreate(5).Value;
        var b = PositiveInt.TryCreate(5).Value;

        // Assert
        (a == b).Should().BeTrue();
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Two_PositiveInt_with_different_value_should_not_be_equal()
    {
        // Arrange
        var a = PositiveInt.TryCreate(5).Value;
        var b = PositiveInt.TryCreate(10).Value;

        // Assert
        (a != b).Should().BeTrue();
        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Can_implicitly_cast_to_int()
    {
        // Arrange
        PositiveInt value = PositiveInt.TryCreate(42).Value;

        // Act
        int intValue = value;

        // Assert
        intValue.Should().Be(42);
    }

    [Fact]
    public void Can_explicitly_cast_from_int()
    {
        // Act
        PositiveInt value = (PositiveInt)42;

        // Assert
        value.Value.Should().Be(42);
    }

    [Fact]
    public void Cannot_explicitly_cast_zero_from_int()
    {
        // Act
        Action act = () => { PositiveInt value = (PositiveInt)0; };

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData("1", 1)]
    [InlineData("42", 42)]
    [InlineData("100", 100)]
    public void Can_try_parse_valid_string(string input, int expected)
    {
        // Act
        PositiveInt.TryParse(input, CultureInfo.InvariantCulture, out var result)
            .Should().BeTrue();

        // Assert
        result!.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("-100")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData(null)]
    public void Cannot_try_parse_invalid_string(string? input)
    {
        // Act
        PositiveInt.TryParse(input, CultureInfo.InvariantCulture, out var result)
            .Should().BeFalse();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("1", 1)]
    [InlineData("42", 42)]
    public void Can_parse_valid_string(string input, int expected)
    {
        // Act
        var result = PositiveInt.Parse(input, CultureInfo.InvariantCulture);

        // Assert
        result.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public void Cannot_parse_zero_or_negative_string(string input)
    {
        // Act
        Action act = () => PositiveInt.Parse(input, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Value must be greater than zero.");
    }

    [Theory]
    [InlineData("abc")]
    public void Cannot_parse_invalid_format_string(string input)
    {
        // Act
        Action act = () => PositiveInt.Parse(input, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Value must be a valid integer.");
    }

    [Fact]
    public void ConvertToJson()
    {
        // Arrange
        var value = PositiveInt.TryCreate(42).Value;
        var expected = "\"42\"";

        // Act
        var actual = JsonSerializer.Serialize(value);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void ConvertFromJson()
    {
        // Arrange
        var json = "\"42\"";

        // Act
        var actual = JsonSerializer.Deserialize<PositiveInt>(json)!;

        // Assert
        actual.Value.Should().Be(42);
    }

    [Fact]
    public void Cannot_deserialize_zero_from_JSON()
    {
        // Arrange
        var json = "\"0\"";

        // Act
        Action act = () => JsonSerializer.Deserialize<PositiveInt>(json);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Value must be greater than zero.");
    }

    [Fact]
    public void Cannot_deserialize_negative_from_JSON()
    {
        // Arrange
        var json = "\"-5\"";

        // Act
        Action act = () => JsonSerializer.Deserialize<PositiveInt>(json);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Value must be greater than zero.");
    }

    [Fact]
    public void TryCreate_with_custom_fieldName()
    {
        // Act
        var result = PositiveInt.TryCreate(0, "pageNumber");

        // Assert
        result.IsFailure.Should().BeTrue();
        var validation = (ValidationError)result.Error;
        validation.FieldErrors[0].FieldName.Should().Be("pageNumber");
    }
}
