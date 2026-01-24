namespace FunctionalDdd;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

/// <summary>
/// Represents an ISO 4217 currency code as a value object.
/// </summary>
/// <remarks>
/// Valid codes are three alphabetic characters. The stored value is uppercase.
/// </remarks>
[JsonConverter(typeof(ParsableJsonConverter<Currency>))]
public class Currency : ScalarValueObject<Currency, string>, IScalarValueObject<Currency, string>, IParsable<Currency>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Currency"/> class.
    /// </summary>
    /// <param name="value">ISO 4217 currency code.</param>
    private Currency(string value) : base(value) { }

    /// <summary>
    /// Attempts to create a currency from a 3-letter ISO 4217 code.
    /// </summary>
    public static Result<Currency> TryCreate(string? value, string? fieldName = null)
    {
        using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(nameof(Currency) + '.' + nameof(TryCreate));

        var field = !string.IsNullOrEmpty(fieldName)
            ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
            : "currency";

        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Currency>(Error.Validation("Currency is required.", field));

        var code = value.Trim();
        if (code.Length != 3 || !code.All(char.IsLetter))
            return Result.Failure<Currency>(Error.Validation("Currency must be a 3-letter ISO 4217 code.", field));

        var upper = code.ToUpperInvariant();
        return new Currency(upper);
    }

    /// <summary>
    /// Parses a currency code.
    /// </summary>
    public static Currency Parse(string? s, IFormatProvider? provider)
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
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Currency result)
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