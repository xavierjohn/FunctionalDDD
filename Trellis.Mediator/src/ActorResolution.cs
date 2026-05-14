namespace Trellis.Mediator;

using Trellis.Authorization;

/// <summary>
/// Internal helper shared by the three authorization pipeline behaviors
/// (<see cref="AuthorizationBehavior{TMessage,TResponse}"/>,
/// <see cref="ResourceAuthorizationBehavior{TMessage,TResource,TResponse}"/>,
/// <see cref="ResourceAuthorizationViaBehavior{TMessage,TLeaf,TOwner,TResponse}"/>).
/// Centralises the "resolve current actor or short-circuit with HTTP 401" idiom so the three
/// behaviors cannot drift on the 401 vs 500 distinction.
/// </summary>
internal static class ActorResolution
{
    /// <summary>
    /// Resolves the current actor via <paramref name="actorProvider"/>. Returns the actor on
    /// success; returns <see langword="null"/> when no authenticated actor is available, in
    /// which case the caller should short-circuit the pipeline with
    /// <see cref="Error.Unauthorized"/> (HTTP 401).
    /// </summary>
    /// <remarks>
    /// <para>
    /// "No authenticated actor" is modelled as <see cref="Maybe{T}.None"/> on the
    /// <see cref="IActorProvider"/> contract — client-error state, not an exception. Provider
    /// implementations that legitimately cannot operate (no <c>HttpContext</c>, mapping
    /// delegate threw, etc.) still throw <see cref="System.InvalidOperationException"/>, which
    /// propagates here unhandled and is caught later by
    /// <see cref="ExceptionBehavior{TMessage,TResponse}"/> as <see cref="Error.InternalServerError"/>
    /// (HTTP 500). That preserves the bug-vs-auth-state distinction.
    /// </para>
    /// </remarks>
    public static async ValueTask<Actor?> TryResolveAsync(
        IActorProvider actorProvider,
        CancellationToken cancellationToken)
    {
        var maybeActor = await actorProvider.GetCurrentActorAsync(cancellationToken).ConfigureAwait(false);
        return maybeActor.TryGetValue(out var actor) ? actor : null;
    }

    /// <summary>
    /// Canonical <see cref="Error.Unauthorized"/> instance for the "no authenticated actor"
    /// short-circuit. Empty <see cref="Error.Unauthorized.Challenges"/> defers the
    /// <c>WWW-Authenticate</c> header to the configured ASP.NET Core authentication handler,
    /// which knows the scheme name (Bearer, etc.) and parameters; the mediator layer does
    /// not have that knowledge. Consumers needing strict RFC 9110 §11.6.1 compliance should
    /// ensure their auth handler writes <c>WWW-Authenticate</c> on 401 responses.
    /// </summary>
    public static Error.Unauthorized AuthenticationRequired() =>
        new() { Detail = "Authentication required." };
}
