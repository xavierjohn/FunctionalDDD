namespace FunctionalDdd;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a non-negative decimal value object (value >= 0).
/// Ensures that decimal values are zero or positive, preventing negative values in the domain model.
/// </summary>
/// <remarks>
/// <para>
/// NonNegativeDecimal is a domain primitive that encapsulates non-negative decimal validation and provides:
/// <list type="bullet">
/// <item>Validation ensuring value >= 0</item>
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
/// <item>Prices and monetary amounts</item>
/// <item>Weights and measurements</item>
/// <item>Percentages (when represented as decimal)</item>
/// <item>Any decimal value that cannot be negative</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Basic usage:
/// <code>
/// var price = NonNegativeDecimal.TryCreate(19.99m);
/// // Returns: Success(NonNegativeDecimal(19.99))
/// 
/// var zero = NonNegativeDecimal.TryCreate(0m);
/// // Returns: Success(NonNegativeDecimal(0))
/// 
/// var invalid = NonNegativeDecimal.TryCreate(-5.50m);
/// // Returns: Failure(ValidationError("Value cannot be negative."))
/// </code>
/// </example>
[JsonConverter(typeof(ParsableJsonConverter<NonNegativeDecimal>))]
public class NonNegativeDecimal : ScalarValueObject<NonNegativeDecimal, decimal>, IScalarValueObject<NonNegativeDecimal, decimal>, IParsable<NonNegativeDecimal>
{
    private NonNegativeDecimal(decimal value) : base(value) { }

    /// <summary>
    /// Gets a <see cref="NonNegativeDecimal"/> representing zero.
    /// </summary>
    public static NonNegativeDecimal Zero => new(0m);

    /// <summary>
    /// Attempts to create a <see cref="NonNegativeDecimal"/> from the specified decimal.
    /// </summary>
    /// <param name="value">The decimal value to validate.</param>
    /// <param name="fieldName">Optional field name to use in validation error messages.</param>
    /// <returns>
    /// Success with the NonNegativeDecimal if the value is >= 0; otherwise Failure with a ValidationError.
    /// </returns>
    public static Result<NonNegativeDecimal> TryCreate(decimal value, string? fieldName = null)
    {
        using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(nameof(NonNegativeDecimal) + '.' + nameof(TryCreate));

        var field = !string.IsNullOrEmpty(fieldName)
            ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
            : "value";

        if (value < 0m)
            return Result.Failure<NonNegativeDecimal>(Error.Validation("Value cannot be negative.", field));

        return new NonNegativeDecimal(value);
    }

    /// <summary>
    /// Attempts to create a <see cref="NonNegativeDecimal"/> from the specified nullable decimal.
    /// </summary>
    /// <param name="value">The nullable decimal value to validate.</param>
    /// <param name="fieldName">Optional field name to use in validation error messages.</param>
    /// <returns>
    /// Success with the NonNegativeDecimal if the value is >= 0; otherwise Failure with a ValidationError.
    /// </returns>
    public static Result<NonNegativeDecimal> TryCreate(decimal? value, string? fieldName = null)
    {
        using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(nameof(NonNegativeDecimal) + '.' + nameof(TryCreate));

        var field = !string.IsNullOrEmpty(fieldName)
            ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
            : "value";

        if (value is null)
            return Result.Failure<NonNegativeDecimal>(Error.Validation("Value is required.", field));

        if (value.Value < 0m)
            return Result.Failure<NonNegativeDecimal>(Error.Validation("Value cannot be negative.", field));

        return new NonNegativeDecimal(value.Value);
    }

    /// <summary>
    /// Parses the string representation of a decimal to its <see cref="NonNegativeDecimal"/> equivalent.
    /// </summary>
    public static NonNegativeDecimal Parse(string? s, IFormatProvider? provider)
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
    /// Tries to parse a string into a <see cref="NonNegativeDecimal"/>.
    /// </summary>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out NonNegativeDecimal result)
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
    /// Explicitly converts a decimal to a <see cref="NonNegativeDecimal"/>.
    /// </summary>
    public static explicit operator NonNegativeDecimal(decimal value) => TryCreate(value).Value;
}
