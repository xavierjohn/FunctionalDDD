namespace Benchmark;

using System.Collections.Frozen;
using BenchmarkDotNet.Attributes;
using Trellis.Authorization;

/// <summary>
/// Benchmarks for the Actor record — permission checking, scoped permissions,
/// forbidden overrides, and attribute lookups.
/// </summary>
[MemoryDiagnoser]
public class ActorBenchmarks
{
    private Actor _smallActor = default!;   // 5 permissions
    private Actor _mediumActor = default!;  // 50 permissions
    private Actor _largeActor = default!;   // 500 permissions
    private Actor _actorWithForbidden = default!;

    private string[] _checkPermissions = default!;

    [GlobalSetup]
    public void Setup()
    {
        // Small actor (typical user)
        _smallActor = Actor.Create("user-1", new HashSet<string>
        {
            "Orders.Read", "Orders.Write", "Products.Read",
            "Users.Read", "Dashboard.View"
        });

        // Medium actor (admin)
        var mediumPerms = new HashSet<string>();
        for (var i = 0; i < 50; i++)
            mediumPerms.Add($"Permission.{i}");
        _mediumActor = Actor.Create("admin-1", mediumPerms);

        // Large actor (superadmin)
        var largePerms = new HashSet<string>();
        for (var i = 0; i < 500; i++)
            largePerms.Add($"Permission.{i}");
        _largeActor = Actor.Create("superadmin-1", largePerms);

        // Actor with forbidden permissions
        var perms = new HashSet<string> { "Orders.Read", "Orders.Write", "Orders.Delete" };
        var forbidden = new HashSet<string> { "Orders.Delete" };
        _actorWithForbidden = new Actor("user-2", perms, forbidden,
            FrozenDictionary<string, string>.Empty);

        _checkPermissions = ["Orders.Read", "Orders.Write", "Products.Read"];
    }

    #region HasPermission

    [Benchmark(Baseline = true)]
    public bool HasPermission_Found()
    {
        return _smallActor.HasPermission("Orders.Read");
    }

    [Benchmark]
    public bool HasPermission_NotFound()
    {
        return _smallActor.HasPermission("Admin.FullAccess");
    }

    [Benchmark]
    public bool HasPermission_Forbidden()
    {
        return _actorWithForbidden.HasPermission("Orders.Delete");
    }

    #endregion

    #region Scoped Permissions

    [Benchmark]
    public bool HasPermission_Scoped()
    {
        return _smallActor.HasPermission("Orders.Read", "Tenant_A");
    }

    #endregion

    #region Bulk Permission Checks

    [Benchmark]
    public bool HasAllPermissions_AllPresent()
    {
        return _smallActor.HasAllPermissions(_checkPermissions);
    }

    [Benchmark]
    public bool HasAllPermissions_SomeMissing()
    {
        return _smallActor.HasAllPermissions(["Orders.Read", "Admin.FullAccess"]);
    }

    [Benchmark]
    public bool HasAnyPermission_OnePresent()
    {
        return _smallActor.HasAnyPermission(["Orders.Read", "Admin.FullAccess"]);
    }

    [Benchmark]
    public bool HasAnyPermission_NonePresent()
    {
        return _smallActor.HasAnyPermission(["Admin.FullAccess", "System.Config"]);
    }

    #endregion

    #region Large Permission Sets

    [Benchmark]
    public bool HasPermission_LargeSet_50_Found()
    {
        return _mediumActor.HasPermission("Permission.25");
    }

    [Benchmark]
    public bool HasPermission_LargeSet_500_Found()
    {
        return _largeActor.HasPermission("Permission.250");
    }

    [Benchmark]
    public bool HasPermission_LargeSet_500_NotFound()
    {
        return _largeActor.HasPermission("Permission.999");
    }

    #endregion

    #region Attributes

    [Benchmark]
    public bool IsOwner_Match()
    {
        return _smallActor.IsOwner("user-1");
    }

    [Benchmark]
    public bool IsOwner_NoMatch()
    {
        return _smallActor.IsOwner("other-user");
    }

    #endregion

    #region Actor Creation

    [Benchmark]
    public Actor Create_Simple()
    {
        return Actor.Create("user-1", new HashSet<string> { "Read", "Write" });
    }

    #endregion
}