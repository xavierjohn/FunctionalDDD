namespace PrimitiveValueObjects.Tests;

using System.Globalization;
using System.Text.Json;

public class NonNegativeIntTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void Can_create_valid_NonNegativeInt(int value)
    {
        // Act
        var result = NonNegativeInt.TryCreate(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Cannot_create_negative_NonNegativeInt(int value)
    {
        // Act
        var result = NonNegativeInt.TryCreate(value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)result.Error;
        validation.FieldErrors[0].Details[0].Should().Be("Value cannot be negative.");
    }

    [Fact]
    public void Cannot_create_null_NonNegativeInt()
    {
        // Act
        var result = NonNegativeInt.TryCreate((int?)null);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validation = (ValidationError)result.Error;
        validation.FieldErrors[0].Details[0].Should().Be("Value is required.");
    }

    [Fact]
    public void Zero_property_returns_zero()
    {
        // Act
        var zero = NonNegativeInt.Zero;

        // Assert
        zero.Value.Should().Be(0);
    }

    [Fact]
    public void Two_NonNegativeInt_with_same_value_should_be_equal()
    {
        // Arrange
        var a = NonNegativeInt.TryCreate(5).Value;
        var b = NonNegativeInt.TryCreate(5).Value;

        // Assert
        (a == b).Should().BeTrue();
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Two_NonNegativeInt_with_different_value_should_not_be_equal()
    {
        // Arrange
        var a = NonNegativeInt.TryCreate(5).Value;
        var b = NonNegativeInt.TryCreate(10).Value;

        // Assert
        (a != b).Should().BeTrue();
        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Can_implicitly_cast_to_int()
    {
        // Arrange
        NonNegativeInt value = NonNegativeInt.TryCreate(42).Value;

        // Act
        int intValue = value;

        // Assert
        intValue.Should().Be(42);
    }

    [Fact]
    public void Can_explicitly_cast_from_int()
    {
        // Act
        NonNegativeInt value = (NonNegativeInt)42;

        // Assert
        value.Value.Should().Be(42);
    }

    [Fact]
    public void Cannot_explicitly_cast_negative_from_int()
    {
        // Act
        Action act = () => { NonNegativeInt value = (NonNegativeInt)(-1); };

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("42", 42)]
    [InlineData("100", 100)]
    public void Can_try_parse_valid_string(string input, int expected)
    {
        // Act
        NonNegativeInt.TryParse(input, CultureInfo.InvariantCulture, out var result)
            .Should().BeTrue();

        // Assert
        result!.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("-100")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData(null)]
    public void Cannot_try_parse_invalid_string(string? input)
    {
        // Act
        NonNegativeInt.TryParse(input, CultureInfo.InvariantCulture, out var result)
            .Should().BeFalse();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("42", 42)]
    public void Can_parse_valid_string(string input, int expected)
    {
        // Act
        var result = NonNegativeInt.Parse(input, CultureInfo.InvariantCulture);

        // Assert
        result.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("-1", "Value cannot be negative.")]
    public void Cannot_parse_negative_string(string input, string expectedMessage)
    {
        // Act
        Action act = () => NonNegativeInt.Parse(input, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage(expectedMessage);
    }

    [Theory]
    [InlineData("abc")]
    public void Cannot_parse_invalid_format_string(string input)
    {
        // Act
        Action act = () => NonNegativeInt.Parse(input, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Value must be a valid integer.");
    }

    [Fact]
    public void ConvertToJson()
    {
        // Arrange
        var value = NonNegativeInt.TryCreate(42).Value;
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
        var actual = JsonSerializer.Deserialize<NonNegativeInt>(json)!;

        // Assert
        actual.Value.Should().Be(42);
    }

    [Fact]
    public void Cannot_deserialize_negative_from_JSON()
    {
        // Arrange
        var json = "\"-5\"";

        // Act
        Action act = () => JsonSerializer.Deserialize<NonNegativeInt>(json);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Value cannot be negative.");
    }

    [Fact]
    public void TryCreate_with_custom_fieldName()
    {
        // Act
        var result = NonNegativeInt.TryCreate(-1, "quantity");

        // Assert
        result.IsFailure.Should().BeTrue();
        var validation = (ValidationError)result.Error;
        validation.FieldErrors[0].FieldName.Should().Be("quantity");
    }
}
