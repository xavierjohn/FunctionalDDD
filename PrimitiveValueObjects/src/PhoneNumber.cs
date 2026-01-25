namespace FunctionalDdd.PrimitiveValueObjects;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

/// <summary>
/// Represents a phone number value object with E.164 format validation.
/// Ensures that phone numbers follow international standards for telephony.
/// </summary>
/// <remarks>
/// <para>
/// PhoneNumber is a domain primitive that encapsulates phone number validation and provides:
/// <list type="bullet">
/// <item>E.164 format validation (international phone number standard)</item>
/// <item>Type safety preventing mixing of phone numbers with other strings</item>
/// <item>Immutability ensuring phone numbers cannot be changed after creation</item>
/// <item>IParsable implementation for .NET parsing conventions</item>
/// <item>JSON serialization support for APIs and persistence</item>
/// <item>Activity tracing for monitoring and diagnostics</item>
/// </list>
/// </para>
/// <para>
/// E.164 format rules:
/// <list type="bullet">
/// <item>Starts with a '+' sign</item>
/// <item>Country code (1-3 digits)</item>
/// <item>Subscriber number (up to 12 digits)</item>
/// <item>Maximum total length: 15 digits (excluding the '+' sign)</item>
/// <item>Minimum total length: 8 digits (excluding the '+' sign)</item>
/// </list>
/// </para>
/// <para>
/// Common use cases:
/// <list type="bullet">
/// <item>Contact information in entities</item>
/// <item>SMS notification recipients</item>
/// <item>Two-factor authentication</item>
/// <item>Customer support systems</item>
/// </list>
/// </para>
/// <para>
/// <b>Note: Opinionated Implementation</b> - If you need different phone number formats
/// (e.g., with extensions like +1-415-555-1234 ext. 123), create your own PhoneNumber
/// value object using the <see cref="ScalarValueObject{TSelf, T}"/> base class.
/// </para>
/// </remarks>
/// <example>
/// Basic usage:
/// <code>
/// var phone = PhoneNumber.TryCreate("+14155551234");
/// // Returns: Success(PhoneNumber("+14155551234"))
/// 
/// var invalid = PhoneNumber.TryCreate("555-1234");
/// // Returns: Failure(ValidationError("Phone number must be in E.164 format (e.g., +14155551234)."))
/// </code>
/// </example>
[JsonConverter(typeof(ParsableJsonConverter<PhoneNumber>))]
public partial class PhoneNumber : ScalarValueObject<PhoneNumber, string>, IScalarValueObject<PhoneNumber, string>, IParsable<PhoneNumber>
{
    private PhoneNumber(string value) : base(value) { }

    /// <summary>
    /// Attempts to create a <see cref="PhoneNumber"/> from the specified string.
    /// </summary>
    /// <param name="value">The phone number string to validate.</param>
    /// <param name="fieldName">Optional field name to use in validation error messages.</param>
    /// <returns>
    /// Success with the PhoneNumber if the string is in E.164 format; otherwise Failure with a ValidationError.
    /// </returns>
    public static Result<PhoneNumber> TryCreate(string? value, string? fieldName = null)
    {
        using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(nameof(PhoneNumber) + '.' + nameof(TryCreate));

        var field = !string.IsNullOrEmpty(fieldName)
            ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
            : "phoneNumber";

        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<PhoneNumber>(Error.Validation("Phone number is required.", field));

        // Normalize: remove spaces, dashes, and parentheses for validation
        var normalized = NormalizeRegex().Replace(value.Trim(), "");

        // Validate E.164 format
        if (!E164Regex().IsMatch(normalized))
            return Result.Failure<PhoneNumber>(Error.Validation("Phone number must be in E.164 format (e.g., +14155551234).", field));

        return new PhoneNumber(normalized);
    }

    /// <summary>
    /// Parses the string representation of a phone number to its <see cref="PhoneNumber"/> equivalent.
    /// </summary>
    public static PhoneNumber Parse(string? s, IFormatProvider? provider)
    {
        var r = TryCreate(s);
        if (r.IsFailure)
        {
            var val = (ValidationError)r.Error;
            throw new FormatException(val.FieldErrors[0].Details[0]);
        }

        return r.Value;
    }

    /// <summary>
    /// Tries to parse a string into a <see cref="PhoneNumber"/>.
    /// </summary>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out PhoneNumber result)
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

    /// <summary>
    /// Gets the country code portion of the phone number.
    /// </summary>
    /// <returns>The country code (digits after + and before subscriber number).</returns>
    public string GetCountryCode()
    {
        // E.164 country codes are 1-3 digits
        // This is a simplified extraction; real-world would need a lookup table
        var digits = Value[1..]; // Skip the '+'

        // Try to match known patterns (simplified)
        if (digits.StartsWith('1'))
            return "1"; // NANP (US, Canada, etc.)
        if (digits.Length >= 2 && int.TryParse(digits[..2], out var twoDigit) && twoDigit >= 20 && twoDigit <= 99)
            return digits[..2];
        if (digits.Length >= 3)
            return digits[..3];

        return digits[..1];
    }

    /// <summary>
    /// Compiled regex for E.164 phone number format validation.
    /// Format: +[country code][subscriber number]
    /// Length: 8-15 digits after the +
    /// </summary>
    [GeneratedRegex(@"^\+[1-9]\d{7,14}$")]
    private static partial Regex E164Regex();

    /// <summary>
    /// Regex to remove common formatting characters for normalization.
    /// </summary>
    [GeneratedRegex(@"[\s\-\(\)]")]
    private static partial Regex NormalizeRegex();
}