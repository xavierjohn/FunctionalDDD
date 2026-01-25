namespace FunctionalDdd;

/// <summary>
/// Non-generic base interface for result types, exposing success/failure state and error information.
/// </summary>
/// <remarks>
/// This interface allows polymorphic handling of results without knowing the success value type.
/// Use <see cref="IResult{TValue}"/> for typed access to the success value.
/// </remarks>
public interface IResult
{
    /// <summary>
    /// Gets a value indicating whether this result represents a successful outcome.
    /// </summary>
    /// <value><c>true</c> if the result is successful; otherwise, <c>false</c>.</value>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether this result represents a failure.
    /// </summary>
    /// <value><c>true</c> if the result is a failure; otherwise, <c>false</c>.</value>
    bool IsFailure { get; }

#pragma warning disable CA1716 // Identifiers should not match keywords
    /// <summary>
    /// Gets the error information if this is a failure result.
    /// </summary>
    /// <value>An <see cref="FunctionalDdd.Error"/> instance describing the failure.</value>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a successful result.</exception>
    Error Error { get; }
#pragma warning restore CA1716 // Identifiers should not match keywords
}