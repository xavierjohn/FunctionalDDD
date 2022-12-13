namespace FunctionalDDD.CommonValueObjects;

using System.Text.RegularExpressions;
using FunctionalDDD;

public partial class EmailAddress : SimpleValueObject<string>
{
    private EmailAddress(string value) : base(value) { }

    public static Result<EmailAddress> Create(string emailString, string? fieldName = null)
    {
        var isEmail = EmailRegEx().IsMatch(emailString);
        if (isEmail) return new EmailAddress(emailString);

        return Result.Failure<EmailAddress>(Error.Validation(fieldName ?? "Email", "Email address is not valid"));
    }

    [GeneratedRegex("\\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\\Z", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex EmailRegEx();
}
