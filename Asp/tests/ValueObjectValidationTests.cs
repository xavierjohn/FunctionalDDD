namespace Asp.Tests;

using System.Text.Json;
using FunctionalDdd;
using FunctionalDdd.Asp.Validation;
using Xunit;

/// <summary>
/// Tests for value object validation during JSON deserialization.
/// Verifies that validation errors are correctly attributed to property names,
/// not type names, especially when the same value object type is used for multiple properties.
/// </summary>
public class ValueObjectValidationTests
{
    #region Test Value Objects

    /// <summary>
    /// A generic name value object used for testing.
    /// Can be used for multiple properties (FirstName, LastName) to verify field name attribution.
    /// </summary>
    public class Name : ScalarValueObject<Name, string>, IScalarValueObject<Name, string>
    {
        private Name(string value) : base(value) { }

        public static Result<Name> TryCreate(string? value, string? fieldName = null)
        {
            var field = fieldName ?? "name";
            if (string.IsNullOrWhiteSpace(value))
                return Error.Validation("Name cannot be empty.", field);
            return new Name(value.Trim());
        }
    }

    /// <summary>
    /// Test email value object.
    /// </summary>
    public class TestEmail : ScalarValueObject<TestEmail, string>, IScalarValueObject<TestEmail, string>
    {
        private TestEmail(string value) : base(value) { }

        public static Result<TestEmail> TryCreate(string? value, string? fieldName = null)
        {
            var field = fieldName ?? "email";
            if (string.IsNullOrWhiteSpace(value))
                return Error.Validation("Email is required.", field);
            if (!value.Contains('@'))
                return Error.Validation("Email must contain @.", field);
            return new TestEmail(value);
        }
    }

    /// <summary>
    /// DTO using the same Name type for multiple properties.
    /// </summary>
    public class PersonDto
    {
        public Name? FirstName { get; set; }
        public Name? LastName { get; set; }
        public TestEmail? Email { get; set; }
    }

    #endregion

    #region Validation Error Context Tests

    [Fact]
    public void ValidationErrorsContext_CollectsErrors_WhenScopeIsActive()
    {
        // Arrange & Act
        using (ValidationErrorsContext.BeginScope())
        {
            ValidationErrorsContext.AddError("field1", "Error 1");
            ValidationErrorsContext.AddError("field2", "Error 2");

            var error = ValidationErrorsContext.GetValidationError();

            // Assert
            error.Should().NotBeNull();
            error!.FieldErrors.Should().HaveCount(2);
            error.FieldErrors[0].FieldName.Should().Be("field1");
            error.FieldErrors[0].Details[0].Should().Be("Error 1");
            error.FieldErrors[1].FieldName.Should().Be("field2");
            error.FieldErrors[1].Details[0].Should().Be("Error 2");
        }
    }

    [Fact]
    public void ValidationErrorsContext_ReturnsNull_WhenNoScopeIsActive()
    {
        // Act
        var error = ValidationErrorsContext.GetValidationError();

        // Assert
        error.Should().BeNull();
    }

    [Fact]
    public void ValidationErrorsContext_ClearsErrors_WhenScopeIsDisposed()
    {
        // Arrange
        using (ValidationErrorsContext.BeginScope())
        {
            ValidationErrorsContext.AddError("field", "Error");
        }

        // Act
        var error = ValidationErrorsContext.GetValidationError();

        // Assert
        error.Should().BeNull();
    }

    [Fact]
    public void ValidationErrorsContext_TracksCurrentPropertyName()
    {
        // Arrange & Act
        ValidationErrorsContext.CurrentPropertyName = "TestProperty";
        var propertyName = ValidationErrorsContext.CurrentPropertyName;

        // Assert
        propertyName.Should().Be("TestProperty");

        // Cleanup
        ValidationErrorsContext.CurrentPropertyName = null;
    }

    #endregion

    #region JSON Converter Factory Tests

    [Fact]
    public void ValidatingJsonConverterFactory_CanConvert_IScalarValueObject()
    {
        // Arrange
        var factory = new ValidatingJsonConverterFactory();

        // Act & Assert
        factory.CanConvert(typeof(Name)).Should().BeTrue();
        factory.CanConvert(typeof(TestEmail)).Should().BeTrue();
    }

    [Fact]
    public void ValidatingJsonConverterFactory_CannotConvert_NonValueObjectTypes()
    {
        // Arrange
        var factory = new ValidatingJsonConverterFactory();

        // Act & Assert
        factory.CanConvert(typeof(string)).Should().BeFalse();
        factory.CanConvert(typeof(int)).Should().BeFalse();
        factory.CanConvert(typeof(PersonDto)).Should().BeFalse();
    }

    #endregion

    #region JSON Deserialization Tests

    [Fact]
    public void Deserialize_ValidData_ReturnsValueObject()
    {
        // Arrange
        var options = CreateJsonOptions();
        var json = "\"John\"";

        using (ValidationErrorsContext.BeginScope())
        {
            ValidationErrorsContext.CurrentPropertyName = "FirstName";

            // Act
            var result = JsonSerializer.Deserialize<Name>(json, options);

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be("John");
            ValidationErrorsContext.GetValidationError().Should().BeNull();
        }
    }

    [Fact]
    public void Deserialize_InvalidData_CollectsValidationError()
    {
        // Arrange
        var options = CreateJsonOptions();
        var json = "\"\""; // Empty string - invalid for Name

        using (ValidationErrorsContext.BeginScope())
        {
            ValidationErrorsContext.CurrentPropertyName = "FirstName";

            // Act
            var result = JsonSerializer.Deserialize<Name>(json, options);

            // Assert
            result.Should().BeNull();
            var error = ValidationErrorsContext.GetValidationError();
            error.Should().NotBeNull();
            error!.FieldErrors.Should().HaveCount(1);
            error.FieldErrors[0].FieldName.Should().Be("FirstName");
            error.FieldErrors[0].Details[0].Should().Be("Name cannot be empty.");
        }
    }

    [Fact]
    public void Deserialize_SameTypeForMultipleProperties_UsesCorrectFieldNames()
    {
        // Arrange
        var options = CreateJsonOptions();

        using (ValidationErrorsContext.BeginScope())
        {
            // Simulate deserializing FirstName (invalid)
            ValidationErrorsContext.CurrentPropertyName = "FirstName";
            JsonSerializer.Deserialize<Name>("\"\"", options);

            // Simulate deserializing LastName (invalid)
            ValidationErrorsContext.CurrentPropertyName = "LastName";
            JsonSerializer.Deserialize<Name>("\"\"", options);

            // Act
            var error = ValidationErrorsContext.GetValidationError();

            // Assert
            error.Should().NotBeNull();
            error!.FieldErrors.Should().HaveCount(2);

            // Both errors should have their respective property names, not "name"
            error.FieldErrors.Should().Contain(e => e.FieldName == "FirstName");
            error.FieldErrors.Should().Contain(e => e.FieldName == "LastName");

            // Neither should have the default type-based name
            error.FieldErrors.Should().NotContain(e => e.FieldName == "name");
        }
    }

    [Fact]
    public void Deserialize_MultipleInvalidFields_CollectsAllErrors()
    {
        // Arrange
        var options = CreateJsonOptions();

        using (ValidationErrorsContext.BeginScope())
        {
            // Invalid FirstName
            ValidationErrorsContext.CurrentPropertyName = "FirstName";
            JsonSerializer.Deserialize<Name>("\"\"", options);

            // Invalid LastName
            ValidationErrorsContext.CurrentPropertyName = "LastName";
            JsonSerializer.Deserialize<Name>("\"\"", options);

            // Invalid Email
            ValidationErrorsContext.CurrentPropertyName = "Email";
            JsonSerializer.Deserialize<TestEmail>("\"not-an-email\"", options);

            // Act
            var error = ValidationErrorsContext.GetValidationError();

            // Assert
            error.Should().NotBeNull();
            error!.FieldErrors.Should().HaveCount(3);
            error.FieldErrors.Should().Contain(e => e.FieldName == "FirstName" && e.Details[0] == "Name cannot be empty.");
            error.FieldErrors.Should().Contain(e => e.FieldName == "LastName" && e.Details[0] == "Name cannot be empty.");
            error.FieldErrors.Should().Contain(e => e.FieldName == "Email" && e.Details[0] == "Email must contain @.");
        }
    }

    [Fact]
    public void Deserialize_NullJson_ReturnsNull()
    {
        // Arrange
        var options = CreateJsonOptions();
        var json = "null";

        using (ValidationErrorsContext.BeginScope())
        {
            ValidationErrorsContext.CurrentPropertyName = "FirstName";

            // Act
            var result = JsonSerializer.Deserialize<Name>(json, options);

            // Assert
            result.Should().BeNull();
            // Null values don't add validation errors - the required validation happens at model level
        }
    }

    #endregion

    #region Property Name Aware Converter Tests

    [Fact]
    public void PropertyNameAwareConverter_SetsAndRestoresPropertyName()
    {
        // Arrange
        var options = CreateJsonOptions();
        var json = "\"test@example.com\"";

        using (ValidationErrorsContext.BeginScope())
        {
            // Set an outer property name (simulating nested object)
            ValidationErrorsContext.CurrentPropertyName = "OuterProperty";

            // Create a property-aware converter
            var innerConverter = new ValidatingJsonConverter<TestEmail, string>();
            var propertyAwareConverter = new PropertyNameAwareConverter<TestEmail>(innerConverter, "InnerEmail");

            // Act
            var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
            reader.Read(); // Move to first token
            var result = propertyAwareConverter.Read(ref reader, typeof(TestEmail), options);

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be("test@example.com");
            // Property name should be restored to outer value
            ValidationErrorsContext.CurrentPropertyName.Should().Be("OuterProperty");
        }
    }

    #endregion

    #region Helper Methods

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ValidatingJsonConverterFactory());
        return options;
    }

    #endregion
}
