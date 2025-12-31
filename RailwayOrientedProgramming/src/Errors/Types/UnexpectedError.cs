namespace FunctionalDdd;

/// <summary>
/// Represents an unexpected system error or exception.
/// Use this for infrastructure failures, unhandled exceptions, or system-level errors.
/// Maps to HTTP 500 Internal Server Error.
/// </summary>
/// <remarks>
/// <para>
/// This error type indicates something went wrong that the client cannot fix.
/// It typically represents bugs, infrastructure issues, or unexpected system states.
/// </para>
/// <para>
/// Common scenarios:
/// - Unhandled exceptions
/// - Database connection failures
/// - External service failures
/// - Infrastructure issues
/// - Null reference exceptions or other bugs
/// </para>
/// <para>
/// Avoid exposing sensitive system details in the detail message for security reasons.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Error.Unexpected("An unexpected error occurred")
/// Error.Unexpected("Database connection failed")
/// Error.Unexpected("Unable to process request due to internal error")
/// </code>
/// </example>
public sealed class UnexpectedError : Error
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnexpectedError"/> class.
    /// </summary>
    /// <param name="detail">Description of the unexpected error.</param>
    /// <param name="code">The error code identifying this type of unexpected error.</param>
    /// <param name="instance">Optional identifier for the operation or resource that failed.</param>
    public UnexpectedError(string detail, string code, string? instance = null) : base(detail, code, instance)
    {
    }
}
