namespace Trellis.Mediator.Tests.Helpers;

using Trellis.Authorization;

/// <summary>
/// Fake <see cref="IActorProvider"/> for testing authorization behaviors.
/// </summary>
internal sealed class FakeActorProvider : IActorProvider
{
    private readonly Maybe<Actor> _actor;

    public FakeActorProvider(Actor actor) => _actor = Maybe.From(actor);

    private FakeActorProvider(Maybe<Actor> actor) => _actor = actor;

    public Task<Maybe<Actor>> GetCurrentActorAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_actor);

    public static FakeActorProvider WithPermissions(string userId, params string[] permissions)
        => new(Actor.Create(userId, permissions.ToHashSet()));

    public static FakeActorProvider NoPermissions(string userId = "user-1")
        => new(Actor.Create(userId, new HashSet<string>()));

    /// <summary>
    /// Fake provider that returns <see cref="Maybe{T}.None"/> — represents an unauthenticated
    /// request. The authorization pipeline should map this to <see cref="Error.Unauthorized"/>
    /// (HTTP 401).
    /// </summary>
    public static FakeActorProvider Anonymous() => new(Maybe<Actor>.None);
}