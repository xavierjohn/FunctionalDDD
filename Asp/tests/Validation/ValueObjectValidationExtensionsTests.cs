namespace Asp.Tests.Validation;

using System.Text.Json;
using System.Text.Json.Serialization;
using FunctionalDdd;
using Xunit;

/// <summary>
/// Tests for reflection fallback paths in the validation system.
/// These tests verify that the system works correctly when the source generator is not used.
/// </summary>
[Collection("ValidatingConverterRegistry")]
public class ReflectionFallbackTests : IDisposable
{
    public ReflectionFallbackTests() =>
        ValidatingConverterRegistry.Clear();

    public void Dispose()
    {
        ValidatingConverterRegistry.Clear();
        GC.SuppressFinalize(this);
    }

    #region ValidatingJsonConverterFactory Reflection Fallback Tests

    [Fact]
    public void ReflectionFallback_NonValueObjectType_CannotConvert()
    {
        // Arrange
        var factory = new ValidatingJsonConverterFactory();

        // Act - string is not a value object
        var canConvert = factory.CanConvert(typeof(string));

        // Assert
        canConvert.Should().BeFalse();
    }

    [Fact]
    public void ReflectionFallback_StructValueObject_CreatesCorrectConverter()
    {
        // Arrange - don't register, should use reflection
        var factory = new ValidatingJsonConverterFactory();
        var options = new JsonSerializerOptions();

        // Act
        var canConvert = factory.CanConvert(typeof(TestStructValueObject));
        var converter = factory.CreateConverter(typeof(TestStructValueObject), options);

        // Assert
        canConvert.Should().BeTrue();
        converter.Should().NotBeNull();
        converter.Should().BeOfType<ValidatingStructJsonConverter<TestStructValueObject>>();
    }

    [Fact]
    public void ReflectionFallback_ClassValueObject_CreatesCorrectConverter()
    {
        // Arrange - don't register, should use reflection
        var factory = new ValidatingJsonConverterFactory();
        var options = new JsonSerializerOptions();

        // Act
        var canConvert = factory.CanConvert(typeof(EmailAddress));
        var converter = factory.CreateConverter(typeof(EmailAddress), options);

        // Assert
        canConvert.Should().BeTrue();
        converter.Should().NotBeNull();
        converter.Should().BeOfType<ValidatingJsonConverter<EmailAddress>>();
    }

    [Fact]
    public void ReflectionFallback_NullableStructValueObject_CreatesConverter()
    {
        // Arrange - don't register, should use reflection
        var factory = new ValidatingJsonConverterFactory();
        var options = new JsonSerializerOptions();

        // Act
        var canConvert = factory.CanConvert(typeof(TestStructValueObject?));
        var converter = factory.CreateConverter(typeof(TestStructValueObject?), options);

        // Assert
        canConvert.Should().BeTrue();
        converter.Should().NotBeNull();
    }

    [Fact]
    public void ReflectionFallback_ConverterDirectly_ValidValue()
    {
        // Arrange - use converter directly
        var converter = new ValidatingJsonConverter<EmailAddress>();
        var options = new JsonSerializerOptions();
        var json = "\"test@example.com\""u8;
        var reader = new Utf8JsonReader(json);
        reader.Read();

        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var result = converter.Read(ref reader, typeof(EmailAddress), options);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be("test@example.com");
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void ReflectionFallback_ConverterDirectly_InvalidValue_CollectsErrors()
    {
        // Arrange - use converter directly
        var converter = new ValidatingJsonConverter<EmailAddress>();
        var options = new JsonSerializerOptions();
        var json = "\"invalid\""u8;
        var reader = new Utf8JsonReader(json);
        reader.Read();

        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        converter.Read(ref reader, typeof(EmailAddress), options);

        // Assert
        ValidationErrorsContext.HasErrors.Should().BeTrue();
        var error = ValidationErrorsContext.GetValidationError();
        error.Should().NotBeNull();
        error!.FieldErrors.Should().HaveCount(1);
    }

    [Fact]
    public void ReflectionFallback_StructConverterDirectly_ValidValue()
    {
        // Arrange - use struct converter directly
        var converter = new ValidatingStructJsonConverter<TestStructValueObject>();
        var options = new JsonSerializerOptions();
        var json = "\"valid-value\""u8;
        var reader = new Utf8JsonReader(json);
        reader.Read();

        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var result = converter.Read(ref reader, typeof(TestStructValueObject), options);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Value.Should().Be("valid-value");
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void ReflectionFallback_StructConverterDirectly_InvalidValue_CollectsErrors()
    {
        // Arrange - use struct converter directly with empty value
        var converter = new ValidatingStructJsonConverter<TestStructValueObject>();
        var options = new JsonSerializerOptions();
        var json = "\"\""u8; // Empty string should fail validation
        var reader = new Utf8JsonReader(json);
        reader.Read();

        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        converter.Read(ref reader, typeof(TestStructValueObject), options);

        // Assert
        ValidationErrorsContext.HasErrors.Should().BeTrue();
        var error = ValidationErrorsContext.GetValidationError();
        error.Should().NotBeNull();
    }

    #endregion
}
