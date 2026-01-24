namespace PrimitiveValueObjects.Tests;

using System.Globalization;
using System.Text.Json;

public class PositiveDecimalTests
{
    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(19.99)]
    [InlineData(1000000.00)]
    public void Can_create_valid_PositiveDecimal(decimal value)
    {
        // Act
        var result = PositiveDecimal.TryCreate(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Cannot_create_zero_or_negative_PositiveDecimal(decimal value)
    {
        // Act
        var result = PositiveDecimal.TryCreate(value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)result.Error;
        validation.FieldErrors[0].Details[0].Should().Be("Value must be greater than zero.");
    }

    [Fact]
    public void Cannot_create_null_PositiveDecimal()
    {
        // Act
        var result = PositiveDecimal.TryCreate((decimal?)null);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validation = (ValidationError)result.Error;
        validation.FieldErrors[0].Details[0].Should().Be("Value is required.");
    }

    [Fact]
    public void One_property_returns_one()
    {
        // Act
        var one = PositiveDecimal.One;

        // Assert
        one.Value.Should().Be(1m);
    }

    [Fact]
    public void Two_PositiveDecimal_with_same_value_should_be_equal()
    {
        // Arrange
        var a = PositiveDecimal.TryCreate(5.5m).Value;
        var b = PositiveDecimal.TryCreate(5.5m).Value;

        // Assert
        (a == b).Should().BeTrue();
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Two_PositiveDecimal_with_different_value_should_not_be_equal()
    {
        // Arrange
        var a = PositiveDecimal.TryCreate(5.5m).Value;
        var b = PositiveDecimal.TryCreate(10.5m).Value;

        // Assert
        (a != b).Should().BeTrue();
        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Can_implicitly_cast_to_decimal()
    {
        // Arrange
        PositiveDecimal value = PositiveDecimal.TryCreate(42.5m).Value;

        // Act
        decimal decimalValue = value;

        // Assert
        decimalValue.Should().Be(42.5m);
    }

    [Fact]
    public void Can_explicitly_cast_from_decimal()
    {
        // Act
        PositiveDecimal value = (PositiveDecimal)42.5m;

        // Assert
        value.Value.Should().Be(42.5m);
    }

    [Fact]
    public void Cannot_explicitly_cast_zero_from_decimal()
    {
        // Act
        Action act = () => { PositiveDecimal value = (PositiveDecimal)0m; };

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData("0.01", 0.01)]
    [InlineData("42.5", 42.5)]
    [InlineData("100.99", 100.99)]
    public void Can_try_parse_valid_string(string input, decimal expected)
    {
        // Act
        PositiveDecimal.TryParse(input, CultureInfo.InvariantCulture, out var result)
            .Should().BeTrue();

        // Assert
        result!.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1.5")]
    [InlineData("-100")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData(null)]
    public void Cannot_try_parse_invalid_string(string? input)
    {
        // Act
        PositiveDecimal.TryParse(input, CultureInfo.InvariantCulture, out var result)
            .Should().BeFalse();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("0.01", 0.01)]
    [InlineData("42.5", 42.5)]
    public void Can_parse_valid_string(string input, decimal expected)
    {
        // Act
        var result = PositiveDecimal.Parse(input, CultureInfo.InvariantCulture);

        // Assert
        result.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1.5")]
    public void Cannot_parse_zero_or_negative_string(string input)
    {
        // Act
        Action act = () => PositiveDecimal.Parse(input, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Value must be greater than zero.");
    }

    [Theory]
    [InlineData("abc")]
    public void Cannot_parse_invalid_format_string(string input)
    {
        // Act
        Action act = () => PositiveDecimal.Parse(input, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Value must be a valid decimal.");
    }

    [Fact]
    public void ConvertToJson()
    {
        // Arrange
        var value = PositiveDecimal.TryCreate(42.5m).Value;
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
        var actual = JsonSerializer.Deserialize<PositiveDecimal>(json)!;

        // Assert
        actual.Value.Should().Be(42.5m);
    }

    [Fact]
    public void Cannot_deserialize_zero_from_JSON()
    {
        // Arrange
        var json = "\"0\"";

        // Act
        Action act = () => JsonSerializer.Deserialize<PositiveDecimal>(json);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Value must be greater than zero.");
    }

    [Fact]
    public void Cannot_deserialize_negative_from_JSON()
    {
        // Arrange
        var json = "\"-5.5\"";

        // Act
        Action act = () => JsonSerializer.Deserialize<PositiveDecimal>(json);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Value must be greater than zero.");
    }

    [Fact]
    public void TryCreate_with_custom_fieldName()
    {
        // Act
        var result = PositiveDecimal.TryCreate(0m, "unitPrice");

        // Assert
        result.IsFailure.Should().BeTrue();
        var validation = (ValidationError)result.Error;
        validation.FieldErrors[0].FieldName.Should().Be("unitPrice");
    }
}
