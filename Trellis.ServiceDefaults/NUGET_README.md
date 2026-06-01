# Trellis.ServiceDefaults

[![NuGet Package](https://img.shields.io/nuget/v/Trellis.ServiceDefaults.svg)](https://www.nuget.org/packages/Trellis.ServiceDefaults)

Opinionated composition defaults for Trellis web services.

## Installation
```bash
dotnet add package Trellis.ServiceDefaults
```

## Quick Example
```csharp
using Trellis.ServiceDefaults;

builder.Services.AddTrellis(options => options
    .UseAsp()
    .UseScalarValueValidation()
    .UseProblemDetails()
    .UseMediator()
    .UseFluentValidation(typeof(Program).Assembly)
    .UseClaimsActorProvider()
    .UseResourceAuthorization(typeof(Program).Assembly)
    .UseEntityFrameworkUnitOfWork<AppDbContext>());
```

`UseEntityFrameworkUnitOfWork<TContext>()` is always applied last so the transactional command behavior runs innermost. `AddDbContext<TContext>(...)` and `AddMediator(...)` remain application-owned registrations.

`UseFluentValidation()` and `UseResourceAuthorization()` both support no-assembly calls for explicit, no-scanning composition; pass assemblies only when you want Trellis to discover validators/resource loaders automatically.

`UseScalarValueValidation()` is independent of `UseAsp()` — it registers the scalar-value model binders, JSON converters, and `SuppressModelStateInvalidFilter` toggle that mutate global `MvcOptions` / `JsonOptions` for both MVC and Minimal API JSON pipelines. Hosts that only need error-to-status mapping (e.g. an MVC site that does not bind value-object DTOs) can call `UseAsp()` alone and skip the binder / converter wiring. Minimal API hosts must still call `app.UseScalarValueValidation()` middleware and chain `.WithScalarValueValidation()` per endpoint.

`UseProblemDetails()` is independent of `UseAsp()` — it registers Trellis ProblemDetails customization (`traceId` on every error, 405 `Allow` header projected as `extensions.allow`, 500 detail rewrite) without pulling in Trellis MVC/result-mapping infrastructure. Composing it with a direct `services.AddTrellisProblemDetails()` call is idempotent — exactly one Trellis post-configure layer ends up registered.

`UseIdempotency(opt => ...)` wires the opt-in IETF `Idempotency-Key` middleware (options + scope resolver + marker). Composition is explicit — the slot does not register a store, so callers add `services.AddInMemoryIdempotencyStore()` (dev / tests) or an EF-backed store (production) and mount the middleware with `app.UseTrellisIdempotency()`. Endpoints opt in with `[Idempotent]`.

`UseWorkerActor(systemActor)` composes the previously selected actor provider with a worker/system fallback for background scopes that have no `HttpContext`. It applies after the actor-provider selection and caching wrap, so HTTP requests still resolve through the inner provider (and its cache) and `BackgroundService` ticks short-circuit to the supplied system actor.

## AOT compatibility

`Trellis.ServiceDefaults` is **not** AOT- or trim-compatible. The fluent assembly-scanning methods (`UseFluentValidation(asm)`, `UseResourceAuthorization(asm)`, `UseDomainEvents(asm)`) wrap underlying `[RequiresUnreferencedCode]` + `[RequiresDynamicCode]` APIs without propagating the attributes. For AOT consumers, use the per-package direct APIs (`services.AddTrellisFluentValidation()` + explicit validator registrations, `services.AddResourceAuthorization<TMessage, TResource, TResponse>()`, `services.AddDomainEventDispatch()` + `services.AddDomainEventHandler<TEvent, THandler>()`).

The parameterless `o.UseFluentValidation()` / `o.UseResourceAuthorization()` / `o.UseDomainEvents()` overloads are AOT-compatible — they only register the adapter / pipeline behaviors and rely on the consumer's explicit per-type registrations.

## Part of Trellis
This package is part of the [Trellis](https://github.com/xavierjohn/Trellis) framework.
