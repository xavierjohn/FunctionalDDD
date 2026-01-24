namespace FunctionalDdd;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a positive decimal value object (value > 0).
/// Ensures that decimal values are strictly positive, preventing zero and negative values in the domain model.
/// </summary>
/// <remarks>
/// <para>
/// PositiveDecimal is a domain primitive that encapsulates positive decimal validation and provides:
/// <list type="bullet">
/// <item>Validation ensuring value > 0</item>
/// <item>Type safety preventing mixing of constrained decimals with other decimals</item>
/// <item>Immutability ensuring values cannot be changed after creation</item>
/// <item>IParsable implementation for .NET parsing conventions</item>
/// <item>JSON serialization support for APIs and persistence</item>
/// <item>Activity tracing for monitoring and diagnostics</item>
/// </list>
/// </para>
/// <para>
/// Common use cases:
/// <list type="bullet">
/// <item>Unit prices (must be positive)</item>
/// <item>Interest rates and percentages</item>
/// <item>Quantities that must be greater than zero</item>
/// <item>Any decimal value that must be strictly positive</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Basic usage:
/// <code>
/// var price = PositiveDecimal.TryCreate(19.99m);
/// // Returns: Success(PositiveDecimal(19.99))
/// 
/// var invalidZero = PositiveDecimal.TryCreate(0m);
/// // Returns: Failure(ValidationError("Value must be greater than zero."))
/// 
/// var invalidNegative = PositiveDecimal.TryCreate(-5.50m);
/// // Returns: Failure(ValidationError("Value must be greater than zero."))
/// </code>
/// </example>
[JsonConverter(typeof(ParsableJsonConverter<PositiveDecimal>))]
public class PositiveDecimal : ScalarValueObject<PositiveDecimal, decimal>, IScalarValueObject<PositiveDecimal, decimal>, IParsable<PositiveDecimal>
{
    private PositiveDecimal(decimal value) : base(value) { }

    /// <summary>
    /// Gets a <see cref="PositiveDecimal"/> representing the value 1.
    /// </summary>
    public static PositiveDecimal One => new(1m);

    /// <summary>
    /// Attempts to create a <see cref="PositiveDecimal"/> from the specified decimal.
    /// </summary>
    /// <param name="value">The decimal value to validate.</param>
    /// <param name="fieldName">Optional field name to use in validation error messages.</param>
    /// <returns>
    /// Success with the PositiveDecimal if the value is > 0; otherwise Failure with a ValidationError.
    /// </returns>
    public static Result<PositiveDecimal> TryCreate(decimal value, string? fieldName = null)
    {
        using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(nameof(PositiveDecimal) + '.' + nameof(TryCreate));

        var field = !string.IsNullOrEmpty(fieldName)
            ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
            : "value";

        if (value <= 0m)
            return Result.Failure<PositiveDecimal>(Error.Validation("Value must be greater than zero.", field));

        return new PositiveDecimal(value);
    }

    /// <summary>
    /// Attempts to create a <see cref="PositiveDecimal"/> from the specified nullable decimal.
    /// </summary>
    /// <param name="value">The nullable decimal value to validate.</param>
    /// <param name="fieldName">Optional field name to use in validation error messages.</param>
    /// <returns>
    /// Success with the PositiveDecimal if the value is > 0; otherwise Failure with a ValidationError.
    /// </returns>
    public static Result<PositiveDecimal> TryCreate(decimal? value, string? fieldName = null)
    {
        using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(nameof(PositiveDecimal) + '.' + nameof(TryCreate));

        var field = !string.IsNullOrEmpty(fieldName)
            ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
            : "value";

        if (value is null)
            return Result.Failure<PositiveDecimal>(Error.Validation("Value is required.", field));

        if (value.Value <= 0m)
            return Result.Failure<PositiveDecimal>(Error.Validation("Value must be greater than zero.", field));

        return new PositiveDecimal(value.Value);
    }

    /// <summary>
    /// Parses the string representation of a decimal to its <see cref="PositiveDecimal"/> equivalent.
    /// </summary>
    public static PositiveDecimal Parse(string? s, IFormatProvider? provider)
    {
        if (!decimal.TryParse(s, provider, out var value))
            throw new FormatException("Value must be a valid decimal.");

        var r = TryCreate(value);
        if (r.IsFailure)
        {
            var val = (ValidationError)r.Error;
            throw new FormatException(val.FieldErrors[0].Details[0]);
        }

        return r.Value;
    }

    /// <summary>
    /// Tries to parse a string into a <see cref="PositiveDecimal"/>.
    /// </summary>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out PositiveDecimal result)
    {
        result = default;

        if (!decimal.TryParse(s, provider, out var value))
            return false;

        var r = TryCreate(value);
        if (r.IsFailure)
            return false;

        result = r.Value;
        return true;
    }

    /// <summary>
    /// Explicitly converts a decimal to a <see cref="PositiveDecimal"/>.
    /// </summary>
    public static explicit operator PositiveDecimal(decimal value) => TryCreate(value).Value;
}
