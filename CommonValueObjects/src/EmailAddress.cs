namespace FunctionalDDD.CommonValueObjects;

using System.Text.RegularExpressions;
using FunctionalDDD.RailwayOrientedProgramming;

public partial class EmailAddress : SimpleValueObject<string>
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

    [GeneratedRegex("\\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\\Z", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex EmailRegEx();
}
