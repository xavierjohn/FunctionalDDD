namespace PrimitiveValueObjects.Tests;

using FunctionalDdd;
using Xunit;

// Test value objects at namespace level (required for source generator)
public partial class TestStringValue : RequiredString { }
public partial class TestGuidValue : RequiredGuid { }

/// <summary>
/// Tests to verify that all primitive value objects correctly implement the ITryCreatable interface.
/// This ensures consistency across all value objects and enables generic programming patterns like model binders.
/// </summary>
public class ITryCreatableImplementationTests
{

    #region Interface Implementation Tests

    [Fact]
    public void EmailAddress_ImplementsITryCreatable()
    {
        // Arrange & Act
        var isImplemented = typeof(EmailAddress).GetInterfaces()
            .Any(i => i.IsGenericType && 
                     i.GetGenericTypeDefinition() == typeof(ITryCreatable<>) &&
                     i.GetGenericArguments()[0] == typeof(EmailAddress));

        // Assert
        isImplemented.Should().BeTrue("EmailAddress should implement ITryCreatable<EmailAddress>");
    }

    [Fact]
    public void RequiredStringDerived_ImplementsITryCreatable()
    {
        // Arrange & Act
        var isImplemented = typeof(TestStringValue).GetInterfaces()
            .Any(i => i.IsGenericType && 
                     i.GetGenericTypeDefinition() == typeof(ITryCreatable<>) &&
                     i.GetGenericArguments()[0] == typeof(TestStringValue));

        // Assert
        isImplemented.Should().BeTrue("Generated RequiredString derivatives should implement ITryCreatable<T>");
    }

    [Fact]
    public void RequiredGuidDerived_ImplementsITryCreatable()
    {
        // Arrange & Act
        var isImplemented = typeof(TestGuidValue).GetInterfaces()
            .Any(i => i.IsGenericType && 
                     i.GetGenericTypeDefinition() == typeof(ITryCreatable<>) &&
                     i.GetGenericArguments()[0] == typeof(TestGuidValue));

        // Assert
        isImplemented.Should().BeTrue("Generated RequiredGuid derivatives should implement ITryCreatable<T>");
    }

    #endregion

    #region Generic Constraint Tests

    [Fact]
    public void GenericMethod_CanUseITryCreatableConstraint_WithEmailAddress()
    {
        // Arrange
        var input = "test@example.com";

        // Act
        var result = CreateUsingInterface<EmailAddress>(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(input);
    }

    [Fact]
    public void GenericMethod_CanUseITryCreatableConstraint_WithRequiredString()
    {
        // Arrange
        var input = "TestValue";

        // Act
        var result = CreateUsingInterface<TestStringValue>(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(input);
    }

    [Fact]
    public void GenericMethod_CanUseITryCreatableConstraint_WithRequiredGuid()
    {
        // Arrange
        var input = Guid.NewGuid().ToString();

        // Act
        var result = CreateUsingInterface<TestGuidValue>(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    // Generic method that can work with any ITryCreatable type
    private static Result<T> CreateUsingInterface<T>(string? input, string? fieldName = null) 
        where T : ITryCreatable<T> =>
        T.TryCreate(input, fieldName);

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
    public void EmailAddress_TryCreate_WithoutFieldName_UsesDefaultFieldName()
    {
        // Act
        var result = EmailAddress.TryCreate(null);

        // Assert
        result.IsFailure.Should().BeTrue();
        var error = (ValidationError)result.Error;
        error.FieldErrors[0].FieldName.Should().Be("email");
    }

    [Fact]
    public void RequiredString_TryCreate_WithoutFieldName_UsesTypeBasedDefaultFieldName()
    {
        // Act
        var result = TestStringValue.TryCreate(null);

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

    #region Model Binder Simulation Test

    [Fact]
    public void ModelBinderPattern_CanBindAnyITryCreatableType()
    {
        // Arrange
        var emailInput = "user@example.com";
        var stringInput = "TestValue";
        var guidInput = Guid.NewGuid().ToString();

        // Act - Simulate model binding using ITryCreatable interface
        var emailResult = BindModel<EmailAddress>(emailInput, "email");
        var stringResult = BindModel<TestStringValue>(stringInput, "testField");
        var guidResult = BindModel<TestGuidValue>(guidInput, "idField");

        // Assert
        emailResult.IsSuccess.Should().BeTrue();
        stringResult.IsSuccess.Should().BeTrue();
        guidResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ModelBinderPattern_HandlesInvalidInput()
    {
        // Act - Simulate model binding with invalid input
        var result = BindModel<EmailAddress>("not-an-email", "emailField");

        // Assert
        result.IsFailure.Should().BeTrue();
        var error = (ValidationError)result.Error;
        error.FieldErrors[0].FieldName.Should().Be("emailField");
    }

    // Simulates a generic model binder that would work with any ITryCreatable type
    private static Result<T> BindModel<T>(string? input, string fieldName) 
        where T : ITryCreatable<T> =>
        T.TryCreate(input, fieldName);

    #endregion

    #region Integration with Combine Tests

    [Fact]
    public void ITryCreatable_WorksWithCombine()
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
    public void ITryCreatable_CollectsAllFieldErrors_WhenMultipleFail()
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
