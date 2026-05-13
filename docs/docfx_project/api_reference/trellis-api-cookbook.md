---
package: Trellis (cross-package recipes)
namespaces: [Trellis, Trellis.Asp, Trellis.EntityFrameworkCore, Trellis.Mediator]
types: [recipes]
related_docs: [trellis-api-core.md, trellis-api-asp.md, trellis-api-efcore.md, trellis-api-mediator.md]
version: v3
last_verified: 2026-05-01
audience: [llm]
---
# Trellis Cross-Package Cookbook

- **Audience:** AI coding agents (and humans) writing Trellis code from documentation alone.
- **Purpose:** End-to-end recipes that cross package boundaries — DDD, Mediator, FluentValidation, EF Core, ASP.NET Core, Authorization, State Machine, Testing, Analyzers — using the *exact* public surface listed in the per-package API references.
- **Companion docs:**
  - [trellis-api-core.md](trellis-api-core.md) — `Result<T>`, `Maybe<T>`, errors, primitives, pagination
  - [trellis-api-primitives.md](trellis-api-primitives.md) — `RequiredString`, `RequiredGuid`, `[Range]`, `[StringLength]`
  - [trellis-api-mediator.md](trellis-api-mediator.md) — `ICommand<T>`, `IQuery<T>`, `IPipelineBehavior<,>`, `AddTrellisBehaviors`
  - [trellis-api-fluentvalidation.md](trellis-api-fluentvalidation.md) — `AddTrellisFluentValidation`
  - [trellis-api-efcore.md](trellis-api-efcore.md) — `SaveChangesResultAsync`, `MaybePropertyMapping`, `RepositoryBase<TAggregate,TId>`
  - [trellis-api-asp.md](trellis-api-asp.md) — `ToHttpResponse`, `HttpResponseOptionsBuilder<T>`, `AddTrellisAsp`, `AsActionResult`
  - [trellis-api-http.md](trellis-api-http.md) — `ToResultAsync`, `ReadJsonAsync`, `ReadJsonOrNoneOn404Async`
  - [trellis-api-authorization.md](trellis-api-authorization.md) — `IActorProvider`, `IAuthorize`, `IAuthorizeResource<>`
  - [trellis-api-servicedefaults.md](trellis-api-servicedefaults.md) — `AddTrellis`, `TrellisServiceBuilder`
  - [trellis-api-statemachine.md](trellis-api-statemachine.md) — `FireResult`, `LazyStateMachine<,>`
  - [trellis-api-testing-reference.md](trellis-api-testing-reference.md) — `Should().Be(...)`, `UnwrapError()`
  - [trellis-api-testing-aspnetcore.md](trellis-api-testing-aspnetcore.md) — `WebApplicationFactoryExtensions`, `.http` replay helpers
  - [trellis-api-analyzers.md](trellis-api-analyzers.md) — `TRLS001`-`TRLS039`, `TrellisDiagnosticIds`

## How to read these recipes

Every recipe follows the same shape:

1. **Problem statement** — what the consumer is trying to accomplish.
2. **Solution code** — copy-pasteable C# that compiles against the documented public surface only. No invented APIs.
3. **What it shows** — the cross-cutting concept being demonstrated.
4. **Anti-pattern → fix** *(when applicable)* — the wrong way and which Trellis analyzer catches it.

Conventions used throughout:

- All Trellis types live in the `Trellis` namespace except where called out (`Trellis.Asp`, `Trellis.Asp.Authorization`, `Trellis.EntityFrameworkCore`, `Trellis.Analyzers`).
- Snippets use C# 12+ features (file-scoped namespaces, primary constructors, collection expressions) — Trellis targets `net10.0`.
- `Result.Ok` / `Result.Fail` are *the* construction APIs. `default(Result<T>)` is a typed failure; do not rely on it as success.
- Every async pipeline uses `*Async` extensions; mixing sync chain methods with `Task<Result<T>>` triggers `TRLS009`.
- Examples reference an `OrderId : RequiredGuid<OrderId>` value object and an `Order` aggregate. Substitute your own types without changing the structure.

## LLM preflight: load the smallest correct reference set

Before writing Trellis code, choose the task in the lookup table below, then load only the package references needed for that task. The cookbook gives the end-to-end recipe; the package references are the source of truth for exact signatures, overloads, ordering, and edge-case behavior.

| If you are changing... | Load these references before coding | Why |
|---|---|---|
| Result, Maybe, errors, value-object bases, aggregates, specifications, pagination | `trellis-api-cookbook.md`, `trellis-api-core.md` | Core owns the ROP primitives and DDD base types used by every package. |
| ASP.NET endpoints, controllers, response mapping, ETags, Prefer, ranges, actor providers | `trellis-api-cookbook.md`, `trellis-api-asp.md`, `trellis-api-core.md`; add `trellis-api-mediator.md` when endpoints send messages | `ToHttpResponse` and scalar validation are ASP-owned, while handlers and result shapes come from Core/Mediator. |
| Mediator handlers, pipeline behaviors, validation, authorization, domain events | `trellis-api-cookbook.md`, `trellis-api-mediator.md`, `trellis-api-core.md`; add `trellis-api-efcore.md` for unit-of-work and `trellis-api-authorization.md` for resource guards | Pipeline ordering and opt-in behaviors are cross-package; missing one reference usually creates a registration-order bug. |
| EF Core persistence, repositories, unit of work, `Maybe<T>` queries, `[OwnedEntity]` | `trellis-api-cookbook.md`, `trellis-api-efcore.md`, `trellis-api-core.md`; add `trellis-api-mediator.md` when commits happen through handlers | EF owns mapping/interceptors; Mediator owns when command commits run. |
| FluentValidation integration | `trellis-api-cookbook.md`, `trellis-api-fluentvalidation.md`, `trellis-api-mediator.md` | FluentValidation plugs into `ValidationBehavior` through `IMessageValidator<TMessage>`; it is not a separate pipeline behavior. |
| Composition-root helpers (`AddTrellis`, `UseXxx`) | `trellis-api-cookbook.md`, `trellis-api-servicedefaults.md`, plus every package reference for selected modules | `TrellisServiceBuilder` preserves canonical order but does not register app-owned services like `DbContext` or Mediator handlers. |
| HTTP client adapters | `trellis-api-cookbook.md`, `trellis-api-http.md`, `trellis-api-core.md` | The HTTP package maps upstream responses into Core `Result<T>` / `Maybe<T>` shapes. |
| Tests | `trellis-api-testing-reference.md`; add `trellis-api-testing-aspnetcore.md` for `WebApplicationFactory` or `.http` replay | Unit/helper assertions and ASP integration helpers live in separate test packages. |
| Analyzer diagnostics | `trellis-api-anti-patterns.md` first for the canonical WRONG/FIX shape to adapt, then `trellis-api-analyzers.md`, then the package reference named by the diagnostic category | Anti-pattern file shows the canonical control-flow shape; analyzer docs explain the warning; the package reference gives the canonical API to use instead. |

Measurable completion check for generated code: every Trellis method call should be traceable to a loaded package reference, every selected integration module should be wired in the documented order, and every public API or behavior change should update the matching package reference plus this cookbook when it affects a cross-package recipe.

Known non-APIs and corrected assumptions:

| Do not write | Correct source-backed statement |
|---|---|
| `WithDocumentPerVersion()` | No Trellis API with this name exists. |
| `MapScalarApiReference()` | Sample-app helper only; not a Trellis framework API. |
| Place `UseScalarValueValidation()` anywhere | Add it before routing/endpoints that deserialize request bodies. |
| Mutate `IAuthorize.RequiredPermissions` | `RequiredPermissions` is an `IReadOnlyList<string>`. |
| `IValidate.Validate()` returns `Result` | The declared return type is `IResult`. |

## Patterns Index

### Task -> recipe lookup

Use this table before writing code. If a task matches a row, read that recipe first.

| Task | Start here |
|---|---|
| Create or load an aggregate with value objects | [Recipe 1](#recipe-1--crud-aggregate-ddd-value-objects--entity--repository-contract) |
| Write a command handler that validates and persists | [Recipe 2](#recipe-2--command--handler--fluentvalidation--ef-persistence), then [Recipe 16](#recipe-16--unit-of-work-in-handlers-add-staging-vs-immediate-saveasync) |
| Load multiple independent resources in one handler (HTTP + DB, two upstream services, factory-created `DbContext`s) | [Recipe 21](#recipe-21--parallel-independent-loads-in-handlers-resultparallelasync--whenallasync) |
| Multi-aggregate orchestration: side effect per element of a related-aggregate set | [Recipe 22](#recipe-22--multi-aggregate-orchestration-fail-loud-on-missing-related-aggregates) |
| Concurrency control on mutating endpoints — when to require `If-Match` | [Recipe 23](#recipe-23--concurrency-control-on-aggregate-mutating-endpoints-when-to-require-if-match) |
| Add a paginated list query | [Recipe 3](#recipe-3--query-handler-returning-paget-paginated-list-with-cursor) |
| Add Minimal API or MVC endpoints | [Recipe 4](#recipe-4--minimal-api-endpoint-wiring-resultt--httpresponseoptionsbuilder--tohttpresponse), [Recipe 5](#recipe-5--mvc-controller-using-asactionresult) |
| Map primitive DTO fields to value objects | [Recipe 18](#recipe-18--dto-primitives-to-value-object-command-no-test-only-unwrap) |
| Add resource authorization | [Recipe 7](#recipe-7--authorization-iactorprovider--iauthorize--resource-based-auth) |
| Map `Maybe<T>` or composite value objects with EF Core | [Recipe 8](#recipe-8--ef-core-maybepropertymapping-for-nullable-value-objects), [Recipe 13](#recipe-13--composite-value-object-end-to-end-domain--api-json-binding--ef-core-ownership) |
| Add optional request/response fields | [Recipe 14](#recipe-14--optional-fields-in-request-dtos-maybetscalar-vs-nullable-transport) |
| Read optional HTTP resources where 404 means absent | [Recipe 19](#recipe-19--http-client-result-safety-and-optional-reads) |
| Choose between fail-fast and accumulating-error collection ops | [Recipe 20](#recipe-20--fail-fast-vs-accumulating-sequencetraverse-vs-sequencealltraverseall) |
| Return synchronous `Result` chains from `Task`/`ValueTask` APIs | [Recipe 2](#recipe-2--command--handler--fluentvalidation--ef-persistence), then `AsTask()` / `AsValueTask()` in [trellis-api-core.md](trellis-api-core.md) |
| Create HTTP-oriented resource errors | Use `ResourceRef.For<TResource>(id)` from [trellis-api-core.md](trellis-api-core.md) |
| Add a state transition | [Recipe 9](#recipe-9--state-machine-canfire--fire-pattern-with-fireresult) |
| Write handler/domain tests | [Recipe 10](#recipe-10--test-handler-test-using-trellistesting-shouldbe--unwraperror) |
| Define domain events | [Recipe 17](#recipe-17--defining-custom-domain-events-occurredat-is-the-only-timestamp) |
| Fix analyzer warnings | [Recipe 11](#recipe-11--anti-pattern--fix-gallery-the-analyzers-in-action) |
| Wire the composition root | [Recipe 12](#recipe-12--di-wiring-playbook-addtrellis-composition-builder) |

### Mistake-regression routing

These rows route recurring LLM lab mistakes to the most relevant reference before code is written.

| If the task involves... | Read first | Why |
|---|---|---|
| Loading independent resources before creating a command result | [Recipe 21](#recipe-21--parallel-independent-loads-in-handlers-resultparallelasync--whenallasync) | Sequential awaits over genuinely independent loads (HTTP + DB, two upstream services) serialise latency. `Result.ParallelAsync(...).WhenAllAsync()` is the framework idiom. **Do NOT use against repositories sharing a scoped `DbContext`** — that races EF Core and throws; sequential is correct there. |
| Overdue/date-filter queries over `Maybe<DateTime>` | [Recipe 8](#recipe-8--ef-core-maybepropertymapping-for-nullable-value-objects), then [trellis-api-efcore.md](trellis-api-efcore.md#patterns-index) | Keep a typed specification and use `MaybeQueryableExtensions` in EF queries. |
| State transitions on an aggregate | [Recipe 9](#recipe-9--state-machine-canfire--fire-pattern-with-fireresult), then [trellis-api-statemachine.md](trellis-api-statemachine.md#patterns-index) | Keep transition methods consistent and put domain mutation after `FireResult` succeeds. |
| Cross-aggregate mutation such as cancel/return releasing stock | [Recipe 1](#recipe-1--crud-aggregate-ddd-value-objects--entity--repository-contract), [Recipe 2](#recipe-2--command--handler--fluentvalidation--ef-persistence), and [trellis-api-core.md](trellis-api-core.md#domain-driven-design) | The application handler orchestrates multiple aggregates; an aggregate mutates only itself. |
| Result-returning ASP endpoints | [Recipe 4](#recipe-4--minimal-api-endpoint-wiring-resultt--httpresponseoptionsbuilder--tohttpresponse), [Recipe 5](#recipe-5--mvc-controller-using-asactionresult), then [trellis-api-asp.md](trellis-api-asp.md#patterns-index) | `AddTrellisAsp()` is required for Result-to-HTTP mapping; exception middleware is not the mapper. |
| Failure-code OpenAPI metadata or `.http` examples | [trellis-api-asp.md](trellis-api-asp.md#endpoint-checklist-for-generated-apis), [trellis-api-testing-aspnetcore.md](trellis-api-testing-aspnetcore.md#api-failure-path-test-checklist) | Generated APIs need failure paths, not happy-path-only docs/tests. |
| Resource authorization guards | [Recipe 7](#recipe-7--authorization-iactorprovider--iauthorize--resource-based-auth), then [trellis-api-authorization.md](trellis-api-authorization.md#patterns-index) | Use `Result.Ensure` for owner/admin boolean guards. |

---

## Recipe 1 — CRUD aggregate (DDD value objects + entity + repository contract)

**Problem.** Model an `Order` aggregate with a typed identifier, a value-object money type, and a repository contract that returns `Result<T>` for not-found.

```csharp
using Trellis;

// Strongly-typed ID: source-generated factory, equality, parsing, JSON converter.
public sealed partial class OrderId : RequiredGuid<OrderId>;

// Value object backed by a 3-letter ISO 4217 currency code.
[StringLength(3, MinimumLength = 3)]
public sealed partial class CurrencyCode : RequiredString<CurrencyCode>;

// Composite value object — must be a class (records can't inherit ValueObject).
public sealed class Money : ValueObject
{
    public Money(decimal amount, CurrencyCode currency) { Amount = amount; Currency = currency; }
    public decimal Amount { get; }
    public CurrencyCode Currency { get; }
    protected override IEnumerable<IComparable?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency.Value;
    }
}

// Aggregate root.
public sealed class Order : Aggregate<OrderId>
{
    public Money Total { get; private set; } = default!;
    public OrderStatus Status { get; private set; }

    private Order(OrderId id) : base(id) { }   // EF Core ctor

    public static Result<Order> Create(OrderId id, Money total) =>
        Result.Ok(new Order(id) { Total = total, Status = OrderStatus.Draft });
}

// Trellis convention: model finite domain states as RequiredEnum<TSelf>
// (NOT C# enums). The partial keyword triggers the source generator.
public partial class OrderStatus : RequiredEnum<OrderStatus>
{
    public static readonly OrderStatus Draft     = new();
    public static readonly OrderStatus Submitted = new();
    public static readonly OrderStatus Cancelled = new();
}

// Repository contract — uses Maybe<T> for "may legitimately find nothing"
// Reserve Result<T> for failures the caller can act on.
public interface IOrderRepository
{
    Task<Maybe<Order>> FindAsync(OrderId id, CancellationToken ct);
    void Add(Order order);
}
```

**What it shows.** Each base class in this recipe supplies a complete surface — your derived type adds only domain-specific state. **Do not redeclare members that are already inherited**; that is the most common Recipe 1 mistake.

- `RequiredGuid<TSelf>` source-generates `TryCreate` overloads, `Parse`/`TryParse`, an explicit `Guid` → `TSelf` operator, the `Value` accessor, equality / `GetHashCode` / `IComparable`, JSON and EF Core converters, plus the `NewUniqueV4()` and `NewUniqueV7()` factories. Do not write your own `TryCreate`, equality members, parse/convert helpers, or JSON/EF converters.
- `RequiredString<TSelf>` source-generates `TryCreate(string?, string?)`, `Parse`/`TryParse`, an explicit `string` → `TSelf` operator, the `Value` accessor, equality, JSON and EF Core converters, plus `Length`/`StartsWith`/`Contains`/`EndsWith` pass-throughs. Same rule applies: derived classes add only domain-specific helpers (e.g., a custom `TryCreateWithValidation` that layers extra rules on top of the generated `TryCreate`).
- `ValueObject` (the base of `Money`, `Address`, etc.) supplies `Equals(object?)`, `Equals(ValueObject?)`, `GetHashCode`, `CompareTo`, and the `==`/`!=`/`<`/`<=`/`>`/`>=` operators — all derived from `GetEqualityComponents()`. Your derived type implements **only** `protected override IEnumerable<IComparable?> GetEqualityComponents()`. Do not override `Equals`/`GetHashCode`/`CompareTo` or write equality operators yourself — that breaks the contract the base class establishes. For `Maybe<T>` components, use the inherited `protected static IComparable? MaybeComponent<T>(Maybe<T>)` helper rather than unwrapping manually.
- `Aggregate<TId>` already supplies inherited infrastructure members: `Id`, protected `DomainEvents`, persistence-managed `ETag`, and `IsChanged` based on pending domain events. Do not redeclare those members on every aggregate; use the inherited surface and add only domain-specific state. Domain events are added via `DomainEvents.Add(...)` from inside the aggregate; the public read-only view is `IAggregate.UncommittedEvents()`.

> **Compiled contract.** The exact signatures of every member listed above are exercised in `Examples/CookbookSnippets/Recipe01_CrudAggregate.cs` → `Recipe1InheritedSurface`. That file is compiled in CI, so if a signature changes in the framework, the build fails and this callout MUST be updated to match. When you need to confirm an exact overload, read the demonstrator — never paraphrase signatures from memory.

`[StringLength]` and `[Range]` come from the **`Trellis` namespace** and are placed on the **class declaration** — using `System.ComponentModel.DataAnnotations` versions silently compiles but is ignored by the Trellis source generator (`TRLS017`).

**Anti-pattern → fix (TRLS017).**

```csharp
// WRONG — using System.ComponentModel.DataAnnotations.StringLength
using System.ComponentModel.DataAnnotations;     // ← wrong namespace
[StringLength(3, MinimumLength = 3)]             // TRLS017
public sealed partial class CurrencyCode : RequiredString<CurrencyCode>;

// FIX
using Trellis;                                   // ← Trellis attributes
[StringLength(3, MinimumLength = 3)]             // generator now picks it up
public sealed partial class CurrencyCode : RequiredString<CurrencyCode>;
```

---

## Recipe 2 — Command + handler + FluentValidation + EF persistence

**Problem.** Wire a `PlaceOrderCommand` end-to-end: validation via FluentValidation, mediator handler that uses an EF repository, transactional commit on success.

```csharp
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Trellis;
using Trellis.Asp;
using Trellis.EntityFrameworkCore;
using Trellis.FluentValidation;
using Trellis.Mediator;
using Trellis.Primitives;

public sealed record PlaceOrderRequest(Guid OrderId, decimal Amount, string Currency);

public sealed record PlaceOrderCommand(OrderId OrderId, Money Total)
    : ICommand<Result<OrderId>>
{
    public static Result<PlaceOrderCommand> TryCreate(PlaceOrderRequest request) =>
        Result.Combine(
                OrderId.TryCreate(request.OrderId, nameof(request.OrderId)),
                MonetaryAmount.TryCreate(request.Amount, nameof(request.Amount)),
                CurrencyCode.TryCreate(request.Currency, nameof(request.Currency)))
            .Map((orderId, amount, currency) =>
                new PlaceOrderCommand(orderId, Money.Create(amount.Value, currency.Value)));
}

public sealed class PlaceOrderValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderValidator()
    {
        RuleFor(x => x.Total.Amount)
            .LessThanOrEqualTo(10_000m)
            .WithMessage("Orders over 10,000 require manual approval.");
    }
}

public sealed class PlaceOrderHandler(IOrderRepository repo)
    : ICommandHandler<PlaceOrderCommand, Result<OrderId>>
{
    public ValueTask<Result<OrderId>> Handle(PlaceOrderCommand cmd, CancellationToken cancellationToken) =>
        Order.Create(cmd.OrderId, cmd.Total)
            .Tap(repo.Add)
            .Map(o => o.Id)
            .AsValueTask();
}

[ApiController]
[Route("orders")]
public sealed class OrdersController(ISender sender) : ControllerBase
{
    [HttpPost]
    public ValueTask<ActionResult<OrderId>> Place([FromBody] PlaceOrderRequest request, CancellationToken ct) =>
        PlaceOrderCommand.TryCreate(request)
            .BindAsync(command => sender.Send(command, ct))
            .ToHttpResponseAsync()
            .AsActionResultAsync<OrderId>();
}

// Composition root
public static class OrdersDi
{
    public static IServiceCollection AddOrdersFeature(this IServiceCollection services) =>
        services
            .AddTrellisBehaviors()                              // Validation + logging + tracing
            .AddTrellisFluentValidation(typeof(PlaceOrderValidator).Assembly)
            .AddTrellisUnitOfWork<AppDbContext>()               // Innermost: commits on success
            .AddScoped<IOrderRepository, EfOrderRepository>();
}
```

**What it shows.** The mediator pipeline already runs `ValidationBehavior<TMessage, TResponse>` before the handler — `AddTrellisFluentValidation` plugs every `IValidator<T>` into it via the open-generic `IMessageValidator<T>` adapter. `AddTrellisUnitOfWork<TContext>` registers `TransactionalCommandBehavior<,>` *after* the others, so it lands innermost and commits only when the handler returns success. The handler itself is pure: no `try`/`catch`, no primitive parsing, no `await db.SaveChangesAsync()` — that's the unit of work's job.

> **Multiple independent resources in the handler?** Reach for [Recipe 21](#recipe-21--parallel-independent-loads-in-handlers-resultparallelasync--whenallasync) when the loads hit *different* stores (HTTP + DB, two upstream services, factory-created `DbContext`s). For two repository reads against the same scoped `DbContext`, stay sequential — that case races EF Core. The Recipe explains the rule and shows both shapes.

> **Validation ownership.** Primitive→VO conversion happens at the transport seam. FluentValidation validates VO-shaped commands for cross-field rules and business invariants. Handlers receive value-object-shaped commands and must not parse primitives. See [Recipe 18](#recipe-18--dto-primitives-to-value-object-command-no-test-only-unwrap) for the canonical controller-seam adapter.

**Anti-pattern → fix (TRLS010).**

```csharp
// WRONG — sync-over-async (.Result deadlocks) + throwing inside the Result chain.
.Bind(id => repo.FindAsync(id, ct).Result is { HasValue: true }
    ? throw new InvalidOperationException("already exists")  // TRLS010 + TRLS005
    : Result.Ok(id))

// FIX — MatchAsync awaits the Maybe carrier and dispatches without leaving the Result chain.
.BindAsync(id => repo.FindAsync(id, ct)
    .MatchAsync(
        some: _  => Result.Fail<OrderId>(new Error.Conflict(ResourceRef.For<Order>(id), "already_exists")),
        none: () => Result.Ok(id)))
```

---

## Recipe 3 — Query handler returning `Page<T>` (paginated list with cursor)

**Problem.** Expose a list endpoint that paginates `Order` rows by cursor, exposes the requested vs. applied limit, and projects a DTO.

```csharp
using Trellis;

// Paging cursor and limit are protocol/query-string controls validated at the transport seam.
public sealed record ListOrdersQuery(string? Cursor, int Limit) : IQuery<Result<Page<OrderListItem>>>;

public sealed record OrderListItem(Guid Id, decimal Amount, string Currency);

public sealed class ListOrdersHandler(AppDbContext db)
    : IQueryHandler<ListOrdersQuery, Result<Page<OrderListItem>>>
{
    private const int MaxLimit = 100;

    public async ValueTask<Result<Page<OrderListItem>>> Handle(ListOrdersQuery q, CancellationToken ct)
    {
        var requested = q.Limit;
        var applied   = Math.Clamp(requested, 1, MaxLimit);

        Guid afterId = Guid.Empty;
        if (q.Cursor is not null && !Guid.TryParseExact(q.Cursor, "N", out afterId))
            return Result.Fail<Page<OrderListItem>>(
                Error.UnprocessableContent.ForField("cursor", "cursor.malformed", "Cursor is not a valid opaque token."));

        var query = db.Orders.AsNoTracking().OrderBy(o => o.Id);
        if (q.Cursor is not null)
            query = query.Where(o => o.Id.Value > afterId);

        var rows = await query.Take(applied + 1).ToListAsync(ct);
        var hasNext = rows.Count > applied;
        var items   = rows.Take(applied)
                          .Select(o => new OrderListItem(o.Id.Value, o.Total.Amount, o.Total.Currency.Value))
                          .ToList();

        return Result.Ok(new Page<OrderListItem>(
            Items: items,
            Next: hasNext ? new Cursor(items[^1].Id.ToString("N")) : null,
            Previous: q.Cursor is null ? null : new Cursor(q.Cursor),
            RequestedLimit: requested,
            AppliedLimit: applied));
    }
}
```

**What it shows.** `Page<T>` is a `readonly record struct`; instances always carry positive limits and a non-null `Items`. `WasCapped` becomes `true` automatically when the server clamped the limit. Use `Page.Empty<T>(req, app)` for the empty case rather than `default(Page<T>)`.

> **Cursor parsing must be ROP, not throwing.** `Guid.Parse(q.Cursor)` would throw on malformed input and escape the handler as a 500. Use `Guid.TryParseExact(..., "N", out var)` and return `Result.Fail<T>(Error.UnprocessableContent.ForField("cursor", ...))` so a bad cursor surfaces as a clean 422, not a stack trace. Apply the same shape (TryParse -> `Result` failure) for any opaque-token format you adopt.

---

## Recipe 4 — Minimal-API endpoint wiring `Result<T>` → `HttpResponseOptionsBuilder` → `ToHttpResponse`

**Problem.** Map a `Result<Order>` to a fully-conformant HTTP response: `200` with strong ETag and `Last-Modified`, `404`/`422` Problem Details on failure, `304` on `If-None-Match` match.

```csharp
using Microsoft.AspNetCore.Builder;
using Trellis;
using Trellis.Asp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTrellisAsp();          // error → status mapping + scalar-value validation
builder.Services.AddOrdersFeature();       // from Recipe 2

var app = builder.Build();

app.MapGet("/orders/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    if (!OrderId.TryCreate(id, nameof(id)).TryGetValue(out var orderId, out var idError))
        return idError.ToHttpResponse();

    Result<Order> result = await mediator.Send(new GetOrderQuery(orderId), ct);

    return result.ToHttpResponse(opts => opts
        .WithETag(o => o.ETag)                         // strong ETag from aggregate
        .WithLastModified(o => o.LastModified)         // RFC 1123
        .Vary("Accept", "Accept-Language")
        .EvaluatePreconditions());                     // 304 / 412 handling
});

app.Run();
```

**What it shows.** `ToHttpResponse` returns `Microsoft.AspNetCore.Http.IResult` and is the **only** supported response verb. The fluent `HttpResponseOptionsBuilder<TDomain>` configures protocol semantics (`WithETag`, `WithLastModified`, `Vary`, `EvaluatePreconditions`) without leaking HTTP into the handler. Failures (`Error.NotFound`, `Error.UnprocessableContent`, …) round-trip through Problem Details using the `TrellisAspOptions` mapping registered by `AddTrellisAsp`.

---

## Recipe 5 — MVC controller using `AsActionResult`

**Problem.** Same payload as Recipe 4 but with a typed MVC `ActionResult<OrderDto>`.

```csharp
using Microsoft.AspNetCore.Mvc;
using Trellis;
using Trellis.Asp;

[ApiController]
[Route("orders")]
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> Get(Guid id, CancellationToken ct)
    {
        if (!OrderId.TryCreate(id, nameof(id)).TryGetValue(out var orderId, out var idError))
            return idError.ToHttpResponse().AsActionResult<OrderDto>();

        Result<Order> result = await mediator.Send(new GetOrderQuery(orderId), ct);

        return result
            .ToHttpResponse(
                body: o => new OrderDto(o.Id.Value, o.Total.Amount, o.Total.Currency.Value),
                configure: opts => opts.WithETag(o => o.ETag).EvaluatePreconditions())
            .AsActionResult<OrderDto>();
    }
}

public sealed record OrderDto(Guid Id, decimal Amount, string Currency);
```

**What it shows.** `.AsActionResult<TBody>()` projects an `IResult` into a typed `ActionResult<TBody>`, so MVC clients still get OpenAPI/Swagger-friendly typed responses while the response itself executes through the same `IResult` pipeline as Minimal API.

---

## Recipe 6 — Conditional GET with `EntityTagValue` and byte-range with `RangeOutcome`

**Problem.** Serve a binary blob with strong-ETag conditional GET *and* RFC 9110 byte-range support.

```csharp
using Microsoft.AspNetCore.Http;
using Trellis;
using Trellis.Asp;

app.MapGet("/blobs/{id:guid}", async (Guid id, HttpRequest req, IBlobRepository repo, CancellationToken ct) =>
{
    Result<BlobContent> result = await repo.FindAsync(new BlobId(id), ct);

    return result.ToHttpResponse(opts => opts
        .WithETag(b => EntityTagValue.Strong(b.Sha256Hex))
        .WithLastModified(b => b.UploadedAt)
        .Vary("Range")
        .WithAcceptRanges("bytes")
        .WithRange(b =>
        {
            var outcome = RangeRequestEvaluator.Evaluate(req, b.Length);
            return outcome switch
            {
                RangeOutcome.PartialContent pc => new System.Net.Http.Headers.ContentRangeHeaderValue(pc.From, pc.To, pc.CompleteLength),
                _                              => new System.Net.Http.Headers.ContentRangeHeaderValue(b.Length),
            };
        })
        .EvaluatePreconditions());
});
```

**What it shows.** `EntityTagValue.Strong(...)` and `EntityTagValue.Weak(...)` build typed ETags; `WithETag` accepts either a `string` (always strong) or an `EntityTagValue`. `RangeRequestEvaluator.Evaluate(...)` (in `Trellis.Asp`) returns the closed-ADT `RangeOutcome`: `FullRepresentation`, `PartialContent(From, To, CompleteLength)`, or `NotSatisfiable(CompleteLength)`. `.EvaluatePreconditions()` honors `If-Match`/`If-None-Match`/`If-Modified-Since`/`If-Unmodified-Since` against the configured ETag and `Last-Modified` selectors.

---

## Recipe 7 — Authorization: `IActorProvider` + `IAuthorize` + resource-based auth

**Problem.** Static (permission) authorization on a delete command, plus resource-based ownership check on an update command — all via the mediator pipeline.

```csharp
using Trellis;
using Trellis.Asp.Authorization;
using Trellis.Authorization;
using Trellis.Primitives;

public sealed record DeleteOrderCommand(OrderId OrderId) : ICommand<Result<Unit>>, IAuthorize
{
    public IReadOnlyList<string> RequiredPermissions => ["orders:delete"];
}

public sealed record UpdateOrderCommand(OrderId OrderId, Money NewTotal)
    : ICommand<Result<Unit>>, IAuthorizeResource<Order>, IIdentifyResource<Order, OrderId>
{
    // Typed VO carried straight through — no parse, no throw.
    // ASP.NET model binding (via IScalarValue<OrderId, string>) handles the
    // string→OrderId conversion at the API edge.
    public OrderId GetResourceId() => OrderId;

    public Trellis.IResult Authorize(Actor actor, Order resource) =>
        resource.OwnerId == actor.Id || actor.Permissions.Contains("orders:write")
            ? Result.Ok()
            : Result.Fail(new Error.Forbidden(PolicyId: "orders.owner", Resource: ResourceRef.For<Order>(OrderId)));
}

// DI wiring
services.AddTrellisBehaviors();
services.AddClaimsActorProvider();               // ClaimsActorProvider for ASP.NET Core
// Pass every assembly that contains command/query types AND every assembly that contains
// IResourceLoader<,> implementations. In a layered app the loader typically lives in the
// ACL assembly, not the Application assembly — passing only the Application assembly will
// register ResourceAuthorizationBehavior<,,> without discovering the shared loader and the
// pipeline will fail at runtime when it cannot resolve IResourceLoader<TMessage, TResource>.
// Note: the scanner does not de-duplicate assemblies; if the two layers happen to collapse
// to one assembly, pass it once (or .Distinct() the array) to avoid registering the behavior twice.
services.AddResourceAuthorization(
    typeof(UpdateOrderCommand).Assembly,        // Application assembly (commands + IAuthorizeResource)
    typeof(OrderResourceLoader).Assembly);      // ACL assembly (IResourceLoader<,> implementations)
```

**What it shows.** `IAuthorize` enforces an AND-permission gate via `AuthorizationBehavior<,>`. `IAuthorizeResource<TResource>` runs *after* `IResourceLoader<TMessage, TResource>` produces the loaded resource, then calls `Authorize(actor, resource)`. Combining `IAuthorizeResource<TResource>` with `IIdentifyResource<TResource, TId>` lets the framework reuse the shared `SharedResourceLoaderById<TResource, TId>` instead of requiring a per-command loader.

---

## Recipe 8 — EF Core: `MaybePropertyMapping` for nullable value objects

**Problem.** Persist a `Maybe<EmailAddress>` property with the EF Core `MaybeConvention`, then verify the generated mapping in a startup diagnostics check.

```csharp
using Trellis;
using Trellis.EntityFrameworkCore;

public sealed partial class EmailAddress : RequiredString<EmailAddress>;

public sealed partial class Customer : Aggregate<CustomerId>
{
    public Customer(CustomerId id) : base(id) { }

    public partial Maybe<EmailAddress> Email { get; set; }   // TRLS035 if not 'partial'
}

// Configure
public sealed class AppDbContext : DbContext
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.ApplyTrellisConventions(typeof(AppDbContext).Assembly);
}

// Diagnostics — print the generated storage members for every Maybe<T> in the model
public static class ModelDiagnostics
{
    public static void DumpMaybeMappings(DbContext db)
    {
        IReadOnlyList<MaybePropertyMapping> mappings = db.GetMaybePropertyMappings();
        foreach (var m in mappings)
            Console.WriteLine($"{m.EntityTypeName}.{m.PropertyName} → {m.MappedBackingFieldName} ({m.StoreType.Name})");
    }
}
```

**What it shows.** `Maybe<T>` properties are routed through `MaybeConvention`, which generates a backing field (`_email` for `Email`) that EF Core maps to a nullable column. The CLR property remains `Maybe<EmailAddress>` everywhere in the domain. `MaybePropertyMapping` is the diagnostic record that exposes both names — useful for `HasIndex` on the storage member.

> For **composite** value objects (multi-field `[OwnedEntity]` types like `ShippingAddress`) — and for `Maybe<T>` where `T` is composite — see [Recipe 13](#recipe-13--composite-value-object-end-to-end-domain--api-json-binding--ef-core-ownership). `Recipe 8` covers scalar `Maybe<T>` only.

**Anti-pattern → fix (TRLS016).**

```csharp
// WRONG — HasIndex against the CLR Maybe<T> property silently fails
modelBuilder.Entity<Customer>().HasIndex(c => c.Email);   // TRLS016

// WRONG — explicit Property() configuration on a Maybe<T> CLR property.
// MaybeConvention generates a private backing field (e.g., _email) and maps THAT.
// Calling builder.Property(c => c.Email) tries to map Maybe<EmailAddress> as a column,
// which is not a supported store type — fails at model validation with
// "The property 'Customer.Email' could not be mapped because the database provider
//  does not support the type 'Maybe<EmailAddress>'."
internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(c => c.Email).IsRequired();          // ❌ — runtime error
        builder.Property(c => c.Email).HasMaxLength(254);     // ❌ — runtime error
    }
}

// FIX — say nothing about Maybe<T> in IEntityTypeConfiguration. The convention owns it.
// If you need column metadata (max length, column name, etc.), configure the *backing field*
// via the diagnostic name from MaybePropertyMapping, or use HasTrellisIndex for indexes.

// FIX 1 — strongly-typed Trellis index helper
modelBuilder.Entity<Customer>().HasTrellisIndex(c => new { c.Status, c.Email });

// FIX 2 — string-based HasIndex against the storage member
modelBuilder.Entity<Customer>().HasIndex("Status", "_email");
```

### Filtering on `Maybe<T>` properties in LINQ and `Specification<T>`

Once `MaybeConvention` maps the storage member, the **`MaybeQueryInterceptor`** (registered by `optionsBuilder.AddTrellisInterceptors()`) lets you write natural LINQ against the CLR `Maybe<T>` property — no `EF.Property<T?>(o, "_x")` boilerplate, no separate query helpers. The interceptor rewrites the expression tree before EF Core compiles it, translating `o.Maybe.HasValue`, `o.Maybe.Value`, `o.Maybe.GetValueOrDefault(d)`, and `o.Maybe == Maybe<T>.None` to the storage-member access.

```csharp
// Specification — exactly the shape you'd write for an aggregate query.
public sealed class OverdueOrderSpecification(DateTime asOf) : Specification<Order>
{
    private readonly DateTime _threshold = asOf.AddDays(-7);

    // Natural multi-clause guard — analyzer-clean (TRLS003 recognises HasValue
    // anywhere in the connected `&&` subtree to the left of the `.Value` access),
    // safe in FakeRepository (HasValue short-circuits before Value), and translated
    // verbatim by MaybeQueryInterceptor in EF.
    public override Expression<Func<Order, bool>> ToExpression() =>
        o => o.Status == OrderStatus.Submitted
             && o.SubmittedAt.HasValue
             && o.SubmittedAt.Value < _threshold;
}

// Repository / DbContext usage — the spec composes through IQueryable.Where.
var overdue = await context.Orders
    .Where(new OverdueOrderSpecification(timeProvider.GetUtcNow().DateTime).ToExpression())
    .ToListAsync(ct);
```

**Why this works in both EF and `FakeRepository<T, TId>`** — the compiled `Func<Order, bool>` that `FakeRepository` evaluates in memory uses C#'s short-circuit `&&`, so `Value` is never read when `HasValue` is `false`. In EF, the interceptor rewrites both clauses to the mapped storage member and emits idiomatic SQL (`status = 'Submitted' AND submitted_at IS NOT NULL AND submitted_at < @threshold`). **One Specification, one predicate, identical semantics in production and in tests.**

**Sharing the spec with `FakeRepository`.** Pass the same `Specification<T>` instance through `QueryAsync` — never duplicate the predicate by hand in a fake adapter. Duplicating the predicate is the most expensive class of test bug to catch in code review (the fake passes while the real query silently returns the wrong rows).

```csharp
public Task<IReadOnlyList<Order>> FindOverdueAsync(DateTime asOf, CancellationToken ct) =>
    fake.QueryAsync(new OverdueOrderSpecification(asOf), ct);
```

> **Prerequisite.** The interceptor only runs when the `DbContext` is configured with `optionsBuilder.AddTrellisInterceptors()`. Without it, EF Core sees `Maybe<T>` as an unmapped CLR type and either drops the predicate silently or fails translation — while the `FakeRepository` tests continue to pass. This is the failure mode that creates "fake says yes, production says no". Always wire interceptors in `AddDbContext`.

```csharp
// Sentinel alternative — for predicates where you'd rather encode "absence acts as
// the most-permissive value" than carry the explicit HasValue clause. Reads as
// "if no SubmittedAt, treat as never overdue (DateTime.MaxValue)".
public override Expression<Func<Order, bool>> ToExpression() =>
    o => o.Status == OrderStatus.Submitted
         && o.SubmittedAt.GetValueOrDefault(DateTime.MaxValue) < _threshold;
```

For ad-hoc `IQueryable<T>` calls (outside a `Specification<T>`), the strongly-typed `IQueryable<T>` extensions in `MaybeQueryableExtensions` — `WhereHasValue`, `WhereNone`, `WhereEquals`, `WhereLessThan`, `WhereGreaterThanOrEqual`, `OrderByMaybe`, etc. — are an alternative that doesn't depend on the interceptor. They compose with the same storage member directly via `EF.Property`.

```csharp
// Equivalent ad-hoc query without a Specification (interceptor not required for this form):
var overdue = await context.Orders
    .Where(o => o.Status == OrderStatus.Submitted)
    .WhereLessThan(o => o.SubmittedAt, threshold)
    .ToListAsync(ct);
```

---

## Recipe 9 — State machine: `CanFire` + `Fire` pattern with `FireResult`

**Problem.** Drive an order through `Draft → Submitted → Shipped` using Stateless, but expose every transition as `Result<TState>` so the mediator pipeline composes naturally.

```csharp
using Stateless;
using Trellis;
using Trellis.StateMachine;

// States and triggers as RequiredEnum value objects (Trellis convention) —
// equality is symbolic, so Stateless's TState/TTrigger generic constraints are satisfied.
public partial class DocumentState : RequiredEnum<DocumentState>
{
    public static readonly DocumentState Draft     = new();
    public static readonly DocumentState Submitted = new();
    public static readonly DocumentState Approved  = new();
}

public partial class DocumentTrigger : RequiredEnum<DocumentTrigger>
{
    public static readonly DocumentTrigger Submit  = new();
    public static readonly DocumentTrigger Approve = new();
    public static readonly DocumentTrigger Reject  = new();
}

public sealed class DocumentService
{
    public Result<DocumentState> Submit(Document doc)
    {
        var machine = new StateMachine<DocumentState, DocumentTrigger>(doc.State);
        machine.Configure(DocumentState.Draft).Permit(DocumentTrigger.Submit, DocumentState.Submitted);
        machine.Configure(DocumentState.Submitted)
               .Permit(DocumentTrigger.Approve, DocumentState.Approved)
               .Permit(DocumentTrigger.Reject,  DocumentState.Draft);

        // FireResult pre-checks CanFire and converts invalid transitions to an
        // Error.UnprocessableContent (HTTP 422) carrying a single RuleViolation with
        // ReasonCode "state.machine.invalid.transition" — invalid transitions are
        // semantic rule violations, not concurrent-modification conflicts.
        Result<DocumentState> result = machine.FireResult(DocumentTrigger.Submit);
        return result.Tap(newState => doc.State = newState);
    }
}
```

**What it shows.** `StateMachineExtensions.FireResult(...)` honors `PermitIf`/`IgnoreIf` guards via `CanFire(...)` rather than parsing exception messages, so it survives Stateless library upgrades. For aggregates whose state lives in a backing field (e.g., loaded from EF), use `LazyStateMachine<TState, TTrigger>` to defer machine creation until the first `FireResult` call.

**Side-effect placement.** Keep Stateless configuration declarative: states, triggers, permitted transitions, and pure/idempotent guards. Put business mutation, domain events, outbox writes, and other side effects after `FireResult` succeeds, usually in `.Tap(...)` as shown above. `FireResult` intentionally invokes `Fire(...)` even when `CanFire(...)` is false so any configured `OnUnhandledTrigger` callback can run. A custom unhandled-trigger callback may swallow the trigger, in which case `FireResult` returns `Result.Ok(stateMachine.State)` — the state read AFTER the callback runs (normally unchanged, but a callback that mutates or reroutes state will surface the resulting state). If side effects live in `OnEntry`, `OnExit`, transition callbacks, or `OnUnhandledTrigger`, they can run outside the visible ROP success/failure path and make handler behavior diverge from tests.

> **HTTP semantics.** Invalid state-machine transitions surface as `Error.UnprocessableContent` (HTTP 422), not `Error.Conflict` (HTTP 409). The reasoning: `Error.Conflict` semantically means "your request is valid but collides with concurrent state — retry may succeed"; a state-machine rejection ("you asked for `Submit` on a `Cancelled` order") is not retriable and is not about concurrent modification — it's a semantic rule violation. Callers that need to distinguish state-machine rejections from other 422s can match on the `RuleViolation.ReasonCode` value `state.machine.invalid.transition`.

```csharp
// Asserting on a state-machine rejection in tests:
var unproc = result.Error.Should().BeOfType<Error.UnprocessableContent>().Subject;
unproc.Rules.Should().ContainSingle().Which.ReasonCode.Should().Be("state.machine.invalid.transition");
```

---

## Recipe 10 — Test: handler test using `Trellis.Testing` `Should().Be(...)` / `UnwrapError()`

**Problem.** Unit-test the `PlaceOrderHandler` from Recipe 2 using FluentAssertions extensions from `Trellis.Testing`.

```csharp
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Trellis;
using Trellis.Primitives;
using Trellis.Testing;
using Xunit;

public class PlaceOrderHandlerTests
{
    [Fact]
    public async Task PlaceOrder_returns_id_on_success()
    {
        var repo = new InMemoryOrderRepository();
        var sut  = new PlaceOrderHandler(repo);

        var command = new PlaceOrderCommand(
            OrderId.TryCreate(Guid.NewGuid()).Unwrap(),
            Money.TryCreate(100m, "USD").Unwrap());

        var result = await sut.Handle(command, CancellationToken.None);

        result.Should().BeSuccess();
        result.Should().HaveValue(repo.Last().Id);                  // structural equality on Result<T>
    }

    [Fact]
    public void PlaceOrder_request_adapter_fails_when_currency_invalid()
    {
        var request = new PlaceOrderRequest(Guid.NewGuid(), 100m, "US"); // 2 chars, not 3

        var result = PlaceOrderCommand.TryCreate(request);

        result.Should().BeFailureOfType<Error.UnprocessableContent>()
            .Which.Should().HaveFieldError("currency");
    }
}
```

**What it shows.** `ResultAssertions<TValue>.HaveValue(...)` does structural comparison; `UnwrapError()` is the safe accessor that *only* returns the error and is intended for use after `Should().BeFailure...`. Calling `.Should()` on an `Error.UnprocessableContent` returns the specialized `ValidationErrorAssertions` (with `HaveFieldError`, `HaveFieldErrorWithDetail`, `HaveFieldCount`). Async assertions have two valid shapes: await the pipeline first and assert the resulting `Result<T>`, or call `BeSuccessAsync` / `BeFailureAsync` directly on `Task<Result<T>>` or `ValueTask<Result<T>>`. The unsupported shape is `await result.Should().BeSuccessAsync()` because `.Should()` returns synchronous `ResultAssertions<TValue>`.

---

## Recipe 11 — Anti-pattern → fix gallery (the analyzers in action)

The anti-pattern catalog moved to its own file so that AI sessions and human readers can load it independently when debugging an analyzer warning. See **[`trellis-api-anti-patterns.md`](trellis-api-anti-patterns.md)** for each common analyzer trigger and its idiomatic Trellis fix (TRLS001, TRLS003, TRLS010, TRLS016, TRLS017, TRLS018, TRLS019).

If you are looking up a specific analyzer by ID, the standalone file is faster than scanning this cookbook. The cookbook recipes still link to the relevant sections of that file where they apply.

---

## Recipe 12 — DI wiring playbook: `AddTrellis` composition builder

**Problem.** Compose Trellis service modules in the correct order so behaviors stack properly without forcing simple apps to install every package.

**Preferred: tiered builder.** Use `Trellis.ServiceDefaults` from the API/composition root. The builder records intent first, then applies modules in the canonical order. `UseEntityFrameworkUnitOfWork<TContext>()`, when selected, is always applied last so `TransactionalCommandBehavior<,>` lands innermost.

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Trellis.ServiceDefaults;

public static class CompositionRoot
{
    public static IServiceCollection AddApp(this IServiceCollection services, string connectionString)
    {
        // App-owned: provider, connection string, migrations, pooling, and Mediator registration.
        services.AddDbContext<AppDbContext>(opts => opts
            .UseSqlServer(connectionString)
            .AddTrellisInterceptors());

        services.AddMediator(options => options.Assemblies = [typeof(PlaceOrderCommand).Assembly]);

        services.AddTrellis(options => options
            .UseAsp()
            .UseMediator()
            .UseFluentValidation(typeof(PlaceOrderValidator).Assembly)
            .UseClaimsActorProvider()
            .UseResourceAuthorization(typeof(UpdateOrderCommand).Assembly)
            .UseEntityFrameworkUnitOfWork<AppDbContext>());

        services.AddScoped<IOrderRepository, EfOrderRepository>();

        return services;
    }
}
```

**Builder modules, summarized.**

| Module | What it applies | Notes |
| ---- | ---- | ----------------- |
| `UseAsp()` | `AddTrellisAsp()` | Error → status mapping plus scalar-value JSON/model-binding validation. |
| `UseMediator()` | `AddTrellisBehaviors()` | Registers the canonical Result-aware pipeline behaviors. |
| `UseFluentValidation(...)` | `AddTrellisFluentValidation(...)` | Implies `UseMediator()`. Pass assemblies to scan, or omit assemblies when validators are registered explicitly. |
| `UseClaimsActorProvider()` / `UseEntraActorProvider()` / `UseDevelopmentActorProvider()` | One ASP actor provider | The builder rejects multiple actor providers. |
| `UseResourceAuthorization(...)` | `AddResourceAuthorization(...)` | Implies `UseMediator()` and scans for resource auth/loaders. |
| `UseEntityFrameworkUnitOfWork<TContext>()` | `AddTrellisUnitOfWork<TContext>()` | Implies `UseMediator()` and is always applied last. |

**Still app-owned.** `AddTrellis(...)` does **not** call `AddDbContext`, `AddMediator`, or route-constraint registration. Those choices depend on provider, connection string, source-generator setup, migrations, route template names, and hosting style.

---

## Recipe 13 — Composite value object end-to-end (Domain + API JSON binding + EF Core ownership)

**Problem.** Persist a multi-field value object (`ShippingAddress` with street/city/state/postalCode/country) as part of a `Customer` aggregate. Every field is required, the VO must validate at construction, and the JSON wire format must reuse the same validation as the domain TryCreate.

The unobvious bits this recipe pins down:

- `ApplyTrellisConventions` already configures `[OwnedEntity]` types as owned navigations — **you do not need `builder.OwnsOne(...)` in your `IEntityTypeConfiguration`** (the `CompositeValueObjectConvention` discovers them by attribute when the assembly is passed to `ApplyTrellisConventions`).
- The class **must** be `partial` (`TRLS036`), inherit `ValueObject` (`TRLS038`), and have **no** parameterless constructor (`TRLS037`) — the source generator emits one for EF Core's materialization path.
- `[JsonConverter(typeof(CompositeValueObjectJsonConverter<TSelf>))]` routes JSON deserialization through the public `TryCreate`, so the API surface and the domain agree on what's valid. Without it, model binding produces a default-constructed VO that bypasses `TryCreate`.

```csharp
using System.Text.Json.Serialization;
using Trellis;
using Trellis.EntityFrameworkCore;
using Trellis.Primitives;

[OwnedEntity]                                                        // TRLS036 if not partial; TRLS037 if you add a parameterless ctor; TRLS038 if not ValueObject
[JsonConverter(typeof(CompositeValueObjectJsonConverter<ShippingAddress>))]
public partial class ShippingAddress : ValueObject
{
    public string Street     { get; private set; } = null!;
    public string City       { get; private set; } = null!;
    public string State      { get; private set; } = null!;
    public string PostalCode { get; private set; } = null!;
    public string Country    { get; private set; } = null!;

    private ShippingAddress(string street, string city, string state, string postalCode, string country)
    {
        Street = street; City = city; State = state; PostalCode = postalCode; Country = country;
    }

    public static Result<ShippingAddress> TryCreate(
        string street, string city, string state, string postalCode, string country, string? fieldName = null)
    {
        var violations = new List<FieldViolation>(5);
        AddIfBlank(violations, street,     fieldName, nameof(Street));
        AddIfBlank(violations, city,       fieldName, nameof(City));
        AddIfBlank(violations, state,      fieldName, nameof(State));
        AddIfBlank(violations, postalCode, fieldName, nameof(PostalCode));
        AddIfBlank(violations, country,    fieldName, nameof(Country));
        return violations.Count > 0
            ? Result.Fail<ShippingAddress>(new Error.UnprocessableContent(EquatableArray.Create(violations.ToArray())))
            : Result.Ok(new ShippingAddress(street.Trim(), city.Trim(), state.Trim(), postalCode.Trim(), country.Trim()));
    }

    protected override IEnumerable<IComparable?> GetEqualityComponents()
    {
        yield return Street; yield return City; yield return State; yield return PostalCode; yield return Country;
    }

    private static void AddIfBlank(List<FieldViolation> v, string value, string? owner, string part)
    {
        if (!string.IsNullOrWhiteSpace(value)) return;
        var leaf = char.ToLowerInvariant(part[0]) + part[1..];
        var pointer = string.IsNullOrWhiteSpace(owner)
            ? InputPointer.ForProperty(leaf)
            : new InputPointer($"/{owner}/{leaf}");
        v.Add(new FieldViolation(pointer, "required") { Detail = $"{part} is required." });
    }
}

public sealed partial class CustomerId : RequiredGuid<CustomerId>;

public sealed partial class Customer : Aggregate<CustomerId>
{
    public string Name { get; private set; } = null!;
    public ShippingAddress ShippingAddress { get; private set; } = null!;     // required composite owned VO
    public partial Maybe<ShippingAddress> BillingAddress { get; set; }        // optional composite owned VO

    private Customer(CustomerId id, string name, ShippingAddress shipping) : base(id)
    {
        Name = name; ShippingAddress = shipping;
    }

    public static Result<Customer> Create(CustomerId id, string name, ShippingAddress shipping) =>
        string.IsNullOrWhiteSpace(name)
            ? Result.Fail<Customer>(Error.UnprocessableContent.ForField("name", "required", "Name is required."))
            : Result.Ok(new Customer(id, name, shipping));
}

// CONFIGURATION — note the absence of OwnsOne(c => c.ShippingAddress).
// CompositeValueObjectConvention picks up [OwnedEntity] types automatically
// from the assemblies passed to ApplyTrellisConventions.
internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired();
        // No builder.OwnsOne(c => c.ShippingAddress) — the convention does this for you.
        // No HasConversion(...) on the inner string fields — they are mapped by EF Core directly.
    }
}

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.ApplyTrellisConventions(typeof(Customer).Assembly);

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
```

**What it shows.**

- `[OwnedEntity]` + `partial` + `ValueObject` + private ctor is the contract. The three diagnostics (`TRLS036`/`037`/`038`) catch each violation at compile time.
- `CompositeValueObjectJsonConverter<T>` makes JSON deserialization round-trip through `TryCreate`, so an API request body with a missing `state` produces the same `Error.UnprocessableContent` shape the domain emits.
- `ApplyTrellisConventions` removes the boilerplate `OwnsOne` call. You only need `OwnsOne` when you want to **override** the convention (custom column names, table splitting, indexes on inner properties).

**Storage shape.**

| Aggregate property | Storage |
|---|---|
| Required `ShippingAddress` (non-nullable) | Table-split: 5 columns on the `Customers` table — `ShippingAddress_Street`, `ShippingAddress_City`, `ShippingAddress_State`, `ShippingAddress_PostalCode`, `ShippingAddress_Country` (all `NOT NULL`). |
| Optional `Maybe<ShippingAddress>` | Because the inner properties are non-nullable, `CompositeValueObjectConvention` switches to a **separate table** named `{Owner}_{Property}` (e.g., `Customer_BillingAddress`) with a `1:0..1` FK back to `Customers`. See the storage rules in [trellis-api-efcore.md](trellis-api-efcore.md) for the full decision matrix. |

**JSON wire shape.**

The `[JsonConverter(typeof(CompositeValueObjectJsonConverter<T>))]` attribute on the value object controls the wire format. There is no auto-discovery — the attribute is required for the converter to engage on request bodies and response payloads.

| C# property | JSON request/response shape |
|---|---|
| `ShippingAddress ShippingAddress { get; private set; }` (required composite VO) | `"shippingAddress": { "street": "1 Main St", "city": "Redmond", "state": "WA", "postalCode": "98052", "country": "US" }` — every field present; missing inner field → `Error.UnprocessableContent` with field path `/shippingAddress/<field>`. |
| `partial Maybe<ShippingAddress> BillingAddress { get; set; }` (optional composite VO on a domain model — **not** used directly on a request DTO; see Recipe 14) | Domain model only. On the wire, request DTOs use a **nullable transport** (`ShippingAddress?`) and the controller adapts via `Maybe.From(...)`. Response DTOs project to `ShippingAddress?` for the same reason. |
| `Money Total { get; private set; }` (required composite VO with scalar inner properties — `decimal Amount`, `Currency Currency`) | `"total": { "amount": 49.99, "currency": "USD" }` — the property casing comes from System.Text.Json's `PropertyNamingPolicy.CamelCase` (set by `AddTrellisAsp()`). Inner scalar VOs (e.g., `Currency : RequiredString<Currency>`) serialize as their underlying primitive (`"USD"`, not `{"value":"USD"}`). |
| Scalar VO (`OrderId : RequiredGuid<OrderId>`, `EmailAddress : RequiredString<EmailAddress>`) | Always serializes as the underlying primitive (`"550e8400-..."`, `"a@b.com"`). Never wrapped in `{ "value": ... }`. This is automatic via the source-generated `IScalarValue<T,P>` JSON converter. |

**Anti-pattern → fix.**

```csharp
// WRONG — explicit Property() on a composite owned VO. The convention has already
// registered an OwnsOne relationship; calling builder.Property() tries to map the
// composite as a single column, which fails at model validation with
// "The property 'Order.Total' could not be mapped because the database provider
//  does not support the type 'Money'."
internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.Total).IsRequired();           // ❌ — runtime error
        builder.Property(o => o.ShippingAddress).IsRequired(); // ❌ — runtime error
    }
}

// FIX — say nothing about composite owned VOs in IEntityTypeConfiguration. The convention
// auto-registers them as OwnsOne. To override (rename column, add an index on an inner
// property, force table-splitting), use OwnsOne explicitly — it is additive, not duplicative,
// because the convention checks IsOwned() before re-registering.

// WRONG — manual OwnsOne after ApplyTrellisConventions duplicates the convention's work
// and silently overrides any annotations the convention set.
builder.OwnsOne(c => c.ShippingAddress, owned => { /* … */ });

// FIX — let the convention own the registration. Use OwnsOne only to override
// (e.g., to rename columns or add an index on an inner property):
builder.OwnsOne(c => c.ShippingAddress, owned =>
{
    owned.Property(a => a.PostalCode).HasColumnName("PostalCode").HasMaxLength(20);
    owned.HasIndex(a => a.Country);
});
```

```csharp
// WRONG — non-partial class (TRLS036) so the generator can't emit the parameterless ctor.
[OwnedEntity]
public class ShippingAddress : ValueObject { /* … */ }

// WRONG — declared parameterless ctor (TRLS037) shadows the generator's emitted one.
[OwnedEntity]
public partial class ShippingAddress : ValueObject { public ShippingAddress() { } }

// WRONG — not a ValueObject (TRLS038), so equality and convention-based mapping break.
[OwnedEntity]
public partial class ShippingAddress { /* … */ }
```

### Owned collections with a private backing field

When an aggregate exposes a collection navigation as an `IReadOnlyList<T>` (or `IReadOnlyCollection<T>`) facade over a private `List<T>` field, **ignore the facade and map via the backing field name**. EF Core cannot instantiate an interface type for a navigation, so it has to bind directly to the concrete `List<T>` field.

```csharp
public sealed partial class Order : Aggregate<OrderId>
{
    private readonly List<LineItem> _lineItems = [];
    public IReadOnlyList<LineItem> LineItems => _lineItems;       // public facade — interface, EF can't materialize
    // ...
}

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        // The public facade is IReadOnlyList<T> — EF cannot instantiate an interface.
        // Ignore the facade and map directly against the private backing field by name.
        builder.Ignore(o => o.LineItems);
        builder.OwnsMany<LineItem>("_lineItems", li =>
        {
            li.ToTable("LineItems");
            li.HasKey(x => x.Id);
            // Inner [OwnedEntity] composites (e.g., LineItem.UnitPrice : Money)
            // are still picked up by CompositeValueObjectConvention — no extra OwnsOne needed here.
        });
    }
}
```

The string `"_lineItems"` is unfortunately part of the public mapping contract: rename the private field and the EF model silently stops working. Two mitigations and what they buy you:

| Mitigation | Compile-time safety | Cost |
|---|---|---|
| Raw string `"_lineItems"` | None — typo or rename breaks at runtime model-validation. | Zero. The pattern shown above. |
| `private const string LineItemsField = "_lineItems";` on `Order`, then `builder.OwnsMany<LineItem>(Order.LineItemsField, …)` | Refactoring tools follow the constant. Still no compile check that the field actually exists. | Leaks the field name through `internal`/`public` constant on the aggregate — adds public surface for a persistence concern. |
| `builder.OwnsMany(o => o.LineItems, …)` directly against the facade | n/a | Does not work: EF reports it cannot determine the relationship from `IReadOnlyList<LineItem>`. |

**Why no `[OwnedEntity]`-style convention for collections (yet).** `[OwnedEntity]` + `CompositeValueObjectConvention` discovers composite owned *value objects* by attribute. An equivalent collection convention would need to walk every aggregate, find `IReadOnlyList<T>` / `IReadOnlyCollection<T>` properties whose `T` is an entity, locate a matching `_camelCase` backing field, and register the `OwnsMany` against it. This is on the roadmap (tracked as the analogue of `MaybeConvention` for collections); for now the cookbook pattern above is the supported approach.

### Supported property shapes inside a composite VO — when to map to a DTO instead

`CompositeValueObjectJsonConverter<T>` is deliberately small. It supports a closed list of primitive types directly and routes Trellis scalar value objects through their underlying primitives. Anything else — including any `Maybe<T>`, arrays, collections, and nested composite VO properties — throws `TrellisJsonValidationException` at the first JSON access. The converter never delegates property reads/writes back to `JsonSerializer`, so adding `[JsonConverter]` to an inner composite type does not rescue the nested case. **EF Core persistence is more permissive** (`Maybe<T>`, arrays, and nested composites work) so the JSON gap only surfaces when the composite VO crosses an HTTP boundary.

The intentional split is: keep the framework converter simple; route consumers with richer shapes through a wire-shape DTO at the controller/endpoint seam (Recipe 14 generalised).

| Property type on a composite VO interior | JSON via `CompositeValueObjectJsonConverter`? | Recommended path |
|---|---|---|
| `string` | ✅ Supported as-is | Use directly. |
| `decimal`, `int`, `long`, `short`, `byte`, `double`, `float`, `bool` | ✅ Supported as-is | Use directly. |
| `Guid`, `DateTime`, `DateTimeOffset` | ✅ Supported as-is | Use directly. |
| Trellis scalar VO (`RequiredString<>`, `RequiredInt<>`, `RequiredGuid<>`, `RequiredEnum<>`, `RequiredDateTime<>`, …) — and shipped concretes like `EmailAddress`, `PhoneNumber`, `Money.Currency` | ✅ Flattens to underlying primitive on the wire | Use directly. The composite converter detects `IScalarValue<,>` and reads/writes the inner primitive. |
| Nested composite owned VO (e.g., a nested `Money` or `ShippingAddress` as a property of another composite VO) | ❌ **Not supported.** `CompositeValueObjectJsonConverter<TOuter>` does not delegate to `JsonSerializer` for property values — it only reads/writes its own primitive whitelist directly. Adding `[JsonConverter]` to the inner type does not help, because the outer converter never asks STJ for an inner converter; it tries to treat the nested type as one of its primitives and trips on the first access. | **Map to a DTO** — declare each nested composite as its own property on the wire-shape DTO. Nested composite VOs serialize correctly only when STJ sees them at the top of a property of a non-Trellis-composite-converter type (i.e., a plain record/class with `[JsonConverter]` on the nested VO type itself). |
| `Maybe<T>` for any T (primitive, scalar VO, composite VO, array) | ❌ **Not supported.** Throws `TrellisJsonValidationException: "Unsupported primitive type 'Maybe`1...'"` regardless of value (`Some` or `None`). | **Map to a wire-shape DTO.** See Recipe 14 — the same `T?` + adapt-at-the-seam pattern applies whether the optional is on the DTO itself or inside a composite VO. |
| Array (`T[]`), collection (`List<T>`, `IReadOnlyList<T>`, `IEnumerable<T>`) | ❌ Not supported. | **Map to a DTO** with the array on the wire-shape type. |

**When to use a DTO at the seam (the general rule).** If a composite VO's interior holds any property shape outside the supported list — most commonly `Maybe<TPrimitive>` or arrays — keep the VO clean as a domain type and declare a wire-shape DTO at the controller/endpoint:

```csharp
// Domain VO: clean, persists fine via EF Core, but cannot serialize as-is.
[OwnedEntity]
public partial class CustomerProfile : ValueObject
{
    public OrderStatus Status { get; private set; } = null!;
    public partial Maybe<int> AgeYears { get; private set; }             // not JSON-serializable on this VO
    public partial Maybe<string> Nickname { get; private set; }          // not JSON-serializable
    public partial Maybe<DateTime[]> LoginHistory { get; private set; }  // not JSON-serializable
    // ...TryCreate, GetEqualityComponents...
}

// Wire-shape DTO at the API seam: everything as nullable transports the converter can serialize.
public sealed record CustomerProfileDto(string Status, int? AgeYears, string? Nickname, DateTime[]? LoginHistory)
{
    public Result<CustomerProfile> ToDomain() =>
        CustomerProfile.TryCreate(
            Status,
            AgeYears.AsMaybe(),
            Nickname is null ? Maybe<string>.None : Maybe.From(Nickname),
            LoginHistory is null ? Maybe<DateTime[]>.None : Maybe.From(LoginHistory));

    public static CustomerProfileDto From(CustomerProfile vo) =>
        new(vo.Status.Value,
            vo.AgeYears.AsNullable(),
            vo.Nickname.HasValue ? vo.Nickname.Value : null,
            vo.LoginHistory.HasValue ? vo.LoginHistory.Value : null);
}
```

The controller takes `CustomerProfileDto` on inbound requests, calls `.ToDomain()` to lift to the domain VO, and projects back via `CustomerProfileDto.From(...)` on responses. The Trellis scalar `Maybe<>` extensions (`AsMaybe` / `AsNullable`) and `Maybe.From` handle the lift cleanly.

**Last-resort escape hatch — write your own `JsonConverter<TComposite>`.** If a service genuinely cannot tolerate the DTO indirection (rare — usually a sign the design wants tightening), the language's standard mechanism still applies: declare `[JsonConverter(typeof(YourCustomConverter))]` on the composite VO and implement `Read`/`Write` directly. The framework does not provide help for this path; the trade-off is more code per-VO in exchange for putting the domain shape directly on the wire. This is not the recommended path — favour the DTO seam unless there is a concrete reason not to.

---

## Recipe 14 — Optional fields in request DTOs: `Maybe<TScalar>` vs nullable transport

**Problem.** A request body has an optional field — say `phoneNumber` on `CreateCustomerRequest`. The domain models it as `Maybe<PhoneNumber>` (the canonical Trellis pattern). What does the DTO declare it as?

The answer depends on whether the inner type is a **scalar** (single-primitive) value object or a **composite** owned value object. Trellis ships a JSON converter + model binder for the scalar case but not the composite case.

| Inner type | Pattern | Why |
|---|---|---|
| `Maybe<TScalar>` where `TScalar : IScalarValue<TScalar, TPrimitive>` (e.g., `Maybe<EmailAddress>`, `Maybe<PhoneNumber>`) | **Use `Maybe<T>` directly on the DTO.** | `AddTrellisAsp()` registers `MaybeScalarValueJsonConverterFactory` (JSON) and `MaybeModelBinder<T,P>` (route/query/header); MVC child-validation suppression for `None` scalar-maybe values is handled internally by the Trellis MVC integration. Call `AddTrellisAsp(...)` before MVC model binding is configured. `null`/missing → `None`; valid → `Maybe.From(validated)`; invalid → ProblemDetails with the same field path the domain emits. |
| `Maybe<TComposite>` where `TComposite : ValueObject` with multiple fields (e.g., `Maybe<ShippingAddress>`) | **Use a nullable transport (`TComposite?`) and adapt at the controller seam.** | No `MaybeCompositeValueObjectJsonConverterFactory` ships today — System.Text.Json would default-construct the inner type, bypassing `TryCreate`. Wrap with `Maybe.From(...)` inside the controller. |
| `Maybe<TPrimitive>` (e.g., `Maybe<int>`, `Maybe<string>`, `Maybe<Guid>`) | **Use `TPrimitive?` on the DTO and `.AsMaybe()` at the seam.** | No JSON converter ships for arbitrary primitive `Maybe<>` — `MaybeScalarValueJsonConverterFactory` only handles `Maybe<T>` where `T : IScalarValue<,>`. If the primitive carries domain meaning, prefer wrapping it in a scalar value object (e.g., `Age : RequiredInt<Age>`) and using `Maybe<Age>` — that shape *is* factory-handled. |

> **The same DTO pattern applies inside a composite VO.** If a *composite value object's interior* contains `Maybe<TPrimitive>` / arrays / collections, `CompositeValueObjectJsonConverter` rejects them too (see Recipe 13 §"Supported property shapes inside a composite VO"). Keep the composite VO clean as a domain type and declare a wire-shape DTO with nullable transports, then lift on inbound (`.AsMaybe()` / `Maybe.From(...)`) and project on outbound (`.AsNullable()`).

### Pattern A — scalar `Maybe<T>` directly on the DTO

```csharp
using Trellis;
using Trellis.Primitives;

public sealed partial class EmailAddress : RequiredString<EmailAddress>;
public sealed partial class PhoneNumber  : RequiredString<PhoneNumber>;

public sealed record CreateCustomerRequest(
    EmailAddress         Email,           // required
    Maybe<PhoneNumber>   PhoneNumber);    // optional — null/missing JSON → Maybe.None

[ApiController]
[Route("customers")]
public sealed class CustomersController(ISender sender) : ControllerBase
{
    [HttpPost]
    public ValueTask<ActionResult<CustomerResponse>> Create(
        [FromBody] CreateCustomerRequest request, CancellationToken ct) =>
        sender.Send(new CreateCustomerCommand(request.Email, request.PhoneNumber), ct)
              .ToHttpResponseAsync(CustomerResponse.From, /* … */)
              .AsActionResultAsync<CustomerResponse>();
}
```

`AddTrellisAsp()` is the only wiring required:

```csharp
services.AddTrellisAsp();      // MaybeScalarValueJsonConverterFactory + MaybeModelBinder + ValidationVisitor patch
services.AddControllers();
```

Send `{"email":"a@b.com","phoneNumber":null}` (or omit `phoneNumber` entirely) → handler receives `Maybe<PhoneNumber>.None`. Send `{"email":"a@b.com","phoneNumber":"not a phone"}` → 422 with field path `/phoneNumber` and the validation message produced by `PhoneNumber.TryCreate`.

### Pattern B — composite owned VO, nullable transport + controller-seam adapter

```csharp
public sealed record CreateCustomerRequest(
    EmailAddress       Email,
    ShippingAddress?   ShippingAddress);   // nullable transport — NOT Maybe<ShippingAddress>

public sealed class CustomersController(ISender sender) : ControllerBase
{
    [HttpPost]
    public ValueTask<ActionResult<CustomerResponse>> Create(
        [FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        var shipping = request.ShippingAddress is null
            ? Maybe<ShippingAddress>.None
            : Maybe.From(request.ShippingAddress);

        return sender.Send(new CreateCustomerCommand(request.Email, shipping), ct)
                     .ToHttpResponseAsync(CustomerResponse.From, /* … */)
                     .AsActionResultAsync<CustomerResponse>();
    }
}
```

The composite VO must still carry `[JsonConverter(typeof(CompositeValueObjectJsonConverter<ShippingAddress>))]` (see Recipe 13) so its inner fields round-trip through `TryCreate`. The seam adapter only handles the optionality.

**Why not just declare `Maybe<ShippingAddress>` on the DTO?** `MaybeScalarValueJsonConverterFactory.CanConvert` checks for `IScalarValue<,>` on the inner type. Composite VOs do not implement `IScalarValue`, so the factory returns false, and `Maybe<ShippingAddress>` falls back to default System.Text.Json serialization — which produces a default-constructed `ShippingAddress` (`{}`) wrapped in `Maybe.From`, silently bypassing `TryCreate`. That's a correctness bug, not just an ergonomics one. **`TRLS020` enforces this rule at compile time**: any DTO property typed `Maybe<TComposite>` where `TComposite` is `[OwnedEntity]` is flagged as a warning, even when `TComposite` itself carries `[JsonConverter(typeof(CompositeValueObjectJsonConverter<TComposite>))]` — the converter operates on `TComposite`, not on `Maybe<TComposite>`, so the inner attribute does not rescue the wrapper shape.

### Anti-pattern → fix

```csharp
// WRONG — composite Maybe<T> on DTO. Compiles, deserializes to Maybe.From(default(ShippingAddress)),
// silently skips TryCreate. Discovered only when the persisted entity has empty strings.
public sealed record CreateCustomerRequest(EmailAddress Email, Maybe<ShippingAddress> ShippingAddress);

// FIX — nullable transport + controller-seam adapter (Pattern B above).
public sealed record CreateCustomerRequest(EmailAddress Email, ShippingAddress? ShippingAddress);

// WRONG — bypassing AddTrellisAsp() (e.g., raw services.AddControllers().AddJsonOptions(...) in isolation)
// drops the Maybe converters AND the SuppressChildValidationMetadataProvider, so MVC's ValidationVisitor
// will throw InvalidOperationException("Maybe has no value.") the moment a None reaches model validation.
services.AddControllers();   // missing AddTrellisAsp()

// FIX — call AddTrellisAsp() before AddControllers(); it is idempotent and configures both pipelines.
services.AddTrellisAsp();
services.AddControllers();
```

> A future `MaybeCompositeValueObjectJsonConverterFactory` could make Pattern B unnecessary; until then, nullable transport plus controller-seam adaptation is the supported pattern.

---

## Recipe 15 — *(retired)*

This recipe previously documented a `GetValueOrDefault(SENTINEL)` workaround for a `TRLS003` false positive on multi-clause `Maybe<T>` predicates inside `Specification<T>.ToExpression()`. The false positive was fixed in the analyzer (`UnsafeMaybeValueAccess` now recognises the multi-clause guard), so the natural shape

```csharp
o => o.Status == OrderStatus.Submitted
     && o.SubmittedAt.HasValue
     && o.SubmittedAt.Value < _threshold;
```

is now both readable AND analyzer-clean inside any expression tree (specifications, FluentValidation, EF). The residual EF-Core/`FakeRepository` parity guidance — `AddTrellisInterceptors()`, `ApplyTrellisConventions`, and "share the same `Specification<T>` between EF and `FakeRepository` — never duplicate the predicate" — has moved into [Recipe 8](#recipe-8--ef-core-maybepropertymapping-for-nullable-value-objects). Ad-hoc query operators (`WhereLessThan`, `WhereHasValue`, etc.) live in [trellis-api-efcore.md](trellis-api-efcore.md).

The recipe number is preserved as a stub so existing bookmark and search-index entries remain stable; future content should renumber from Recipe 22 rather than reusing 15.

---

## Recipe 16 — Unit of work in handlers: `Add` staging vs immediate `SaveAsync`

**Problem.** A command handler creates a new aggregate. Where does the `SaveChanges` call go? The first time you read a Trellis handler that ends with `repo.Add(order); return Result.Ok(order.Id);` the question is unavoidable: *who actually saves it?*

```csharp
public sealed class CreateOrderHandler(IOrderRepository repo)
    : ICommandHandler<CreateOrderCommand, Result<OrderId>>
{
    public ValueTask<Result<OrderId>> Handle(CreateOrderCommand cmd, CancellationToken ct) =>
        Order.Create(cmd.Total)
            .Tap(repo.Add)                  // stages — no save here
            .Map(o => o.Id)
            .AsValueTask();                 // handler returns immediately
}
```

> `repo.Add(entity)` stages the aggregate for insertion via EF Core; `TransactionalCommandBehavior`, registered by `services.AddTrellisUnitOfWork<TContext>()` in your ACL composition root, automatically calls `SaveChangesAsync` after every successful handler — no explicit save call is needed in the handler.

**What it shows.** Handlers in Trellis follow a strict separation: the handler shapes domain state and the pipeline owns the commit boundary. `IRepository.Add` returns `void` precisely to signal "staged, not yet persisted" — the `void` return makes it impossible to write the (wrong) `await repo.Add(...).Should().BeSuccess()`. The mediator pipeline for command handlers is, innermost first: `TransactionalCommandBehavior` → `ValidationBehavior` → `LoggingBehavior` → handler. When the handler returns a successful `Result<T>`, the transactional behavior calls `SaveChangesAsync` and only then surfaces the result; on failure or exception, nothing is committed.

| Method | Signature | Saves immediately? | When to use |
|---|---|---|---|
| `IRepository.Add(T)` (and `Remove(T)`, `RemoveByIdAsync(TId)`) | `void` / `Task<Result<Unit>>` for not-found | **No** — staged for the UoW | Handlers and any production-shaped repository contract |
| `FakeRepository.Add(T)` | `void` | n/a (in-memory; visible immediately) | **Test setup** — "put this in the store so the handler can find it" |
| `FakeRepository.SaveAsync(T)` | `Task<Result<Unit>>` | n/a (in-memory; visible immediately) | Tests that explicitly assert on the `Result` shape, e.g., conflict-result handling |

**Anti-pattern → fix.**

```csharp
// ❌ Wrong — explicit SaveChangesAsync in the handler. Bypasses TransactionalCommandBehavior,
//   so cross-aggregate behaviors that depend on a single commit boundary (outbox writes,
//   ETag bumps, audit logs) end up in inconsistent states. Also: you've now committed even
//   if a later behavior in the pipeline fails post-handler.
public ValueTask<Result<OrderId>> Handle(CreateOrderCommand cmd, CancellationToken ct) =>
    Order.Create(cmd.Total)
        .Tap(repo.Add)
        .TapAsync(_ => dbContext.SaveChangesAsync(ct));   // ❌ — duplicates UoW, racy

// ❌ Wrong — calling SaveAsync from a production handler. SaveAsync is a FakeRepository
//   convenience for tests. EF repositories don't expose it (and shouldn't).
.TapAsync(o => repo.SaveAsync(o, ct))   // ❌ — IRepository<Order>.SaveAsync doesn't exist

// ✅ Correct — stage with Add, let TransactionalCommandBehavior commit on success.
.Tap(repo.Add)
```

**Test setup pattern.** When unit-testing a handler with `FakeRepository`, prefer `Add` for setup and reserve `SaveAsync` for tests that specifically assert on the Result of the save (conflict handling, etc.). The void surface keeps the test intent visually honest: setup should not have a return value to assert on.

```csharp
// ✅ Setup: void Add — no .GetAwaiter().GetResult(), no Result assertion in setup.
var customers = new FakeRepository<Customer, CustomerId>();
customers.Add(Customer.Create(/* ... */));   // matches the handler's surface exactly

// ✅ Conflict-result test: SaveAsync returns the Error.Conflict so the test can assert.
var customers = new FakeRepository<Customer, CustomerId>().WithUniqueConstraint(c => c.Email);
customers.Add(Customer.Create("alice@x.com"));
var result = await customers.SaveAsync(Customer.Create("alice@x.com"));   // intentional conflict
result.UnwrapError().Should().BeOfType<Error.Conflict>();
```

> `FakeRepository.Add` enforces unique constraints **eagerly** by throwing `InvalidOperationException` — setup-time violations are almost always test bugs and should fail loud at the offending call site, not at a deferred Result assertion further down. Use `SaveAsync` when you specifically want to test handler behavior on conflict (where the `Error.Conflict` Result is the system-under-test, not a setup mistake).

**DI prerequisites checklist.**

```csharp
services
    .AddTrellisBehaviors()                              // validation/logging/tracing
    .AddTrellisFluentValidation(typeof(MyValidator).Assembly)
    .AddTrellisUnitOfWork<AppDbContext>()               // ⬅ registers TransactionalCommandBehavior
    .AddScoped<IOrderRepository, EfOrderRepository>();
```

Without `AddTrellisUnitOfWork<TContext>()`, `repo.Add(order)` stages the entity but **nothing ever calls `SaveChangesAsync`** — handler tests against EF (or against a real database) silently insert nothing. This is the production analogue of the fake/real divergence trap covered in [Recipe 8](#recipe-8--ef-core-maybepropertymapping-for-nullable-value-objects): the tests pass against `FakeRepository` (which has no UoW boundary, so `Add` is immediately visible), and production silently commits nothing. Always wire `AddTrellisUnitOfWork` in the ACL composition root, not inside each handler.

---

## Recipe 17 — Defining custom domain events: `OccurredAt` is the only timestamp

**Problem.** You're modeling an order workflow and reach for a domain event:

```csharp
// ❌ Wrong — CS0535 'OrderSubmitted does not implement IDomainEvent.OccurredAt'
public sealed record OrderSubmitted(OrderId OrderId, Money Total, DateTimeOffset SubmittedAt) : IDomainEvent;
```

The compile error is unambiguous, but the obvious "fix" — adding `OccurredAt` *alongside* `SubmittedAt` — is the wrong shape:

```csharp
// ❌ Wrong — duplicate timestamps. SubmittedAt and OccurredAt always carry the same value.
public sealed record OrderSubmitted(OrderId OrderId, Money Total, DateTimeOffset SubmittedAt, DateTimeOffset OccurredAt) : IDomainEvent;
```

**Fix.** `OccurredAt` is the canonical, only timestamp on every domain event. The semantic meaning ("when the order was submitted") is carried by the *event type name* (`OrderSubmitted`), not by a parallel timestamp field. Drop the semantic alias:

```csharp
// ✅ Correct — OccurredAt is the timestamp; the event name carries the semantic.
public sealed record OrderSubmitted(OrderId OrderId, Money Total, DateTimeOffset OccurredAt) : IDomainEvent;
public sealed record OrderApproved(OrderId OrderId, ActorId ApprovedBy, DateTimeOffset OccurredAt) : IDomainEvent;
public sealed record OrderShipped(OrderId OrderId, TrackingNumber Tracking, DateTimeOffset OccurredAt) : IDomainEvent;
```

**Raising the event.** Always pass `TimeProvider.GetUtcNow()` (the .NET 8 testable-clock primitive returns `DateTimeOffset` directly — no conversion needed). The aggregate's domain method, not the event constructor, is where time enters the system:

```csharp
public Result<Order> Submit(TimeProvider clock)
{
    return this.ToResult()
        .Ensure(_ => Status == OrderStatus.Draft, Error.UnprocessableContent.ForRule("order.already-submitted", "Already submitted"))
        .Tap(_ =>
        {
            Status = OrderStatus.Submitted;
            DomainEvents.Add(new OrderSubmitted(Id, Total, clock.GetUtcNow()));
        });
}
```

**On the aggregate.** If your aggregate also exposes a public `SubmittedAt` property (e.g., to drive UI sort order or read-model projections), source it from the event timestamp at write time — don't track it independently:

```csharp
public DateTimeOffset? SubmittedAt { get; private set; }

public Result<Order> Submit(TimeProvider clock)
{
    var occurredAt = clock.GetUtcNow();
    // ... ensure rules ...
    Status = OrderStatus.Submitted;
    SubmittedAt = occurredAt;
    DomainEvents.Add(new OrderSubmitted(Id, Total, occurredAt));
    return Result.Ok(this);
}
```

**Why a single timestamp.** Domain events flow into outbox tables, integration buses, audit projections, and event-sourced read models. Every consumer assumes `OccurredAt` is *the* occurrence time. Adding `SubmittedAt`/`ApprovedAt`/`ShippedAt` to individual events forces every consumer to know which field to project per event type — and the two fields can drift if the aggregate's setter and the event constructor are passed different `clock.GetUtcNow()` calls.

**Why `DateTimeOffset`.** `OccurredAt` is `DateTimeOffset` (not `DateTime`) so the explicit offset is a part of the value and round-trips unambiguously through serialization. `TimeProvider.GetUtcNow()` returns `DateTimeOffset` directly — events stored in outbox tables, integration buses, and audit projections retain their authored instant without timezone-loss bugs.

**See also.** The XML doc on `IDomainEvent.OccurredAt` (in `Trellis.Core`) calls this out explicitly. If your IDE shows the doc on hover, the rule is right there before you hit the compile error.

---

## Recipe 18 — DTO primitives to value-object command: no test-only `Unwrap()`

**Problem.** Request DTOs often carry primitive transport fields (`string email`, `string customerName`), while commands and domain methods should receive Trellis value objects. Each `TryCreate` returns `Result<TVO>`. Do not use `Unwrap()` in production code — it is a `Trellis.Testing` helper for tests.

```csharp
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Trellis;
using Trellis.Asp;
using Trellis.Primitives;

public sealed record CreateCustomerRequest(string Email, string CustomerName);

public sealed partial class CustomerId : RequiredGuid<CustomerId>;

public sealed record CustomerResponse(CustomerId Id, string Email, string CustomerName);

[StringLength(200, MinimumLength = 1)]
public sealed partial class CustomerName : RequiredString<CustomerName>;

public sealed record CreateCustomerCommand(EmailAddress Email, CustomerName CustomerName)
    : ICommand<Result<CustomerResponse>>
{
    public static Result<CreateCustomerCommand> TryCreate(CreateCustomerRequest request) =>
        Result.Combine(
                EmailAddress.TryCreate(request.Email, nameof(request.Email)),
                CustomerName.TryCreate(request.CustomerName, nameof(request.CustomerName)))
            .Map((email, customerName) => new CreateCustomerCommand(email, customerName));
}

[ApiController]
[Route("customers")]
public sealed class CustomersController(ISender sender) : ControllerBase
{
    [HttpPost]
    public ValueTask<ActionResult<CustomerResponse>> Create(
        [FromBody] CreateCustomerRequest request,
        CancellationToken ct) =>
        CreateCustomerCommand.TryCreate(request)
            .BindAsync(command => sender.Send(command, ct))
            .ToHttpResponseAsync()
            .AsActionResultAsync<CustomerResponse>();
}
```

**What it shows.**

- Keep DTOs transport-shaped and commands/domain methods value-object-shaped.
- Pass field names into `TryCreate` so failures point at the request field (`/Email`, `/CustomerName` after pointer normalization at the ASP boundary).
- Use `Result.Combine(...)` to aggregate per-field `Error.UnprocessableContent` failures into one validation response.
- Stay on the ROP track: invalid input short-circuits before `sender.Send(...)`; valid input creates the command and continues.

**Anti-pattern -> fix.**

```csharp
// WRONG — Unwrap() is test-only and turns validation failures into thrown exceptions.
var command = new CreateCustomerCommand(
    EmailAddress.TryCreate(request.Email).Unwrap(),
    CustomerName.TryCreate(request.CustomerName).Unwrap());

// FIX — aggregate value-object creation results and bind into the command.
var command = Result.Combine(
        EmailAddress.TryCreate(request.Email, nameof(request.Email)),
        CustomerName.TryCreate(request.CustomerName, nameof(request.CustomerName)))
    .Map((email, customerName) => new CreateCustomerCommand(email, customerName));
```

---

## Recipe 19 — HTTP client result safety and optional reads

**Problem.** Call an upstream HTTP resource safely, preserving non-success status codes as Trellis errors and treating a missing optional resource as `Maybe.None`.

```csharp
using System.Net;
using System.Text.Json.Serialization;
using Trellis;
using Trellis.Http;

[JsonSerializable(typeof(OrderDto))]
public sealed partial class OrderJsonContext : JsonSerializerContext;

public sealed record OrderDto(Guid Id, decimal Total);

public Task<Result<OrderDto>> GetRequiredOrderAsync(HttpClient client, Guid id, CancellationToken ct) =>
    client.GetAsync($"/orders/{id}", ct)
        .ToResultAsync()
        .ReadJsonAsync(OrderJsonContext.Default.OrderDto, ct);

public Task<Result<Maybe<OrderDto>>> FindOrderAsync(HttpClient client, Guid id, CancellationToken ct) =>
    client.GetAsync($"/orders/{id}", ct)
        .ReadJsonOrNoneOn404Async(OrderJsonContext.Default.OrderDto, ct);
```

**What it shows.**

- Bare `ToResultAsync()` is strict in v3: 2xx responses stay on the success track; non-2xx responses become typed Trellis errors.
- Use `ReadJsonOrNoneOn404Async(...)` when `404` is expected domain absence, not failure.
- Use explicit status mapping only when the upstream status needs a domain-specific resource or policy:

```csharp
client.GetAsync($"/orders/{id}", ct)
    .ToResultAsync(status => status == HttpStatusCode.NotFound
        ? new Error.NotFound(ResourceRef.For<OrderDto>(id))
        : null)
    .ReadJsonAsync(OrderJsonContext.Default.OrderDto, ct);
```

---

## Recipe 20 — Fail-fast vs accumulating: `Sequence`/`Traverse` vs `SequenceAll`/`TraverseAll`

**Problem.** A single pipeline contains two distinct collection-level concerns:

1. **Form-style validation** of a batch payload — every invalid row should be reported in one response, not just the first.
2. **A fan-out fetch** — once one upstream fetch fails, finishing the rest is wasted I/O; the first failure should win.

Trellis ships both shapes on the same surface so you can pick the semantics per call site.

```csharp
using Trellis;
using Trellis.Primitives;

public sealed record CreateContactRow(string Email, string Name);

// 1) Accumulating: every row's failure surfaces in the response.
public Result<IReadOnlyList<EmailAddress>> ValidateAddresses(IEnumerable<CreateContactRow> rows) =>
    rows.TraverseAll(row => EmailAddress.TryCreate(row.Email));
//        ↑ TraverseAll runs the selector for every row.
//          - All succeed   → Ok(list)
//          - One bad email → that single Error.UnprocessableContent (no Aggregate wrap)
//          - Many bad      → one merged Error.UnprocessableContent whose
//                            Fields/Rules concatenate every per-item violation
//          - Mixed kinds   → flat Error.Aggregate of every distinct error

// 2) Fail-fast: stop on the first upstream miss.
public Task<Result<IReadOnlyList<Order>>> LoadOrders(IEnumerable<OrderId> ids, CancellationToken ct) =>
    ids.TraverseAsync((id, c) => repo.LoadAsync(id, c), ct);
//        ↑ TraverseAsync short-circuits on the first failure — no
//          subsequent repository calls are issued.
```

**What it shows.**

- `TraverseAll` / `SequenceAll` exist precisely to solve "show me every error". They use the same `Error.Combine` extension as `EnsureAll`, so two `UnprocessableContent` failures merge and unrelated failures flatten into `Error.Aggregate`.
- `Traverse` / `Sequence` exist precisely to solve "stop wasting work on the first failure". They never *accumulate into* an `Error.Aggregate`; they propagate the first failure as-is (which means if a selector itself returns `Result.Fail<T>(new Error.Aggregate(...))`, that `Aggregate` flows through unchanged — not because Traverse created it, but because Traverse preserves whatever the failing selector produced).
- `TraverseAll` ships the same async surface as `Traverse`: sync, `Task`, `Task` + `CancellationToken`, `ValueTask`, `ValueTask` + `CancellationToken`, plus a `Task<Result<Unit>>` + `CancellationToken` overload. `SequenceAll` is sync-only because `Sequence` is sync-only; if async siblings ever land for `Sequence`, they land for `SequenceAll` at the same time.
- Already have an `IEnumerable<Result<T>>` (e.g. from a `Select` over a `TryCreate`)? Pick `.Sequence()` (fail-fast) or `.SequenceAll()` (accumulating); they're the identity-selector forms of `Traverse` / `TraverseAll`.

**Anti-pattern → fix.**

```csharp
// ❌ Manual loop with early return: loses every error after the first.
foreach (var row in rows)
{
    var r = EmailAddress.TryCreate(row.Email);
    if (r.IsFailure) return r.Map(_ => default(IReadOnlyList<EmailAddress>)!);
    parsed.Add(r.Unwrap());
}
// ✅ Explicit choice between fail-fast and accumulating semantics:
return rows.TraverseAll(row => EmailAddress.TryCreate(row.Email));
```

---

## Recipe 21 — Parallel independent loads in handlers: `Result.ParallelAsync` + `WhenAllAsync`

**Problem.** A handler needs two (or more) loads that are *genuinely* independent — a customer record from one upstream service, a product record from another; an HTTP call to authn plus a DB read for profile; or two reads against two distinct EF Core `DbContext` instances. Written sequentially, each `await` blocks the next, so latency = sum of fetches. Written naively in parallel with `Task.WhenAll`, error handling falls back to throwing, you lose the `Result<T>` track, and the lab evidence shows that authors (human and AI) reach for the sequential form by default because it "looks correct" and the tests pass.

`Result.ParallelAsync(...)` is the framework's opinionated entry point: factory-takes-no-args, eagerly invokes each factory so both tasks actually run concurrently, returns a tuple of `Task<Result<T>>` that the matching `.WhenAllAsync()` extension awaits with `Task.WhenAll` and folds via `Result.Combine` into a single `Result<(T1, T2, …)>`. Failures combine through `Error.Combine`, so two `Error.UnprocessableContent` failures merge their fields, heterogeneous failures become an `Error.Aggregate`.

> 🛑 **Danger — `Result.ParallelAsync` against repositories that share a `DbContext` instance will race and throw.** The hard rule, independent of DI: **never start a second operation on a `DbContext` before the first one has completed.** EF Core's `DbContext` is documented as not thread-safe; the second concurrent operation throws `InvalidOperationException: A second operation was started on this context instance before a previous operation completed.` The typical Trellis EF wiring triggers this case by construction: `services.AddDbContext<TContext>(...)` registers the context with its default scoped lifetime (this is the default of the parameterless overload — `AddDbContext<TContext>(..., ServiceLifetime)` can override it), and `services.AddTrellisUnitOfWork<TContext>()` registers `IUnitOfWork` as scoped and consumes the scoped `TContext`. So **every repository resolved from the same request scope shares one `DbContext` instance**, and parallelising two repository reads parallelises two operations on that one context. Keep them sequential — see "When NOT to use it" below. `Result.ParallelAsync` is for genuinely cross-resource concurrency (HTTP + DB, two distinct upstream services, factory-created independent contexts), not for two repository reads against the same store.

```csharp
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Trellis;

public sealed record CheckoutCommand(UserId UserId, ProductSku Sku) : ICommand<Result<CheckoutQuote>>;

// Genuinely cross-resource: one upstream HTTP call + one read against a DIFFERENT
// store (a pricing cache). These have no shared state and run safely in parallel.
public sealed class CheckoutHandler(
    IUserDirectoryClient directory,   // outbound HTTP
    IPricingCache pricing)            // independent in-memory store
    : ICommandHandler<CheckoutCommand, Result<CheckoutQuote>>
{
    public ValueTask<Result<CheckoutQuote>> Handle(CheckoutCommand command, CancellationToken cancellationToken) =>
        new(Result.ParallelAsync(
                //  ↑ takes parameterless factory funcs — NOT pre-started tasks.
                //    Each factory is invoked eagerly here so both fetches execute concurrently.
                () => directory.FindUserAsync(command.UserId, cancellationToken),
                () => pricing.GetQuoteAsync(command.Sku, cancellationToken))
            .WhenAllAsync()
            //  ↑ awaits Task.WhenAll, folds the two Result<T> into Result<(User, Quote)>
            //    via Result.Combine. Two Error.UnprocessableContent failures merge their
            //    Fields/Rules; heterogeneous errors flatten into Error.Aggregate.
            .BindAsync(t => CheckoutQuote.Create(t.Item1, t.Item2, command.Sku)));
}
```

**What it shows.**

- `Result.ParallelAsync` takes `Func<Task<Result<T>>>` factories, NOT `Task<Result<T>>` instances. The factory shape is the API's only safeguard against the "I started the tasks before passing them in" mistake that makes them sequential anyway.
- `.WhenAllAsync()` on the tuple is the matching extension. Without it you still have a tuple of `Task<Result<T>>` — which isn't awaitable on its own; you'd have to await each task individually and combine the results by hand. `.WhenAllAsync()` is the one-line fold.
- The combined `Result<(T1, T2)>` flows back into the standard ROP chain (`BindAsync`, `MapAsync`, `TapAsync`) — no `match` / `if (success)` branches.
- `Result.ParallelAsync` ships overloads for 2–9 factories. For collections, prefer `TraverseAsync` ([Recipe 20](#recipe-20--fail-fast-vs-accumulating-sequencetraverse-vs-sequencealltraverseall)) — it's the right tool when the count is dynamic.

**When NOT to use it.**

1. **Two or more repositories sharing the same scoped `DbContext`.** The most common case in a typical Trellis service. The repos look independent at the C# level, but they all resolve `IRepositoryBase<T, TId>` from the same scoped `TContext`. Parallelising them races the underlying connection and throws `InvalidOperationException`. **Keep sequential `Bind` for this case** — the savings vs the sum-of-fetches are negligible against a local DB anyway, and the integrity loss is real.
2. **The second factory's body references a value produced by the first.** Not independent — keep the sequential `BindAsync` chain. The rule is mechanical: if the second load requires data the first one produced (an id, a filter, a cursor), the two are sequential by definition.
3. **Side-effecting writes.** `Result.ParallelAsync` is for reads. Parallel `repository.Add(...)` calls against a shared context have the same race as parallel reads, plus tracker contention; parallel writes against per-scope contexts need transaction coordination outside this helper.

**Anti-pattern → fix.**

```csharp
// ❌ UNSAFE — two repository calls against repositories that share a scoped DbContext.
// Looks "obviously parallelisable" but races the underlying EF context. `Task.WhenAll`
// waits for both factories to complete (or one to throw), so the concrete failure mode
// is an `InvalidOperationException("A second operation was started on this context...")`
// thrown by EF Core when the second concurrent operation hits the shared connection.
// Reproduction is timing-dependent: the throw is reliable under contention but can be
// missed on a near-instant warm cache, which lulls authors into thinking it's correct.
public ValueTask<Result<DraftOrderId>> Handle(CreateDraftOrderCommand command, CancellationToken cancellationToken) =>
    new(Result.ParallelAsync(
            () => _customers.FindByIdAsync(command.CustomerId, cancellationToken),  // shared DbContext
            () => _products.FindByIdAsync(command.ProductId, cancellationToken))    // shared DbContext
        .WhenAllAsync()
        .BindAsync(t => DraftOrder.CreateDraft(t.Item1, t.Item2, command.Quantity)));

// ✅ Sequential against a shared DbContext — correct by construction. The latency
// cost is the sum of two local reads, which is negligible in practice.
public async ValueTask<Result<DraftOrderId>> Handle(CreateDraftOrderCommand command, CancellationToken cancellationToken)
{
    var customerResult = await _customers.FindByIdAsync(command.CustomerId, cancellationToken);
    var productResult  = await _products.FindByIdAsync(command.ProductId, cancellationToken);

    return Result.Combine(customerResult, productResult)
        .Bind(t => DraftOrder.CreateDraft(t.Item1, t.Item2, command.Quantity))
        .Tap(_orders.Add)
        .Map(o => o.Id);
}

// ✅ Truly parallel — only when the two loads hit independent resources. The HTTP
// call and the in-memory cache have no shared state; ParallelAsync is safe here.
//
// For two EF reads in genuine parallel, inject `IDbContextFactory<TContext>` and
// open two short-lived contexts inside the handler:
//
//   await using var aCtx = await _factory.CreateDbContextAsync(ct);
//   await using var bCtx = await _factory.CreateDbContextAsync(ct);
//   // ... use aCtx and bCtx independently inside ParallelAsync factories ...
//
// The trade-off: two extra contexts and their connections per request, in exchange
// for parallel reads. Worth it only when the queries are slow.
```

---

## Recipe 22 — Multi-aggregate orchestration: fail-loud on missing related aggregates

**Problem.** A command needs to mutate one primary aggregate (an Order) and trigger a side effect on each element of a related-aggregate set (release reserved stock on every Product referenced by the Order's line items). The naive shape is a `foreach` with a dictionary lookup of the related aggregate by id. If a referenced related aggregate is missing from that lookup — invariant violation; under normal flow it should never happen — what is the correct behaviour?

The wrong answer: `if (!dict.TryGetValue(id, out var related)) continue;` and move on. The handler appears to succeed, the primary aggregate transitions, the unit of work commits, and the side effect is silently skipped for the missing element. The orchestration has broken its own post-condition ("stock is released for every line item") with no signal to the caller and no rollback.

The right answer: fail-loud. Return `Error.NotFound` referencing the missing related aggregate so the unit of work rolls back the primary aggregate's mutation. The invariant — "every line item resolves to a Product, or the command fails atomically" — is now enforced and observable.

Two operating principles drive the snippet below:

- **Preflight before mutating, not during.** An in-memory aggregate mutation persists through the rest of the handler's object graph even if the later database commit rolls back. So a release-then-discover pattern leaks partially-released stock into any subsequent read of the same aggregate within the request scope. Compute the missing-set *before* any side effect.
- **Report the full problem.** That's the value of preflighting — surface every missing id in one round-trip via `Error.Aggregate`, with each missing id keeping its own structured `ResourceRef`. (This recipe assumes product ids are non-sensitive; if your threat model differs, drop the per-id refs and emit a single generic `NotFound`.)

```csharp
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Trellis;

public sealed class ReturnOrderHandler(
    IOrderRepository orders,
    IProductRepository products,
    TimeProvider timeProvider) : ICommandHandler<ReturnOrderCommand, Result<Order>>
{
    public async ValueTask<Result<Order>> Handle(ReturnOrderCommand command, CancellationToken cancellationToken)
    {
        // Repository find returns Result<T> — matches Recipe 21's repository shape;
        // .TryGetValue extracts the success value or short-circuits on the existing Error.
        var orderResult = await orders.FindByIdAsync(command.OrderId, cancellationToken);
        if (!orderResult.TryGetValue(out var order))
            return orderResult;

        // Batch fetch returns what it found — the set-difference is the orchestrator's job.
        var productIds = order.LineItems.Select(li => li.ProductId).Distinct().ToArray();
        var loaded = await products.GetByIdsAsync(productIds, cancellationToken);
        var byId = loaded.ToDictionary(p => p.Id);

        // Preflight: prove EVERY related aggregate is reachable BEFORE any side effect.
        // NEVER `continue` past a missing related aggregate, and NEVER mutate before the
        // set is fully reachable. Report ALL missing ids via Error.Aggregate.
        static Error NotFoundFor(ProductId id) => new Error.NotFound(ResourceRef.For<Product>(id))
        {
            Detail = "Product referenced by line item is missing — cannot release stock.",
        };
        var missing = productIds.Where(id => !byId.ContainsKey(id)).ToArray();
        if (missing.Length == 1)
            return Result.Fail<Order>(NotFoundFor(missing[0]));
        if (missing.Length > 1)
            return Result.Fail<Order>(new Error.Aggregate(missing.Select(NotFoundFor).ToArray()));

        // All related aggregates reachable. Safe to apply side effects.
        foreach (var li in order.LineItems)
        {
            var product = byId[li.ProductId];
            var release = product.ReleaseStock(li.Quantity);
            if (release.Error is { } err)
                return Result.Fail<Order>(err);
        }

        return order.Return(command.Reason, timeProvider.GetUtcNow()).Map(_ => order);
    }
}
```

**The test-design principle: Partial-Failure Atomicity.**

> For every post-condition phrased as *"X is done for every Y in a set,"* write a test where one Y is unreachable at operation time. The post-condition must hold under one of two regimes:
>
> 1. **All-or-nothing:** the operation fails atomically; no X is performed.
> 2. **Best-effort with explicit signal:** every reachable Y has X performed; every unreachable Y produces a recorded error in the result.
>
> The implementation must pick one explicitly. If a "for every" handler silently `continue`s past an unreachable Y with neither failure nor signal, it has broken the post-condition and the unit of work has committed an inconsistent state.
>
> The test must hold the related-aggregate's pre-operation state and assert that no partial side-effect leaked through.

The generator test (the one a coverage-driven author would never produce from a happy-path-only mental model):

```csharp
[Fact]
public async Task Return_with_missing_product_fails_atomically_and_does_not_release_stock()
{
    // Seed two products A and B; build a Delivered order with line items referencing both.
    var (order, productA, productB) = await SeedDeliveredWithTwoProductsAsync();
    var aStockBefore = productA.StockQuantity;
    var bStockBefore = productB.StockQuantity;

    // Disrupt the universally-quantified set: drop productB from the repository.
    _products.Remove(productB);

    var r = await _sender.Send(new ReturnOrderCommand(order.Id, _reason), cancellationToken);

    // The orchestration MUST be atomic: either every line item's stock is released,
    // or the operation fails and NOTHING is released.
    r.Should().BeFailureOfType<Error.NotFound>();
    productA.StockQuantity.Should().Be(aStockBefore);  // A NOT partially released
    productB.StockQuantity.Should().Be(bStockBefore);  // B unmutated (the handler never reached it)
    order.Status.Should().Be(OrderStatus.Delivered);    // Order NOT transitioned (UoW rolls back)
}
```

**Generalisation.** Anywhere a handler does:

```csharp
foreach (var x in someSet)
{
    if (!cache.TryGetValue(x.RelatedId, out var related))
        continue;                              // ❌ silent skip — see anti-pattern below
    related.SideEffect(x.Args);
}
```

…there is a hidden missing-related-aggregate path. Apply the Partial-Failure Atomicity test pattern to every such loop. Failing tests will not be caught by a happy-path-only suite because every test sets up a known-good related-aggregate set.

**Anti-pattern → fix.**

```csharp
// ❌ Silent skip — passes every happy-path test, but if a Product disappears between
// the order being created and the return being processed (or if a future refactor
// adds a DeleteProduct endpoint), the return "succeeds" with a partially-released
// stock state. No exception, no Result failure, no log entry. Data corruption.
foreach (var li in order.LineItems)
{
    if (!byId.TryGetValue(li.ProductId, out var product))
        continue;                              // ← invariant violation hidden here
    product.ReleaseStock(li.Quantity);
}

// ✅ Fail-loud — invariant violation surfaces as Error.NotFound; UoW rolls back.
foreach (var li in order.LineItems)
{
    if (!byId.TryGetValue(li.ProductId, out var product))
        return Result.Fail<Order>(new Error.NotFound(ResourceRef.For<Product>(li.ProductId))
        {
            Detail = $"Product {li.ProductId} referenced by line item is missing — cannot release stock.",
        });

    var release = product.ReleaseStock(li.Quantity);
    if (release.Error is { } err)
        return Result.Fail<Order>(err);
}
```

---

## Recipe 23 — Concurrency control on aggregate-mutating endpoints: when to require `If-Match`

**Problem.** RFC 9110 §13.1.1 lets clients send `If-Match: "etag"` on unsafe methods to detect stale-read race conditions: if the resource's current ETag doesn't match, the server returns `412 Precondition Failed` instead of overwriting concurrent changes. For mutating handlers, the framework provides `ETagHelper.ParseIfMatch(request)` to extract the typed `EntityTagValue[]` from the incoming `If-Match` header, plus the `Result<T>.OptionalETag(...)` / `RequireETag(...)` extensions (and their `*Async` overloads) that evaluate the precondition at the read-modify-write boundary inside the handler chain. (`opts.WithETag(...).EvaluatePreconditions()` on the response builder is a different feature — it only runs on `GET` / `HEAD` for `If-None-Match` → `304` and `If-Match` → `412` on safe-method reads; it is **not** the mutation hook.) The decision question: which mutating endpoints actually need this?

A blanket "wire `RequireETag` on every mutation" rule is wrong — it adds ceremony to endpoints where there is no lost-update window to begin with. Use the decision table:

| Endpoint shape | Lost-update window? | Use `If-Match`? |
|---|---|---|
| **Body-less state-transition POST** (`POST /orders/{id}/submit`, `.../approve`, `.../cancel`, `.../return`) | **No.** The state machine + transition guards check the current state. A stale client calling `.../approve` on an order that has already shipped gets `422 Unprocessable Content` from the transition guard; there is nothing to overwrite. | **No.** The state machine substitutes for the precondition. Ceremony without benefit. |
| **Body-carrying full-update PUT** (`PUT /orders/{id}` with a full replacement body) | **Yes.** The body silently overwrites whatever the concurrent edit wrote. | **Yes — `RequireETag`.** RFC 6585 says `428 Precondition Required` when missing, RFC 9110 says `412 Precondition Failed` when stale. |
| **Body-carrying partial-update PATCH** with a JSON Patch / JSON Merge Patch document | **Yes.** Same overwrite risk as full update. | **Yes — `RequireETag`.** |
| **Destructive `DELETE /resources/{id}`** | **Yes.** A stale client can delete a version it has not seen after another writer changed it. | **Yes — `RequireETag`** as the default. Drop the precondition only if the endpoint is explicitly modeled as a guarded state-machine transition with equivalent domain or EF concurrency-token protection. |
| **Additive set operation** (`POST /orders/{id}/line-items`, `POST /products/{id}/stock-additions +5`) | **Maybe.** Depends on commutativity. Two concurrent `+5` calls produce `+10` correctly; "remove the last line item" against a stale read of "list has 2 items" can drop the wrong item. | **Case-by-case.** Commutative additive ops can stay precondition-free; remove-by-position or "remove the last X" operations should `RequireETag` (or `OptionalETag` only when the endpoint deliberately admits unconditional callers). |
| **Resource creation** (`POST /customers`, `POST /products`) | **N/A.** No prior version to match against. | **No.** |

```csharp
using Mediator;
using Trellis;
using Trellis.Asp;
using Trellis.EntityFrameworkCore;

// State-transition POST — no If-Match. The state machine guards the transition.
app.MapPost("/orders/{id:guid}/approve", (OrderId id, ISender sender, CancellationToken ct) =>
    sender.Send(new ApproveOrderCommand(id), ct)
        .ToHttpResponseAsync(OrderResponse.From));

// Full-update PUT — RequireETag at the read-modify-write boundary.
// Missing If-Match → 428; stale → 412; current → proceeds.
app.MapPut("/orders/{id:guid}", (OrderId id, ReplaceOrderRequest request, OrderDbContext db, HttpContext httpContext, CancellationToken ct) =>
    db.Orders
        .FirstOrDefaultResultAsync(o => o.Id == id, new Error.NotFound(ResourceRef.For<Order>(id)), ct)
        .RequireETagAsync(ETagHelper.ParseIfMatch(httpContext.Request))
        .BindAsync(o => o.Replace(request))
        .CheckAsync(_ => db.SaveChangesResultUnitAsync(ct))
        .ToHttpResponseAsync(OrderResponse.From, opts => opts.HonorPrefer()));
```

> **Direct `DbContext` in the Minimal API lambda vs command handler via Mediator.** Both shapes are canonical Trellis. This recipe shows the direct-`DbContext` shape because the precondition (`RequireETag`) belongs at the *read-modify-write* boundary — the same atomic unit where the read happens. If you prefer to dispatch through a command handler, move the same chain (`db.Orders.FirstOrDefaultResultAsync(...).RequireETagAsync(...).BindAsync(...).CheckAsync(_ => db.SaveChangesResultUnitAsync(ct))`) into the handler body; `TransactionalCommandBehavior` then owns the commit, and `db.SaveChangesResultUnitAsync(...)` becomes redundant (drop the `.CheckAsync` step). The precondition placement does not change — it still wraps the freshly-loaded aggregate and runs before any mutation.

**Rationale.** The framework helper is fine; the decision is whether the endpoint's semantics admit a lost-update window. State machines, additive set operations, and resource creation each carry their own concurrency control inside the domain or in the absence of prior state. Only payload-carrying overwrites need explicit precondition checking.

---

## Cross-cutting tips

- **Run analyzers in CI.** `Trellis.Analyzers` ships in the framework and runs as part of every `dotnet build`. Treat warnings as errors for `TRLS00x` once your codebase is clean.
- **Two independent `await` calls in a handler?** `Result.ParallelAsync` + `WhenAllAsync` is the framework idiom — **but only when the loads hit different resources**. Two repository reads against the same scoped `DbContext` (the typical Trellis setup with `AddTrellisUnitOfWork<TContext>()`) race EF Core and throw `InvalidOperationException`; keep those sequential. The recipe spells out the safe shapes (HTTP + DB, two distinct upstream services, factory-created `DbContext`s via `IDbContextFactory<T>`) and the anti-pattern. See [Recipe 21](#recipe-21--parallel-independent-loads-in-handlers-resultparallelasync--whenallasync). The rule for "independent": the second factory's body does not reference any value produced by the first **and** the two factories hit distinct underlying resources.
- **Do not mix sync chain methods with async lambdas.** `result.Map(async v => …)` triggers `TRLS009`; use `MapAsync`. The fix provider can apply this rewrite automatically.
- **Construct errors via the closed ADT.** `new Error.NotFound(ResourceRef.For<Order>(id))` — never `new Error("not_found", "...")`, which won't compile against the abstract base record.
- **Use `Result.Combine` (or `EnsureAll`) for accumulating validation.** Manual `IsSuccess` checks across multiple results trigger `TRLS008`.
- **Aggregate per-item Results with `Traverse` / `Sequence` (fail-fast) or `TraverseAll` / `SequenceAll` (accumulating).** When you have a collection and a per-item function returning `Result<T>`, use `items.Traverse(item => Compute(item))` to lift it into `Result<IReadOnlyList<T>>`. When you already have an `IEnumerable<Result<T>>` (e.g., from a `Select`), call `.Sequence()` instead. Both short-circuit on the first failure. When you need to surface every failure (form-style validation), use `TraverseAll` / `SequenceAll`: they run through every item and fold failures via `Error.Combine` — two `UnprocessableContent` errors merge their fields/rules, heterogeneous errors flatten into `Error.Aggregate`. See [Recipe 20](#recipe-20--fail-fast-vs-accumulating-sequencetraverse-vs-sequencealltraverseall) for when to choose which.
- **Use `Error.UnprocessableContent.ForField` / `.ForRule` for single-violation 422s.** The most common shape (every primitive `TryCreate`, every value-object invariant, every `RequiredEnum`/`RequiredString` failure) is a single `FieldViolation` or a single `RuleViolation`. Use the factories instead of the verbose constructor: `Error.UnprocessableContent.ForField("email", "invalid_format", "must contain @")` over `new Error.UnprocessableContent(EquatableArray.Create(new FieldViolation(InputPointer.ForProperty("email"), "invalid_format") { Detail = "must contain @" }))`. There is also `ForField(InputPointer field, …)` for nested/array pointers (e.g. `new InputPointer("/items/0/quantity")`) or `InputPointer.Root` for whole-body violations, and `ForRule(reasonCode, detail)` for global rules. For aggregating multiple per-field violations into one error (e.g. composite VO `TryCreate`), keep the manual constructor with an `EquatableArray<FieldViolation>` or use the `Validate` builder.
- **`InputPointer.Root` for whole-body violations.** Use `InputPointer.ForProperty(name)` for field-level violations and `InputPointer.Root` when the rule is object-level.
- **Only the `Trellis` namespace is auto-imported.** The template's implicit usings include `Trellis` (which exposes `Result`, `Result<T>`, `Error`, `Maybe<T>`, `RequiredString<T>`, `RequiredGuid<T>`, `RequiredInt<T>`, `RequiredDecimal<T>`, `RequiredDateTime<T>`, etc.). Every other Trellis namespace requires an explicit `using` per file — e.g. `using Trellis.Primitives;` for `Money` / `EmailAddress` / `PhoneNumber` / `MonetaryAmount` / `CurrencyCode` / `CountryCode` / etc., `using Stateless;` for the upstream `StateMachine<TState, TTrigger>` type plus `using Trellis.StateMachine;` for the Trellis `FireResult` extension and `LazyStateMachine<TState, TTrigger>`, `using Trellis.Authorization;` for permission types. This is intentional: implicit usings cannot be added at the template level without breaking services that don't reference the package.
- **Accessing `Maybe<T>.Value` inside `Expression<Func<...>>` lambdas (EF Core `Where`/`Select`, FluentValidation `RuleFor`, Specifications):** TRLS003 still applies inside expression trees, but it now recognises the multi-clause guard — `e => e.Status == X && e.Y.HasValue && e.Y.Value == y` is analyzer-clean, and `MaybeQueryInterceptor` translates each clause faithfully to SQL when `AddTrellisInterceptors()` is wired. Hoist into a guarded variable for projections that the interceptor doesn't cover. Do not suppress with `#pragma warning disable TRLS003`. See [Recipe 8](#recipe-8--ef-core-maybepropertymapping-for-nullable-value-objects) for the full Specification walkthrough.
- **`EquatableArray<T>` does not implement `IEnumerable<T>` — project through `.Items` for LINQ / FluentAssertions / `string.Join`.** The sequence-equality wrapper exposes a duck-typed `GetEnumerator()` for allocation-free `foreach` but deliberately does not implement `IEnumerable<T>`. LINQ extension methods (`Select`, `Where`, `Any`, `ToList`) and FluentAssertions extensions (`Should().ContainSingle()`, `Should().HaveCount(...)`, `Should().BeEquivalentTo(...)`) bind on `IEnumerable<T>` and will not compile against the raw wrapper. Call `.Items` first — it returns the wrapped `ImmutableArray<T>`, which IS `IEnumerable<T>`. This shows up most often in test assertions on `Error.UnprocessableContent.Fields` / `.Rules` and in error-rendering helpers. See [`EquatableArray<T>`](trellis-api-core.md#public-readonly-struct-equatablearrayt--iequatableequatablearrayt) in the Core reference for the worked example.

---

## Cross-references

- [trellis-api-core.md](trellis-api-core.md#extension-class-catalog-full-signatures) — every `Result*Extensions(Async)` family with full signatures.
- [trellis-api-core.md](trellis-api-core.md#pagination) — `Cursor`, `Page<T>`, `Page.Empty<T>`.
- [trellis-api-asp.md](trellis-api-asp.md) — `HttpResponseOptionsBuilder<TDomain>` member-by-member.
- [trellis-api-mediator.md](trellis-api-mediator.md#canonical-pipeline-order) — exact behavior ordering.
- [trellis-api-analyzers.md](trellis-api-analyzers.md#constants--trellisdiagnosticids) — every `TrellisDiagnosticIds` constant + emitting analyzer.
