namespace FunctionalDdd;

/// <summary>
/// Represents an aggregate of multiple errors that occurred together.
/// Use this when multiple independent errors need to be returned as a single failure result.
/// </summary>
/// <remarks>
/// <para>
/// This is useful for scenarios where multiple operations fail independently,
/// such as batch processing or parallel validation of multiple entities.
/// </para>
/// <para>
/// The aggregated errors can be of different types and are accessible via the <see cref="Errors"/> property.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var errors = new List&lt;Error&gt;
/// {
///     Error.Validation("Invalid email", "email"),
///     Error.NotFound("User not found", userId),
///     Error.Domain("Insufficient balance")
/// };
/// var aggregateError = new AggregateError(errors);
/// </code>
/// </example>
public sealed class AggregateError : Error
{
    public AggregateError(List<Error> errors, string code) : base("Aggregated error", code)
    {
        if (errors.Count < 1)
            throw new ArgumentException("At least one error is required", nameof(errors));
        Errors = errors;
    }

    public AggregateError(List<Error> errors) : this(errors, "aggregate.error")
    {
    }

    /// <summary>
    /// Gets the collection of aggregated errors.
    /// </summary>
    /// <value>A list of <see cref="Error"/> instances that were aggregated together.</value>
    public IList<Error> Errors { get; }
}
