namespace FunctionalDdd;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

/// <summary>
/// Age value object (0 to 150 inclusive).
/// </summary>
[JsonConverter(typeof(ParsableJsonConverter<Age>))]
public class Age : ScalarValueObject<Age, int>, IScalarValueObject<Age, int>, IParsable<Age>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Age"/> class.
    /// </summary>
    private Age(int value) : base(value) { }

    /// <summary>
    /// Attempts to create an age.
    /// </summary>
    public static Result<Age> TryCreate(int value, string? fieldName = null)
    {
        using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(nameof(Age) + '.' + nameof(TryCreate));
        var field = !string.IsNullOrEmpty(fieldName)
            ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
            : "age";
        if (value < 0)
            return Result.Failure<Age>(Error.Validation("Age must be non-negative.", field));
        if (value > 150)
            return Result.Failure<Age>(Error.Validation("Age is unrealistically high.", field));
        return new Age(value);
    }

    /// <summary>
    /// Parses an age.
    /// </summary>
    public static Age Parse(string? s, IFormatProvider? provider)
    {
        if (!int.TryParse(s, out var v))
            throw new FormatException("Value must be a valid integer.");
        var r = TryCreate(v);
        if (r.IsFailure)
        {
            var val = (ValidationError)r.Error;
            throw new FormatException(val.FieldErrors[0].Details[0]);
        }

        return r.Value;
    }

    /// <summary>
    /// Tries to parse an age.
    /// </summary>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Age result)
    {
        result = default;
        if (!int.TryParse(s, out var v))
            return false;
        var r = TryCreate(v);
        if (r.IsFailure)
            return false;

        result = r.Value;
        return true;
    }
}