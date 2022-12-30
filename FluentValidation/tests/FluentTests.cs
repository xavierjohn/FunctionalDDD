namespace FluentValidationExt.Tests;

using FunctionalDDD;

public class FluentTests
{
    [Fact]
    public void Can_create_user()
    {
        var rUser = User.Create(
            FirstName.Create("John").Value,
            LastName.Create("Doe").Value,
            EmailAddress.Create("xavier@somewhere.com").Value,
            "password");

        rUser.IsSuccess.Should().BeTrue();
        var user = rUser.Value;
        user.FirstName.Value.Should().Be("John");
        user.LastName.Value.Should().Be("Doe");
        user.Email.Value.Should().Be("xavier@somewhere.com");
        user.Password.Should().Be("password");
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
            Error.Validation("FirstName", "'First Name' must not be empty."),
            Error.Validation("LastName", "'Last Name' must not be empty."),
            Error.Validation("Email", "'Email' must not be empty.")
        };

        // Act
        var rUser = User.Create(firstName, lastName, email, "password");

        // Assert
        rUser.IsFailure.Should().BeTrue();
        rUser.Errors.Should().HaveCount(3);
        rUser.Errors.Should().BeEquivalentTo(expectedValidationErrors);
    }

    [Fact]
    public void Cannot_create_user_with_invalid_parameters_variation()
    {
        // Arrange
        FirstName firstName = default!;
        LastName lastName = default!;
        EmailAddress email = EmailAddress.Create("xavier@somewhere.com").Value;
        var expectedValidationErrors = new[]
        {
            Error.Validation("FirstName", "'First Name' must not be empty."),
            Error.Validation("LastName", "'Last Name' must not be empty."),
            Error.Validation("Password", "'Password' must not be empty.")
        };

        // Act
        var rUser = User.Create(firstName, lastName, email, string.Empty);

        // Assert
        rUser.IsFailure.Should().BeTrue();
        rUser.Errors.Should().HaveCount(3);
        rUser.Errors.Should().BeEquivalentTo(expectedValidationErrors);
    }
}
