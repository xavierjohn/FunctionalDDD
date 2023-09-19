namespace FunctionalDDD.Domain;

using System.Text.RegularExpressions;
using FunctionalDDD.Results;
using FunctionalDDD.Results.Errors;

public partial class EmailAddress : ScalarValueObject<string>
{
    private EmailAddress(string value) : base(value) { }

    public static Result<EmailAddress> New(string emailString, string? fieldName = null)
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
