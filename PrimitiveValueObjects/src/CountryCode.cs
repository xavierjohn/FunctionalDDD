namespace FunctionalDdd.PrimitiveValueObjects;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

/// <summary>
/// ISO 3166-1 alpha-2 country code value object.
/// </summary>
/// <remarks>
/// <b>Validation Rules (Opinionated):</b>
/// <list type="bullet">
/// <item>Exactly 2 letters (ISO 3166-1 alpha-2 format)</item>
/// <item>Normalized to uppercase</item>
/// </list>
/// <para>
/// <b>If these rules don't fit your domain</b> (e.g., you need alpha-3 or numeric codes),
/// create your own CountryCode value object using the <see cref="ScalarValueObject{TSelf, T}"/> base class.
/// </para>
/// </remarks>
[JsonConverter(typeof(ParsableJsonConverter<CountryCode>))]
public class CountryCode : ScalarValueObject<CountryCode, string>, IScalarValue<CountryCode, string>, IParsable<CountryCode>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CountryCode"/> class.
    /// </summary>
    private CountryCode(string value) : base(value) { }

    /// <summary>
    /// Attempts to create a country code.
    /// </summary>
    public static Result<CountryCode> TryCreate(string? value, string? fieldName = null)
    {
        using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(nameof(CountryCode) + '.' + nameof(TryCreate));
        var field = !string.IsNullOrEmpty(fieldName)
            ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
            : "countryCode";
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<CountryCode>(Error.Validation("Country code is required.", field));
        var code = value.Trim();
        if (code.Length != 2 || !code.All(char.IsLetter))
            return Result.Failure<CountryCode>(Error.Validation("Country code must be an ISO 3166-1 alpha-2 code.", field));
        return new CountryCode(code.ToUpperInvariant());
    }

    /// <summary>
    /// Parses a country code.
    /// </summary>
    public static CountryCode Parse(string? s, IFormatProvider? provider)
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
    /// Tries to parse a country code.
    /// </summary>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out CountryCode result)
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
}
