namespace Trellis.Mediator.Tests.Helpers;

using global::Mediator;

/// <summary>
/// Helper to create <see cref="MessageHandlerDelegate{TMessage, TResponse}"/> instances for testing.
/// </summary>
internal static class NextDelegate
{
    /// <summary>
    /// Creates a delegate that returns the specified response.
    /// </summary>
    public static MessageHandlerDelegate<TMessage, TResponse> ReturningAsync<TMessage, TResponse>(TResponse response)
        where TMessage : global::Mediator.IMessage
        => (_, _) => new ValueTask<TResponse>(response);

    /// <summary>
    /// Creates a delegate that returns the specified response and tracks invocation.
    /// </summary>
    public static (MessageHandlerDelegate<TMessage, TResponse> Delegate, Tracker Tracker)
        TrackingAsync<TMessage, TResponse>(TResponse response)
        where TMessage : global::Mediator.IMessage
    {
        var tracker = new Tracker();
        MessageHandlerDelegate<TMessage, TResponse> del = (_, _) =>
        {
            tracker.WasInvoked = true;
            return new ValueTask<TResponse>(response);
        };
        return (del, tracker);
    }

    /// <summary>
    /// Creates a delegate that throws the specified exception.
    /// </summary>
    public static MessageHandlerDelegate<TMessage, TResponse> Throwing<TMessage, TResponse>(Exception ex)
        where TMessage : global::Mediator.IMessage
        => (_, _) => throw ex;

    /// <summary>Tracks whether a delegate was invoked.</summary>
    internal sealed class Tracker
    {
        public bool WasInvoked { get; set; }
    }
}