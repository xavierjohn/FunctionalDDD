namespace FluentValidationExt.Tests;

using FluentValidation;
using Xunit;

public class FluentTests
{
    private const string StrongPassword = "P@ssw0rd";

    [Fact]
    public void Can_create_user()
    {
        var rUser = User.TryCreate(
            FirstName.TryCreate("John").Value,
            LastName.TryCreate("Doe").Value,
            EmailAddress.TryCreate("xavier@somewhere.com").Value,
            StrongPassword);

        rUser.IsSuccess.Should().BeTrue();
        var user = rUser.Value;
        user.FirstName.Value.Should().Be("John");
        user.LastName.Value.Should().Be("Doe");
        user.Email.Value.Should().Be("xavier@somewhere.com");
        user.Password.Should().Be(StrongPassword);
    }

    [Fact]
    public void Cannot_create_user_with_invalid_parameters()
    {
        // Arrange
        FirstName firstName = default!;
        LastName lastName = default!;
        EmailAddress email = default!;
        var expectedValidationErrors = new ValidationError.FieldDetails[]
        {
            new("FirstName", ["'First Name' must not be empty."]),
            new("LastName", ["'Last Name' must not be empty."]),
            new("Email", ["'Email' must not be empty."])
        };

        // Act
        var rUser = User.TryCreate(firstName, lastName, email, StrongPassword);

        // Assert
        rUser.IsFailure.Should().BeTrue();
        var validationErrors = (ValidationError)rUser.Error;
        validationErrors.Errors.Should().HaveCount(3);
        validationErrors.Errors.Should().BeEquivalentTo(expectedValidationErrors);
    }

    [Fact]
    public void Cannot_create_user_with_invalid_parameters_variation()
    {
        // Arrange
        FirstName firstName = default!;
        LastName lastName = default!;
        EmailAddress email = EmailAddress.TryCreate("xavier@somewhere.com").Value;
        var expectedValidationErrors = new ValidationError.FieldDetails[]
        {
            new("FirstName", ["'First Name' must not be empty."]),
            new("LastName", ["'Last Name' must not be empty."] ),
            new("Password", ["Password must contain at least one number.", "Password must contain at least one special character." ])
        };

        // Act
        var rUser = User.TryCreate(firstName, lastName, email, "WeakPassword");

        // Assert
        rUser.IsFailure.Should().BeTrue();
        var validationErrors = (ValidationError)rUser.Error;
        validationErrors.Errors.Should().HaveCount(3);
        validationErrors.Errors.Should().BeEquivalentTo(expectedValidationErrors);
    }

    [Theory]
    [InlineData("98052", true, null)]
    [InlineData("98052-1234", true, null)]
    [InlineData("98052-12345", false, "'zip Code' is not in the correct format.")]
    [InlineData("98052-123", false, "'zip Code' is not in the correct format.")]
    [InlineData("98052-1234-1234", false, "'zip Code' is not in the correct format.")]
    [InlineData(null, false, "'zipCode' must not be empty.")]
    public void Validate_zipcode(string? strZip, bool success, string? errorMessage)
    {
        // Arrange
        // Act
        var result = ZipCode.TryCreate(strZip);

        // Assert
        result.IsSuccess.Should().Be(success);
        if (success)
        {
            result.Value.Value.Should().Be(strZip);
        }
        else
        {
            result.Error.Should().BeOfType<ValidationError>();
            var validationError = (ValidationError)result.Error;
            validationError.Errors.Should().HaveCount(1);
            validationError.Errors[0].FieldName.Should().Be("zipCode");
            validationError.Errors[0].Details[0].Should().Be(errorMessage);
        }

    }

    [Fact]
    public void Validate_null_value()
    {
        // Arrange
        string? alias = null;
       InlineValidator<string?> validator = new()
       {
          v => v.RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
       };

        ValidationError.FieldDetails[] expectedValidationErrors = [
            new("alias", ["'alias' must not be empty."]),
        ];

        // Act
        var result = validator.ValidateToResult(alias);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
        ValidationError error = (ValidationError)result.Error;
        error.Errors.Should().BeEquivalentTo(expectedValidationErrors);
    }

    [Fact]
    public void Validate_null_value_custom_message()
    {
        // Arrange
        string? alias = null;
        InlineValidator<string?> validator = new()
       {
          v => v.RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
       };

        ValidationError.FieldDetails[] expectedValidationErrors = [
            new("Alias", ["Hello There"]),
        ];

        // Act
        var result = validator.ValidateToResult(alias, "Alias", "Hello There");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
        ValidationError error = (ValidationError)result.Error;
        error.Errors.Should().BeEquivalentTo(expectedValidationErrors);
    }
}
