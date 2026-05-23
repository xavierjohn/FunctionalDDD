---
title: ASP.NET Core Integration
package: Trellis.Asp
topics: [asp, minimal-api, controllers, http-result, problem-details, etag, prefer, pagination]
related_api_reference: [trellis-api-asp.md, trellis-api-core.md]
last_verified: 2026-05-01
audience: [developer]
---
# ASP.NET Core Integration

`Trellis.Asp` maps `Result`, `Result<T>`, `Result<WriteOutcome<T>>`, and `Result<Page<T>>` to ASP.NET Core HTTP responses (status codes, Problem Details, ETags, `Prefer`, paginated envelopes) using the single verb `ToHttpResponse(...)`.

## Patterns Index

| Goal | Use | See |
|---|---|---|
| Map `Result<T>` to a Minimal API response | `result.ToHttpResponse(...)` / `ToHttpResponseAsync(...)` | [Quick start](#quick-start) |
| Project the response body separately from the domain value | `result.ToHttpResponse(body: domain => dto, configure: opts => ...)` | [Body projection](#body-projection) |
| Return `ActionResult<T>` from an MVC controller | `.AsActionResult<T>()` / `.AsActionResultAsync<T>()` | [MVC controllers](#mvc-controllers) |
| Map `Result<Unit>` (no payload) | Return `Result.Ok()` — `ToHttpResponse` emits `204 No Content` | [Result&lt;Unit&gt; → 204](#resultunit--204-no-content) |
| Override an error → status mapping for one endpoint | `opts.WithErrorMapping<TError>(status)` / `opts.WithErrorMapping(err => ...)` | [Error mapping](#error-mapping) |
| Override mappings globally | `AddTrellisAsp(opts => opts.MapError<TError>(status))` | [Error mapping](#error-mapping) |
| Conditional `GET`/`HEAD` (`If-None-Match`, `If-Modified-Since`) | `opts.WithETag(...).EvaluatePreconditions()` | [Conditional requests](#conditional-requests) |
| Honor `Prefer: return=minimal` / `return=representation` | `opts.HonorPrefer()` on a `WriteOutcome` response | [Prefer header](#prefer-header) |
| Emit `201 Created` with a `Location` header | `opts.CreatedAtRoute(name, values)` (AOT-safe) / `Created(...)` / `CreatedAtAction(...)` | [Created responses](#created-responses) |
| Return paginated JSON + RFC 8288 `Link` header | `Result<Page<T>>.ToHttpResponse(nextUrlBuilder, body)` | [Pagination](#pagination) |
| Validate scalar value objects (route, query, JSON body) | `AddScalarValueValidation` + `UseScalarValueValidation` + `WithScalarValueValidation` | [Scalar value validation](#scalar-value-validation) |
| Bind value objects in route segments | `AddTrellisRouteConstraint<T>("Name")` then `"/x/{id:Name}"` | [Route constraints](#route-constraints) |
| Hydrate the current `Actor` from JWT claims | `AddClaimsActorProvider` / `AddEntraActorProvider` / `AddDevelopmentActorProvider` | [Actor providers](#actor-providers) |

## Use this guide when

- Your application returns `Result<T>` and you need predictable HTTP status, Problem Details, and conditional-request behavior at the boundary.
- You are wiring Minimal API endpoints or MVC controllers and want one verb (`ToHttpResponse`) instead of a `switch`-per-endpoint.
- You need ETag, `If-Match` / `If-None-Match`, `Prefer`, or paginated-list semantics that match RFC 9110 / 7240 / 8288 without hand-rolling header parsing.
- You bind scalar value objects (`IScalarValue<TSelf, TPrimitive>`) from routes, queries, or JSON bodies and want validation collected as `Error.InvalidInput`.
- You hydrate the current `Actor` from JWT/OIDC claims for downstream authorization checks.

## Surface at a glance

| Type | Purpose |
|---|---|
| `HttpResponseExtensions` | `ToHttpResponse` / `ToHttpResponseAsync` for `Error`, `Result<T>`, `Result<WriteOutcome<T>>`, `Result<Page<T>>` (with optional body projector). |
| `HttpResponseOptionsBuilder<TDomain>` | Fluent options for the generic overloads (`WithETag`, `WithLastModified`, `Vary`, `Created`/`CreatedAtRoute`/`CreatedAtAction`, `EvaluatePreconditions`, `HonorPrefer`, `WithErrorMapping`, …). |
| `HttpResponseOptionsBuilder` | Non-generic builder for the `Error` overload (`Vary`, `HonorPrefer`, `WithErrorMapping`). |
| `ActionResultAdapterExtensions` | `AsActionResult<T>` / `AsActionResultAsync<T>` to wrap an `IResult` for MVC. |
| `TrellisAspOptions` | DI-registered error-type → status-code map; configure via `AddTrellisAsp(opts => opts.MapError<TError>(status))`. |
| `ETagHelper` | `ParseIfMatch` / `ParseIfNoneMatch` returning `EntityTagValue[]?`; `IfMatchSatisfied` / `IfNoneMatchMatches` comparison helpers. |
| `IfNoneMatchExtensions` | `EnforceIfNoneMatchPrecondition(EntityTagValue[]?)` — converts a successful result into `Error.TransportFault(new HttpError.PreconditionFailed(...))` when `If-None-Match: *` is sent and the resource exists. |
| `PreferHeader` | `Parse(HttpRequest)` → `ReturnRepresentation`, `ReturnMinimal`, `RespondAsync`, `Wait`, `HandlingStrict`, `HandlingLenient`, `HasPreferences`. |
| `PagedResponse<TResponse>` / `PageLink` | JSON envelope and `Link` header entries returned by the `Result<Page<T>>` overload. |
| `Trellis.Asp.Authorization.*` | `AddClaimsActorProvider` / `AddEntraActorProvider` / `AddDevelopmentActorProvider` / `AddCachingActorProvider<T>`. |
| `Trellis.Asp.ModelBinding.*` | `ScalarValueModelBinder<,>` / `MaybeModelBinder<,>` / `ScalarValueModelBinderProvider`. |
| `Trellis.Asp.Routing.*` | `TrellisValueObjectRouteConstraint<T>` + `AddTrellisRouteConstraint<T>` / `AddTrellisRouteConstraints`. |
| `Trellis.Asp.Validation.*` | `ValidatingJsonConverter<,>`, `MaybeScalarValueJsonConverter<,>`, `ScalarValueValidationFilter` (MVC), `ScalarValueValidationEndpointFilter` (Minimal API), `ScalarValueValidationMiddleware`, `ValidationErrorsContext`. |

Full signatures: [trellis-api-asp.md](../api_reference/trellis-api-asp.md).

## Installation

```bash
dotnet add package Trellis.Asp
```

`Trellis.Asp` bundles the AOT-friendly `Trellis.AspSourceGenerator.dll` (attached automatically) and contains the actor providers formerly published as `Trellis.Asp.Authorization`.

## Quick start

A composition root that wires Trellis.Asp once, then a Minimal API endpoint that returns `Result<T>`:

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Trellis;
using Trellis.Asp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTrellisAsp();

var app = builder.Build();
app.UseScalarValueValidation();

app.MapGet("/users/{id}", async (string id, IUserService users, CancellationToken ct) =>
        (await users.GetByIdAsync(id, ct))
            .ToHttpResponse(user => new UserResponse(user.Id, user.Email)))
    .WithName("Users_GetById")
    .WithScalarValueValidation();

app.Run();

public sealed record User(string Id, string Email);
public sealed record UserResponse(string Id, string Email);

public interface IUserService
{
    Task<Result<User>> GetByIdAsync(string id, CancellationToken ct);
}
```

Behavior:

- Success → `200 OK` with the projected `UserResponse` body.
- `Error.NotFound` → `404 Not Found` Problem Details.
- `Error.InvalidInput` → `422 Unprocessable Content` validation Problem Details.
- Any other failure → status from `TrellisAspOptions` (default `500`).

## `Result<Unit>` → 204 No Content

Side-effecting commands return `Result<Unit>` from `Result.Ok()` / `Result.Fail(error)`. `Trellis.Asp` maps a successful `Result<Unit>` to `204 No Content` automatically — no body, no projector required.

```csharp
app.MapDelete("/users/{id}", async (string id, IUserService users, CancellationToken ct) =>
    (await users.DeleteAsync(id, ct)).ToHttpResponse());

public interface IUserService
{
    Task<Result<Unit>> DeleteAsync(string id, CancellationToken ct);
}
```

## MVC controllers

For MVC, convert with `ToHttpResponse` / `ToHttpResponseAsync`, then adapt with `AsActionResult<T>` / `AsActionResultAsync<T>` so the action signature stays `Task<ActionResult<T>>` for OpenAPI / `[ProducesResponseType<T>]`.

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Trellis;
using Trellis.Asp;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddControllers()
    .AddScalarValueValidation();
builder.Services.AddTrellisAsp();

var app = builder.Build();
app.UseScalarValueValidation();
app.MapControllers();
app.Run();

[ApiController]
[Route("users")]
public sealed class UsersController(IUserService users) : ControllerBase
{
    [HttpGet("{id}", Name = nameof(GetById))]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<ActionResult<UserResponse>> GetById(string id, CancellationToken ct) =>
        users.GetByIdAsync(id, ct)
            .ToHttpResponseAsync(user => new UserResponse(user.Id, user.Email))
            .AsActionResultAsync<UserResponse>();

    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken ct) =>
        users.CreateAsync(request, ct)
            .ToHttpResponseAsync(
                body: user => new UserResponse(user.Id, user.Email),
                configure: opts => opts.CreatedAtRoute(
                    nameof(GetById),
                    user => new RouteValueDictionary { ["id"] = user.Id }))
            .AsActionResultAsync<UserResponse>();
}

public sealed record User(string Id, string Email);
public sealed record CreateUserRequest(string Email);
public sealed record UserResponse(string Id, string Email);

public interface IUserService
{
    Task<Result<User>> GetByIdAsync(string id, CancellationToken ct);
    Task<Result<User>> CreateAsync(CreateUserRequest request, CancellationToken ct);
}
```

> [!NOTE]
> `AsActionResult<T>` only exists for the generic `ActionResult<T>` shape. For value-less responses, return `IResult` directly from a Minimal API endpoint or use `IActionResult` in MVC.

## Body projection

Every generic `ToHttpResponse` overload accepts an optional `body: Func<TDomain, TBody>` projector. The selectors in the options builder (`WithETag`, `WithLastModified`, `Created(selector)`, `CreatedAtRoute(values)`, `WithContentLocation`) **always run against the domain value**, not the projected body. This keeps response DTOs free of representation concerns:

```csharp
return result.ToHttpResponse(
    body: product => new ProductResponse(product.Id.Value, product.Name.Value, product.Price.Value),
    configure: opts => opts
        .WithETag(product => product.ETag)
        .CreatedAtRoute("Products_GetById", product => new RouteValueDictionary { ["id"] = product.Id.Value }));
```

There is **no** `WithBody(...)` builder method. Pass the projector as the second positional argument to `ToHttpResponse` instead.

## Error mapping

Trellis.Core.Error is transport-neutral. The ASP boundary translates domain failures to HTTP per the table below.

| Domain case | HTTP status | wire `kind` | Headers |
|---|---|---|---|
| `Error.InvalidInput` | `422` | `unprocessable-content` | — |
| `Error.InvariantViolation` | `422` | `unprocessable-content` | — |
| `Error.NotFound` | `404` | `not-found` | — |
| `Error.Forbidden` | `403` | `forbidden` | — |
| `Error.Conflict` + `ReasonCode == "concurrent_modification"` and request had `If-Match` | `412` | `precondition-failed` | — |
| `Error.Conflict` | `409` | `conflict` | — |
| `Error.Gone` | `410` | `gone` | — |
| `Error.AuthenticationRequired` | `401` | `unauthorized` | `WWW-Authenticate` |
| `Error.Unavailable` | `503` | `service-unavailable` | `Retry-After` |
| `Error.RateLimited` | `429` | `too-many-requests` | `Retry-After` |
| `Error.Unexpected` (`ReasonCode == "not_implemented"`) | `501` | `not-implemented` | — |
| `Error.Unexpected` | `500` | `internal-server-error` | `faultId` when set |
| `Error.Aggregate` | worst child | `multi` | per-child |
| `Error.TransportFault` wrapping `HttpError.*` | `405/406/412/413/415/416/428` | inner wire kind | `Allow` / `Content-Range` / inner-specific |

Domain `Kind` and wire `kind` are intentionally distinct for `Error.InvalidInput` and `Error.InvariantViolation`: the domain slugs remain `invalid-input` / `invariant-violation`, while the on-wire `kind` stays `unprocessable-content` for backward compatibility.

Override globally:

```csharp
builder.Services.AddTrellisAsp(opts =>
{
    opts.MapError<Error.Conflict>(StatusCodes.Status400BadRequest);
});
```

Override per call (highest precedence first):

```csharp
return result.ToHttpResponse(
    body: order => new OrderResponse(order.Id),
    configure: opts => opts
        .WithErrorMapping<Error.Conflict>(StatusCodes.Status409Conflict)
        .WithErrorMapping(err => err is Error.NotFound ? StatusCodes.Status410Gone : default));
```

Resolution order: `WithErrorMapping(Func<Error,int>)` → `WithErrorMapping<TError>(int)` → `TrellisAspOptions.MapError<TError>(int)` → `500`.

## Problem Details output

Failures are emitted as `application/problem+json`. `Error.InvalidInput` with field violations uses `Results.ValidationProblem(...)`; everything else uses `Results.Problem(...)`. Companion headers are synthesized automatically from the error payload: `Allow` for `HttpError.MethodNotAllowed`, `Content-Range` for `HttpError.RangeNotSatisfiable`, `Retry-After` from `RetryAdvice` on `Error.RateLimited` / `Error.Unavailable`, and `WWW-Authenticate` from `Error.AuthenticationRequired.Scheme` or the registered `IAuthenticationSchemeProvider` fallback.

`Error.Aggregate` renders as one outer Problem Details object with the worst child status and an RFC 9457 `errors[]` extension containing one child object per inner error (`type`, `status`, `code`, `kind`, `detail`). `Error.TransportFault` projects the wrapped `HttpError` payload into `extensions` and uses the inner `code` / `kind`, not the outer `transport-fault` envelope. Every response also carries `instance`; `5xx` responses replace the public detail with `"An internal error occurred."`.

`Error.InvalidInput` is routed to `Results.ValidationProblem(...)`:

```http
HTTP/1.1 422 Unprocessable Content
Content-Type: application/problem+json

{
  "title": "One or more validation errors occurred.",
  "status": 422,
  "instance": "/api/customers?api-version=2026-11-12",
  "code": "invalid-input",
  "kind": "unprocessable-content",
  "errors": {
    "email": ["Email is required"]
  }
}
```

The `errors` dictionary keys are each `FieldViolation.Field.Path` translated from RFC 6901 JSON Pointer (e.g. `/items/0/name`) to the dot+bracket convention used by ASP.NET Core's default `ValidationProblemDetails` (e.g. `items[0].name`). RFC 6901 escapes (`~1` → `/`, `~0` → `~`) are decoded so segments containing literal `/` or `~` appear correctly. Values are `Detail ?? ReasonCode`. The wire shape matches what `[ApiController]` emits in its automatic 400 responses, so OpenAPI codegen consumers (axios + react-query, NSwag, etc.) and React form libraries (`react-hook-form`, Formik) can lookup `setError(name, ...)` directly without a slash → dot translation shim. The original JSON Pointer is preserved verbatim inside `extensions["rules"][n].fields[]` for rule-level violations.

## Created responses

Three options for `201 Created`. Pick by your AOT requirement and the link source.

| Builder | Location source | AOT-safe |
|---|---|---|
| `Created(string locationLiteral)` | Caller-supplied literal | Yes |
| `Created(Func<TDomain, string> selector)` | Selector over the domain value | Yes |
| `CreatedAtRoute(string routeName, Func<TDomain, RouteValueDictionary> routeValues)` | `LinkGenerator.GetUriByName` (resolved at execute time) | Yes |
| `CreatedAtAction(string actionName, Func<TDomain, RouteValueDictionary> routeValues, string? controllerName = null)` | `LinkGenerator.GetUriByAction` | **No** — `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]` |

```csharp
app.MapPost("/products", async (CreateProduct cmd, IProductWriter writer, CancellationToken ct) =>
        (await writer.CreateAsync(cmd, ct)).ToHttpResponse(
            body: product => new ProductResponse(product.Id.Value, product.Name.Value),
            configure: opts => opts.CreatedAtRoute(
                "Products_GetById",
                product => new RouteValueDictionary { ["id"] = product.Id.Value })))
    .WithName("Products_Create");
```

> [!WARNING]
> Under query-string or header API versioning, the route values dictionary **must** include `["api-version"] = ApiVersion`. Without it the emitted `Location` header omits the version and 404s on dereference (the response itself looks correct, so tests pass). The recommended path is to chain `.WithVersionedRoute()` from [`Trellis.Asp.ApiVersioning`](#api-version-aware-location-headers), which injects the version automatically. The [`TRLS023`](analyzers/TRLS023.md) analyzer warns on bare `CreatedAtRoute` / `CreatedAtAction` / `WithLocation` calls inside `[ApiVersion]`-decorated controllers and offers a code fix that appends `.WithVersionedRoute()`.

### API-version-aware `Location` headers

The `Trellis.Asp.ApiVersioning` package adds a `WithVersionedRoute()` extension on `HttpResponseOptionsBuilder<T>` that injects the requested `api-version` into the generated `Location` URL automatically — eliminating the recurring "201 looks correct, GET 404s" bug under query/header versioning. Chain it after any builder method that emits a builder-generated `Location` header — `CreatedAtRoute(...)` / `CreatedAtAction(...)` for 201 Created, `WithLocation(...)` for 2xx state-transition responses on existing resources.

```csharp
using Trellis.Asp.ApiVersioning;

[ApiController]
[ApiVersion("2026-12-01")]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    [HttpGet("{id:int}", Name = "Orders_GetById")]
    public IActionResult Get(int id) => Ok(...);

    [HttpPost]
    public ActionResult<Order> Create([FromBody] CreateOrderRequest req) =>
        _mediator.Send(new CreateOrderCommand(req))
            .ToHttpResponse(opts => opts
                .CreatedAtRoute(
                    "Orders_GetById",
                    o => new RouteValueDictionary { ["id"] = o.Id })
                .WithVersionedRoute())
            .AsActionResult<Order>();
}
```

The resolver runs per request inside the `LinkGenerator` callback. Resolution order:

1. **`HttpContext.RequestedApiVersion`** — primary signal; reflects whatever the configured `IApiVersionReader` parsed (query, header, media-type, composite).
2. **Endpoint metadata** — when (1) is null and exactly one declared version exists, fall back to it.
3. **`ApiVersioningOptions.DefaultApiVersion`** — final fallback. If a multi-version action has no client-supplied version and no default, the resolver throws rather than silently picking one.

The resolver short-circuits to a no-op (no `api-version` injected) for `[ApiVersionNeutral]` endpoints and URL-segment versioning (route template contains `:apiVersion`). Two overloads:

| Overload | Use case |
|---|---|
| `WithVersionedRoute()` | Per-request resolution; the default. |
| `WithVersionedRoute(ApiVersion explicitVersion)` | Pin the `Location` to a specific `ApiVersion` regardless of client request — used for cross-version redirects on deprecated endpoints. |

The underlying `WithRouteValueResolver(string key, Func<HttpContext, string?> resolver)` hook on `HttpResponseOptionsBuilder<T>` is also exposed publicly for any other cross-cutting per-request route-value injection (tenant id, request culture, etc.).

> [!NOTE]
> See [`trellis-api-asp-apiversioning.md`](../api_reference/trellis-api-asp-apiversioning.md) for the full LLM-targeted reference and [`TRLS023`](analyzers/TRLS023.md) for the analyzer that catches missed migrations.

### `WriteOutcome<T>`

When commands return `Result<WriteOutcome<T>>`, the response is RFC 9110-shaped from the variant:

| `WriteOutcome<T>` | Status |
|---|---|
| `Created(value)` | `201 Created` (plus `Location` from `Created`/`CreatedAtRoute`/`CreatedAtAction`) |
| `Updated(value)` | `200 OK` (or `204 No Content` with `Prefer: return=minimal` and `HonorPrefer()`) |
| `UpdatedNoContent` | `204 No Content` |
| `Accepted(value)` | `202 Accepted` (with `Retry-After` when configured) |
| `AcceptedNoContent` | `202 Accepted` |

```csharp
app.MapPut("/orders/{id:guid}", async (
        Guid id,
        UpdateOrderRequest request,
        IOrderService orders,
        CancellationToken ct) =>
    (await orders.UpdateAsync(id, request, ct)).ToHttpResponse(
        body: order => new OrderResponse(order.Id, order.Total),
        configure: opts => opts
            .WithETag(order => order.ETag)
            .HonorPrefer()));
```

## Conditional requests

`EvaluatePreconditions()` runs only on `GET` / `HEAD` and only when at least one selector (`WithETag` / `WithLastModified`) is configured. Evaluation order (RFC 9110): `If-Match` → `If-Unmodified-Since` → `If-None-Match` → `If-Modified-Since`. Failed `If-Match` / `If-Unmodified-Since` → `412`; failed `If-None-Match` / `If-Modified-Since` on `GET`/`HEAD` → `304`.

```csharp
app.MapGet("/products/{id:guid}", async (Guid id, IProductReader reader, CancellationToken ct) =>
    (await reader.GetAsync(id, ct)).ToHttpResponse(
        body: product => new ProductResponse(product.Id, product.Name, product.Price, product.ETag),
        configure: opts => opts
            .WithETag(product => product.ETag)
            .WithLastModified(product => product.UpdatedAt)
            .EvaluatePreconditions()));
```

For unsafe methods (`PUT`, `POST`), evaluate preconditions **before** the mutation. Use the typed parsers from `ETagHelper`:

```csharp
using Trellis.Asp;

EntityTagValue[]? ifMatch = ETagHelper.ParseIfMatch(httpContext.Request);
EntityTagValue[]? ifNoneMatch = ETagHelper.ParseIfNoneMatch(httpContext.Request);
```

`ParseIfMatch` returns `null` (header absent), `[]` (present but empty / weak-only — strong-only enforcement), the wildcard, or the parsed strong tags. `ParseIfNoneMatch` returns `null`, `[]`, the wildcard, or the parsed strong/weak tags.

The aggregate-side concurrency helpers `OptionalETag(...)` / `RequireETag(...)` consume `EntityTagValue[]?`. They live in `Trellis.Core` — see [`trellis-api-core.md`](../api_reference/trellis-api-core.md).

For "create only if absent" flows (`PUT` / `POST` with `If-None-Match: *`), use `EnforceIfNoneMatchPrecondition`:

```csharp
using Trellis.Asp;

var guarded = result.EnforceIfNoneMatchPrecondition(
    ETagHelper.ParseIfNoneMatch(httpContext.Request));
```

When the header contains `*` and the result is currently a success, it is replaced with `Error.TransportFault(new HttpError.PreconditionFailed(...))` using `PreconditionKind.IfNoneMatch`. No-op otherwise.

## Prefer header

`HonorPrefer()` honors RFC 7240 `Prefer: return=minimal` / `return=representation` on a `WriteOutcome` response. It always emits `Vary: Prefer`; `Preference-Applied` is emitted only when Trellis honored a preference.

| Sent header | Effect on `WriteOutcome.Updated` |
|---|---|
| `Prefer: return=minimal` | `204 No Content` + `Preference-Applied: return=minimal` |
| `Prefer: return=representation` | `200 OK` with body + `Preference-Applied: return=representation` |
| (none) | `200 OK` with body (no `Preference-Applied`) |

For raw access to the parsed header:

```csharp
using Trellis.Asp;

var prefer = PreferHeader.Parse(httpContext.Request);
if (prefer.ReturnMinimal) { /* … */ }
```

> [!NOTE]
> `PreferHeader.HasPreferences` is `true` only when at least one **recognized** standard preference (`return`, `respond-async`, `wait`, `handling`) was parsed. Unknown tokens do not set it.

## Pagination

The `Result<Page<T>>` overload always emits a `PagedResponse<TBody>` JSON envelope. The RFC 8288 `Link` header is added only when `Page.Next` and/or `Page.Previous` cursors are present.

```csharp
app.MapGet("/products", async (
        string? cursor,
        int? limit,
        IProductReader reader,
        HttpContext ctx,
        CancellationToken ct) =>
    (await reader.ListAsync(cursor, limit ?? 50, ct)).ToHttpResponse(
        nextUrlBuilder: (next, applied) =>
            $"{ctx.Request.Scheme}://{ctx.Request.Host}/products?cursor={next.Token}&limit={applied}",
        body: product => new ProductResponse(product.Id, product.Name)));

public sealed record ProductResponse(string Id, string Name);

public interface IProductReader
{
    Task<Result<Page<Product>>> ListAsync(string? cursor, int limit, CancellationToken ct);
}
```

Failure on the page result short-circuits through the standard error pipeline (Problem Details, default mapping).

## Scalar value validation

`Trellis.Asp` validates value objects implementing `IScalarValue<TSelf, TPrimitive>` at every binding site (route, query, JSON body) and surfaces the result as `Error.InvalidInput`.

| Host | Required wiring |
|---|---|
| MVC controllers | `services.AddControllers().AddScalarValueValidation();` + `app.UseScalarValueValidation();` |
| Minimal API | `services.AddScalarValueValidationForMinimalApi();` + `app.UseScalarValueValidation();` + `.WithScalarValueValidation()` per endpoint |
| Either | `services.AddTrellisAsp();` (registers `TrellisAspOptions` and chains `AddScalarValueValidation()` for **both** MVC and Minimal API JSON pipelines) |

> [!IMPORTANT]
> The `IServiceCollection`-receiver `AddScalarValueValidation()` only configures shared JSON support. For MVC apps you still need `AddControllers().AddScalarValueValidation()` so the `ScalarValueValidationFilter` and `ScalarValueModelBinderProvider` are registered.

`Maybe<T>` rules:

- Omitted or JSON `null` → `Maybe<T>.None`.
- Valid value → `Maybe.From(value)`.
- Invalid value (fails `TValue.TryCreate`) → validation error collected in `ValidationErrorsContext`; the request short-circuits with a validation Problem Details before the handler runs.

```csharp
using Trellis;
using Trellis.Primitives;

public sealed record UpdateCustomerRequest(
    FirstName Name,
    Maybe<PhoneNumber> Phone,
    Maybe<Url> Website);
```

> [!WARNING]
> `AddTrellisAsp` / `AddScalarValueValidation` only wire **scalar** VO converters. **Composite** value objects (multi-field `[OwnedEntity]` types like `ShippingAddress`, `Money`) need an explicit `[JsonConverter(typeof(CompositeValueObjectJsonConverter<MyVo>))]` on the type; otherwise model binding silently bypasses `TryCreate` and an invalid payload reaches the domain layer.

## Route constraints

Bind value objects (any `IParsable<T>`) directly from a route segment.

```csharp
using Trellis.Asp;

// AOT-safe — explicit registration per type
services.AddTrellisRouteConstraint<ProductId>("ProductId");

// Reflection-based — scans the calling assembly + Trellis.Core for IScalarValue<,> + IParsable<>
services.AddTrellisRouteConstraints();

app.MapGet("/products/{id:ProductId}", (ProductId id) => Results.Ok(id));
```

`AddTrellisRouteConstraints` is reflection-based and **not Native AOT compatible**; `AddTrellisRouteConstraint<T>` is AOT-safe.

## Actor providers

`Trellis.Asp.Authorization` hydrates the current `Actor` from JWT/OIDC claims. The `Actor` and `IActorProvider` types themselves live in `Trellis.Authorization`.

Each `AddXxxActorProvider` helper **replaces** the `IActorProvider` slot — chaining multiple helpers does not stack them. Pick one provider per environment, then optionally wrap with caching:

```csharp
using Trellis.Asp.Authorization;

if (env.IsDevelopment())
{
    services.AddDevelopmentActorProvider(opts =>
    {
        opts.DefaultActorId = "development";
        opts.DefaultPermissions = new HashSet<string> { "orders:read", "orders:create" };
    });
}
else
{
    services.AddEntraActorProvider(opts =>
    {
        opts.MapPermissions = claims => claims
            .Where(c => string.Equals(c.Type, "roles", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value)
            .ToHashSet();
    });

    // Wraps EntraActorProvider with per-request caching. The inner provider type is
    // registered idempotently via TryAddScoped<T>(); the outer IActorProvider slot
    // is replaced with the caching decorator.
    services.AddCachingActorProvider<EntraActorProvider>();
}
```

`DevelopmentActorProvider` throws `InvalidOperationException` outside the Development environment regardless of header presence; in Development it reads the `X-Test-Actor` header (JSON: `{ "Id", "Permissions", "ForbiddenPermissions", "Attributes" }`, case-insensitive). See [`trellis-api-testing-aspnetcore.md`](../api_reference/trellis-api-testing-aspnetcore.md) for `WebApplicationFactory.CreateClientWithActor`.

## Composition

Once the application returns `Result<T>` / `Result<WriteOutcome<T>>`, every cross-cutting concern composes through the same `ToHttpResponse` call — there is no per-endpoint `switch` to keep in sync with the error catalog.

```csharp
app.MapPut("/products/{id:guid}", async (
        Guid id,
        UpdateProductRequest request,
        IProductService products,
        HttpContext httpContext,
        CancellationToken ct) =>
{
    var ifMatch = ETagHelper.ParseIfMatch(httpContext.Request);
    var ifNoneMatch = ETagHelper.ParseIfNoneMatch(httpContext.Request);

    return await products.UpdateAsync(id, request, ifMatch, ct)
        .EnforceIfNoneMatchPreconditionAsync(ifNoneMatch)
        .ToHttpResponseAsync(
            body: product => new ProductResponse(product.Id, product.Name, product.Price, product.ETag),
            configure: opts => opts
                .WithETag(product => product.ETag)
                .HonorPrefer()
                .WithErrorMapping<Error.Conflict>(StatusCodes.Status409Conflict));
});
```

When you genuinely need a custom payload shape (non-Problem-Details body, endpoint-specific JSON, extra cookies), reach for `MatchAsync` from `Trellis.Core` instead of `ToHttpResponse`. Treat that as the exception, not the rule.

## Practical guidance

- **Convert at the API boundary only.** Keep `Result<T>` flowing through your application layer; convert to `IResult` / `ActionResult<T>` exactly once, at the endpoint.
- **`AddTrellisAsp()` is the one-call setup.** It registers `TrellisAspOptions` and configures both the MVC and Minimal API JSON pipelines for scalar-value / `Maybe<T>` deserialization. You still need `UseScalarValueValidation()` middleware and (for Minimal APIs) `WithScalarValueValidation()` per endpoint.
- **Document failure status codes.** Add `[ProducesResponseType<ProblemDetails>(...)]` for every spec-listed failure status (`422`, `409`, `403`, `404`, …). The `IEndpointMetadataProvider` on Trellis result types already declares the union of statuses the writer can emit (`200`, `201`, `304`, `400`, `404`, `412`, `500`); layer your spec-specific metadata on top.
- **`Result<Unit>` for side-effect commands**. A successful `Result<Unit>` produces `204 No Content` with no body.
- **Use typed ETag parsers.** `ETagHelper.ParseIfMatch` / `ParseIfNoneMatch` return `EntityTagValue[]?`, which feeds `OptionalETag` / `RequireETag` (Core) and `EnforceIfNoneMatchPrecondition` (Asp) directly.
- **Versioned `Location` headers.** Under query-string or header API versioning, every `CreatedAtRoute` / `CreatedAtAction` / `WithLocation` call must include `["api-version"] = ApiVersion` in the route values, otherwise the `Location` 404s on dereference and tests still pass. Prefer chaining `.WithVersionedRoute()` from [`Trellis.Asp.ApiVersioning`](#api-version-aware-location-headers) — it injects the version per request automatically. The [`TRLS023`](analyzers/TRLS023.md) analyzer flags bare `CreatedAtRoute` / `CreatedAtAction` / `WithLocation` inside `[ApiVersion]` controllers and the code fix appends `.WithVersionedRoute()`.
- **Avoid controller-level `[Consumes("application/json")]`.** Trigger-style POSTs without bodies (e.g., `POST /orders/{id}/submission`) return `415` for any request without a `Content-Type`. Apply `[Consumes]` per body-bearing action.
- **Prefer `CreatedAtRoute` over `CreatedAtAction`** for trim/AOT scenarios; `CreatedAtAction` is annotated `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]`.
- **Prove `422` mapping in integration tests.** Exception middleware does not map Trellis `Result` failures — assert at least one business-validation failure surfaces as `422` Problem Details end-to-end.

## Cross-references

- API surface: [`trellis-api-asp.md`](../api_reference/trellis-api-asp.md)
- `Result`, `Result<T>`, `Error`, `WriteOutcome<T>`, `Page<T>`, `EntityTagValue`, `OptionalETag` / `RequireETag`: [`trellis-api-core.md`](../api_reference/trellis-api-core.md)
- `Actor`, `IActorProvider`, `IAuthorize`: [`trellis-api-authorization.md`](../api_reference/trellis-api-authorization.md)
- `IScalarValue<TSelf, TPrimitive>`, `Maybe<T>`, ready-to-use value objects: [`trellis-api-primitives.md`](../api_reference/trellis-api-primitives.md)
- Integration-test helpers (`CreateClientWithActor` for `X-Test-Actor`): [`trellis-api-testing-aspnetcore.md`](../api_reference/trellis-api-testing-aspnetcore.md)
- Composite value object end-to-end pattern (Recipe 13): [`trellis-api-cookbook.md`](../api_reference/trellis-api-cookbook.md#recipe-13--composite-value-object-end-to-end-domain--api-json-binding--ef-core-ownership)
- HTTP client side (consuming results): [`integration-http.md`](integration-http.md)
- FluentValidation pipeline: [`integration-fluentvalidation.md`](integration-fluentvalidation.md)
- EF Core integration (`FirstOrDefaultResultAsync`, `SaveChangesResultUnitAsync`): [`integration-ef.md`](integration-ef.md)
