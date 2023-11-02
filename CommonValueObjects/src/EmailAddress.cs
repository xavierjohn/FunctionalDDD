namespace FunctionalDdd;

using System.Text.RegularExpressions;

/// <summary>
/// Represents an email address value object. It checks for valid email address.
/// </summary>
public partial class EmailAddress : ScalarValueObject<string>
{
    private EmailAddress(string value) : base(value) { }

    public static Result<EmailAddress> TryCreate(string emailString, string? fieldName = null)
    {
        if (emailString is not null)
        {
            var isEmail = EmailRegEx().IsMatch(emailString);
            if (isEmail) return new EmailAddress(emailString);
        }

        return Result.Failure<EmailAddress>(Error.Validation("Email address is not valid.", fieldName?.ToCamelCase() ?? "email"));
    }

    [GeneratedRegex("\\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\\Z",
        RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegEx();
}
