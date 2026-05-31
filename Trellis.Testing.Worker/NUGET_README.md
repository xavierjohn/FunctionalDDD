# Trellis.Testing.Worker

[![NuGet Package](https://img.shields.io/nuget/v/Trellis.Testing.Worker.svg)](https://www.nuget.org/packages/Trellis.Testing.Worker)

Integration-test harness for `BackgroundService` workers built on Trellis.

## Installation
```bash
dotnet add package Trellis.Testing.Worker
```

## Quick Example
```csharp
using Microsoft.Extensions.DependencyInjection;
using Trellis.Authorization;
using Trellis.Mediator;
using Trellis.Testing.Worker;

var systemActor = Actor.Create(
    "subscription-renewal-worker",
    new HashSet<string>(["Subscriptions.Read", "Subscriptions.Write"]));

await using var harness = await WorkerHarness<SubscriptionRenewalWorker>.CreateAsync(opts =>
{
    opts.SystemActor = systemActor;
    opts.ConfigureServices(services =>
    {
        services.AddLogging();
        services.AddDomainEventDispatch();   // same registration as production
        services.AddSingleton<ISubscriptionRepository, FakeSubscriptionRepository>();
        services.AddScoped<IExternalGateway, FakeExternalGateway>();
    });
    opts.SeedAsync(async (sp, ct) =>
    {
        var repo = sp.GetRequiredService<ISubscriptionRepository>();
        await repo.AddAsync(new Subscription { RenewsAt = opts.InitialTime.AddDays(1) }, ct);
    });
});

await harness.StartAsync();
await harness.SettleAsync(); // yield real time so the worker registers its first Task.Delay
harness.Time.Advance(TimeSpan.FromDays(1));

var reminded = await harness.WaitForEventAsync<SubscriptionReminderSent>();
reminded.SubscriptionId.Should().Be(expectedId);
```

> **Deterministic alternative to `SettleAsync()`:** have the worker call
> `IWorkerTickSignal.SignalAsync("ready", ct)` at the top of `ExecuteAsync` before its first
> `Task.Delay`, then replace `await harness.SettleAsync()` with
> `await harness.WaitForTickAsync("ready")` — no real-time yield, no flakiness.
>
> **Wiring EF Core with SQLite?** Don't pair `AddDbContextFactory<T>` with
> `Data Source=:memory:` — every new connection opens a fresh database, so the seed and the
> worker see different empty stores. Use a shared/open `SqliteConnection` (see the
> [integration-testing article](https://xavierjohn.github.io/Trellis/articles/integration-testing.html#background-workers))
> or a temp-file SQLite database.

## Key Features
- **`IHost` with a deterministic `FakeTimeProvider`** — advance the clock with `harness.Time.Advance(...)` to drive `Task.Delay(interval, timeProvider, ct)` continuations on demand.
- **`TestActorProvider` registered as the `IActorProvider`** — gives workers an ambient system actor without `HttpContext`. Override with `opts.SystemActor`.
- **Domain-event capture** — every event published through Trellis's mediator pipeline is captured. Inspect via `harness.Events<TEvent>()`; await one via `harness.WaitForEventAsync<TEvent>(predicate)`.
- **Optional tick signal** — workers that emit no domain events can resolve `IWorkerTickSignal` and call `SignalAsync(name)` at the end of each tick so the test can block on `harness.WaitForTickAsync(name)`.
- **Race-proof waits** — `WaitForEventAsync` and `WaitForTickAsync` snapshot existing captures before subscribing so events that fire between `Advance(...)` and the wait still satisfy the wait.
- **Real-time timeouts** — wait timeouts are measured against the real clock, not the fake one; `harness.Time.Advance(...)` does not consume the timeout budget.

## What the harness does *not* do
- It does **not** call `services.AddDomainEventDispatch()` — worker tests are integration tests of the production composition root. Register it in your `ConfigureServices` callback the same way the worker's production host does.
- It does **not** wire `FakeLogger`. Add `Microsoft.Extensions.Diagnostics.Testing` and `opts.ConfigureLogging(b => b.AddFakeLogging())` in your test if you need it.

## Documentation
- [Full documentation](https://xavierjohn.github.io/Trellis/articles/integration-testing.html)
- [API Reference](https://xavierjohn.github.io/Trellis/api/index.html)

## Part of Trellis
This package is part of the [Trellis](https://github.com/xavierjohn/Trellis) framework.
