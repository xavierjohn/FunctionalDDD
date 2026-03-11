namespace Trellis.Testing.Tests.Fakes;

using Trellis.Authorization;
using Trellis.Testing.Fakes;

/// <summary>
/// Tests for <see cref="TestActorProvider"/> and <see cref="TestActorScope"/>.
/// </summary>
public class TestActorProviderTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithActor_SetsCurrentActor()
    {
        var actor = Actor.Create("user-1", new HashSet<string>(["Read"]));

        var provider = new TestActorProvider(actor);

        provider.GetCurrentActor().Should().BeSameAs(actor);
    }

    [Fact]
    public void Constructor_WithUserIdAndPermissions_CreatesActor()
    {
        var provider = new TestActorProvider("user-1", "Read", "Write");

        var actor = provider.GetCurrentActor();
        actor.Id.Should().Be("user-1");
        actor.HasPermission("Read").Should().BeTrue();
        actor.HasPermission("Write").Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithNoPermissions_CreatesActorWithEmptyPermissions()
    {
        var provider = new TestActorProvider("user-1");

        var actor = provider.GetCurrentActor();
        actor.Id.Should().Be("user-1");
        actor.HasPermission("Read").Should().BeFalse();
    }

    #endregion

    #region WithActor Scope Tests

    [Fact]
    public async Task WithActor_Actor_ChangesCurrentActor()
    {
        var original = Actor.Create("admin", new HashSet<string>(["Admin"]));
        var scoped = Actor.Create("user-1", new HashSet<string>(["Read"]));
        var provider = new TestActorProvider(original);

        await using (provider.WithActor(scoped))
        {
            provider.GetCurrentActor().Should().BeSameAs(scoped);
        }
    }

    [Fact]
    public async Task WithActor_Actor_RestoresOriginalOnDispose()
    {
        var original = Actor.Create("admin", new HashSet<string>(["Admin"]));
        var scoped = Actor.Create("user-1", new HashSet<string>(["Read"]));
        var provider = new TestActorProvider(original);

        await using (provider.WithActor(scoped))
        {
            // Actor changed inside scope
        }

        provider.GetCurrentActor().Should().BeSameAs(original);
    }

    [Fact]
    public async Task WithActor_UserIdAndPermissions_ChangesCurrentActor()
    {
        var provider = new TestActorProvider("admin", "Admin");

        await using (provider.WithActor("user-1", "Read"))
        {
            var actor = provider.GetCurrentActor();
            actor.Id.Should().Be("user-1");
            actor.HasPermission("Read").Should().BeTrue();
            actor.HasPermission("Admin").Should().BeFalse();
        }
    }

    [Fact]
    public async Task WithActor_UserIdAndPermissions_RestoresOriginalOnDispose()
    {
        var provider = new TestActorProvider("admin", "Admin");

        await using (provider.WithActor("user-1", "Read"))
        {
            // Actor changed inside scope
        }

        var actor = provider.GetCurrentActor();
        actor.Id.Should().Be("admin");
        actor.HasPermission("Admin").Should().BeTrue();
    }

    [Fact]
    public async Task WithActor_NestedScopes_RestoresCorrectActorAtEachLevel()
    {
        var provider = new TestActorProvider("admin", "Admin");

        await using (provider.WithActor("user-1", "Read"))
        {
            provider.GetCurrentActor().Id.Should().Be("user-1");

            await using (provider.WithActor("user-2", "Write"))
            {
                provider.GetCurrentActor().Id.Should().Be("user-2");
            }

            provider.GetCurrentActor().Id.Should().Be("user-1");
        }

        provider.GetCurrentActor().Id.Should().Be("admin");
    }

    [Fact]
    public void WithActor_SynchronousDispose_RestoresOriginalActor()
    {
        var original = Actor.Create("admin", new HashSet<string>(["Admin"]));
        var provider = new TestActorProvider(original);

        using (provider.WithActor("user-1", "Read"))
        {
            provider.GetCurrentActor().Id.Should().Be("user-1");
        }

        provider.GetCurrentActor().Should().BeSameAs(original);
    }

    [Fact]
    public async Task WithActor_DoubleDispose_IsIdempotent()
    {
        var original = Actor.Create("admin", new HashSet<string>(["Admin"]));
        var provider = new TestActorProvider(original);

        var scope = provider.WithActor("user-1", "Read");
        await scope.DisposeAsync();

        // Change actor after first dispose
        using (provider.WithActor("user-2", "Write"))
        {
            // Second dispose should be a no-op, not restore to "admin" again
            await scope.DisposeAsync();
            provider.GetCurrentActor().Id.Should().Be("user-2");
        }
    }

    #endregion

    #region IActorProvider Contract Tests

    [Fact]
    public void GetCurrentActor_ImplementsIActorProvider()
    {
        var provider = new TestActorProvider("user-1", "Read");

        var actor = ((IActorProvider)provider).GetCurrentActor();

        actor.Id.Should().Be("user-1");
    }

    #endregion
}
