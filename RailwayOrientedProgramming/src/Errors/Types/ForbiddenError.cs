namespace FunctionalDdd;

/// <summary>
/// Represents a forbidden error when an authenticated user lacks permission to access a resource.
/// Use this when the user is authenticated but does not have the required authorization or role.
/// Maps to HTTP 403 Forbidden.
/// </summary>
/// <remarks>
/// <para>
/// This is distinct from <see cref="UnauthorizedError"/> which indicates missing authentication.
/// ForbiddenError means the user is known but not allowed to perform the requested operation.
/// </para>
/// <para>
/// Common scenarios:
/// - User lacks required role or permission
/// - Resource owner attempting to access another user's private data
/// - Insufficient privileges for administrative operations
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Error.Forbidden("You do not have permission to delete this resource")
/// Error.Forbidden("Administrator role required for this operation")
/// Error.Forbidden("Cannot access another user's private information")
/// </code>
/// </example>
public sealed class ForbiddenError : Error
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenError"/> class.
    /// </summary>
    /// <param name="detail">Description of why access is forbidden.</param>
    /// <param name="code">The error code identifying this type of forbidden error.</param>
    /// <param name="instance">Optional identifier for the forbidden resource.</param>
    public ForbiddenError(string detail, string code, string? instance = null) : base(detail, code, instance)
    {
    }
}
