namespace FunctionalDdd;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a positive integer value object (value > 0).
/// Ensures that integer values are strictly positive, preventing zero and negative values in the domain model.
/// </summary>
/// <remarks>
/// <para>
/// PositiveInt is a domain primitive that encapsulates positive integer validation and provides:
/// <list type="bullet">
/// <item>Validation ensuring value > 0</item>
/// <item>Type safety preventing mixing of constrained integers with other integers</item>
/// <item>Immutability ensuring values cannot be changed after creation</item>
/// <item>IParsable implementation for .NET parsing conventions</item>
/// <item>JSON serialization support for APIs and persistence</item>
/// <item>Activity tracing for monitoring and diagnostics</item>
/// </list>
/// </para>
/// <para>
/// Common use cases:
/// <list type="bullet">
/// <item>Entity IDs (when using integer identifiers)</item>
/// <item>Page numbers in pagination</item>
/// <item>Quantities that must be at least 1</item>
/// <item>Counts that cannot be zero</item>
/// <item>Any value that must be strictly positive</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Basic usage:
/// <code>
/// var pageNumber = PositiveInt.TryCreate(1);
/// // Returns: Success(PositiveInt(1))
/// 
/// var invalidZero = PositiveInt.TryCreate(0);
/// // Returns: Failure(ValidationError("Value must be greater than zero."))
/// 
/// var invalidNegative = PositiveInt.TryCreate(-1);
/// // Returns: Failure(ValidationError("Value must be greater than zero."))
/// </code>
/// </example>
[JsonConverter(typeof(ParsableJsonConverter<PositiveInt>))]
public class PositiveInt : ScalarValueObject<PositiveInt, int>, IScalarValueObject<PositiveInt, int>, IParsable<PositiveInt>
{
    private PositiveInt(int value) : base(value) { }

    /// <summary>
    /// Gets a <see cref="PositiveInt"/> representing the value 1.
    /// </summary>
    public static PositiveInt One => new(1);

    /// <summary>
    /// Attempts to create a <see cref="PositiveInt"/> from the specified integer.
    /// </summary>
    /// <param name="value">The integer value to validate.</param>
    /// <param name="fieldName">Optional field name to use in validation error messages.</param>
    /// <returns>
    /// Success with the PositiveInt if the value is > 0; otherwise Failure with a ValidationError.
    /// </returns>
    public static Result<PositiveInt> TryCreate(int value, string? fieldName = null)
    {
        using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(nameof(PositiveInt) + '.' + nameof(TryCreate));

        var field = !string.IsNullOrEmpty(fieldName)
            ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
            : "value";

        if (value <= 0)
            return Result.Failure<PositiveInt>(Error.Validation("Value must be greater than zero.", field));

        return new PositiveInt(value);
    }

    /// <summary>
    /// Attempts to create a <see cref="PositiveInt"/> from the specified nullable integer.
    /// </summary>
    /// <param name="value">The nullable integer value to validate.</param>
    /// <param name="fieldName">Optional field name to use in validation error messages.</param>
    /// <returns>
    /// Success with the PositiveInt if the value is > 0; otherwise Failure with a ValidationError.
    /// </returns>
    public static Result<PositiveInt> TryCreate(int? value, string? fieldName = null)
    {
        using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(nameof(PositiveInt) + '.' + nameof(TryCreate));

        var field = !string.IsNullOrEmpty(fieldName)
            ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
            : "value";

        if (value is null)
            return Result.Failure<PositiveInt>(Error.Validation("Value is required.", field));

        if (value.Value <= 0)
            return Result.Failure<PositiveInt>(Error.Validation("Value must be greater than zero.", field));

        return new PositiveInt(value.Value);
    }

    /// <summary>
    /// Parses the string representation of an integer to its <see cref="PositiveInt"/> equivalent.
    /// </summary>
    public static PositiveInt Parse(string? s, IFormatProvider? provider)
    {
        if (!int.TryParse(s, provider, out var value))
            throw new FormatException("Value must be a valid integer.");

        var r = TryCreate(value);
        if (r.IsFailure)
        {
            var val = (ValidationError)r.Error;
            throw new FormatException(val.FieldErrors[0].Details[0]);
        }

        return r.Value;
    }

    /// <summary>
    /// Tries to parse a string into a <see cref="PositiveInt"/>.
    /// </summary>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out PositiveInt result)
    {
        result = default;

        if (!int.TryParse(s, provider, out var value))
            return false;

        var r = TryCreate(value);
        if (r.IsFailure)
            return false;

        result = r.Value;
        return true;
    }

    /// <summary>
    /// Explicitly converts an integer to a <see cref="PositiveInt"/>.
    /// </summary>
    public static explicit operator PositiveInt(int value) => TryCreate(value).Value;
}
