namespace FunctionalDdd;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a non-negative integer value object (value >= 0).
/// Ensures that integer values are zero or positive, preventing negative values in the domain model.
/// </summary>
/// <remarks>
/// <para>
/// NonNegativeInt is a domain primitive that encapsulates non-negative integer validation and provides:
/// <list type="bullet">
/// <item>Validation ensuring value >= 0</item>
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
/// <item>Quantities in orders (quantity >= 0)</item>
/// <item>Inventory counts</item>
/// <item>Age values</item>
/// <item>Array indexes</item>
/// <item>Any count or quantity that cannot be negative</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Basic usage:
/// <code>
/// var quantity = NonNegativeInt.TryCreate(5);
/// // Returns: Success(NonNegativeInt(5))
/// 
/// var zero = NonNegativeInt.TryCreate(0);
/// // Returns: Success(NonNegativeInt(0))
/// 
/// var invalid = NonNegativeInt.TryCreate(-1);
/// // Returns: Failure(ValidationError("Value cannot be negative."))
/// </code>
/// </example>
[JsonConverter(typeof(ParsableJsonConverter<NonNegativeInt>))]
public class NonNegativeInt : ScalarValueObject<NonNegativeInt, int>, IScalarValueObject<NonNegativeInt, int>, IParsable<NonNegativeInt>
{
    private NonNegativeInt(int value) : base(value) { }

    /// <summary>
    /// Gets a <see cref="NonNegativeInt"/> representing zero.
    /// </summary>
    public static NonNegativeInt Zero => new(0);

    /// <summary>
    /// Attempts to create a <see cref="NonNegativeInt"/> from the specified integer.
    /// </summary>
    /// <param name="value">The integer value to validate.</param>
    /// <param name="fieldName">Optional field name to use in validation error messages.</param>
    /// <returns>
    /// Success with the NonNegativeInt if the value is >= 0; otherwise Failure with a ValidationError.
    /// </returns>
    public static Result<NonNegativeInt> TryCreate(int value, string? fieldName = null)
    {
        using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(nameof(NonNegativeInt) + '.' + nameof(TryCreate));

        var field = !string.IsNullOrEmpty(fieldName)
            ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
            : "value";

        if (value < 0)
            return Result.Failure<NonNegativeInt>(Error.Validation("Value cannot be negative.", field));

        return new NonNegativeInt(value);
    }

    /// <summary>
    /// Attempts to create a <see cref="NonNegativeInt"/> from the specified nullable integer.
    /// </summary>
    /// <param name="value">The nullable integer value to validate.</param>
    /// <param name="fieldName">Optional field name to use in validation error messages.</param>
    /// <returns>
    /// Success with the NonNegativeInt if the value is >= 0; otherwise Failure with a ValidationError.
    /// </returns>
    public static Result<NonNegativeInt> TryCreate(int? value, string? fieldName = null)
    {
        using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(nameof(NonNegativeInt) + '.' + nameof(TryCreate));

        var field = !string.IsNullOrEmpty(fieldName)
            ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
            : "value";

        if (value is null)
            return Result.Failure<NonNegativeInt>(Error.Validation("Value is required.", field));

        if (value.Value < 0)
            return Result.Failure<NonNegativeInt>(Error.Validation("Value cannot be negative.", field));

        return new NonNegativeInt(value.Value);
    }

    /// <summary>
    /// Parses the string representation of an integer to its <see cref="NonNegativeInt"/> equivalent.
    /// </summary>
    public static NonNegativeInt Parse(string? s, IFormatProvider? provider)
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
    /// Tries to parse a string into a <see cref="NonNegativeInt"/>.
    /// </summary>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out NonNegativeInt result)
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
    /// Explicitly converts an integer to a <see cref="NonNegativeInt"/>.
    /// </summary>
    public static explicit operator NonNegativeInt(int value) => TryCreate(value).Value;
}
