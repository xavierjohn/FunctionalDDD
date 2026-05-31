namespace Trellis.Testing.Worker.Tests;

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Trellis.Authorization;
using Trellis.Mediator;
using Trellis.Testing.Worker.Tests.Helpers;

public class WorkerHarnessTests
{
    [Fact]
    public async Task CreateAsync_with_defaults_does_not_start_the_host()
    {
        await using var harness = await CreateMinimalHarnessAsync();

        var lifetime = harness.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.IsCancellationRequested.Should().BeFalse(
            "AutoStart defaults to false so tests can subscribe before ticks begin");
    }

    [Fact]
    public async Task AutoStart_true_starts_the_host_before_returning()
    {
        await using var harness = await WorkerHarness<TestWorker>.CreateAsync(
            ConfigureMinimalWorker(autoStart: true),
            TestContext.Current.CancellationToken);

        var lifetime = harness.Services.GetRequiredService<IHostApplicationLifetime>();
        await WaitForApplicationStartedAsync(lifetime, TestContext.Current.CancellationToken);
        lifetime.ApplicationStarted.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public async Task WaitForEventAsync_returns_event_dispatched_after_time_advance()
    {
        await using var harness = await CreateMinimalHarnessAsync(TimeSpan.FromMinutes(5));

        await harness.StartAsync(TestContext.Current.CancellationToken);
        await harness.SettleAsync(cancellationToken: TestContext.Current.CancellationToken);

        harness.Time.Advance(TimeSpan.FromMinutes(5));

        var first = await harness.WaitForEventAsync<TestTickCompletedEvent>(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        first.Iteration.Should().Be(1);
        first.OccurredAt.Should().Be(WorkerHarnessOptions.DefaultTestStartInstant + TimeSpan.FromMinutes(5));
        harness.Events<TestTickCompletedEvent>().Should().HaveCount(1);
    }

    [Fact]
    public async Task WaitForEventAsync_returns_pre_captured_event_without_waiting()
    {
        await using var harness = await CreateMinimalHarnessAsync(TimeSpan.FromMinutes(1));

        await harness.StartAsync(TestContext.Current.CancellationToken);
        await harness.SettleAsync(cancellationToken: TestContext.Current.CancellationToken);

        harness.Time.Advance(TimeSpan.FromMinutes(1));
        var first = await harness.WaitForEventAsync<TestTickCompletedEvent>(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);
        first.Iteration.Should().Be(1);

        var stopwatch = Stopwatch.StartNew();
        var second = await harness.WaitForEventAsync<TestTickCompletedEvent>(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);
        stopwatch.Stop();

        second.Should().BeSameAs(first);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(500));
    }

    [Fact]
    public async Task WaitForEventAsync_with_predicate_returns_first_matching_event()
    {
        await using var harness = await CreateMinimalHarnessAsync(TimeSpan.FromMinutes(1));

        await harness.StartAsync(TestContext.Current.CancellationToken);
        await harness.SettleAsync(cancellationToken: TestContext.Current.CancellationToken);

        for (var i = 1; i <= 3; i++)
        {
            harness.Time.Advance(TimeSpan.FromMinutes(1));
            _ = await harness.WaitForEventAsync<TestTickCompletedEvent>(
                e => e.Iteration == i,
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);
        }

        var matched = await harness.WaitForEventAsync<TestTickCompletedEvent>(
            e => e.Iteration == 3,
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        matched.Iteration.Should().Be(3);
    }

    [Fact]
    public async Task WaitForEventAsync_propagates_live_predicate_exception_as_task_failure()
    {
        await using var harness = await CreateMinimalHarnessAsync(TimeSpan.FromMinutes(1));

        await harness.StartAsync(TestContext.Current.CancellationToken);
        await harness.SettleAsync(cancellationToken: TestContext.Current.CancellationToken);

        var sentinel = new InvalidOperationException("predicate sentinel");
        var waitTask = harness.WaitForEventAsync<TestTickCompletedEvent>(
            _ => throw sentinel,
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        harness.Time.Advance(TimeSpan.FromMinutes(1));

        var act = async () => await waitTask;
        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Should().BeSameAs(sentinel,
            "a predicate that throws on the publisher path must surface to the test instead of timing out");
    }

    [Fact]
    public async Task WaitForEventAsync_throws_WorkerHarnessTimeoutException_when_no_event_arrives()
    {
        await using var harness = await CreateMinimalHarnessAsync(TimeSpan.FromHours(1));

        await harness.StartAsync(TestContext.Current.CancellationToken);

        var act = async () => await harness.WaitForEventAsync<OtherTestEvent>(
            TimeSpan.FromMilliseconds(50),
            TestContext.Current.CancellationToken);

        var ex = await act.Should().ThrowAsync<WorkerHarnessTimeoutException>();
        ex.Which.Message.Should().Contain(nameof(OtherTestEvent));
    }

    [Fact]
    public async Task WaitForEventAsync_propagates_caller_cancellation_as_OperationCanceledException()
    {
        await using var harness = await CreateMinimalHarnessAsync(TimeSpan.FromHours(1));

        await harness.StartAsync(TestContext.Current.CancellationToken);

        using var cts = new CancellationTokenSource();
        var waitTask = harness.WaitForEventAsync<OtherTestEvent>(TimeSpan.FromMinutes(1), cts.Token);

        await cts.CancelAsync();
        var act = async () => await waitTask;
        var thrown = await act.Should().ThrowAsync<OperationCanceledException>();
        thrown.Which.Should().NotBeOfType<WorkerHarnessTimeoutException>();
    }

    [Fact]
    public async Task Events_returns_snapshot_unaffected_by_subsequent_dispatches()
    {
        await using var harness = await CreateMinimalHarnessAsync(TimeSpan.FromMinutes(1));

        await harness.StartAsync(TestContext.Current.CancellationToken);
        await harness.SettleAsync(cancellationToken: TestContext.Current.CancellationToken);

        harness.Time.Advance(TimeSpan.FromMinutes(1));
        _ = await harness.WaitForEventAsync<TestTickCompletedEvent>(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        var snapshot = harness.Events<TestTickCompletedEvent>();
        snapshot.Should().HaveCount(1);

        harness.Time.Advance(TimeSpan.FromMinutes(1));
        _ = await harness.WaitForEventAsync<TestTickCompletedEvent>(
            e => e.Iteration == 2,
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        snapshot.Should().HaveCount(1, "the snapshot returned earlier must not reflect later captures");
        harness.Events<TestTickCompletedEvent>().Should().HaveCount(2);
    }

    [Fact]
    public async Task WaitForTickAsync_releases_after_tick_signal()
    {
        await using var harness = await CreateMinimalHarnessAsync(TimeSpan.FromMinutes(2));

        await harness.StartAsync(TestContext.Current.CancellationToken);
        await harness.SettleAsync(cancellationToken: TestContext.Current.CancellationToken);

        harness.Time.Advance(TimeSpan.FromMinutes(2));
        await harness.WaitForTickAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task WaitForTickAsync_with_name_matches_only_named_signal()
    {
        const string named = "reminder-job";

        await using var harness = await WorkerHarness<TestWorker>.CreateAsync(
            ConfigureMinimalWorker(TimeSpan.FromMinutes(1), tickName: named),
            TestContext.Current.CancellationToken);

        await harness.StartAsync(TestContext.Current.CancellationToken);
        await harness.SettleAsync(cancellationToken: TestContext.Current.CancellationToken);

        harness.Time.Advance(TimeSpan.FromMinutes(1));
        await harness.WaitForTickAsync(named, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task WaitForTickAsync_throws_with_recorded_signals_in_message_on_timeout()
    {
        await using var harness = await WorkerHarness<TestWorker>.CreateAsync(
            ConfigureMinimalWorker(TimeSpan.FromMinutes(1), tickName: "actual"),
            TestContext.Current.CancellationToken);

        await harness.StartAsync(TestContext.Current.CancellationToken);
        await harness.SettleAsync(cancellationToken: TestContext.Current.CancellationToken);

        harness.Time.Advance(TimeSpan.FromMinutes(1));
        await harness.WaitForTickAsync("actual", TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        var act = async () => await harness.WaitForTickAsync(
            "expected",
            TimeSpan.FromMilliseconds(50),
            TestContext.Current.CancellationToken);

        var ex = await act.Should().ThrowAsync<WorkerHarnessTimeoutException>();
        ex.Which.Message.Should().Contain("expected").And.Contain("actual");
    }

    [Fact]
    public async Task SeedAsync_runs_before_host_starts_and_inside_dedicated_scope()
    {
        var seedRan = false;
        IServiceProvider? seedScopeServices = null;

        await using var harness = await WorkerHarness<TestWorker>.CreateAsync(
            opts =>
            {
                ApplyMinimalRegistrations(opts);
                opts.SeedAsync((sp, _) =>
                {
                    seedRan = true;
                    seedScopeServices = sp;
                    return Task.CompletedTask;
                });
            },
            TestContext.Current.CancellationToken);

        seedRan.Should().BeTrue();
        seedScopeServices.Should().NotBeSameAs(harness.Services,
            "the harness must invoke seeds inside a dedicated DI scope");
        harness.Services.GetRequiredService<IHostApplicationLifetime>()
            .ApplicationStarted.IsCancellationRequested
            .Should().BeFalse("seeds must run before StartAsync");
    }

    [Fact]
    public async Task SystemActor_overrides_the_default_actor_returned_by_provider()
    {
        var custom = Actor.Create(
            "subscription-renewal-worker",
            new HashSet<string>(["Subscriptions.Read", "Subscriptions.Write"]));

        await using var harness = await WorkerHarness<TestWorker>.CreateAsync(
            opts =>
            {
                opts.SystemActor = custom;
                ApplyMinimalRegistrations(opts);
            },
            TestContext.Current.CancellationToken);

        var provider = harness.Services.GetRequiredService<IActorProvider>();
        var resolved = await provider.GetCurrentActorAsync(TestContext.Current.CancellationToken);
        resolved.HasValue.Should().BeTrue();
        resolved.Value.Should().BeSameAs(custom);
    }

    [Fact]
    public async Task CreateAsync_throws_when_worker_is_already_registered_as_hosted_service()
    {
        var act = async () => await WorkerHarness<TestWorker>.CreateAsync(
            opts =>
            {
                ApplyMinimalRegistrations(opts);
                opts.ConfigureServices(s => s.AddHostedService<TestWorker>());
            },
            TestContext.Current.CancellationToken);

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Contain(nameof(TestWorker));
    }

    [Fact]
    public async Task Capture_handler_does_not_displace_user_registered_handlers()
    {
        var userHandler = new RecordingTickHandler();

        await using var harness = await WorkerHarness<TestWorker>.CreateAsync(
            opts =>
            {
                ApplyMinimalRegistrations(opts, TimeSpan.FromMinutes(1));
                opts.ConfigureServices(s =>
                    s.AddScoped<IDomainEventHandler<TestTickCompletedEvent>>(_ => userHandler));
            },
            TestContext.Current.CancellationToken);

        await harness.StartAsync(TestContext.Current.CancellationToken);
        await harness.SettleAsync(cancellationToken: TestContext.Current.CancellationToken);

        harness.Time.Advance(TimeSpan.FromMinutes(1));
        var captured = await harness.WaitForEventAsync<TestTickCompletedEvent>(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        userHandler.Received.Should().ContainSingle()
            .Which.Iteration.Should().Be(captured.Iteration);
    }

    [Fact]
    public async Task DisposeAsync_is_safe_to_call_when_host_was_never_started()
    {
        var harness = await CreateMinimalHarnessAsync();

        await harness.DisposeAsync();
        await harness.DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_stops_the_host_when_started()
    {
        var harness = await WorkerHarness<TestWorker>.CreateAsync(
            ConfigureMinimalWorker(autoStart: true),
            TestContext.Current.CancellationToken);

        var lifetime = harness.Services.GetRequiredService<IHostApplicationLifetime>();
        await WaitForApplicationStartedAsync(lifetime, TestContext.Current.CancellationToken);

        await harness.DisposeAsync();
        lifetime.ApplicationStopped.IsCancellationRequested.Should().BeTrue();
    }

    private static Task<WorkerHarness<TestWorker>> CreateMinimalHarnessAsync(TimeSpan? interval = null) =>
        WorkerHarness<TestWorker>.CreateAsync(
            ConfigureMinimalWorker(interval),
            TestContext.Current.CancellationToken);

    private static Action<WorkerHarnessOptions> ConfigureMinimalWorker(
        TimeSpan? interval = null,
        string tickName = "",
        bool autoStart = false) =>
        opts =>
        {
            if (autoStart) opts.AutoStart = true;
            ApplyMinimalRegistrations(opts, interval, tickName);
        };

    private static void ApplyMinimalRegistrations(
        WorkerHarnessOptions options,
        TimeSpan? interval = null,
        string tickName = "") =>
        options.ConfigureServices(services =>
        {
            services.AddLogging();
            services.AddDomainEventDispatch();
            services.AddSingleton(new TestWorkerOptions
            {
                Interval = interval ?? TimeSpan.FromMinutes(5),
                TickName = tickName,
            });
        });

    private static async Task WaitForApplicationStartedAsync(
        IHostApplicationLifetime lifetime,
        CancellationToken cancellationToken)
    {
        if (lifetime.ApplicationStarted.IsCancellationRequested)
            return;

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var registration = lifetime.ApplicationStarted.Register(() => tcs.TrySetResult());
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
        await using var cancelReg = timeoutCts.Token.Register(() => tcs.TrySetCanceled(timeoutCts.Token));
        await tcs.Task;
    }

    private sealed class RecordingTickHandler : IDomainEventHandler<TestTickCompletedEvent>
    {
        public List<TestTickCompletedEvent> Received { get; } = [];

        public ValueTask HandleAsync(TestTickCompletedEvent domainEvent, CancellationToken cancellationToken)
        {
            Received.Add(domainEvent);
            return ValueTask.CompletedTask;
        }
    }
}
