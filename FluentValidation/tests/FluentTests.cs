namespace FluentValidationExt.Tests;

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
        var expectedValidationErrors = new[]
        {
            Error.ValidationError("'First Name' must not be empty.", "FirstName"),
            Error.ValidationError("'Last Name' must not be empty.", "LastName"),
            Error.ValidationError("'Email' must not be empty.", "Email")
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
        var expectedValidationErrors = new[]
        {
            Error.ValidationError("'First Name' must not be empty.", "FirstName"),
            Error.ValidationError("'Last Name' must not be empty.","LastName" ),
            Error.ValidationError("'Password' must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit and one special character.", "Password")
        };

        // Act
        var rUser = User.TryCreate(firstName, lastName, email, "WeakPassword");

        // Assert
        rUser.IsFailure.Should().BeTrue();
        var validationErrors = (ValidationError)rUser.Error;
        validationErrors.Errors.Should().HaveCount(3);
        validationErrors.Errors.Should().BeEquivalentTo(expectedValidationErrors);
    }
}
