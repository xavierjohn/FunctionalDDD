namespace Asp.Tests.Validation;

using System.Text.Json;
using FunctionalDdd;
using Xunit;

// Test value objects for use in tests
public partial class TestFirstName : RequiredString { }
public partial class TestLastName : RequiredString { }

/// <summary>
/// Tests for ValidationErrorsContext thread-safe error collection.
/// </summary>
public class ValidationErrorsContextTests
{
    [Fact]
    public void BeginScope_CreatesNewScope()
    {
        // Act
        using var scope = ValidationErrorsContext.BeginScope();

        // Assert
        ValidationErrorsContext.HasErrors.Should().BeFalse();
        ValidationErrorsContext.GetValidationError().Should().BeNull();
    }

    [Fact]
    public void AddError_WithActiveScope_CollectsError()
    {
        // Arrange
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        ValidationErrorsContext.AddError("fieldName", "Error message");

        // Assert
        ValidationErrorsContext.HasErrors.Should().BeTrue();
        var error = ValidationErrorsContext.GetValidationError();
        error.Should().NotBeNull();
        error!.FieldErrors.Should().HaveCount(1);
        error.FieldErrors[0].FieldName.Should().Be("fieldName");
        error.FieldErrors[0].Details.Should().Contain("Error message");
    }

    [Fact]
    public void AddError_WithoutActiveScope_IsNoOp()
    {
        // Act - should not throw
        ValidationErrorsContext.AddError("fieldName", "Error message");

        // Assert
        ValidationErrorsContext.HasErrors.Should().BeFalse();
        ValidationErrorsContext.GetValidationError().Should().BeNull();
    }

    [Fact]
    public void AddError_MultipleErrors_AggregatesAllErrors()
    {
        // Arrange
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        ValidationErrorsContext.AddError("firstName", "First name is required");
        ValidationErrorsContext.AddError("lastName", "Last name is required");
        ValidationErrorsContext.AddError("email", "Email is invalid");

        // Assert
        var error = ValidationErrorsContext.GetValidationError();
        error.Should().NotBeNull();
        error!.FieldErrors.Should().HaveCount(3);
    }

    [Fact]
    public void AddError_SameFieldMultipleTimes_AggregatesDetails()
    {
        // Arrange
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        ValidationErrorsContext.AddError("password", "Password is required");
        ValidationErrorsContext.AddError("password", "Password must be at least 8 characters");

        // Assert
        var error = ValidationErrorsContext.GetValidationError();
        error.Should().NotBeNull();
        error!.FieldErrors.Should().HaveCount(1);
        error.FieldErrors[0].FieldName.Should().Be("password");
        error.FieldErrors[0].Details.Should().HaveCount(2);
        error.FieldErrors[0].Details.Should().Contain("Password is required");
        error.FieldErrors[0].Details.Should().Contain("Password must be at least 8 characters");
    }

    [Fact]
    public void AddError_DuplicateErrorMessages_AreDeduped()
    {
        // Arrange
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        ValidationErrorsContext.AddError("email", "Invalid email");
        ValidationErrorsContext.AddError("email", "Invalid email"); // Duplicate

        // Assert
        var error = ValidationErrorsContext.GetValidationError();
        error!.FieldErrors[0].Details.Should().HaveCount(1);
    }

    [Fact]
    public void AddError_ValidationError_AddsAllFieldErrors()
    {
        // Arrange
        using var scope = ValidationErrorsContext.BeginScope();
        var validationError = ValidationError.For("field1", "Error 1")
            .And("field2", "Error 2");

        // Act
        ValidationErrorsContext.AddError(validationError);

        // Assert
        var error = ValidationErrorsContext.GetValidationError();
        error!.FieldErrors.Should().HaveCount(2);
    }

    [Fact]
    public void NestedScopes_MaintainSeparateErrors()
    {
        // Arrange
        using var outerScope = ValidationErrorsContext.BeginScope();
        ValidationErrorsContext.AddError("outer", "Outer error");

        // Act
        using (var innerScope = ValidationErrorsContext.BeginScope())
        {
            ValidationErrorsContext.AddError("inner", "Inner error");
            
            // Assert inner scope
            var innerError = ValidationErrorsContext.GetValidationError();
            innerError!.FieldErrors.Should().HaveCount(1);
            innerError.FieldErrors[0].FieldName.Should().Be("inner");
        }

        // Assert outer scope after inner disposed
        var outerError = ValidationErrorsContext.GetValidationError();
        outerError!.FieldErrors.Should().HaveCount(1);
        outerError.FieldErrors[0].FieldName.Should().Be("outer");
    }

    [Fact]
    public void Dispose_ClearsScope()
    {
        // Arrange
        var scope = ValidationErrorsContext.BeginScope();
        ValidationErrorsContext.AddError("field", "Error");

        // Act
        scope.Dispose();

        // Assert
        ValidationErrorsContext.HasErrors.Should().BeFalse();
        ValidationErrorsContext.GetValidationError().Should().BeNull();
    }
}

/// <summary>
/// Tests for ValidatingJsonConverter deserialization with validation.
/// </summary>
public class ValidatingJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public ValidatingJsonConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _options.Converters.Add(new ValidatingJsonConverterFactory());
    }

    [Fact]
    public void Deserialize_ValidEmailAddress_ReturnsValue()
    {
        // Arrange
        var json = "\"test@example.com\"";
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var result = JsonSerializer.Deserialize<EmailAddress>(json, _options);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be("test@example.com");
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void Deserialize_InvalidEmailAddress_CollectsError()
    {
        // Arrange
        var json = "\"not-an-email\"";
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var result = JsonSerializer.Deserialize<EmailAddress>(json, _options);

        // Assert
        result.Should().BeNull();
        ValidationErrorsContext.HasErrors.Should().BeTrue();
        var error = ValidationErrorsContext.GetValidationError();
        error!.FieldErrors[0].FieldName.Should().Be("emailAddress");
    }

    [Fact]
    public void Deserialize_NullValue_ReturnsNull()
    {
        // Arrange
        var json = "null";
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var result = JsonSerializer.Deserialize<EmailAddress>(json, _options);

        // Assert
        result.Should().BeNull();
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void Deserialize_EmptyString_CollectsError()
    {
        // Arrange
        var json = "\"\"";
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var result = JsonSerializer.Deserialize<EmailAddress>(json, _options);

        // Assert
        result.Should().BeNull();
        ValidationErrorsContext.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Serialize_ValidValue_WritesString()
    {
        // Arrange
        var email = EmailAddress.TryCreate("test@example.com").Value;

        // Act
        var json = JsonSerializer.Serialize(email, _options);

        // Assert
        json.Should().Be("\"test@example.com\"");
    }

    [Fact]
    public void Serialize_NullValue_WritesNull()
    {
        // Arrange
        EmailAddress? email = null;

        // Act
        var json = JsonSerializer.Serialize(email, _options);

        // Assert
        json.Should().Be("null");
    }
}

/// <summary>
/// Tests for ValidatingJsonConverterFactory type detection.
/// </summary>
public class ValidatingJsonConverterFactoryTests
{
    private readonly ValidatingJsonConverterFactory _factory = new();

    [Fact]
    public void CanConvert_ITryCreatableType_ReturnsTrue() =>
        _factory.CanConvert(typeof(EmailAddress)).Should().BeTrue();

    [Fact]
    public void CanConvert_RequiredStringDerived_ReturnsTrue() =>
        _factory.CanConvert(typeof(TestFirstName)).Should().BeTrue();

    [Fact]
    public void CanConvert_RegularString_ReturnsFalse() =>
        _factory.CanConvert(typeof(string)).Should().BeFalse();

    [Fact]
    public void CanConvert_RegularClass_ReturnsFalse() =>
        _factory.CanConvert(typeof(object)).Should().BeFalse();

    [Fact]
    public void CanConvert_Int_ReturnsFalse() =>
        _factory.CanConvert(typeof(int)).Should().BeFalse();

    [Fact]
    public void CanConvert_NullableInt_ReturnsFalse() =>
        _factory.CanConvert(typeof(int?)).Should().BeFalse();

    [Fact]
    public void CreateConverter_ForEmailAddress_CreatesConverter()
    {
        // Act
        var converter = _factory.CreateConverter(typeof(EmailAddress), new JsonSerializerOptions());

        // Assert
        converter.Should().NotBeNull();
        converter.Should().BeOfType<ValidatingJsonConverter<EmailAddress>>();
    }

    [Fact]
    public void CreateConverter_ForRequiredString_CreatesClassConverter()
    {
        // Act
        var converter = _factory.CreateConverter(typeof(TestFirstName), new JsonSerializerOptions());

        // Assert
        converter.Should().NotBeNull();
        converter.Should().BeOfType<ValidatingJsonConverter<TestFirstName>>();
    }
}

/// <summary>
/// Additional edge case tests for ValidatingJsonConverter.
/// </summary>
public class ValidatingJsonConverterEdgeCaseTests
{
    private static readonly JsonSerializerOptions s_options = CreateOptions();
    
    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new ValidatingJsonConverterFactory());
        return options;
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA1869:Cache and reuse 'JsonSerializerOptions' instances", Justification = "Test requires custom converter configuration")]
    public void Deserialize_WithCustomPropertyName_UsesPropertyNameInError()
    {
        // Arrange - intentionally creating new options with custom converter
        var converter = new ValidatingJsonConverter<EmailAddress>("myEmailField");
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);
        var json = "\"not-valid\"";
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        JsonSerializer.Deserialize<EmailAddress>(json, options);

        // Assert
        var error = ValidationErrorsContext.GetValidationError();
        error!.FieldErrors[0].FieldName.Should().Be("myEmailField");
    }

    [Fact]
    public void Deserialize_NonValidationError_StillCollectsAsValidationError()
    {
        // This test verifies that non-ValidationError errors are converted properly
        // The error collection works for all Error types, not just ValidationError
        var json = "\"\"";
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        JsonSerializer.Deserialize<EmailAddress>(json, s_options);

        // Assert
        ValidationErrorsContext.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Serialize_RoundTrip_PreservesValue()
    {
        // Arrange
        var original = EmailAddress.TryCreate("test@example.com").Value;
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var json = JsonSerializer.Serialize(original, s_options);
        var deserialized = JsonSerializer.Deserialize<EmailAddress>(json, s_options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Value.Should().Be(original.Value);
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void Deserialize_WhitespaceOnlyString_CollectsError()
    {
        // Arrange
        var json = "\"   \"";
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var result = JsonSerializer.Deserialize<TestFirstName>(json, s_options);

        // Assert
        result.Should().BeNull();
        ValidationErrorsContext.HasErrors.Should().BeTrue();
    }
}

/// <summary>
/// Tests for DTO deserialization with multiple value objects.
/// </summary>
public class DtoDeserializationTests
{
    private readonly JsonSerializerOptions _options;

    public DtoDeserializationTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _options.Converters.Add(new ValidatingJsonConverterFactory());
    }

    public record CreateUserDto(TestFirstName? FirstName, TestLastName? LastName, EmailAddress? Email);

    [Fact]
    public void Deserialize_ValidDto_ReturnsAllValues()
    {
        // Arrange
        var json = """
        {
            "firstName": "John",
            "lastName": "Doe",
            "email": "john@example.com"
        }
        """;
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var result = JsonSerializer.Deserialize<CreateUserDto>(json, _options);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName!.Value.Should().Be("John");
        result.LastName!.Value.Should().Be("Doe");
        result.Email!.Value.Should().Be("john@example.com");
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void Deserialize_InvalidEmail_CollectsError()
    {
        // Arrange
        var json = """
        {
            "firstName": "John",
            "lastName": "Doe",
            "email": "not-valid"
        }
        """;
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var result = JsonSerializer.Deserialize<CreateUserDto>(json, _options);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName!.Value.Should().Be("John");
        result.LastName!.Value.Should().Be("Doe");
        result.Email.Should().BeNull(); // Failed validation
        ValidationErrorsContext.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Deserialize_MultipleInvalidFields_CollectsAllErrors()
    {
        // Arrange
        var json = """
        {
            "firstName": "",
            "lastName": "",
            "email": "not-valid"
        }
        """;
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var result = JsonSerializer.Deserialize<CreateUserDto>(json, _options);

        // Assert
        result.Should().NotBeNull();
        ValidationErrorsContext.HasErrors.Should().BeTrue();
        var error = ValidationErrorsContext.GetValidationError();
        error!.FieldErrors.Should().HaveCount(3);
    }

    [Fact]
    public void Deserialize_AllNullFields_NoErrors()
    {
        // Arrange
        var json = """
        {
            "firstName": null,
            "lastName": null,
            "email": null
        }
        """;
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var result = JsonSerializer.Deserialize<CreateUserDto>(json, _options);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().BeNull();
        result.LastName.Should().BeNull();
        result.Email.Should().BeNull();
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }
}

/// <summary>
/// Tests for ValidatingStructJsonConverter - JSON converter for struct value objects.
/// </summary>
public class ValidatingStructJsonConverterTests
{
    private static readonly JsonSerializerOptions s_options = CreateOptions();
    
    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new ValidatingJsonConverterFactory());
        return options;
    }

    #region Read Tests

    [Fact]
    public void Read_ValidValue_ReturnsStruct()
    {
        // Arrange
        var json = "\"test-value\"";
        using var scope = ValidationErrorsContext.BeginScope();

        // Act - use nullable type for deserialization as the converter handles T?
        var result = JsonSerializer.Deserialize<TestStructValueObject?>(json, s_options);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Value.Should().Be("test-value");
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void Read_NullValue_ReturnsNull()
    {
        // Arrange
        var json = "null";
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var result = JsonSerializer.Deserialize<TestStructValueObject?>(json, s_options);

        // Assert
        result.Should().BeNull();
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void Read_InvalidValue_CollectsError()
    {
        // Arrange
        var json = "\"\""; // Empty string fails validation
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var result = JsonSerializer.Deserialize<TestStructValueObject?>(json, s_options);

        // Assert
        result.Should().BeNull();
        ValidationErrorsContext.HasErrors.Should().BeTrue();
        var error = ValidationErrorsContext.GetValidationError();
        error!.FieldErrors.Should().HaveCount(1);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA1869:Cache and reuse 'JsonSerializerOptions' instances", Justification = "Test requires custom converter configuration")]
    public void Read_WithPropertyName_UsesPropertyNameInError()
    {
        // Arrange - intentionally creating new options with custom converter
        var converter = new ValidatingStructJsonConverter<TestStructValueObject>("customField");
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);
        var json = "\"\"";
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        JsonSerializer.Deserialize<TestStructValueObject?>(json, options);

        // Assert
        var error = ValidationErrorsContext.GetValidationError();
        error!.FieldErrors[0].FieldName.Should().Be("customField");
    }

    #endregion

    #region Write Tests

    [Fact]
    public void Write_ValidValue_WritesString()
    {
        // Arrange
        var value = TestStructValueObject.TryCreate("test-value").Value;

        // Act
        var json = JsonSerializer.Serialize<TestStructValueObject?>(value, s_options);

        // Assert
        json.Should().Be("\"test-value\"");
    }

    [Fact]
    public void Write_NullValue_WritesNull()
    {
        // Arrange
        TestStructValueObject? value = null;

        // Act
        var json = JsonSerializer.Serialize(value, s_options);

        // Assert
        json.Should().Be("null");
    }

    #endregion

    #region Factory Integration Tests

    [Fact]
    public void Factory_CanConvert_StructType_ReturnsTrue()
    {
        // Arrange
        var factory = new ValidatingJsonConverterFactory();

        // Act & Assert
        factory.CanConvert(typeof(TestStructValueObject)).Should().BeTrue();
    }

    [Fact]
    public void Factory_CreateConverter_StructType_ReturnsStructConverter()
    {
        // Arrange
        var factory = new ValidatingJsonConverterFactory();

        // Act
        var converter = factory.CreateConverter(typeof(TestStructValueObject), s_options);

        // Assert
        converter.Should().BeOfType<ValidatingStructJsonConverter<TestStructValueObject>>();
    }

    [Fact]
    public void Factory_CanConvert_NullableStructType_ReturnsTrue()
    {
        // Arrange
        var factory = new ValidatingJsonConverterFactory();

        // Act & Assert
        factory.CanConvert(typeof(TestStructValueObject?)).Should().BeTrue();
    }

    #endregion

    #region DTO with Struct Value Objects

    [Fact]
    public void Deserialize_DtoWithStructAndClass_BothValidated()
    {
        // Arrange
        var json = """
        {
            "structValue": "test",
            "email": "test@example.com"
        }
        """;
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var result = JsonSerializer.Deserialize<DtoWithStructs>(json, s_options);

        // Assert
        result.Should().NotBeNull();
        result!.StructValue!.Value.Value.Should().Be("test");
        result.Email!.Value.Should().Be("test@example.com");
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void Deserialize_DtoWithInvalidStruct_CollectsError()
    {
        // Arrange
        var json = """
        {
            "structValue": "",
            "email": "test@example.com"
        }
        """;
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var result = JsonSerializer.Deserialize<DtoWithStructs>(json, s_options);

        // Assert
        result.Should().NotBeNull();
        result!.StructValue.Should().BeNull();
        result.Email!.Value.Should().Be("test@example.com");
        ValidationErrorsContext.HasErrors.Should().BeTrue();
    }

    #endregion
}

/// <summary>
/// DTO for testing struct and class value objects together.
/// </summary>
public record DtoWithStructs(TestStructValueObject? StructValue, EmailAddress? Email);
