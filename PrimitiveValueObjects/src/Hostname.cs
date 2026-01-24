namespace FunctionalDdd;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

/// <summary>
/// RFC 1123 compliant hostname value object.
/// </summary>
[JsonConverter(typeof(ParsableJsonConverter<Hostname>))]
public partial class Hostname : ScalarValueObject<Hostname, string>, IScalarValueObject<Hostname, string>, IParsable<Hostname>
{
    private Hostname(string value) : base(value) { }

    /// <summary>
    /// Attempts to create a hostname.
    /// </summary>
    public static Result<Hostname> TryCreate(string? value, string? fieldName = null)
    {
        using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(nameof(Hostname) + '.' + nameof(TryCreate));
        var field = !string.IsNullOrEmpty(fieldName)
            ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
            : "hostname";
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Hostname>(Error.Validation("Hostname is required.", field));
        var trimmed = value.Trim();
        if (!HostnameRegex().IsMatch(trimmed))
            return Result.Failure<Hostname>(Error.Validation("Hostname must be RFC 1123 compliant.", field));
        return new Hostname(trimmed);
    }

    /// <summary>
    /// Parses a hostname.
    /// </summary>
    public static Hostname Parse(string? s, IFormatProvider? provider)
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
    /// Tries to parse a hostname.
    /// </summary>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Hostname result)
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

    // RFC 1123 hostname: labels 1-63 chars, alphanum and hyphens, no leading/trailing hyphen, total <=255
    [GeneratedRegex(@"^(?=.{1,255}$)([a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$")]
    private static partial Regex HostnameRegex();
}