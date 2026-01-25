namespace FunctionalDdd;

/// <summary>
/// Represents a "not found" error when a requested resource does not exist.
/// Use this when an entity, record, or resource cannot be located.
/// Maps to HTTP 404 Not Found.
/// </summary>
/// <remarks>
/// Use this for missing resources, not for empty collections or null-but-valid scenarios.
/// Include the resource type and identifier in the detail message when possible.
/// </remarks>
/// <example>
/// <code>
/// Error.NotFound($"User with ID {userId} not found", userId)
/// Error.NotFound("Product SKU-123 not found in catalog")
/// Error.NotFound("Order not found", orderId)
/// </code>
/// </example>
public sealed class NotFoundError : Error
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundError"/> class.
    /// </summary>
    /// <param name="detail">Description of what was not found.</param>
    /// <param name="code">The error code identifying this type of not found error.</param>
    /// <param name="instance">Optional identifier for the missing resource.</param>
    public NotFoundError(string detail, string code, string? instance = null) : base(detail, code, instance)
    {
    }
}