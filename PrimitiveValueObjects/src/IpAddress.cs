namespace FunctionalDdd.PrimitiveValueObjects;

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;

/// <summary>
/// Represents an IP address (IPv4 or IPv6) as a value object.
/// </summary>
/// <remarks>
/// Validates using <see cref="System.Net.IPAddress.TryParse(string?, out System.Net.IPAddress?)"/>.
/// Provides parsing and JSON serialization support.
/// </remarks>
[JsonConverter(typeof(ParsableJsonConverter<IpAddress>))]
public class IpAddress : ScalarValueObject<IpAddress, string>, IScalarValue<IpAddress, string>, IParsable<IpAddress>
{
    private readonly IPAddress _ip;

    /// <summary>
    /// Initializes a new instance of the <see cref="IpAddress"/> class.
    /// </summary>
    /// <param name="value">The original string representation.</param>
    /// <param name="ip">The parsed <see cref="System.Net.IPAddress"/>.</param>
    private IpAddress(string value, IPAddress ip) : base(value) => _ip = ip;

    /// <summary>
    /// Attempts to create an IP address.
    /// </summary>
    public static Result<IpAddress> TryCreate(string? value, string? fieldName = null)
    {
        using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(nameof(IpAddress) + '.' + nameof(TryCreate));
        var field = !string.IsNullOrEmpty(fieldName)
            ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
            : "ipAddress";
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<IpAddress>(Error.Validation("IP address is required.", field));
        var trimmed = value.Trim();
        if (!IPAddress.TryParse(trimmed, out var ip))
            return Result.Failure<IpAddress>(Error.Validation("IP address must be a valid IPv4 or IPv6.", field));
        return new IpAddress(trimmed, ip);
    }

    /// <summary>
    /// Gets the underlying <see cref="System.Net.IPAddress"/>.
    /// </summary>
    public IPAddress ToIPAddress() => _ip;

    /// <summary>
    /// Parses an IP address.
    /// </summary>
    public static IpAddress Parse(string? s, IFormatProvider? provider)
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
    /// Tries to parse an IP address.
    /// </summary>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out IpAddress result)
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