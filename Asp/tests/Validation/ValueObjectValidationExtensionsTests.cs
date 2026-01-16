namespace Asp.Tests.Validation;

using System.Text.Json;
using FunctionalDdd;
using Microsoft.Extensions.DependencyInjection;
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

    #region ConfigureJsonOptions and ModifyTypeInfo Integration Tests

    [Fact]
    public void AddValueObjectValidation_ConfiguresJsonOptions_TypeInfoModifierApplied()
    {
        // Arrange - This exercises ConfigureJsonOptions and ModifyTypeInfo
        var services = new ServiceCollection();
        services.AddValueObjectValidation();
        var provider = services.BuildServiceProvider();

        // Get the configured Minimal API JSON options
        var jsonOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>();

        // Assert - TypeInfoResolver should be set
        jsonOptions.Value.SerializerOptions.TypeInfoResolver.Should().NotBeNull();
    }

    [Fact]
    public void ModifyTypeInfo_IntegrationTest_ValidatesAndUsesPropertyNames()
    {
        // This test exercises ModifyTypeInfo, ImplementsITryCreatable, and CreateConverterWithReflection
        // by deserializing a DTO with value objects through the full pipeline

        // Arrange
        var services = new ServiceCollection();
        services.AddValueObjectValidation();
        var provider = services.BuildServiceProvider();

        var jsonOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>();
        var options = jsonOptions.Value.SerializerOptions;

        // Valid JSON
        var json = """{"userEmail":"test@example.com","userName":"John"}""";

        using var scope = ValidationErrorsContext.BeginScope();

        // Act - deserialize through the configured pipeline
        var result = JsonSerializer.Deserialize<TestUserDto>(json, options);

        // Assert
        result.Should().NotBeNull();
        result!.UserEmail.Should().NotBeNull();
        result.UserEmail!.Value.Should().Be("test@example.com");
        result.UserName.Should().NotBeNull();
        result.UserName!.Value.Should().Be("John");
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void ModifyTypeInfo_IntegrationTest_InvalidValues_CollectsErrorsWithPropertyNames()
    {
        // This test exercises the full ModifyTypeInfo path with invalid values

        // Arrange
        var services = new ServiceCollection();
        services.AddValueObjectValidation();
        var provider = services.BuildServiceProvider();

        var jsonOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>();
        var options = jsonOptions.Value.SerializerOptions;

        // Invalid email and empty name
        var json = """{"userEmail":"invalid","userName":""}""";

        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        JsonSerializer.Deserialize<TestUserDto>(json, options);

        // Assert
        ValidationErrorsContext.HasErrors.Should().BeTrue();
        var error = ValidationErrorsContext.GetValidationError();
        error.Should().NotBeNull();
        error!.FieldErrors.Should().HaveCount(2);

        // Verify property names are used (not type names)
        error.FieldErrors.Should().Contain(fe => fe.FieldName == "userEmail");
        error.FieldErrors.Should().Contain(fe => fe.FieldName == "userName");
    }

    [Fact]
    public void ModifyTypeInfo_SkipsNonObjectTypes()
    {
        // ModifyTypeInfo should skip non-object types (arrays, primitives, etc.)

        // Arrange
        var services = new ServiceCollection();
        services.AddValueObjectValidation();
        var provider = services.BuildServiceProvider();

        var jsonOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>();
        var options = jsonOptions.Value.SerializerOptions;

        // Array of strings (not an object)
        var json = """["a","b","c"]""";

        // Act - should not throw
        var result = JsonSerializer.Deserialize<string[]>(json, options);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public void ModifyTypeInfo_SkipsPropertiesWithoutITryCreatable()
    {
        // Properties that don't implement ITryCreatable should be skipped

        // Arrange
        var services = new ServiceCollection();
        services.AddValueObjectValidation();
        var provider = services.BuildServiceProvider();

        var jsonOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>();
        var options = jsonOptions.Value.SerializerOptions;

        // DTO with mixed value objects and regular properties
        var json = """{"userEmail":"test@example.com","description":"Hello"}""";

        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var result = JsonSerializer.Deserialize<TestMixedDto>(json, options);

        // Assert
        result.Should().NotBeNull();
        result!.UserEmail!.Value.Should().Be("test@example.com");
        result.Description.Should().Be("Hello");
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void ModifyTypeInfo_NullableValueObject_HandlesCorrectly()
    {
        // Nullable value objects should be handled correctly

        // Arrange
        var services = new ServiceCollection();
        services.AddValueObjectValidation();
        var provider = services.BuildServiceProvider();

        var jsonOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>();
        var options = jsonOptions.Value.SerializerOptions;

        // null value for nullable property
        var json = """{"userEmail":null}""";

        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var result = JsonSerializer.Deserialize<TestNullableDto>(json, options);

        // Assert
        result.Should().NotBeNull();
        result!.UserEmail.Should().BeNull();
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }

    #endregion

    #region PropertyNameAwareConverterFactory Reflection Fallback Tests

    [Fact]
    public void PropertyNameAwareConverterFactory_ReflectionFallback_WhenRegistryEmpty()
    {
        // This test exercises PropertyNameAwareConverterFactory.CreateWithReflection
        // by ensuring the registry is empty when deserializing

        // Arrange - Clear registry to force reflection path
        ValidatingConverterRegistry.Clear();

        var services = new ServiceCollection();
        services.AddValueObjectValidation();
        var provider = services.BuildServiceProvider();

        var jsonOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>();
        var options = jsonOptions.Value.SerializerOptions;

        // Valid JSON
        var json = """{"userEmail":"test@example.com"}""";

        using var scope = ValidationErrorsContext.BeginScope();

        // Act - This should use reflection to create PropertyNameAwareConverter
        var result = JsonSerializer.Deserialize<TestNullableDto>(json, options);

        // Assert
        result.Should().NotBeNull();
        result!.UserEmail.Should().NotBeNull();
        result.UserEmail!.Value.Should().Be("test@example.com");
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void PropertyNameAwareConverterFactory_ReflectionFallback_InvalidValue_CollectsErrors()
    {
        // Test that reflection-created wrappers still collect errors with correct property names

        // Arrange - Clear registry to force reflection path
        ValidatingConverterRegistry.Clear();

        var services = new ServiceCollection();
        services.AddValueObjectValidation();
        var provider = services.BuildServiceProvider();

        var jsonOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>();
        var options = jsonOptions.Value.SerializerOptions;

        // Invalid email
        var json = """{"userEmail":"invalid"}""";

        using var scope = ValidationErrorsContext.BeginScope();

        // Act - This should use reflection path and still use property name
        JsonSerializer.Deserialize<TestNullableDto>(json, options);

        // Assert
        ValidationErrorsContext.HasErrors.Should().BeTrue();
        var error = ValidationErrorsContext.GetValidationError();
        error.Should().NotBeNull();
        error!.FieldErrors.Should().HaveCount(1);
        // Property name should be "userEmail" not "emailAddress"
        error.FieldErrors.Should().Contain(fe => fe.FieldName == "userEmail");
    }

    [Fact]
    public void PropertyNameAwareConverterFactory_ReflectionFallback_StructValueObject()
    {
        // Test reflection fallback for struct value objects

        // Arrange - Clear registry
        ValidatingConverterRegistry.Clear();

        var services = new ServiceCollection();
        services.AddValueObjectValidation();
        var provider = services.BuildServiceProvider();

        var jsonOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>();
        var options = jsonOptions.Value.SerializerOptions;

        // Valid struct value
        var json = """{"structValue":"test-value"}""";

        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var result = JsonSerializer.Deserialize<TestStructDto>(json, options);

        // Assert
        result.Should().NotBeNull();
        result!.StructValue.Should().NotBeNull();
        result.StructValue!.Value.Value.Should().Be("test-value");
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void PropertyNameAwareConverterFactory_ReflectionFallback_Write()
    {
        // Test that Write works via reflection path

        // Arrange - Clear registry
        ValidatingConverterRegistry.Clear();

        var services = new ServiceCollection();
        services.AddValueObjectValidation();
        var provider = services.BuildServiceProvider();

        var jsonOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>();
        var options = jsonOptions.Value.SerializerOptions;

        var email = EmailAddress.TryCreate("test@example.com").Value;
        var dto = new TestNullableDto(email);

        // Act
        var json = JsonSerializer.Serialize(dto, options);

        // Assert
        json.Should().Contain("test@example.com");
    }

    #endregion

    #region Helper Types

    private record TestUserDto(EmailAddress? UserEmail, TestFirstName? UserName);

    private record TestMixedDto(EmailAddress? UserEmail, string? Description);

    private record TestNullableDto(EmailAddress? UserEmail);

    private record TestStructDto(TestStructValueObject? StructValue);

    #endregion
}
