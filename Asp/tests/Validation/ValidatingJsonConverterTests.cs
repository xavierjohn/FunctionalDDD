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
    public void CreateConverter_ForEmailAddress_CreatesConverter()
    {
        // Act
        var converter = _factory.CreateConverter(typeof(EmailAddress), new JsonSerializerOptions());

        // Assert
        converter.Should().NotBeNull();
        converter.Should().BeOfType<ValidatingJsonConverter<EmailAddress>>();
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
