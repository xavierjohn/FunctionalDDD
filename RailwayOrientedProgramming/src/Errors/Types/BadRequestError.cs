namespace FunctionalDdd;

/// <summary>
/// Represents a bad request error indicating the request was malformed or syntactically invalid.
/// Use this for requests that cannot be processed due to syntax errors, missing required data, or malformed content.
/// Maps to HTTP 400 Bad Request.
/// </summary>
/// <remarks>
/// <para>
/// Use BadRequestError for syntactic errors (malformed JSON, missing required fields, invalid format).
/// For business rule violations, use <see cref="ValidationError"/> or <see cref="DomainError"/> instead.
/// </para>
/// <para>
/// This represents a client error that should not be retried without modification.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Error.BadRequest("Request body is not valid JSON")
/// Error.BadRequest("Missing required header: Authorization")
/// Error.BadRequest("Invalid date format in query parameter")
/// </code>
/// </example>
public sealed class BadRequestError : Error
{
    public BadRequestError(string detail, string code, string? instance = null) : base(detail, code, instance)
    {
    }
}
