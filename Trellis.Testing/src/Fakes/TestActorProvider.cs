namespace Trellis.Testing.Fakes;

using Trellis.Authorization;

/// <summary>
/// Mutable <see cref="IActorProvider"/> for integration and authorization testing.
/// Use <see cref="WithActor(Actor)"/> to temporarily switch the actor for a test scope;
/// the previous actor is restored automatically on dispose.
/// </summary>
/// <example>
/// <code>
/// var actorProvider = new TestActorProvider("admin", "Orders.Read", "Orders.Write");
///
/// // Temporarily switch to a restricted user
/// await using var scope = actorProvider.WithActor("user-1", "Orders.Read");
/// var result = await mediator.Send(new CreateOrderCommand());
/// result.Should().BeFailure(); // missing Orders.Write
/// // scope disposes → actor reverts to admin
/// </code>
/// </example>
public sealed class TestActorProvider : IActorProvider
{
    private Actor _currentActor;

    /// <summary>
    /// Initializes a new <see cref="TestActorProvider"/> with the specified <see cref="Actor"/>.
    /// </summary>
    /// <param name="actor">The initial actor.</param>
    public TestActorProvider(Actor actor) =>
        _currentActor = actor;

    /// <summary>
    /// Initializes a new <see cref="TestActorProvider"/> with an actor created from the specified user ID and permissions.
    /// </summary>
    /// <param name="userId">The unique identifier of the actor.</param>
    /// <param name="permissions">The permissions granted to the actor.</param>
    public TestActorProvider(string userId, params string[] permissions)
        : this(Actor.Create(userId, new HashSet<string>(permissions)))
    {
    }

    /// <inheritdoc />
    public Actor GetCurrentActor() => _currentActor;

    /// <summary>
    /// Temporarily replaces the current actor. Returns a <see cref="TestActorScope"/> that
    /// restores the previous actor on dispose.
    /// </summary>
    /// <param name="actor">The actor to use for the duration of the scope.</param>
    /// <returns>A disposable scope that restores the previous actor.</returns>
    public TestActorScope WithActor(Actor actor)
    {
        var previous = _currentActor;
        _currentActor = actor;
        return new TestActorScope(this, previous);
    }

    /// <summary>
    /// Temporarily replaces the current actor with one created from the specified user ID and permissions.
    /// Returns a <see cref="TestActorScope"/> that restores the previous actor on dispose.
    /// </summary>
    /// <param name="userId">The unique identifier of the actor.</param>
    /// <param name="permissions">The permissions granted to the actor.</param>
    /// <returns>A disposable scope that restores the previous actor.</returns>
    public TestActorScope WithActor(string userId, params string[] permissions) =>
        WithActor(Actor.Create(userId, new HashSet<string>(permissions)));

    internal void RestoreActor(Actor actor) => _currentActor = actor;
}
