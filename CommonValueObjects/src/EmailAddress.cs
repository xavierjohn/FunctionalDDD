namespace FunctionalDdd;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

/// <summary>
/// Represents an email address value object. It checks for valid email address.
/// </summary>
[JsonConverter(typeof(ParsableJsonConverter<EmailAddress>))]
public partial class EmailAddress : ScalarValueObject<string>, IParsable<EmailAddress>
{
    private EmailAddress(string value) : base(value) { }

    public static Result<EmailAddress> TryCreate(string? emailString, string? fieldName = null)
    {
        if (emailString is not null)
        {
            var isEmail = EmailRegEx().IsMatch(emailString);
            if (isEmail) return new EmailAddress(emailString);
        }

        return Result.Failure<EmailAddress>(Error.Validation("Email address is not valid.", fieldName?.ToCamelCase() ?? "email"));
    }
    public static EmailAddress Parse(string? s, IFormatProvider? provider)
    {
        var r = TryCreate(s);
        if (r.IsFailure)
        {
            var val = (ValidationError)r.Error;
            throw new FormatException(val.Errors[0].Details[0]);
        }

        return r.Value;
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out EmailAddress result)
    {
        var r = TryCreate(s);
        if (r.IsFailure)
        {
            result = default;
            return false;
        }

        result = r.Value;
        return true;
    }

    [GeneratedRegex("\\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\\Z",
        RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegEx();
}
