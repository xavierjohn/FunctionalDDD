# Changelog

All notable changes to the Trellis project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added — idempotent inserts

- **`Trellis.EntityFrameworkCore`** — new `DbContextIdempotencyExtensions.TryInsertUniqueAsync<TEntity>(this DbContext, TEntity, CancellationToken)` helper that adds the entity, persists it, and converts a provider-level unique-constraint violation into `Result.Fail<TEntity>(new Error.Conflict(null, "duplicate.key") { Detail = "A record with the same unique value already exists.", ConstraintName, ConstraintTableName })`. The added entity is detached from the change tracker on the duplicate path so a caller retrying with a freshly-constructed entity does not re-flush the original. Foreign-key violations, `DbUpdateConcurrencyException`, connection-level exceptions, and `OperationCanceledException` propagate normally. The helper throws `InvalidOperationException` when the context already has pending changes on entry so a duplicate-key violation cannot be mis-attributed to the inserted entity.
- **`Trellis.EntityFrameworkCore`** — new `DbExceptionClassifier.ExtractConstraintIdentity(DbUpdateException)` returning `(string? ConstraintName, string? TableName)`. Typed extraction first for PostgreSQL (`Npgsql.PostgresException.ConstraintName` / `TableName` / `SchemaName` via reflection so the package stays provider-agnostic; output is `"schema.table"` when both are present); falls back to regex-based parsing for SQL Server errors 2627 / 2601 / FK 547, SQLite `UNIQUE constraint failed: <Table>.<Column>` / `PRIMARY KEY` (FK form returns null pair because SQLite does not name the constraint), and MySQL `Duplicate entry '...' for key '<table>.<key>'`. Defensive: returns `(null, null)` on any unexpected exception so telemetry extraction never breaks a caller.
- **`Trellis.Core`** — `Error.Conflict` gains two `[JsonIgnore]` init-only telemetry fields `ConstraintName` and `ConstraintTableName` (defaults `null`, preserves equality for existing call sites). The new fields are intended for structured logging and are never serialized to API responses; the safe-for-clients message stays in `Detail`.
- **`Trellis.EntityFrameworkCore`** — `SaveChangesResultAsync` and `SaveChangesWithRetryAsync` now populate `ConstraintName` / `ConstraintTableName` on the `Error.Conflict` they return for the `duplicate.key` and `referential.integrity` reason codes (no change to `retry.exhausted` / `retry.aborted` paths).

### Added — `Trellis.Testing.Worker`

- **`Trellis.Testing.Worker`** — new package shipping `WorkerHarness<TWorker>` for integration-testing `BackgroundService` workers. Builds an `IHost` with a deterministic `FakeTimeProvider`, a configurable `TestActorProvider`, and an open-generic capture handler wired into the mediator's `IDomainEventHandler<>` pipeline so tests can advance time, await the first domain event of a given type (with an optional predicate), or await a named tick signal via `IWorkerTickSignal`. The harness owns hosted-service registration for the worker under test and fails fast if `ConfigureServices` also calls `AddHostedService<TWorker>()`. Wait timeouts run on real time so `Time.Advance(...)` only drives the worker's `Task.Delay` / `PeriodicTimer` continuations. `AutoStart` defaults to `false` so tests can subscribe before the worker emits. `WaitForEventAsync` returns the matching `TEvent`; timeouts raise `WorkerHarnessTimeoutException` with a diagnostic that names the awaited condition, captured-event counts, and reminds the user to register `AddDomainEventDispatch()` if missing. Caller cancellation propagates as plain `OperationCanceledException` so tests can distinguish wait-expired from caller-cancelled.

### Added — retry classification

- **`Trellis.Core`** — transport-neutral retry classification over the closed `Error` catalog: `RetryClassification` enum (`Transient`, `Permanent`, `FailFast`) and the static `ErrorRetryExtensions` class with `error.Classify()`, `error.IsTransient()`, `error.IsPermanent()`, `error.IsFailFast()`, and `error.GetRetryAdvice()`. `Classify` is exhaustive over all 12 `Error` cases. `Error.Aggregate` uses max-severity semantics over its inners; `GetRetryAdvice` returns `null` for `Error.Aggregate` by design. `Error.TransportFault` defaults to `Permanent` because every transport-specific payload shipped today (`HttpError.MethodNotAllowed`, `NotAcceptable`, `PreconditionFailed`, `ContentTooLarge`, `UnsupportedMediaType`, `RangeNotSatisfiable`, `PreconditionRequired`) is a caller-side error; retryable transient transport outcomes (HTTP 429, HTTP 503) are mapped at the boundary to `Error.RateLimited` / `Error.Unavailable` and never reach `Error.TransportFault`. Replaces hand-rolled `gatewayResult.Error is Error.Unavailable or Error.RateLimited` switches in worker loops, message consumers, and outbound-gateway clients.

### Added — pagination ergonomics

- **`Trellis.Core`** — first-class cursor pagination primitives shared by every storage adapter and transport: `PageSize` (with `FromRequested` lenient parser and `TryCreate` strict parser), `Cursor` (opaque `readonly record struct`), `CursorCodec` (URL-safe base64 encode / `TryDecode<TKey>` returning `Result<TKey>` with `Error.InvalidInput.ForField(..., "cursor.malformed", ...)` on bad input), `Page<T>` (`Items`, `Next`, `Previous`, `RequestedLimit`, `AppliedLimit`, `WasCapped`, `DeliveredCount`), `Page<T>.Map<TOut>` (preserves cursors and limits across DTO projection), and `PageBuilder.FromOverFetch` (storage-agnostic over-fetch slicer for single-key and composite `(CreatedAt, Id)` seek). All AOT-friendly — no JSON, no reflection, no `Expression.Compile`.
- **`Trellis.EntityFrameworkCore`** — `PaginationQueryableExtensions.ToPageAsync<T, TKey>(this IQueryable<T>, PageSize, Cursor?, Expression<Func<T, TKey>>, string?, CancellationToken)`. The helper owns the `OrderBy(keySelector)`, the cursor decode (returns `Result.Fail<Page<T>>` with `cursor.malformed` on bad input — never throws), the seek `WHERE` predicate (`Expression.GreaterThan` for numeric / `DateTime` / `DateTimeOffset` keys; `IComparable<TKey>.CompareTo` for `Guid` and `string` keys), the `Take(Applied + 1)` over-fetch, and the slice via `PageBuilder.FromOverFetch`. The canonical EF query-handler shape collapses to one line: `await db.Orders.AsNoTracking().ToPageAsync(pageSize, cursor, o => o.Id.Value, "cursor", ct)`. Value-object Id projections (`c => c.Id.Value`) require `AddTrellisInterceptors()`. Single-key seek requires a stable, unique ascending key; a composite `(CreatedAt, Id)` overload is deferred to a follow-up release.
- **`Trellis.Core`** — `Maybe<T>.HasValueWhere(Func<T, bool>)` for fluent presence-and-predicate checks; `Money.Sum` extension methods over `IEnumerable<Money>` with single-currency validation.

### Added — ProblemDetails `Instance` synthesised from `ResourceRef`

- **`Trellis.Core`** — new `ResourceCollectionNameAttribute(string name)` for marking an aggregate type with a non-default URI collection name (for example `[ResourceCollectionName("people")]` on `Person`). The attribute exposes the value via the `Name` property and validates that it is a non-empty single URL path segment composed only of RFC 3986 `unreserved` characters (ASCII letters and digits plus `-`, `.`, `_`, `~`) at construction time so misconfiguration fails fast. The `IsSafePathSegment(string?)` predicate is exposed as a public static helper (null/empty input returns `false`) so other layers can validate user-provided segment names against the same rule.
- **`Trellis.Asp`** — `ResponseFailureWriter` now synthesises `ProblemDetails.Instance` from the failing `ResourceRef` when the request URL does not already identify the resource. For example, `POST /api/orders` with body `{ customerId: "abc-123" }` that fails with `Error.NotFound(ResourceRef.For("Customer", "abc-123"))` now emits `"instance": "/customers/abc-123"` (the failing resource URI) and preserves the original request URI under `Extensions["request"]`. Applies to every error case that carries a `ResourceRef`: `NotFound`, `Gone`, `Conflict?`, `Forbidden?`, `InvariantViolation?`, and `TransportFault(HttpError.PreconditionFailed)`. Synthesis is suppressed when the URL already identifies the resource (segment- and query-value-aware match against both the raw id and its percent-decoded form; `+` is treated as space in query values to match ASP.NET Core's form-encoding semantics). The aggregate envelope (`Error.Aggregate`) never promotes a child's `ResourceRef` because the envelope itself carries no resource identity.
- **`Trellis.Asp`** — new `TrellisAspOptions.SynthesizeProblemDetailsInstanceFromResourceRef` toggle (default `true`). Set to `false` to preserve the historical request-URL-only `Instance` behavior. The new shape is strictly more informative, so the toggle exists for backward-compat-strict callers only.
- **`Trellis.Asp`** — four new `IServiceCollection` extensions for overriding the default collection name (`{Type.ToLowerInvariant()}s`): `AddResourceCollectionName<T>(string)` and `AddResourceCollectionName(string resourceType, string collectionName)` are AOT- and trim-friendly; `AddResourceCollectionNames(Assembly)` and `AddResourceCollectionNames(params Assembly[])` scan the supplied assemblies for `[ResourceCollectionName]` (both marked `[RequiresUnreferencedCode]`). Lookups are case-insensitive. Registered overrides are emitted verbatim into the synthesised URI (the lowercase guarantee applies only to the naive plural fallback). Conflicting registrations (same type → different collection names) fail fast at registry activation; identical registrations coalesce silently.

### Breaking changes — `Trellis.Core.Error` union DDD realignment

The `Trellis.Core.Error` discriminated union no longer embeds HTTP/RFC transport vocabulary. The domain stays transport-neutral; HTTP-specific error types now live in a new `Trellis.Http.Abstractions` package and flow through `Result<T>` via the `Error.TransportFault(ITransportFault Fault)` envelope.

The closed union now has 12 cases: `InvalidInput`, `InvariantViolation`, `NotFound`, `Forbidden`, `Conflict`, `Gone`, `AuthenticationRequired`, `Unavailable`, `RateLimited`, `Unexpected`, `Aggregate`, `TransportFault`.

#### Migration table

| Old | New |
|---|---|
| `Error.BadRequest("X")` | `Error.InvalidInput.ForRule("X")` |
| `Error.BadRequest("X", pointer)` | `Error.InvalidInput.ForField(pointer, "X")` |
| `Error.BadRequest("X") { Detail = d }` | `Error.InvalidInput.ForRule("X", d)` |
| `Error.UnprocessableContent(fields, rules)` | `new Error.InvalidInput(fields, rules)` |
| `Error.UnprocessableContent.ForField/ForRule(...)` | `Error.InvalidInput.ForField/ForRule(...)` |
| `Error.Unauthorized()` | `Error.AuthenticationRequired()` (optional `Scheme`) |
| `Error.TooManyRequests()` | `Error.RateLimited()` (optional `RetryAdvice`) |
| `Error.ServiceUnavailable()` | `Error.Unavailable()` (optional `ReasonCode`, `RetryAdvice`) |
| `Error.InternalServerError(faultId)` | `new Error.Unexpected(reasonCode, faultId)` |
| `Error.NotImplemented("X")` | `new Error.Unexpected("not_implemented") { Detail = "Feature 'X' is not implemented." }` |
| `Error.MethodNotAllowed`, `NotAcceptable`, `UnsupportedMediaType`, `RangeNotSatisfiable`, `ContentTooLarge`, `PreconditionFailed`, `PreconditionRequired` | Removed from `Trellis.Core`. Use `new Error.TransportFault(new HttpError.X(...))` from `Trellis.Http.Abstractions`. |

#### New cases

- `Error.InvariantViolation(ReasonCode, ResourceRef?)` — global / multi-field business invariant violated outside the inbound-validation pipeline.
- `Error.Aggregate(EquatableArray<Error>)` — first-class envelope for multiple failures.
- `Error.TransportFault(ITransportFault)` — envelope for transport-specific failures (currently `HttpError.*`).

#### New transport-neutral type

- `RetryAdvice(TimeSpan? After = null, DateTimeOffset? At = null)` in `Trellis.Core` — replaces the HTTP-specific `RetryAfterValue` on retry-bearing error cases. `RetryAfterValue` still exists, but now lives in `Trellis.Http.Abstractions` and is used only at the HTTP boundary.

#### Kind-slug changes

Telemetry consumers that aggregate failures by `Error.Kind` need to update their slug sets:

| Old slug | New slug |
|---|---|
| `bad-request` | `invalid-input` (BadRequest folded into InvalidInput) |
| `unprocessable-content` | `invalid-input` |
| `unauthorized` | `authentication-required` |
| `too-many-requests` | `rate-limited` |
| `service-unavailable` | `unavailable` |
| `internal-server-error` | `unexpected` |
| `not-implemented` | `unexpected` (with `ReasonCode == "not_implemented"`) |

#### Wire format unchanged

The HTTP boundary (`Trellis.Asp.ResponseFailureWriter`) preserves the historical problem-details `kind` extension tokens (`unprocessable-content`, `unauthorized`, `too-many-requests`, `service-unavailable`, `internal-server-error`, `not-implemented`) verbatim. External HTTP API consumers parsing problem-details see no wire change. The top-level Problem Details `type` field continues to default to ASP.NET's status-code URL. RFC 9110, 9457, and 6585 compliance is unaffected.

#### New package

`Trellis.Http.Abstractions` — shared by `Trellis.Asp` (server) and `Trellis.Http` (client). Hosts the `HttpError.*` closed union and the HTTP supporting types previously in `Trellis.Core` (`PreconditionKind`, `AuthChallenge`, `RetryAfterValue`, `EntityTagValue`, `AggregateETagExtensions`, `RepresentationMetadata`, `WriteOutcome<T>`). Add `<PackageReference Include="Trellis.Http.Abstractions" .../>` only when boundary code references these types directly; `Trellis.Asp` and `Trellis.Http` bring it in transitively.

See [`MIGRATION_v3.md`](MIGRATION_v3.md#error-union-ddd-realignment) for code-level before/after examples.

### Fixed

- **`Trellis.EntityFrameworkCore`** — `ApplyTrellisConventions(...)` now includes the `Trellis.Authorization` assembly in its default scan set. After the v3 typed-`ActorId` change, an aggregate carrying a `CreatedByActorId : ActorId` audit field silently failed EF mapping because the convention previously only included `Trellis.Core` and `Trellis.Primitives` by default; consumers had to pass `typeof(ActorId).Assembly` explicitly to get the scalar converter. The default scan set now mirrors the `Trellis.Primitives` precedent so `ApplyTrellisConventions(typeof(MyDomainId).Assembly)` is sufficient. `Trellis.EntityFrameworkCore` gains a project reference on `Trellis.Authorization` — a lightweight dependency (its only reference is `Trellis.Core`) that ASP consumers already receive transitively via `Trellis.Asp`.

### Breaking changes — server-side byte-range emission removed

Trellis targets general business web services, not media servers. The server-side byte-range emission surface duplicated `Microsoft.AspNetCore.Http.Results.File(stream, enableRangeProcessing: true)` and added no Trellis-specific value, so it has been removed.

**Removed public API**

- `Trellis.Asp.PartialContentHttpResult` (Minimal API `IResult`)
- `Trellis.Asp.PartialContentResult` (MVC `ObjectResult`)
- `Trellis.Asp.RangeRequestEvaluator` and the `RangeOutcome` closed union (`FullRepresentation` / `PartialContent` / `NotSatisfiable`)
- `HttpResponseOptionsBuilder<T>.WithRange(Func<T, ContentRangeHeaderValue>)`, `WithRange(long, long, long)`, `WithAcceptRanges(string)`
- `RepresentationMetadata.AcceptRanges` and `RepresentationMetadata.Builder.SetAcceptRanges(string)` (every other `RepresentationMetadata` member is unchanged)
- The `Status206PartialContent` `ProducesResponseTypeMetadata` entry from `TrellisHttpResult<TDomain, TBody>.PopulateMetadata` (OpenAPI no longer advertises `206` for Trellis-mapped endpoints)

**Migration**

- For binary downloads with RFC 9110 §14 byte-range semantics, call `Microsoft.AspNetCore.Http.Results.File(stream, enableRangeProcessing: true)` directly — ASP.NET Core implements byte semantics natively.
- For advisory headers such as `Accept-Ranges: none`, write the header on `HttpContext.Response.Headers` from middleware or the endpoint handler.

**Preserved (client-side typed-error round-trip)**

`HttpError.RangeNotSatisfiable(long CompleteLength, string Unit = "bytes")` still exists in `Trellis.Http.Abstractions`. Inbound `416` responses on the HTTP client continue to surface as `Error.TransportFault(new HttpError.RangeNotSatisfiable(...))` with the upstream `Content-Range` length and unit preserved, and `Trellis.Asp.ResponseFailureWriter` still emits `416` plus `Content-Range: {Unit} */{CompleteLength}` when such a fault propagates up through a `Result` chain.

## [3.0.0]

The first GA release under the **Trellis** name. This release supersedes
the `FunctionalDdd` 2.x line; consumers upgrading from `FunctionalDdd 2.1`
should follow the [migration guide](docs/docfx_project/articles/migration.md)
for step-by-step instructions. The summary below describes what changed in
the move from FunctionalDdd 2.1 to Trellis 3.0.

### Project rename and package reorganization (breaking)

- The project is renamed from `FunctionalDdd` to `Trellis`. The root
  namespace, all package ids, and all repository URLs change accordingly
  (e.g., `FunctionalDdd.RailwayOrientedProgramming` becomes `Trellis.Core`).
- The five FunctionalDdd packages are consolidated and expanded into the
  Trellis package family:

  | FunctionalDdd 2.1 package        | Trellis 3.0 package                                  |
  |----------------------------------|------------------------------------------------------|
  | `RailwayOrientedProgramming`     | `Trellis.Core` (folded with DomainDrivenDesign)      |
  | `DomainDrivenDesign`             | `Trellis.Core`                                       |
  | `CommonValueObjects`             | `Trellis.Primitives`                                 |
  | `Asp`                            | `Trellis.Asp`                                        |
  | `FluentValidation`               | `Trellis.FluentValidation`                           |

### New packages

- `Trellis.Mediator` — Result-aware in-process mediator with a canonical
  pipeline (exception → tracing → logging → authorization → resource
  authorization → validation → transactional commit, outermost to innermost).
  Supports both reflection-based and AOT-friendly source-generated dispatch.
- `Trellis.Authorization` — typed `Actor` / `ActorId`, `IAuthorize`,
  `IAuthorizeResource<TResource>`, `IAuthorizeResourceVia<TOwner>`, and the
  ASP integration points (`IActorProvider`, `ClaimsActorProvider`,
  `EntraActorProvider`, `CachingActorProvider`).
- `Trellis.EntityFrameworkCore` — unit-of-work, transactional command
  behavior, `TrellisScalarConverter`, the composite value object EF
  convention, `[OwnedEntity]` attribute, and supporting analyzers.
- `Trellis.StateMachine` — declarative aggregate state machines with
  compile-time transition validation.
- `Trellis.Asp.ApiVersioning` — `WithVersionedRoute()` helper (chained after
  `CreatedAtRoute(...)`, `CreatedAtAction(...)`, or `WithLocation(...)`) and versioned-projection guard
  rails for `Asp.Versioning.Http`.
- `Trellis.ServiceDefaults` — single composition root (`AddTrellis(...)`)
  that wires every framework slot in the right order.
- `Trellis.Http` — typed HTTP-client primitives for outbound calls.
- `Trellis.Testing` and `Trellis.Testing.AspNetCore` — FluentAssertions
  extensions for `Result<T>` / `IResult`, problem-details assertions, and
  WebApplicationFactory helpers.
- `Trellis.Analyzers` — Roslyn analyzers covering Maybe / Result misuse,
  ValueObject derivation, EF / JSON converter wiring, etc. (`TRLS001` …
  `TRLS042+`).

### `Error` redesigned as a closed ADT (breaking)

`Error` is no longer an open class with public constructors and ad-hoc
factories. It is now a closed algebraic data type whose only inhabitants
are the documented kinds: `Validation` / `UnprocessableContent`, `NotFound`,
`Conflict`, `Forbidden`, `Unauthorized`, `InternalServerError`. Each kind
has a typed factory (e.g., `Error.NotFound(...)`, `Error.Conflict(...)`)
that surfaces the metadata the wire mapper needs (resource references,
problem-details fields).

Consumers porting from `FunctionalDdd.Error`'s open constructor / generic
factories should map each call site to one of the kind-specific factories.
The migration guide covers the mechanical replacements.

### `Result<T>` JSON safety net (breaking)

`Result<T>` (and the `IResult` / `IResult<T>` interfaces) now carry a
default `[JsonConverter]` that throws `NotSupportedException` on any direct
JSON serialize / deserialize attempt. Previously, returning a raw
`Result<T>` from a controller silently produced a struct-dump JSON shape
(`{"IsSuccess": true, "Value": ..., "Error": null}`) with no HTTP
status-code mapping. The new converter fires on the first request and
names the canonical fix: call `.ToHttpResponse()` (Trellis.Asp) or unwrap
via `Match` / `TryGetValue` before serialization. Option-registered
converters take precedence and let consumers opt back in for logging /
IPC / storage scenarios.

### `RequiredXxx<T>` POLA realignment (breaking)

The `RequiredXxx<T>` family now follows a single rule: **reject only null**.
Per-type "zero value" rejection (`""` for strings, `0` for numerics,
`Guid.Empty`, `DateTime.MinValue`) is opt-in via the new `[NotDefault]`
attribute. String trim is opt-in via `[Trim]`. This makes the family
uniform with `RequiredInt<T>(0)` — which has always succeeded — and matches
the Principle of Least Astonishment.

Validation order in the generated `TryCreate` is `null → [Trim] →
[NotDefault] → [StringLength] / [Range] → ValidateAdditional`. New
compile-time diagnostics `TRLS040` (`[NotDefault]` on `RequiredBool<T>`),
`TRLS041` (`[Trim]` on a non-`RequiredString`), and `TRLS042` (`[NotDefault]`
on `RequiredEnum<T>`) cover degenerate combinations.

The EF Core `TrellisScalarConverter` rehydrates via `TryCreate`, so
lenient-by-default types now accept persisted sentinel values
(`Guid.Empty`, `""`, `DateTime.MinValue`). Add `[NotDefault]` to any
`RequiredGuid` / `RequiredDateTime` used as an aggregate id or EF-mapped
property to preserve strict rehydration.

### Actor model

- `Actor` is an entity (identity-based equality on `ActorId`); equality
  is no longer a record-style structural compare over every field.
- `Actor.Id` is strongly typed as `ActorId : RequiredString<ActorId>`
  (`[Trim, NotDefault]`). The string-accepting constructors and `Create`
  overloads remain for authentication-boundary code (claim → actor) and
  wrap the raw value via `ActorId.Create` internally. JSON serialization
  emits the raw string for wire compatibility.
- `IActorProvider.GetCurrentActorAsync` returns `Task<Maybe<Actor>>`
  (breaking) — anonymous requests are an absence, not a thrown exception.
  `CachingActorProvider` caches both successes and synchronous failures
  from the inner provider.
- `ClaimsActorProvider` understands both short and long claim-name forms
  (including `PermissionsClaim` and `JwtBearer.MapInboundClaims`).

### Mediator, domain events, and resource authorization

- `Trellis.Mediator` introduces a Result-aware pipeline; `AddMediator` and
  the AOT-friendly source-generator path share the same canonical order.
- Domain event dispatch lands inside the unit-of-work commit so events
  publish only when the transaction succeeds.
- Resource authorization (`IAuthorizeResource<TResource>`,
  `IAuthorizeResourceVia<TOwner>`) checks ownership / permissions against
  the loaded resource exactly once per request, slotted immediately before
  the validation behavior.
- Unified validation: a single `ValidationBehavior` runs `IValidate.Validate`
  plus every `IMessageValidator<TMessage>` in DI. FluentValidation plugs in
  as one such validator (`AddTrellisFluentValidation()`) rather than as its
  own pipeline slot.

### EF Core integration

- Composite value object convention: `[OwnedEntity]`-decorated composite
  VOs flow through `TrellisCompositeValueObjectConvention` and a generated
  EF model configuration. The supported shape is `{ get; private set; }`
  on every property — `TRLS022` enforces this.
- `TrellisScalarConverter` round-trips `ScalarValueObject<TSelf, T>` via
  `TryCreate`, surfacing creation failures as
  `TrellisPersistenceMappingException`.
- `CompositeValueObjectJsonConverter<T>` and the matching analyzer
  (`TRLS020`) ensure DTOs exposing `[OwnedEntity]` composites carry a
  `[JsonConverter]` so STJ deserialization goes through `TryCreate`.
- New analyzer `TRLS021` flags redundant manual EF configuration that the
  convention already handles.

### ASP wire contract

- `Error.UnprocessableContent` is the canonical validation failure code;
  domain validation, binder-level value-object validation, and MVC body
  validation all return 422 with a problem-details payload that expands
  composite value-object failures into per-leaf entries.
- `WWW-Authenticate` is emitted on mediator-produced 401 responses.
- `ProblemDetails.Instance` is populated from the request URL (#496).
- `HttpResponseOptionsBuilder<T>` and `HttpResponseOptionsBuilder<Page<T>>`
  support `WithCacheControl(...)` / `CacheControl` presets, `VaryForActor()`
  / `IProvideActorVaryHeaders`, `WithETag` / `WithLastModified` /
  `Vary` / `WithContentLanguage` / `WithContentLocation`, and
  `EvaluatePreconditions()` (`If-None-Match` / `If-Modified-Since` → 304;
  failing `If-Match` / `If-Unmodified-Since` → 412), evaluated once per
  request so non-deterministic selectors do not produce inconsistent
  headers.
- `Maybe<TPrimitive>` is supported directly on DTOs (via
  `MaybePrimitiveJsonConverterFactory`) and on route / query / header
  parameters (via `MaybePrimitiveModelBinder<T>`).

### Documentation

- New cookbook (`docs/docfx_project/api_reference/trellis-api-cookbook.md`)
  with 26+ task-oriented recipes and a per-package API reference under
  `docs/docfx_project/api_reference/`.
- Value-object taxonomy reference
  (`trellis-value-object-taxonomy.md`) covering scalar / composite /
  primitive variants.
- Analyzer surface documented per rule under
  `docs/docfx_project/articles/analyzers/`.
- Migration guide (`docs/docfx_project/articles/migration.md`) covers the
  step-by-step move from FunctionalDdd 2.1 to Trellis 3.0.
- `Trellis.Core` and `Trellis.ServiceDefaults` README files describe the
  composition root and pipeline order.

---

## Previous Releases

Releases prior to 3.0.0 shipped under the FunctionalDdd project name and
are tracked in that repository's history.

[Unreleased]: https://github.com/xavierjohn/Trellis/compare/v3.0.0...HEAD
[3.0.0]: https://github.com/xavierjohn/Trellis/releases/tag/v3.0.0
