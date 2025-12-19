
# Domain Driven Design

[![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.DomainDrivenDesign.svg)](https://www.nuget.org/packages/FunctionalDDD.DomainDrivenDesign)

Building blocks for implementing Domain-Driven Design tactical patterns in C# with functional programming principles.

## Installation

```bash
dotnet add package FunctionalDDD.DomainDrivenDesign
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

### Aggregate

Cluster of entities and value objects treated as a unit. Manages domain events.

```csharp
public record OrderCreatedEvent(OrderId Id, CustomerId CustomerId) : IDomainEvent;
public record OrderSubmittedEvent(OrderId Id, Money Total) : IDomainEvent;

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
        DomainEvents.Add(new OrderCreatedEvent(id, customerId));
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
                DomainEvents.Add(new OrderSubmittedEvent(Id, Total));
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
// ? Good - domain event
DomainEvents.Add(new OrderSubmittedEvent(Id));

// ? Avoid - direct coupling
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
- **[Railway Oriented Programming](https://www.nuget.org/packages/FunctionalDDD.RailwayOrientedProgramming)** - Result type and functional patterns

## License

MIT License - see LICENSE file for details