# Trellis.Mediator

Result-aware pipeline behaviors for [martinothamar/Mediator](https://github.com/martinothamar/Mediator). This package bridges the Trellis `Result<T>` type system with Mediator's pipeline model, providing reusable, cross-cutting behaviors that understand success/failure and short-circuit correctly.

## What It Is

A thin integration layer — **not** a mediator implementation. The actual mediator is martinothamar/Mediator (source-generated, `ValueTask`-based, AOT-friendly). This package provides:

- **ValidationBehavior** — short-circuits on validation failure via `IValidate`
- **AuthorizationBehavior** — checks static permissions via `IAuthorize`
- **ResourceAuthorizationBehavior** — checks resource-based auth via `IAuthorizeResource`
- **LoggingBehavior** — structured logging with duration and Result outcome
- **TracingBehavior** — OpenTelemetry activity with Result status
- **ExceptionBehavior** — catches unhandled exceptions as `Error.Unexpected` failures

## Installation

```
dotnet add package Trellis.Mediator
```

## Quick Start

### 1. Register behaviors

```csharp
using Trellis.Mediator;

services.AddMediator(options =>
{
    options.Assemblies = [typeof(MyCommand).Assembly];
    options.PipelineBehaviors = ServiceCollectionExtensions.PipelineBehaviors;
});
```

### 2. Define a command with validation

```csharp
using Mediator;
using Trellis;
using Trellis.Mediator;

public sealed record CreateOrderCommand(string CustomerId, int Quantity)
    : ICommand<Result<Order>>, IValidate
{
    public IResult Validate() =>
        string.IsNullOrWhiteSpace(CustomerId)
            ? Result.Failure<Order>(Error.Validation("Required", "CustomerId"))
            : Quantity <= 0
                ? Result.Failure<Order>(Error.Validation("Must be positive", "Quantity"))
                : Result.Success();
}
```

### 3. Add authorization

```csharp
// Static permission check
public sealed record DeleteOrderCommand(OrderId OrderId)
    : ICommand<Result<Unit>>, IAuthorize
{
    public IReadOnlyList<string> RequiredPermissions => ["Orders.Delete"];
}

// Resource-based check
public sealed record CancelOrderCommand(OrderId OrderId, string OrderOwnerId)
    : ICommand<Result<Order>>, IAuthorizeResource
{
    public IResult Authorize(Actor actor) =>
        actor.Id == OrderOwnerId || actor.HasPermission("Orders.CancelAny")
            ? Result.Success()
            : Result.Failure(Error.Forbidden("Only the order owner or admins can cancel"));
}
```

### 4. Implement IActorProvider in your API layer

```csharp
internal sealed class HttpActorProvider(IHttpContextAccessor accessor) : IActorProvider
{
    public Actor GetCurrentActor()
    {
        var user = accessor.HttpContext?.User
            ?? throw new InvalidOperationException("No authenticated user.");
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var permissions = user.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToHashSet();
        return new Actor(id, permissions);
    }
}
```

## Pipeline Execution Order

```
Request → ExceptionBehavior → TracingBehavior → LoggingBehavior
  → AuthorizationBehavior → ResourceAuthorizationBehavior
    → ValidationBehavior → Handler → Result<T>
```

## Package References

| Project | Package |
|---------|---------|
| Application layer | `Mediator.Abstractions` + `Trellis.Mediator` |
| API/Host (composition root) | `Mediator.SourceGenerator` |

`Mediator.SourceGenerator` is installed **only** in the outermost project.
