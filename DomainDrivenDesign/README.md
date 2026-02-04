# Domain Driven Design

[![NuGet Package](https://img.shields.io/nuget/v/FunctionalDdd.DomainDrivenDesign.svg)](https://www.nuget.org/packages/FunctionalDdd.DomainDrivenDesign)

Building blocks for implementing Domain-Driven Design tactical patterns in C# with functional programming principles.

## Installation

```bash
dotnet add package FunctionalDdd.DomainDrivenDesign
```

## Quick Start

### Entity

Objects with unique identity. Equality based on ID.

```csharp
public class CustomerId : ScalarValueObject<Guid>
{
    private CustomerId(Guid value) : base(value) { }
    public static CustomerId NewUnique() => new(Guid.NewGuid());
}

public class Customer : Entity<CustomerId>
{
    public string Name { get; private set; }
    
    private Customer(CustomerId id, string name) : base(id)
    {
        Name = name;
    }
    
    public static Result<Customer> TryCreate(string name) =>
        name.ToResult()
            .Ensure(n => !string.IsNullOrWhiteSpace(n), Error.Validation("Name required"))
            .Map(n => new Customer(CustomerId.NewUnique(), n));
}
```

### Value Object

Immutable objects with no identity. Equality based on all properties.

```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
    
    public static Result<Money> TryCreate(decimal amount, string currency = "USD") =>
        (amount, currency).ToResult()
            .Ensure(x => x.amount >= 0, Error.Validation("Amount cannot be negative"))
            .Map(x => new Money(x.amount, x.currency));
    
    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
    
    public Money Add(Money other) =>
        Currency == other.Currency
            ? new Money(Amount + other.Amount, Currency)
            : throw new InvalidOperationException("Currency mismatch");
}
```

### SmartEnum

Type-safe enumerations with behavior. Unlike C# enums, SmartEnums prevent invalid values and can encapsulate domain logic.

```csharp
[JsonConverter(typeof(SmartEnumJsonConverter<OrderState>))]
public class OrderState : SmartEnum<OrderState>
{
    public static readonly OrderState Draft = new(1, "Draft", canModify: true, canCancel: true);
    public static readonly OrderState Confirmed = new(2, "Confirmed", canModify: false, canCancel: true);
    public static readonly OrderState Shipped = new(3, "Shipped", canModify: false, canCancel: false);
    public static readonly OrderState Delivered = new(4, "Delivered", canModify: false, canCancel: false);

    public bool CanModify { get; }
    public bool CanCancel { get; }

    private OrderState(int value, string name, bool canModify, bool canCancel) 
        : base(value, name)
    {
        CanModify = canModify;
        CanCancel = canCancel;
    }

    public IReadOnlyList<OrderState> AllowedTransitions => this switch
    {
        _ when this == Draft => [Confirmed],
        _ when this == Confirmed => [Shipped],
        _ when this == Shipped => [Delivered],
        _ => []
    };

    public Result<OrderState> TryTransitionTo(OrderState newState) =>
        AllowedTransitions.Contains(newState)
            ? newState
            : Error.Validation($"Cannot transition from '{Name}' to '{newState.Name}'");
}

// Usage
var state = OrderState.TryFromName("Draft");           // Result<OrderState>
var state2 = OrderState.TryFromValue(1);               // Result<OrderState>
var all = OrderState.GetAll();                         // All defined states

if (order.State.CanModify)
    order.AddLine(product, quantity);

order.State.TryTransitionTo(OrderState.Confirmed)
    .Tap(newState => order.State = newState);
```

### Aggregate

Cluster of entities and value objects treated as a unit. Manages domain events.

```csharp
public record OrderCreated(OrderId Id, CustomerId CustomerId, DateTime OccurredAt) : IDomainEvent;
public record OrderSubmitted(OrderId Id, Money Total, DateTime OccurredAt) : IDomainEvent;

public class Order : Aggregate<OrderId>
{
    private readonly List<OrderLine> _lines = [];
    
    public CustomerId CustomerId { get; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public Money Total { get; private set; }
    public OrderStatus Status { get; private set; }
    
    private Order(OrderId id, CustomerId customerId) : base(id)
    {
        CustomerId = customerId;
        Status = OrderStatus.Draft;
        Total = Money.TryCreate(0).Value;
        DomainEvents.Add(new OrderCreated(id, customerId, DateTime.UtcNow));
    }
    
    public static Result<Order> TryCreate(CustomerId customerId) =>
        new Order(OrderId.NewUnique(), customerId).ToResult();
    
    public Result<Order> AddLine(ProductId productId, string name, Money price, int qty) =>
        this.ToResult()
            .Ensure(_ => Status == OrderStatus.Draft, Error.Validation("Order not editable"))
            .Ensure(_ => qty > 0, Error.Validation("Quantity must be positive"))
            .Tap(_ =>
            {
                _lines.Add(new OrderLine(productId, name, price, qty));
                RecalculateTotal();
            });
    
    public Result<Order> Submit() =>
        this.ToResult()
            .Ensure(_ => Status == OrderStatus.Draft, Error.Validation("Already submitted"))
            .Ensure(_ => Lines.Count > 0, Error.Validation("Cannot submit empty order"))
            .Tap(_ =>
            {
                Status = OrderStatus.Submitted;
                DomainEvents.Add(new OrderSubmitted(Id, Total, DateTime.UtcNow));
            });
    
    private void RecalculateTotal()
    {
        var total = Lines.Sum(l => l.Price.Amount * l.Quantity);
        Total = Money.TryCreate(total).Value;
    }
}
```

### Domain Events

Publish events after persisting:

```csharp
var order = Order.TryCreate(customerId)
    .Bind(o => o.AddLine(productId, "Widget", price, 5))
    .Bind(o => o.Submit());

if (order.IsSuccess)
{
    await repository.SaveAsync(order.Value);
    
    foreach (var evt in order.Value.UncommittedEvents())
    {
        await eventBus.PublishAsync(evt);
    }
    
    order.Value.AcceptChanges();
}
```

## Best Practices

**1. Use entities when identity matters**
```csharp
public class Customer : Entity<CustomerId> { } // Identity-based
public class Address : ValueObject { }          // Value-based
```

**2. Keep aggregates small**
```csharp
public class Order : Aggregate<OrderId>
{
    // ? Include: OrderLine (part of aggregate)
    // ? Exclude: Customer, Shipment (reference by ID)
    public CustomerId CustomerId { get; }
}
```

**3. Reference other aggregates by ID**
```csharp
// ? Good
public CustomerId CustomerId { get; }

// ? Avoid
public Customer Customer { get; }
```

**4. Enforce invariants in aggregate root**
```csharp
public Result<Order> AddLine(...) =>
    this.ToResult()
        .Ensure(_ => Status == OrderStatus.Draft, ...)
        .Ensure(_ => quantity > 0, ...)
        .Tap(_ => _lines.Add(...));
```

**5. Use domain events for side effects**
```csharp
// ✅ Good - domain event
DomainEvents.Add(new OrderSubmitted(Id, Total, DateTime.UtcNow));

// ❌ Avoid - direct coupling
_emailService.SendConfirmation();
```

**6. Validate using Result types**
```csharp
// ? Good
public Result<Order> Cancel(string reason) =>
this.ToResult()
    .Ensure(_ => Status == OrderStatus.Draft, ...);

// ? Avoid
if (Status != OrderStatus.Draft)
    throw new InvalidOperationException(...);
```

**7. Make value objects immutable**
```csharp
// ? Good
public decimal Amount { get; }  // No setter

// ? Avoid
public decimal Amount { get; set; }
```

## Core Concepts

### Entity<TId>
- Identity-based equality
- Mutable state
- Lifecycle tracked by ID

### ValueObject
- No identity
- Immutable
- Equality based on all properties
- Override `GetEqualityComponents()`

### ScalarValueObject<T>
- Wraps single value
- Type safety for primitives
- Implicit conversion to `T`

### SmartEnum<T>
- Type-safe enumeration with behavior
- Prevents invalid values (unlike C# enums)
- Supports state machine patterns
- Built-in JSON serialization

### Aggregate<TId>
- Consistency boundary
- Manages domain events
- Properties: `IsChanged`, `UncommittedEvents()`, `AcceptChanges()`

### IDomainEvent
- Marker interface for domain events
- Use records for immutability
- Publish after persistence

## Resources

- **[SAMPLES.md](SAMPLES.md)** - Comprehensive examples and patterns
- **[Main Documentation](https://github.com/xavierjohn/FunctionalDDD)** - Full repository documentation
- **[Railway Oriented Programming](https://www.nuget.org/packages/FunctionalDdd.RailwayOrientedProgramming)** - Result type and functional patterns
- **[Primitive Value Objects](https://www.nuget.org/packages/FunctionalDdd.PrimitiveValueObjects)** - RequiredString, RequiredGuid, EmailAddress
- **[Ardalis.SmartEnum](https://github.com/ardalis/SmartEnum)** - Inspiration for SmartEnum pattern

## License

MIT License - see LICENSE file for details
