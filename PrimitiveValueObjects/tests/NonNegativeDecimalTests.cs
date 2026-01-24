namespace PrimitiveValueObjects.Tests;

using System.Globalization;
using System.Text.Json;

public class NonNegativeDecimalTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(19.99)]
    [InlineData(1000000.00)]
    public void Can_create_valid_NonNegativeDecimal(decimal value)
    {
        // Act
        var result = NonNegativeDecimal.TryCreate(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Cannot_create_negative_NonNegativeDecimal(decimal value)
    {
        // Act
        var result = NonNegativeDecimal.TryCreate(value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)result.Error;
        validation.FieldErrors[0].Details[0].Should().Be("Value cannot be negative.");
    }

    [Fact]
    public void Cannot_create_null_NonNegativeDecimal()
    {
        // Act
        var result = NonNegativeDecimal.TryCreate((decimal?)null);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validation = (ValidationError)result.Error;
        validation.FieldErrors[0].Details[0].Should().Be("Value is required.");
    }

    [Fact]
    public void Zero_property_returns_zero()
    {
        // Act
        var zero = NonNegativeDecimal.Zero;

        // Assert
        zero.Value.Should().Be(0m);
    }

    [Fact]
    public void Two_NonNegativeDecimal_with_same_value_should_be_equal()
    {
        // Arrange
        var a = NonNegativeDecimal.TryCreate(5.5m).Value;
        var b = NonNegativeDecimal.TryCreate(5.5m).Value;

        // Assert
        (a == b).Should().BeTrue();
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Two_NonNegativeDecimal_with_different_value_should_not_be_equal()
    {
        // Arrange
        var a = NonNegativeDecimal.TryCreate(5.5m).Value;
        var b = NonNegativeDecimal.TryCreate(10.5m).Value;

        // Assert
        (a != b).Should().BeTrue();
        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Can_implicitly_cast_to_decimal()
    {
        // Arrange
        NonNegativeDecimal value = NonNegativeDecimal.TryCreate(42.5m).Value;

        // Act
        decimal decimalValue = value;

        // Assert
        decimalValue.Should().Be(42.5m);
    }

    [Fact]
    public void Can_explicitly_cast_from_decimal()
    {
        // Act
        NonNegativeDecimal value = (NonNegativeDecimal)42.5m;

        // Assert
        value.Value.Should().Be(42.5m);
    }

    [Fact]
    public void Cannot_explicitly_cast_negative_from_decimal()
    {
        // Act
        Action act = () => { NonNegativeDecimal value = (NonNegativeDecimal)(-1m); };

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("42.5", 42.5)]
    [InlineData("100.99", 100.99)]
    public void Can_try_parse_valid_string(string input, decimal expected)
    {
        // Act
        NonNegativeDecimal.TryParse(input, CultureInfo.InvariantCulture, out var result)
            .Should().BeTrue();

        // Assert
        result!.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("-1.5")]
    [InlineData("-100")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData(null)]
    public void Cannot_try_parse_invalid_string(string? input)
    {
        // Act
        NonNegativeDecimal.TryParse(input, CultureInfo.InvariantCulture, out var result)
            .Should().BeFalse();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("42.5", 42.5)]
    public void Can_parse_valid_string(string input, decimal expected)
    {
        // Act
        var result = NonNegativeDecimal.Parse(input, CultureInfo.InvariantCulture);

        // Assert
        result.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("-1.5", "Value cannot be negative.")]
    public void Cannot_parse_negative_string(string input, string expectedMessage)
    {
        // Act
        Action act = () => NonNegativeDecimal.Parse(input, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage(expectedMessage);
    }

    [Theory]
    [InlineData("abc")]
    public void Cannot_parse_invalid_format_string(string input)
    {
        // Act
        Action act = () => NonNegativeDecimal.Parse(input, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Value must be a valid decimal.");
    }

    [Fact]
    public void ConvertToJson()
    {
        // Arrange
        var value = NonNegativeDecimal.TryCreate(42.5m).Value;
        var expected = "\"42.5\"";

        // Act
        var actual = JsonSerializer.Serialize(value);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void ConvertFromJson()
    {
        // Arrange
        var json = "\"42.5\"";

        // Act
        var actual = JsonSerializer.Deserialize<NonNegativeDecimal>(json)!;

        // Assert
        actual.Value.Should().Be(42.5m);
    }

    [Fact]
    public void Cannot_deserialize_negative_from_JSON()
    {
        // Arrange
        var json = "\"-5.5\"";

        // Act
        Action act = () => JsonSerializer.Deserialize<NonNegativeDecimal>(json);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Value cannot be negative.");
    }

    [Fact]
    public void TryCreate_with_custom_fieldName()
    {
        // Act
        var result = NonNegativeDecimal.TryCreate(-1m, "price");

        // Assert
        result.IsFailure.Should().BeTrue();
        var validation = (ValidationError)result.Error;
        validation.FieldErrors[0].FieldName.Should().Be("price");
    }
}
