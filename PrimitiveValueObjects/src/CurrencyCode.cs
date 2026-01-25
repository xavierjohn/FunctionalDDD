namespace FunctionalDdd.PrimitiveValueObjects;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

/// <summary>
/// Represents an ISO 4217 currency code as a value object.
/// </summary>
/// <remarks>
/// <b>Validation Rules (Opinionated):</b>
/// <para>
/// Valid codes are three alphabetic characters (e.g., USD, EUR, GBP). The stored value is uppercase.
/// </para>
/// <para>
/// <b>If these rules don't fit your domain</b> (e.g., cryptocurrency codes like BTC, ETH),
/// create your own CurrencyCode value object using the <see cref="ScalarValueObject{TSelf, T}"/> base class.
/// </para>
/// </remarks>
[JsonConverter(typeof(ParsableJsonConverter<CurrencyCode>))]
public class CurrencyCode : ScalarValueObject<CurrencyCode, string>, IScalarValue<CurrencyCode, string>, IParsable<CurrencyCode>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyCode"/> class.
    /// </summary>
    /// <param name="value">ISO 4217 currency code.</param>
    private CurrencyCode(string value) : base(value) { }

    /// <summary>
    /// Attempts to create a currency code from a 3-letter ISO 4217 code.
    /// </summary>
    public static Result<CurrencyCode> TryCreate(string? value, string? fieldName = null)
    {
        using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(nameof(CurrencyCode) + '.' + nameof(TryCreate));

        var field = !string.IsNullOrEmpty(fieldName)
            ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
            : "currencyCode";

        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<CurrencyCode>(Error.Validation("Currency code is required.", field));

        var code = value.Trim();
        if (code.Length != 3 || !code.All(char.IsLetter))
            return Result.Failure<CurrencyCode>(Error.Validation("Currency code must be a 3-letter ISO 4217 code.", field));

        var upper = code.ToUpperInvariant();
        return new CurrencyCode(upper);
    }

    /// <summary>
    /// Parses a currency code.
    /// </summary>
    public static CurrencyCode Parse(string? s, IFormatProvider? provider)
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
    /// Tries to parse a currency code.
    /// </summary>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out CurrencyCode result)
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