namespace PrimitiveValueObjects.Tests;

using FunctionalDdd;
using Xunit;

// Test value objects at namespace level (required for source generator)
public partial class TestStringValue : RequiredString<TestStringValue> { }
public partial class TestGuidValue : RequiredGuid<TestGuidValue> { }

/// <summary>
/// Tests to verify that primitive value objects correctly implement TryCreate factory methods
/// and support generic programming patterns like model binders.
/// </summary>
public class TryCreateImplementationTests
{
    #region TryCreate Method Tests

    [Fact]
    public void EmailAddress_TryCreate_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        var input = "test@example.com";

        // Act
        var result = EmailAddress.TryCreate(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(input);
    }

    [Fact]
    public void RequiredString_TryCreate_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        var input = "TestValue";

        // Act
        var result = TestStringValue.TryCreate(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(input);
    }

    [Fact]
    public void RequiredGuid_TryCreate_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        var input = Guid.NewGuid();

        // Act
        var result = TestGuidValue.TryCreate(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(input);
    }

    #endregion

    #region FieldName Parameter Tests

    [Fact]
    public void EmailAddress_TryCreate_WithFieldName_UsesCustomFieldName()
    {
        // Arrange
        var customFieldName = "UserEmail";

        // Act
        var result = EmailAddress.TryCreate(null, customFieldName);

        // Assert
        result.IsFailure.Should().BeTrue();
        var error = (ValidationError)result.Error;
        error.FieldErrors[0].FieldName.Should().Be("userEmail"); // Should be camelCased
    }

    [Fact]
    public void EmailAddress_TryCreate_WithCustomFieldName_UsesProvidedFieldNameNotDefault()
    {
        // Arrange
        var customFieldName = "ContactEmailAddress";

        // Act
        var result = EmailAddress.TryCreate("invalid-email", customFieldName);

        // Assert
        result.IsFailure.Should().BeTrue();
        var error = (ValidationError)result.Error;
        error.FieldErrors[0].FieldName.Should().Be("contactEmailAddress"); // Uses provided name, not "email"
        error.FieldErrors[0].FieldName.Should().NotBe("email"); // Explicitly verify it's not the default
    }

    [Fact]
    public void RequiredString_TryCreate_WithFieldName_UsesCustomFieldName()
    {
        // Arrange
        var customFieldName = "CustomField";

        // Act
        var result = TestStringValue.TryCreate(null, customFieldName);

        // Assert
        result.IsFailure.Should().BeTrue();
        var error = (ValidationError)result.Error;
        error.FieldErrors[0].FieldName.Should().Be("customField"); // Should be camelCased
    }

    [Fact]
    public void RequiredString_TryCreate_WithCustomFieldName_UsesProvidedFieldNameNotDefault()
    {
        // Arrange
        var customFieldName = "ProductDescription";

        // Act
        var result = TestStringValue.TryCreate("", customFieldName);

        // Assert
        result.IsFailure.Should().BeTrue();
        var error = (ValidationError)result.Error;
        error.FieldErrors[0].FieldName.Should().Be("productDescription"); // Uses provided name, not "testStringValue"
        error.FieldErrors[0].FieldName.Should().NotBe("testStringValue"); // Explicitly verify it's not the default
    }

    [Fact]
    public void RequiredGuid_TryCreate_WithFieldName_UsesCustomFieldName()
    {
        // Arrange
        var customFieldName = "EntityId";

        // Act
        var result = TestGuidValue.TryCreate(Guid.Empty, customFieldName);

        // Assert
        result.IsFailure.Should().BeTrue();
        var error = (ValidationError)result.Error;
        error.FieldErrors[0].FieldName.Should().Be("entityId"); // Should be camelCased
    }

    [Fact]
    public void RequiredGuid_TryCreate_WithCustomFieldName_UsesProvidedFieldNameNotDefault()
    {
        // Arrange
        var customFieldName = "OrderIdentifier";

        // Act
        var result = TestGuidValue.TryCreate(Guid.Empty, customFieldName);

        // Assert
        result.IsFailure.Should().BeTrue();
        var error = (ValidationError)result.Error;
        error.FieldErrors[0].FieldName.Should().Be("orderIdentifier"); // Uses provided name, not "testGuidValue"
        error.FieldErrors[0].FieldName.Should().NotBe("testGuidValue"); // Explicitly verify it's not the default
    }

    [Fact]
    public void EmailAddress_TryCreate_WithoutFieldName_UsesDefaultFieldName()
    {
        // Act
        var result = EmailAddress.TryCreate(null, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        var error = (ValidationError)result.Error;
        error.FieldErrors[0].FieldName.Should().Be("email");
    }

    [Fact]
    public void RequiredString_TryCreate_WithoutFieldName_UsesTypeBasedDefaultFieldName()
    {
        // Act
        var result = TestStringValue.TryCreate(null, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        var error = (ValidationError)result.Error;
        error.FieldErrors[0].FieldName.Should().Be("testStringValue");
    }

    [Fact]
    public void RequiredGuid_TryCreate_WithoutFieldName_UsesTypeBasedDefaultFieldName()
    {
        // Act
        var result = TestGuidValue.TryCreate(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        var error = (ValidationError)result.Error;
        error.FieldErrors[0].FieldName.Should().Be("testGuidValue");
    }

    [Fact]
    public void FieldName_CamelCaseConversion_HandlesEdgeCases()
    {
        // Arrange
        var singleChar = "X";
        var empty = "";

        // Act
        var result1 = TestStringValue.TryCreate(null, singleChar);
        var result2 = TestStringValue.TryCreate(null, empty);

        // Assert
        var error1 = (ValidationError)result1.Error;
        error1.FieldErrors[0].FieldName.Should().Be("x"); // Single char should lowercase

        var error2 = (ValidationError)result2.Error;
        error2.FieldErrors[0].FieldName.Should().Be("testStringValue"); // Empty should use default
    }

    #endregion

    #region Integration with Combine Tests

    [Fact]
    public void TryCreate_WorksWithCombine()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";

        // Act
        var result = EmailAddress.TryCreate(email, "email")
            .Combine(TestStringValue.TryCreate(firstName, "firstName"));

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void TryCreate_CollectsAllFieldErrors_WhenMultipleFail()
    {
        // Act
        var result = EmailAddress.TryCreate("invalid", "email")
            .Combine(TestStringValue.TryCreate("", "firstName"))
            .Combine(TestGuidValue.TryCreate(Guid.Empty, "userId"));

        // Assert
        result.IsFailure.Should().BeTrue();
        var error = (ValidationError)result.Error;
        error.FieldErrors.Should().HaveCount(3); // Combine creates separate FieldErrors
        error.FieldErrors.Should().Contain(e => e.FieldName == "email" && e.Details.Contains("Email address is not valid."));
        error.FieldErrors.Should().Contain(e => e.FieldName == "firstName" && e.Details.Contains("Test String Value cannot be empty."));
        error.FieldErrors.Should().Contain(e => e.FieldName == "userId" && e.Details.Contains("Test Guid Value cannot be empty."));
    }

    #endregion
}
