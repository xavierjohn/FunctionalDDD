namespace FunctionalDdd.PrimitiveValueObjects;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

/// <summary>
/// URL-safe slug value object (lowercase letters, digits, single hyphens).
/// </summary>
/// <remarks>
/// <b>Validation Rules (Opinionated):</b>
/// <list type="bullet">
/// <item>Lowercase letters only (no uppercase)</item>
/// <item>Digits allowed</item>
/// <item>Hyphens allowed but not consecutive, leading, or trailing</item>
/// </list>
/// <para>
/// <b>If these rules don't fit your domain</b> (e.g., you allow uppercase in slugs),
/// create your own Slug value object using the <see cref="ScalarValueObject{TSelf, T}"/> base class.
/// </para>
/// </remarks>
[JsonConverter(typeof(ParsableJsonConverter<Slug>))]
public partial class Slug : ScalarValueObject<Slug, string>, IScalarValue<Slug, string>, IParsable<Slug>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Slug"/> class.
    /// </summary>
    private Slug(string value) : base(value) { }

    /// <summary>
    /// Attempts to create a slug.
    /// </summary>
    public static Result<Slug> TryCreate(string? value, string? fieldName = null)
    {
        using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(nameof(Slug) + '.' + nameof(TryCreate));
        var field = !string.IsNullOrEmpty(fieldName)
            ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
            : "slug";
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Slug>(Error.Validation("Slug is required.", field));
        var trimmed = value.Trim();
        // lower-case, numbers, hyphens, single hyphen separators
        if (!SlugRegex().IsMatch(trimmed))
            return Result.Failure<Slug>(Error.Validation("Slug must contain lower-case letters, numbers, and hyphens, without leading/trailing hyphens.", field));
        return new Slug(trimmed);
    }

    /// <summary>
    /// Parses a slug.
    /// </summary>
    public static Slug Parse(string? s, IFormatProvider? provider)
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
    /// Tries to parse a slug.
    /// </summary>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Slug result)
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

    [GeneratedRegex(@"^(?!-)(?!.*--)[a-z0-9-]+(?<!-)$")]
    private static partial Regex SlugRegex();
}
