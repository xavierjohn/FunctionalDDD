namespace FunctionalDdd;

using System.Collections.Immutable;
using System.Diagnostics;
using static FunctionalDdd.ValidationError;

[DebuggerDisplay("{Detail}")]
/// <summary>
/// Base class for all error types in the functional DDD library.
/// Errors represent failure states and contain structured information about what went wrong.
/// </summary>
/// <remarks>
/// Use the static factory methods (Validation, NotFound, etc.) to create specific error types.
/// All errors have a Code for programmatic handling and a Detail for human-readable messages.
/// </remarks>
#pragma warning disable CA1716 // Identifiers should not match keywords
public class Error : IEquatable<Error>
#pragma warning restore CA1716 // Identifiers should not match keywords
{
    /// <summary>
    /// Gets the machine-readable error code. Use this for programmatic error handling.
    /// </summary>
    /// <value>A string code like "validation.error" or "not.found.error".</value>
    public string Code { get; }

    /// <summary>
    /// Gets the human-readable error description. Use this for displaying error messages to users or in logs.
    /// </summary>
    /// <value>A descriptive message explaining what went wrong.</value>
    public string Detail { get; }

    /// <summary>
    /// Gets an optional identifier for the specific instance that caused the error (e.g., a resource ID).
    /// </summary>
    /// <value>An instance identifier, or null if not applicable.</value>
    public string? Instance { get; }

    public Error(string detail, string code)
    {
        Detail = detail;
        Code = code;
    }
    public Error(string detail, string code, string? instance)
    {
        Detail = detail;
        Code = code;
        Instance = instance;
    }

    /// <summary>
    /// Determines equality based solely on the error Code.
    /// </summary>
    /// <param name="other">The error to compare with.</param>
    /// <returns>True if the error codes match; false otherwise.</returns>
    /// <remarks>
    /// IMPORTANT: Two errors are considered equal if they have the same Code, 
    /// regardless of their Detail, Instance, or concrete type.
    /// This design allows treating errors with the same code as equivalent for programmatic handling.
    /// Example: Error.NotFound("User not found", "user-1") == Error.NotFound("Product not found", "user-1") returns true
    /// because both have the code "not.found.error".
    /// </remarks>
    public bool Equals(Error? other)
    {
        if (other == null) return false;
        return Code == other.Code;
    }
    public override bool Equals(object? obj)
    {
        if (obj is Error error)
            return Equals(error);
        else
            return false;
    }

    public override int GetHashCode() => Code.GetHashCode();

    public override string ToString()
        => $"Type: {GetType().Name}, Code: {Code}, Detail: {Detail}, Instance: {Instance ?? "N/A"}";

    /// <summary>
    /// Creates a <see cref="ValidationError"/> for a single field validation failure.
    /// </summary>
    /// <param name="fieldDetail">Description of what's wrong with the field value.</param>
    /// <param name="fieldName">Name of the field that failed validation. Empty string if not field-specific.</param>
    /// <param name="detail">Optional overall error detail. If null, uses fieldDetail.</param>
    /// <param name="instance">Optional identifier for the instance being validated.</param>
    /// <returns>A <see cref="ValidationError"/> representing the validation failure.</returns>
    /// <example>
    /// <code>
    /// Error.Validation("Email address is not valid", "email")
    /// Error.Validation("Age must be 18 or older", "age")
    /// </code>
    /// </example>
    public static ValidationError Validation(string fieldDetail, string fieldName = "", string? detail = null, string? instance = null)
        => new(fieldDetail, fieldName, "validation.error", detail, instance);

    /// <summary>
    /// Creates a <see cref="ValidationError"/> for multiple field validation failures.
    /// </summary>
    /// <param name="fieldDetails">Collection of field-specific validation errors.</param>
    /// <param name="detail">Overall error description.</param>
    /// <param name="instance">Optional identifier for the instance being validated.</param>
    /// <returns>A <see cref="ValidationError"/> containing all field errors.</returns>
    /// <example>
    /// <code>
    /// var errors = ImmutableArray.Create(
    ///     new FieldError("email", "Invalid format"),
    ///     new FieldError("age", "Must be 18 or older")
    /// );
    /// Error.Validation(errors, "User validation failed")
    /// </code>
    /// </example>
    public static ValidationError Validation(ImmutableArray<FieldError> fieldDetails, string detail = "", string? instance = null)
        => new(fieldDetails, "validation.error", detail, instance);

    public static ValidationError Validation(ImmutableArray<FieldError> fieldDetails, string detail, string? instance, string code)
        => new(fieldDetails, code, detail, instance);

    /// <summary>
    /// Creates a <see cref="BadRequestError"/> indicating the request was malformed or invalid.
    /// </summary>
    /// <param name="detail">Description of why the request is bad.</param>
    /// <param name="instance">Optional identifier for the bad request.</param>
    /// <returns>A <see cref="BadRequestError"/>.</returns>
    /// <remarks>Use this for syntactic errors or malformed requests, not for business rule violations (use Validation instead).</remarks>
    public static BadRequestError BadRequest(string detail, string? instance = null) =>
        new(detail, "bad.request.error", instance);

    /// <summary>
    /// Creates a <see cref="ConflictError"/> indicating a conflict with the current state.
    /// </summary>
    /// <param name="detail">Description of the conflict.</param>
    /// <param name="instance">Optional identifier for the conflicting resource.</param>
    /// <returns>A <see cref="ConflictError"/>.</returns>
    /// <example>
    /// <code>
    /// Error.Conflict("Email address already in use")
    /// Error.Conflict("Cannot delete user with active subscriptions")
    /// </code>
    /// </example>
    public static ConflictError Conflict(string detail, string? instance = null) =>
        new(detail, "conflict.error", instance);

    /// <summary>
    /// Creates a <see cref="NotFoundError"/> indicating a requested resource was not found.
    /// </summary>
    /// <param name="detail">Description of what was not found.</param>
    /// <param name="instance">Optional identifier for the missing resource.</param>
    /// <returns>A <see cref="NotFoundError"/>.</returns>
    /// <example>
    /// <code>
    /// Error.NotFound($"User with ID {userId} not found", userId)
    /// Error.NotFound("Product not found in catalog")
    /// </code>
    /// </example>
    public static NotFoundError NotFound(string detail, string? instance = null) =>
        new(detail, "not.found.error", instance);

    /// <summary>
    /// Creates an <see cref="UnauthorizedError"/> indicating authentication is required.
    /// </summary>
    /// <param name="detail">Description of why authorization failed.</param>
    /// <param name="instance">Optional identifier for the unauthorized request.</param>
    /// <returns>An <see cref="UnauthorizedError"/>.</returns>
    /// <remarks>Use this when the user is not authenticated (not logged in).</remarks>
    public static UnauthorizedError Unauthorized(string detail, string? instance = null) =>
        new(detail, "unauthorized.error", instance);

    /// <summary>
    /// Creates a <see cref="ForbiddenError"/> indicating the user lacks permission.
    /// </summary>
    /// <param name="detail">Description of why access is forbidden.</param>
    /// <param name="instance">Optional identifier for the forbidden resource.</param>
    /// <returns>A <see cref="ForbiddenError"/>.</returns>
    /// <remarks>Use this when the user is authenticated but doesn't have permission to access the resource.</remarks>
    public static ForbiddenError Forbidden(string detail, string? instance = null) =>
        new(detail, "forbidden.error", instance);

    /// <summary>
    /// Creates an <see cref="UnexpectedError"/> indicating an unexpected system error occurred.
    /// </summary>
    /// <param name="detail">Description of what went wrong.</param>
    /// <param name="instance">Optional identifier for the operation that failed.</param>
    /// <returns>An <see cref="UnexpectedError"/>.</returns>
    /// <remarks>
    /// Use this for system errors, infrastructure failures, or exceptions.
    /// This typically maps to HTTP 500 Internal Server Error.
    /// </remarks>
    public static UnexpectedError Unexpected(string detail, string? instance = null) =>
        new(detail, "unexpected.error", instance);

    /// <summary>
    /// Creates a <see cref="DomainError"/> indicating a business rule violation.
    /// </summary>
    /// <param name="detail">Description of the business rule that was violated.</param>
    /// <param name="instance">Optional identifier for the entity or operation.</param>
    /// <returns>A <see cref="DomainError"/>.</returns>
    /// <remarks>
    /// Use this for domain logic violations that prevent the operation from completing.
    /// This is distinct from validation errors (field-level) and conflicts (state-based).
    /// Maps to HTTP 422 Unprocessable Entity.
    /// </remarks>
    /// <example>
    /// <code>
    /// Error.Domain("Cannot withdraw more than account balance")
    /// Error.Domain("Minimum order quantity is 10 units")
    /// Error.Domain("Cannot cancel order after shipment")
    /// </code>
    /// </example>
    public static DomainError Domain(string detail, string? instance = null) =>
        new(detail, "domain.error", instance);

    /// <summary>
    /// Creates a <see cref="RateLimitError"/> indicating too many requests have been made.
    /// </summary>
    /// <param name="detail">Description of the rate limit violation.</param>
    /// <param name="instance">Optional identifier for the client or resource being rate limited.</param>
    /// <returns>A <see cref="RateLimitError"/>.</returns>
    /// <remarks>
    /// Use this when a client has exceeded their request quota or rate limit.
    /// Maps to HTTP 429 Too Many Requests.
    /// </remarks>
    /// <example>
    /// <code>
    /// Error.RateLimit("API rate limit exceeded. Please try again in 60 seconds")
    /// Error.RateLimit("Too many login attempts. Account temporarily locked")
    /// </code>
    /// </example>
    public static RateLimitError RateLimit(string detail, string? instance = null) =>
        new(detail, "rate.limit.error", instance);

    /// <summary>
    /// Creates a <see cref="ServiceUnavailableError"/> indicating temporary service unavailability.
    /// </summary>
    /// <param name="detail">Description of why the service is unavailable.</param>
    /// <param name="instance">Optional identifier for the unavailable service or resource.</param>
    /// <returns>A <see cref="ServiceUnavailableError"/>.</returns>
    /// <remarks>
    /// Use this when the service is temporarily unable to handle the request.
    /// This indicates a temporary condition - the service is expected to be available again.
    /// Maps to HTTP 503 Service Unavailable.
    /// </remarks>
    /// <example>
    /// <code>
    /// Error.ServiceUnavailable("Service is under maintenance. Please try again later")
    /// Error.ServiceUnavailable("External payment gateway is temporarily unavailable")
    /// </code>
    /// </example>
    public static ServiceUnavailableError ServiceUnavailable(string detail, string? instance = null) =>
        new(detail, "service.unavailable.error", instance);

    public static BadRequestError BadRequest(string detail, string code, string? instance) =>
        new(detail, code, instance);

    public static ConflictError Conflict(string detail, string code, string? instance) =>
        new(detail, code, instance);

    public static NotFoundError NotFound(string detail, string code, string? instance) =>
        new(detail, code, instance);

    public static UnauthorizedError Unauthorized(string detail, string code, string? instance) =>
        new(detail, code, instance);

    public static ForbiddenError Forbidden(string detail, string code, string? instance) =>
        new(detail, code, instance);

    public static UnexpectedError Unexpected(string detail, string code, string? instance) =>
        new(detail, code, instance);

    public static DomainError Domain(string detail, string code, string? instance) =>
        new(detail, code, instance);

    public static RateLimitError RateLimit(string detail, string code, string? instance) =>
        new(detail, code, instance);

    public static ServiceUnavailableError ServiceUnavailable(string detail, string code, string? instance) =>
        new(detail, code, instance);
}

