namespace FunctionalDdd.PrimitiveValueObjects;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

/// <summary>
/// ISO 639-1 language code value object.
/// </summary>
[JsonConverter(typeof(ParsableJsonConverter<LanguageCode>))]
public class LanguageCode : ScalarValueObject<LanguageCode, string>, IScalarValue<LanguageCode, string>, IParsable<LanguageCode>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageCode"/> class.
    /// </summary>
    private LanguageCode(string value) : base(value) { }

    /// <summary>
    /// Attempts to create a language code.
    /// </summary>
    public static Result<LanguageCode> TryCreate(string? value, string? fieldName = null)
    {
        using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(nameof(LanguageCode) + '.' + nameof(TryCreate));
        var field = !string.IsNullOrEmpty(fieldName)
            ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
            : "languageCode";
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<LanguageCode>(Error.Validation("Language code is required.", field));
        var code = value.Trim();
        if (code.Length != 2 || !code.All(char.IsLetter))
            return Result.Failure<LanguageCode>(Error.Validation("Language code must be an ISO 639-1 alpha-2 code.", field));
        return new LanguageCode(code.ToLowerInvariant());
    }

    /// <summary>
    /// Parses a language code.
    /// </summary>
    public static LanguageCode Parse(string? s, IFormatProvider? provider)
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
    /// Tries to parse a language code.
    /// </summary>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out LanguageCode result)
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