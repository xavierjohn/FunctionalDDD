namespace Trellis.Mediator.Tests.Helpers;

using Trellis.Authorization;

/// <summary>
/// Fake <see cref="IActorProvider"/> for testing authorization behaviors.
/// </summary>
internal sealed class FakeActorProvider(Actor actor) : IActorProvider
{
    public Actor GetCurrentActor() => actor;

    public static FakeActorProvider WithPermissions(string userId, params string[] permissions)
        => new(new Actor(userId, permissions.ToHashSet()));

    public static FakeActorProvider NoPermissions(string userId = "user-1")
        => new(new Actor(userId, new HashSet<string>()));
}
