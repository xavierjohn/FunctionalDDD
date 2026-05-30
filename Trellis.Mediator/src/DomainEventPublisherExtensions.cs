namespace Trellis.Mediator;

using System.ComponentModel;

/// <summary>
/// Extension methods over <see cref="IDomainEventPublisher"/> for call sites that need to dispatch
/// an aggregate's <see cref="IAggregate.UncommittedEvents"/> manually (outside the
/// <see cref="DomainEventDispatchBehavior{TMessage, TResponse}"/> pipeline).
/// </summary>
public static class DomainEventPublisherExtensions
{
    // MUST match DomainEventDispatchBehavior<,>.MaxDispatchWaves. Kept as a local literal so the
    // helper does not need to instantiate the closed generic just to read its public constant.
    // If one changes, change both.
    private const int MaxDispatchWaves = 8;

    /// <summary>
    /// <b>POST-COMMIT ONLY.</b> Publishes <paramref name="aggregate"/>'s uncommitted domain events in
    /// wave order and then calls <see cref="IChangeTracking.AcceptChanges"/> when the dispatch fully
    /// drains. Provided as the manual counterpart to
    /// <see cref="DomainEventDispatchBehavior{TMessage, TResponse}"/> for handlers whose response type
    /// is not an aggregate-valued result (e.g., <c>Result&lt;Unit&gt;</c>, <c>Result&lt;TDto&gt;</c>,
    /// <c>Result&lt;(A,B)&gt;</c>) and for non-Mediator call sites such as <c>BackgroundService</c> workers.
    /// </summary>
    /// <param name="publisher">The publisher used to fan out each event to its registered handlers.</param>
    /// <param name="aggregate">The aggregate whose <see cref="IAggregate.UncommittedEvents"/> are dispatched.</param>
    /// <param name="cancellationToken">A token to observe. Cancellation propagates before
    /// <see cref="IChangeTracking.AcceptChanges"/> so undispatched events stay on the aggregate.</param>
    /// <returns>A <see cref="Task"/> that completes once every event has been published and
    /// <see cref="IChangeTracking.AcceptChanges"/> has cleared the aggregate's pending list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/> or <paramref name="aggregate"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when more than
    /// <see cref="DomainEventDispatchBehavior{TMessage, TResponse}.MaxDispatchWaves"/> waves of events
    /// are raised on the aggregate during dispatch (typically caused by a handler raising new events
    /// on the same aggregate). <see cref="IChangeTracking.AcceptChanges"/> is not called in this case
    /// so the caller can inspect the still-undispatched events.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/>
    /// is canceled. Dispatched events stay dispatched (handlers must be idempotent for retry);
    /// <see cref="IChangeTracking.AcceptChanges"/> is not called so undispatched events remain on
    /// the aggregate.</exception>
    /// <remarks>
    /// <para>
    /// <b>POST-COMMIT ONLY.</b> Domain events must be published only after the underlying unit of
    /// work has committed. Calling this helper from inside a command handler that relies on
    /// <see cref="DomainEventDispatchBehavior{TMessage, TResponse}"/>'s sibling
    /// <c>TransactionalCommandBehavior</c> for its commit will publish events before the database
    /// transaction is durable; if the commit then fails, the events have already escaped to their
    /// handlers and <see cref="IChangeTracking.AcceptChanges"/> has cleared them off the aggregate
    /// — making the failure non-replayable.
    /// </para>
    /// <para>
    /// Safe call sites:
    /// <list type="bullet">
    ///   <item>After a manual <c>IUnitOfWork.CommitAsync</c> in a handler that does not chain the
    ///     transactional behavior.</item>
    ///   <item>From an outer <c>IPipelineBehavior</c> that runs after
    ///     <c>TransactionalCommandBehavior</c> (i.e., registered earlier in the pipeline so its
    ///     post-await section executes later).</item>
    ///   <item>From a <c>BackgroundService</c> tick after the underlying <c>DbContext.SaveChangesAsync</c>
    ///     call has succeeded.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Wave-loop semantics match <see cref="DomainEventDispatchBehavior{TMessage, TResponse}"/>:
    /// events are dispatched sequentially by index, so events raised by a handler on the same
    /// aggregate are picked up on the next wave. After
    /// <see cref="DomainEventDispatchBehavior{TMessage, TResponse}.MaxDispatchWaves"/> waves the
    /// helper throws <see cref="InvalidOperationException"/> (handlers should be side-effect-only
    /// and not raise additional events on the same aggregate).
    /// </para>
    /// <para>
    /// Re-entrant calls on the same aggregate are not supported. Calling this helper from inside an
    /// <see cref="IDomainEventHandler{TEvent}"/> that is currently draining the same
    /// <paramref name="aggregate"/> will republish the in-flight event (its index has not yet
    /// advanced, and <see cref="IChangeTracking.AcceptChanges"/> has not yet run); each nested
    /// invocation also starts with its own wave counter, so the wave cap does not prevent runaway
    /// recursion. Treat domain event handlers as side-effect-only and dispatch is owned by exactly
    /// one outer call.
    /// </para>
    /// <para>
    /// An opt-in companion pipeline behavior is planned (issue #537) to automate this dispatch for
    /// handlers that mutate aggregates the EF change tracker tracks instead of returning them through
    /// <c>Result&lt;TAggregate&gt;</c>; until that ships, this helper is the supported path for the
    /// non-aggregate-response and worker cases.
    /// </para>
    /// </remarks>
    public static async Task DispatchAggregateEventsAsync(
        this IDomainEventPublisher publisher,
        IAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(aggregate);

        // Track how many events have been published. UncommittedEvents() returns a fresh
        // snapshot each call; the underlying DomainEvents list is append-only until
        // AcceptChanges() runs, so handler-raised events appear at successive indices.
        // Holding off AcceptChanges() until the loop completes preserves not-yet-dispatched
        // events on the aggregate when cancellation propagates mid-loop.
        var dispatched = 0;
        for (var wave = 0; wave < MaxDispatchWaves; wave++)
        {
            var events = aggregate.UncommittedEvents();
            if (events.Count <= dispatched)
                break;

            for (var i = dispatched; i < events.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await publisher.PublishAsync(events[i], cancellationToken).ConfigureAwait(false);
                dispatched = i + 1;
            }
        }

        var pendingAfterLoop = aggregate.UncommittedEvents().Count - dispatched;
        if (pendingAfterLoop > 0)
        {
            // Surface accidental re-entry instead of silently abandoning events. The caller invoked
            // this helper explicitly, so failing loud is more useful than the pipeline behavior's
            // log-and-abandon stance (which exists there to avoid throwing mid-request).
            // AcceptChanges is intentionally NOT called: undispatched events stay on the aggregate
            // so the caller can inspect them.
            throw new InvalidOperationException(
                $"Domain event dispatch exceeded {MaxDispatchWaves} waves for " +
                $"{aggregate.GetType().FullName ?? aggregate.GetType().Name}; {pendingAfterLoop} event(s) remain " +
                "undispatched on the aggregate. Domain event handlers should be side-effect-only and not raise " +
                "additional events on the same aggregate.");
        }

        // Only reach here on the full-success path: cancellation propagates above and skips this
        // clear, leaving undispatched events on the aggregate.
        aggregate.AcceptChanges();
    }
}
