namespace CommonValueObjects.Tests;

using FunctionalDdd;
using System.Globalization;
using System.Text.Json;
using Xunit;

public class EmailAddressTests
{
    [Theory]
    [MemberData(nameof(GetBadEmailAddresses))]
    public void Cannot_create_invalid_email(string? email)
    {
        // Arrange & Act
        var result = EmailAddress.TryCreate(email, "school email");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Validation("Email address is not valid", "school email"));
    }

    [Theory]
    [MemberData(nameof(GetGoodEmailAddresses))]
    public void Can_create_valid_email(string? email)
    {
        // Arrange
        var result = EmailAddress.TryCreate(email, "school email");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<EmailAddress>();
    }

    [Theory]
    [MemberData(nameof(GetGoodEmailAddresses))]
    public void Can_create_EmailAddress_try_parsing_valid_string(string? strEmail)
    {
        // Arrange & Act
        EmailAddress.TryParse(strEmail, null, out var email)

        // Assert
        .Should().BeTrue();
        email.Should().BeOfType<EmailAddress>();
        email!.ToString(CultureInfo.InvariantCulture).Should().Be(strEmail);
    }

    [Theory]
    [MemberData(nameof(GetBadEmailAddresses))]
    public void Cannot_create_EmailAddress_try_parsing_invalid_string(string? strEmail)
    {
        // Arrange & Act
        EmailAddress.TryParse(strEmail, null, out var email)

        // Assert
        .Should().BeFalse();
        email.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(GetGoodEmailAddresses))]
    public void Can_create_EmailAddress_parsing_valid_string(string? strEmail)
    {
        // Arrange & Act
        var email = EmailAddress.Parse(strEmail, null);

        // Assert
        email.Should().BeOfType<EmailAddress>();
        email.ToString(CultureInfo.InvariantCulture).Should().Be(strEmail);
    }

    [Fact]
    public void Cannot_create_EmailAddress_parsing_null_string()
    {
        // Arrange
        string? str = null;
        // Act
        Action act = () => EmailAddress.Parse(str, null);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Email address is not valid.");
    }

    [Theory]
    [MemberData(nameof(GetBadEmailAddresses))]
    public void Cannot_create_EmailAddress_parsing_invalid_string(string? email)
    {
        // Arrange & Act
        Action act = () => EmailAddress.Parse(email, null);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Email address is not valid.");
    }

    [Fact]
    public void ConvertToJson()
    {
        // Arrange
        EmailAddress email = EmailAddress.TryCreate("chris@somewhere.com").Value;
        string primEmail = "chris@somewhere.com";

        var expected = JsonSerializer.Serialize(primEmail);

        // Act
        var actual = JsonSerializer.Serialize(email);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void ConvertFromJson()
    {
        // Arrange
        string primEmail = "chris@somewhere.com";

        var json = JsonSerializer.Serialize(primEmail);

        // Act
        EmailAddress actual = JsonSerializer.Deserialize<EmailAddress>(json)!;

        // Assert
        actual.Value.Should().Be(primEmail);
    }

    [Theory]
    [MemberData(nameof(GetBadEmailAddresses))]
    public void Cannot_create_EmailAddress_parsing_invalid_string_from_JSON(string? email)
    {
        // Arrange
        var json = JsonSerializer.Serialize(email);

        // Act
        Action act = () => JsonSerializer.Deserialize<EmailAddress>(json);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Email address is not valid.");
    }

    public static TheoryData<string?> GetGoodEmailAddresses() =>
    [
        "xavier@somewhere.com",
        "0987654321@example.com",
        "_______@email.com"
    ];

    public static TheoryData<string?> GetBadEmailAddresses() =>
    [
        string.Empty,
        "xavier",
        "xavier@",
        "@com",
        "@#@@##@%^%#$@#$@#.com",
        "John Doe <example@email.com>",
        "CAT…123@email.com"
    ];
}
