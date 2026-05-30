namespace Trellis.Mediator.Tests;

using Trellis.Mediator.Tests.Helpers;

/// <summary>
/// Tests for <see cref="DomainEventPublisherExtensions.DispatchAggregateEventsAsync"/>.
/// Mirrors the wave-loop and cancellation contracts already covered by
/// <see cref="DomainEventDispatchBehavior{TMessage, TResponse}"/> but for the standalone helper.
/// </summary>
public class DomainEventPublisherExtensionsTests
{
    private static readonly TestAggregateId Id1 = new(Guid.Parse("11111111-1111-1111-1111-111111111111"));

    [Fact]
    public async Task Dispatches_each_event_in_order_and_calls_accept_changes()
    {
        var aggregate = new TestAggregate(Id1);
        var eventA = new TestEventA("first", DateTimeOffset.UtcNow);
        var eventB = new TestEventB(42, DateTimeOffset.UtcNow);
        aggregate.RaiseEvent(eventA);
        aggregate.RaiseEvent(eventB);

        var publisher = new RecordingPublisher();

        await publisher.DispatchAggregateEventsAsync(aggregate, TestContext.Current.CancellationToken);

        publisher.Published.Should().HaveCount(2);
        publisher.Published[0].Should().BeSameAs(eventA);
        publisher.Published[1].Should().BeSameAs(eventB);
        aggregate.UncommittedEvents().Should().BeEmpty("AcceptChanges runs on the full-success path");
        aggregate.IsChanged.Should().BeFalse();
    }

    [Fact]
    public async Task No_events_calls_accept_changes_and_publishes_nothing()
    {
        var aggregate = new TestAggregate(Id1);
        var publisher = new RecordingPublisher();

        await publisher.DispatchAggregateEventsAsync(aggregate, TestContext.Current.CancellationToken);

        publisher.Published.Should().BeEmpty();
        aggregate.IsChanged.Should().BeFalse();
    }

    [Fact]
    public async Task Events_raised_during_dispatch_are_picked_up_on_next_wave()
    {
        var aggregate = new TestAggregate(Id1);
        var first = new TestEventA("first", DateTimeOffset.UtcNow);
        var cascaded = new TestEventB(99, DateTimeOffset.UtcNow);
        aggregate.RaiseEvent(first);

        var publisher = new RecordingPublisher
        {
            OnPublishing = evt =>
            {
                if (ReferenceEquals(evt, first))
                    aggregate.RaiseEvent(cascaded);
            },
        };

        await publisher.DispatchAggregateEventsAsync(aggregate, TestContext.Current.CancellationToken);

        publisher.Published.Should().Equal(first, cascaded);
        aggregate.UncommittedEvents().Should().BeEmpty();
    }

    [Fact]
    public async Task Exceeding_wave_cap_throws_and_leaves_undispatched_events_on_aggregate()
    {
        var aggregate = new TestAggregate(Id1);
        // Seed with one event; have the publisher re-raise on every publish so each wave keeps
        // producing new events that exceed the 8-wave cap.
        aggregate.RaiseEvent(new TestEventA("wave-0", DateTimeOffset.UtcNow));

        var publisher = new RecordingPublisher
        {
            OnPublishing = _ => aggregate.RaiseEvent(new TestEventA("cascade", DateTimeOffset.UtcNow)),
        };

        var act = async () => await publisher.DispatchAggregateEventsAsync(aggregate, TestContext.Current.CancellationToken);

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Contain("exceeded");
        ex.Which.Message.Should().Contain(typeof(TestAggregate).FullName);

        // 8 waves of 1 event each = 8 published; the next-wave event raised by the last publish
        // pushes one undispatched event onto the aggregate.
        publisher.Published.Should().HaveCount(8);
        aggregate.UncommittedEvents().Should().NotBeEmpty(
            "the helper leaves undispatched events on the aggregate when the cap is exceeded so the caller can inspect them");
        aggregate.IsChanged.Should().BeTrue("AcceptChanges is not called when the cap is exceeded");
    }

    [Fact]
    public async Task Cancellation_mid_loop_throws_and_skips_accept_changes()
    {
        var aggregate = new TestAggregate(Id1);
        var first = new TestEventA("first", DateTimeOffset.UtcNow);
        var second = new TestEventA("second", DateTimeOffset.UtcNow);
        var third = new TestEventA("third", DateTimeOffset.UtcNow);
        aggregate.RaiseEvent(first);
        aggregate.RaiseEvent(second);
        aggregate.RaiseEvent(third);

        using var cts = new CancellationTokenSource();
        var publisher = new RecordingPublisher
        {
            OnPublishing = evt =>
            {
                if (ReferenceEquals(evt, second))
                    cts.Cancel();
            },
        };

        var act = async () => await publisher.DispatchAggregateEventsAsync(aggregate, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        publisher.Published.Should().Equal(first, second);
        aggregate.UncommittedEvents().Should().Equal(
            new IDomainEvent[] { first, second, third },
            "AcceptChanges never runs on cancellation, so the entire event list stays on the aggregate; handlers must be idempotent because a retry will re-publish events that already fired before cancellation");
    }

    [Fact]
    public async Task Pre_canceled_token_throws_before_any_publish()
    {
        var aggregate = new TestAggregate(Id1);
        aggregate.RaiseEvent(new TestEventA("should-not-publish", DateTimeOffset.UtcNow));

        var publisher = new RecordingPublisher();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var act = async () => await publisher.DispatchAggregateEventsAsync(aggregate, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        publisher.Published.Should().BeEmpty();
        aggregate.UncommittedEvents().Should().HaveCount(1);
    }

    [Fact]
    public async Task Calling_twice_does_not_re_publish_first_call_events()
    {
        var aggregate = new TestAggregate(Id1);
        var firstWave = new TestEventA("first-wave", DateTimeOffset.UtcNow);
        aggregate.RaiseEvent(firstWave);

        var publisher = new RecordingPublisher();

        await publisher.DispatchAggregateEventsAsync(aggregate, TestContext.Current.CancellationToken);

        publisher.Published.Should().HaveCount(1);

        // Second call with a fresh event; the first-call event is already cleared by AcceptChanges.
        var secondWave = new TestEventB(7, DateTimeOffset.UtcNow);
        aggregate.RaiseEvent(secondWave);

        await publisher.DispatchAggregateEventsAsync(aggregate, TestContext.Current.CancellationToken);

        publisher.Published.Should().Equal(firstWave, secondWave);
        aggregate.UncommittedEvents().Should().BeEmpty();
    }

    [Fact]
    public async Task Null_publisher_throws_argument_null()
    {
        var aggregate = new TestAggregate(Id1);
        IDomainEventPublisher publisher = null!;

        var act = async () => await publisher.DispatchAggregateEventsAsync(aggregate, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>().Where(e => e.ParamName == "publisher");
    }

    [Fact]
    public async Task Null_aggregate_throws_argument_null()
    {
        var publisher = new RecordingPublisher();

        var act = async () => await publisher.DispatchAggregateEventsAsync(null!, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>().Where(e => e.ParamName == "aggregate");
    }

    [Fact]
    public async Task Cap_matches_pipeline_behavior_constant()
    {
        // Guardrail: the helper's local MaxDispatchWaves constant MUST match the pipeline
        // behavior's public constant. This test fails loudly if they drift apart so reviewers
        // notice during a refactor.
        var aggregate = new TestAggregate(Id1);
        aggregate.RaiseEvent(new TestEventA("seed", DateTimeOffset.UtcNow));

        var publisher = new RecordingPublisher
        {
            OnPublishing = _ => aggregate.RaiseEvent(new TestEventA("cascade", DateTimeOffset.UtcNow)),
        };

        var act = async () => await publisher.DispatchAggregateEventsAsync(aggregate, TestContext.Current.CancellationToken);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // After cap exceeded, exactly the cap's worth of events were published.
        publisher.Published.Should().HaveCount(DomainEventDispatchBehavior<AggregateCommand, Result<TestAggregate>>.MaxDispatchWaves);
    }

    [Fact]
    public async Task Publisher_exception_propagates_and_skips_accept_changes()
    {
        // Locks in the documented contract: if the IDomainEventPublisher implementation propagates
        // a handler exception (rather than swallowing it like MediatorDomainEventPublisher does),
        // the helper rethrows and AcceptChanges() is NOT called so undispatched events remain on
        // the aggregate for the caller to inspect.
        var aggregate = new TestAggregate(Id1);
        var first = new TestEventA("first", DateTimeOffset.UtcNow);
        var second = new TestEventA("second", DateTimeOffset.UtcNow);
        aggregate.RaiseEvent(first);
        aggregate.RaiseEvent(second);

        var publisher = new ThrowingPublisher(throwOn: second, new InvalidOperationException("handler-blew-up"));

        var act = async () => await publisher.DispatchAggregateEventsAsync(aggregate, TestContext.Current.CancellationToken);

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Be("handler-blew-up");
        publisher.Published.Should().Equal(first);
        aggregate.UncommittedEvents().Should().Equal(
            new IDomainEvent[] { first, second },
            "AcceptChanges() is not called when a publisher propagates a handler exception, so the entire event list stays on the aggregate");
        aggregate.IsChanged.Should().BeTrue();
    }

    private sealed class RecordingPublisher : IDomainEventPublisher
    {
        public List<IDomainEvent> Published { get; } = [];

        public Action<IDomainEvent>? OnPublishing { get; set; }

        public ValueTask PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            OnPublishing?.Invoke(domainEvent);
            Published.Add(domainEvent);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class ThrowingPublisher : IDomainEventPublisher
    {
        private readonly IDomainEvent _throwOn;
        private readonly Exception _exception;

        public ThrowingPublisher(IDomainEvent throwOn, Exception exception)
        {
            _throwOn = throwOn;
            _exception = exception;
        }

        public List<IDomainEvent> Published { get; } = [];

        public ValueTask PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            if (ReferenceEquals(domainEvent, _throwOn))
                throw _exception;

            Published.Add(domainEvent);
            return ValueTask.CompletedTask;
        }
    }
}
