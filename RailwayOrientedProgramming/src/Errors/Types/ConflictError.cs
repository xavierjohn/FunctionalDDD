namespace FunctionalDdd;

/// <summary>
/// Represents a conflict error indicating the request conflicts with the current state of the resource.
/// Use this when an operation cannot be completed due to a conflict with existing data or state.
/// Maps to HTTP 409 Conflict.
/// </summary>
/// <remarks>
/// <para>
/// Common scenarios include:
/// - Duplicate keys or unique constraint violations
/// - Concurrent modification conflicts (optimistic locking)
/// - State-based conflicts (e.g., cannot delete a resource that has dependencies)
/// </para>
/// <para>
/// This is distinct from validation errors (field-level) and domain errors (business rules).
/// Conflicts are typically state-based and may be resolved by the client with different input or timing.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Error.Conflict("Email address already in use")
/// Error.Conflict("Cannot delete user with active subscriptions")
/// Error.Conflict("Resource has been modified by another user")
/// Error.Conflict("Product SKU already exists in catalog")
/// </code>
/// </example>
public sealed class ConflictError : Error
{
    public ConflictError(string detail, string code, string? instance = null) : base(detail, code, instance)
    {
    }
}
