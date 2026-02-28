namespace Trellis.Authorization.Tests;

/// <summary>
/// Tests for the <see cref="Actor"/> record.
/// </summary>
public class ActorTests
{
    [Fact]
    public void HasPermission_WithMatchingPermission_ReturnsTrue()
    {
        var actor = new Actor("user-1", new HashSet<string> { "Orders.Read", "Orders.Write" });

        actor.HasPermission("Orders.Read").Should().BeTrue();
    }

    [Fact]
    public void HasPermission_WithoutMatchingPermission_ReturnsFalse()
    {
        var actor = new Actor("user-1", new HashSet<string> { "Orders.Read" });

        actor.HasPermission("Admin.Write").Should().BeFalse();
    }

    [Fact]
    public void HasAllPermissions_WithAllMatching_ReturnsTrue()
    {
        var actor = new Actor("user-1", new HashSet<string> { "Orders.Read", "Orders.Write", "Admin.Read" });

        actor.HasAllPermissions(["Orders.Read", "Orders.Write"]).Should().BeTrue();
    }

    [Fact]
    public void HasAllPermissions_WithSomeMissing_ReturnsFalse()
    {
        var actor = new Actor("user-1", new HashSet<string> { "Orders.Read" });

        actor.HasAllPermissions(["Orders.Read", "Orders.Write"]).Should().BeFalse();
    }

    [Fact]
    public void HasAllPermissions_WithEmptyList_ReturnsTrue()
    {
        var actor = new Actor("user-1", new HashSet<string>());

        actor.HasAllPermissions([]).Should().BeTrue();
    }

    [Fact]
    public void HasAnyPermission_WithOneMatching_ReturnsTrue()
    {
        var actor = new Actor("user-1", new HashSet<string> { "Orders.Read" });

        actor.HasAnyPermission(["Orders.Read", "Admin.Write"]).Should().BeTrue();
    }

    [Fact]
    public void HasAnyPermission_WithNoneMatching_ReturnsFalse()
    {
        var actor = new Actor("user-1", new HashSet<string> { "Other.Permission" });

        actor.HasAnyPermission(["Orders.Read", "Admin.Write"]).Should().BeFalse();
    }

    [Fact]
    public void HasAnyPermission_WithEmptyList_ReturnsFalse()
    {
        var actor = new Actor("user-1", new HashSet<string> { "Orders.Read" });

        actor.HasAnyPermission([]).Should().BeFalse();
    }
}
