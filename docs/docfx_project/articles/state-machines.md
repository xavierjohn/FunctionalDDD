# State Machines

**Level:** Intermediate | **Time:** 20-30 min | **Prerequisites:** [Basics](basics.md)

Integrate state machines with Railway-Oriented Programming using the **Trellis.Stateless** package. This package wraps the [Stateless](https://github.com/dotnet-state-machine/stateless) library's `Fire()` method to return `Result<TState>` instead of throwing on invalid transitions, making state machines composable with Trellis pipelines.

## Installation

```bash
dotnet add package Trellis.Stateless
```

## The Problem

The Stateless library throws `InvalidOperationException` when you attempt an invalid state transition. This breaks ROP pipelines and forces try/catch blocks:

```csharp
// ❌ Stateless throws on invalid transitions
try
{
    machine.Fire(OrderTrigger.Ship);  // InvalidOperationException if not allowed
}
catch (InvalidOperationException ex)
{
    return BadRequest(ex.Message);
}
```

## The Solution

Trellis.Stateless provides `FireResult()` extension methods that return `Result<TState>`:

```csharp
// ✅ Returns Result<TState> — composable with Trellis pipelines
Result<OrderState> result = machine.FireResult(OrderTrigger.Ship);
```

## Basic Usage

```csharp
using Stateless;
using Trellis;

// Define states and triggers
public enum OrderState { Draft, Submitted, Approved, Shipped, Cancelled }
public enum OrderTrigger { Submit, Approve, Ship, Cancel }

// Configure the state machine
var machine = new StateMachine<OrderState, OrderTrigger>(OrderState.Draft);

machine.Configure(OrderState.Draft)
    .Permit(OrderTrigger.Submit, OrderState.Submitted)
    .Permit(OrderTrigger.Cancel, OrderState.Cancelled);

machine.Configure(OrderState.Submitted)
    .Permit(OrderTrigger.Approve, OrderState.Approved)
    .Permit(OrderTrigger.Cancel, OrderState.Cancelled);

machine.Configure(OrderState.Approved)
    .Permit(OrderTrigger.Ship, OrderState.Shipped)
    .Permit(OrderTrigger.Cancel, OrderState.Cancelled);

machine.Configure(OrderState.Shipped)
    .Ignore(OrderTrigger.Cancel);  // Can't cancel a shipped order

// Use FireResult — returns Result<OrderState>
Result<OrderState> result = machine.FireResult(OrderTrigger.Submit);
// result.IsSuccess == true, result.Value == OrderState.Submitted

Result<OrderState> invalid = machine.FireResult(OrderTrigger.Ship);
// invalid.IsFailure == true, invalid.Error is DomainError
```

## Composing with ROP Pipelines

`FireResult()` integrates naturally with Trellis pipelines:

```csharp
public class Order : Aggregate<OrderId>
{
    private readonly StateMachine<OrderState, OrderTrigger> _machine;

    public Result<Order> Submit() =>
        _machine.FireResult(OrderTrigger.Submit)
            .Tap(_ => DomainEvents.Add(new OrderSubmittedEvent(Id)))
            .Map(_ => this);

    public Result<Order> Approve(UserId approvedBy) =>
        _machine.FireResult(OrderTrigger.Approve)
            .Tap(_ => ApprovedBy = approvedBy)
            .Tap(_ => DomainEvents.Add(new OrderApprovedEvent(Id, approvedBy)))
            .Map(_ => this);

    public Result<Order> Ship(TrackingNumber tracking) =>
        _machine.FireResult(OrderTrigger.Ship)
            .Tap(_ => TrackingNumber = tracking)
            .Tap(_ => ShippedAt = DateTime.UtcNow)
            .Tap(_ => DomainEvents.Add(new OrderShippedEvent(Id, tracking)))
            .Map(_ => this);
}
```

Use in a service:

```csharp
public async Task<Result<Order>> SubmitOrderAsync(OrderId orderId, CancellationToken ct)
    => await _repository.GetByIdAsync(orderId, ct)
        .ToResultAsync(Error.NotFound("Order not found"))
        .BindAsync(order => order.Submit())
        .TapAsync(order => _repository.SaveAsync(order, ct));
```

## How It Works

- Uses Stateless's `CanFire()` to check before firing — **no try/catch internally**
- Returns `Result<TState>` with the new state on success
- Returns a `DomainError` with details about the invalid transition on failure
- Preserves Stateless's guard clause support

## Spec Mapping

When a specification says:

> "The status transitions from Draft → Submitted → Approved → Shipped. An order can be cancelled from any state except Shipped."

This maps directly to:

```csharp
machine.Configure(OrderState.Draft)
    .Permit(OrderTrigger.Submit, OrderState.Submitted)
    .Permit(OrderTrigger.Cancel, OrderState.Cancelled);

machine.Configure(OrderState.Submitted)
    .Permit(OrderTrigger.Approve, OrderState.Approved)
    .Permit(OrderTrigger.Cancel, OrderState.Cancelled);

machine.Configure(OrderState.Approved)
    .Permit(OrderTrigger.Ship, OrderState.Shipped)
    .Permit(OrderTrigger.Cancel, OrderState.Cancelled);

machine.Configure(OrderState.Shipped)
    .Ignore(OrderTrigger.Cancel);  // "except from Shipped"
```

Every transition returns `Result<OrderState>`, making invalid transitions a regular error flow rather than an exception.

## Next Steps

- [Basics](basics.md) — Learn core ROP operations
- [Clean Architecture](clean-architecture.md) — Full architecture patterns with state machines
- [Trellis for AI Code Generation](ai-code-generation.md) — How specs map to Trellis constructs
