# Trellis.Mediator

Result-aware pipeline behaviors for [martinothamar/Mediator](https://github.com/martinothamar/Mediator).

Provides validation, authorization, logging, tracing, and exception handling behaviors that understand Trellis `Result<T>` types and short-circuit correctly.

Authorization types (`Actor`, `IActorProvider`, `IAuthorize`, `IAuthorizeResource`) live in the separate `Trellis.Authorization` package — use them with or without CQRS.

## Behaviors

| Behavior | Purpose |
|----------|---------|
| `ExceptionBehavior` | Catches unhandled exceptions → `Error.Unexpected` |
| `TracingBehavior` | OpenTelemetry Activity with Result status |
| `LoggingBehavior` | Structured logging with duration |
| `AuthorizationBehavior` | Static permission checks (`IAuthorize`) |
| `ResourceAuthorizationBehavior` | Resource-based auth (`IAuthorizeResource`) |
| `ValidationBehavior` | Self-validation via `IValidate` |

## Usage

```csharp
services.AddMediator(options =>
{
    options.Assemblies = [typeof(MyCommand).Assembly];
    options.PipelineBehaviors = ServiceCollectionExtensions.PipelineBehaviors;
});
```

See the [full documentation](https://xavierjohn.github.io/Trellis/) for details.
