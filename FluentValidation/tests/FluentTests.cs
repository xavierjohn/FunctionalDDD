﻿namespace FluentValidationExt.Tests;

using FunctionalDDD;

public class FluentTests
{
    [Fact]
    public void Can_create_user()
    {
        var rUser = User.Create(
            FirstName.Create("John").Ok,
            LastName.Create("Doe").Ok,
            EmailAddress.Create("xavier@somewhere.com").Ok,
            "password");

        rUser.IsOk.Should().BeTrue();
        var user = rUser.Ok;
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
            Error.ValidationError("'First Name' must not be empty.", "FirstName"),
            Error.ValidationError("'Last Name' must not be empty.", "LastName"),
            Error.ValidationError("'Email' must not be empty.", "Email")
        };

        // Act
        var rUser = User.Create(firstName, lastName, email, "password");

        // Assert
        rUser.IsError.Should().BeTrue();
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
        EmailAddress email = EmailAddress.Create("xavier@somewhere.com").Ok;
        var expectedValidationErrors = new[]
        {
            Error.ValidationError("'First Name' must not be empty.", "FirstName"),
            Error.ValidationError("'Last Name' must not be empty.","LastName" ),
            Error.ValidationError("'Password' must not be empty.", "Password")
        };

        // Act
        var rUser = User.Create(firstName, lastName, email, string.Empty);

        // Assert
        rUser.IsError.Should().BeTrue();
        var validationErrors = (ValidationError)rUser.Error;
        validationErrors.Errors.Should().HaveCount(3);
        validationErrors.Errors.Should().BeEquivalentTo(expectedValidationErrors);
    }
}
