namespace Trellis.Mediator;

using global::Mediator;
using Trellis.Authorization;

/// <summary>
/// Pipeline behavior that delegates authorization to the message's
/// <see cref="IAuthorizeResource.Authorize"/> method, passing the current actor.
/// Short-circuits with the returned error if authorization fails.
/// </summary>
/// <typeparam name="TMessage">The message type, constrained to <see cref="IAuthorizeResource"/>.</typeparam>
/// <typeparam name="TResponse">The response type, constrained to <see cref="IResult"/> and <see cref="IFailureFactory{TSelf}"/>.</typeparam>
public sealed class ResourceAuthorizationBehavior<TMessage, TResponse>
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IAuthorizeResource, global::Mediator.IMessage
    where TResponse : IResult, IFailureFactory<TResponse>
{
    private readonly IActorProvider _actorProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceAuthorizationBehavior{TMessage, TResponse}"/> class.
    /// </summary>
    /// <param name="actorProvider">Provides the current authenticated actor.</param>
    public ResourceAuthorizationBehavior(IActorProvider actorProvider)
        => _actorProvider = actorProvider;

    /// <inheritdoc />
    public ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var actor = _actorProvider.GetCurrentActor();
        var authResult = message.Authorize(actor);

        if (authResult.IsFailure)
            return new ValueTask<TResponse>(TResponse.CreateFailure(authResult.Error));

        return next(message, cancellationToken);
    }
}
