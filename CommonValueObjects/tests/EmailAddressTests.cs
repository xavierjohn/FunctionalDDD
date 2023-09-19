namespace CommonValueObjects.Tests;

using FunctionalDDD.CommonValueObjects;
using FunctionalDDD.Results.Errors;

public class EmailAddressTests
{
    [Theory]
    [InlineData("xavier")]
    [InlineData("xavier@")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("@com")]
    [InlineData("@#@@##@%^%#$@#$@#.com")]
    [InlineData("John Doe <example@email.com>")]
    [InlineData("CAT…123@email.com")]
    public void Cannot_create_invalid_email(string email)
    {
        // Arrange
        var result = EmailAddress.New(email, "school email");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Validation("Email address is not valid", "school email"));
    }

    [Theory]
    [InlineData("xavier@somewhere.com")]
    [InlineData("0987654321@example.com")]
    [InlineData("_______@email.com")]
    public void Can_create_valid_email(string email)
    {
        // Arrange
        var result = EmailAddress.New(email, "school email");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<EmailAddress>();
    }
}
