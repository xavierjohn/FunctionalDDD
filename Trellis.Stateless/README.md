# Trellis.Stateless

Wraps the [Stateless](https://github.com/dotnet-state-machine/stateless) library's `Fire()` method to return `Result<TState>` instead of throwing on invalid transitions.

## Usage

```csharp
using Stateless;
using Trellis;

var machine = new StateMachine<OrderState, OrderTrigger>(OrderState.New);
machine.Configure(OrderState.New)
    .Permit(OrderTrigger.Submit, OrderState.Submitted);

// Returns Result<OrderState> — no exceptions on invalid transitions
Result<OrderState> result = machine.FireResult(OrderTrigger.Submit);
// result.IsSuccess == true, result.Value == OrderState.Submitted

Result<OrderState> invalid = machine.FireResult(OrderTrigger.Cancel);
// invalid.IsFailure == true, invalid.Error is DomainError
```

## How It Works

- Uses Stateless's `CanFire()` to check before firing — **no try/catch internally**
- Returns `Result<TState>` with the new state on success
- Returns a `DomainError` on invalid transitions
