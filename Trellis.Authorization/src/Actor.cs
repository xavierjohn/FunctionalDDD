namespace Trellis.Authorization;

/// <summary>
/// Represents the current authenticated user making the request.
/// Contains identity and permissions used by authorization behaviors.
/// </summary>
/// <param name="Id">The unique identifier of the actor.</param>
/// <param name="Permissions">The set of permissions granted to the actor.</param>
public sealed record Actor(string Id, IReadOnlySet<string> Permissions)
{
    /// <summary>Returns true if this actor has the specified permission.</summary>
    /// <param name="permission">The permission to check.</param>
    /// <returns>True if the actor has the permission; otherwise false.</returns>
    public bool HasPermission(string permission) => Permissions.Contains(permission);

    /// <summary>Returns true if this actor has ALL of the specified permissions.</summary>
    /// <param name="permissions">The permissions to check.</param>
    /// <returns>True if the actor has every specified permission; otherwise false.</returns>
    public bool HasAllPermissions(IEnumerable<string> permissions) =>
        permissions.All(Permissions.Contains);

    /// <summary>Returns true if this actor has ANY of the specified permissions.</summary>
    /// <param name="permissions">The permissions to check.</param>
    /// <returns>True if the actor has at least one of the specified permissions; otherwise false.</returns>
    public bool HasAnyPermission(IEnumerable<string> permissions) =>
        permissions.Any(Permissions.Contains);
}
