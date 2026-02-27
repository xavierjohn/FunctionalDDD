# Domain Driven Design

[![NuGet Package](https://img.shields.io/nuget/v/Trellis.DomainDrivenDesign.svg)](https://www.nuget.org/packages/Trellis.DomainDrivenDesign)

Building blocks for implementing Domain-Driven Design tactical patterns in C# with functional programming principles.

## Installation

```bash
dotnet add package Trellis.DomainDrivenDesign
```

## Quick Start

### Entity

Objects with unique identity. Equality based on ID.

```csharp
public partial class CustomerId : RequiredGuid<CustomerId> { }

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
            .Map(n => new Customer(CustomerId.NewUniqueV7(), n));
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
}
```

### Aggregate

Cluster of entities and value objects treated as a unit. Manages domain events.

```csharp
public record OrderCreated(OrderId Id, CustomerId CustomerId, DateTime OccurredAt) : IDomainEvent;

public class Order : Aggregate<OrderId>
{
    private readonly List<OrderLine> _lines = [];
    
    public CustomerId CustomerId { get; }
    public OrderStatus Status { get; private set; }
    
    private Order(OrderId id, CustomerId customerId) : base(id)
    {
        CustomerId = customerId;
        Status = OrderStatus.Draft;
        DomainEvents.Add(new OrderCreated(id, customerId, DateTime.UtcNow));
    }
    
    public static Result<Order> TryCreate(CustomerId customerId) =>
        new Order(OrderId.NewUniqueV7(), customerId).ToResult();
    
    public Result<Order> AddLine(ProductId productId, string name, Money price, int qty) =>
        this.ToResult()
            .Ensure(_ => Status == OrderStatus.Draft, Error.Validation("Order not editable"))
            .Ensure(_ => qty > 0, Error.Validation("Quantity must be positive"))
            .Tap(_ => _lines.Add(new OrderLine(productId, name, price, qty)));
}
```

### Specification

Encapsulate business rules as composable, storage-agnostic expression trees.

```csharp
public class HighValueOrderSpec(decimal threshold) : Specification<Order>
{
    public override Expression<Func<Order, bool>> ToExpression() =>
        order => order.TotalAmount > threshold;
}

// Compose specifications
var spec = new OverdueOrderSpec(now).And(new HighValueOrderSpec(500m));

// Use with EF Core
var orders = await dbContext.Orders.Where(spec).ToListAsync();

// In-memory evaluation
if (spec.IsSatisfiedBy(order))
    // order matches
```

### Domain Events

Publish events after persisting:

```csharp
if (order.IsSuccess)
{
    await repository.SaveAsync(order.Value);
    
    foreach (var evt in order.Value.UncommittedEvents())
        await eventBus.PublishAsync(evt);
    
    order.Value.AcceptChanges();
}
```

## Core Types

| Type | Purpose | Equality |
|------|---------|----------|
| **Entity\<TId\>** | Objects with identity | By ID |
| **ValueObject** | Immutable, no identity | By all properties |
| **ScalarValueObject\<TSelf, T\>** | Wraps single primitive | By value |
| **Aggregate\<TId\>** | Consistency boundary + events | By ID |
| **Specification\<T\>** | Composable business rules | — |
| **IDomainEvent** | Marker for domain events | — |

## Best Practices

1. **Use entities when identity matters** — `Customer : Entity<CustomerId>`
2. **Keep aggregates small** — Include only what's needed for invariants
3. **Reference other aggregates by ID** — `public CustomerId CustomerId { get; }` not `public Customer Customer { get; }`
4. **Use `Maybe<T>` for optional properties** — `Maybe<Url> Website` instead of `Url?`
5. **Enforce invariants in aggregate root** — Use `Result<T>` for validation
6. **Use domain events for side effects** — Not direct service calls
7. **Make value objects immutable** — No setters

## Related Packages

- [Trellis.Results](https://www.nuget.org/packages/Trellis.Results) — Core `Result<T>` type
- [Trellis.Primitives](https://www.nuget.org/packages/Trellis.Primitives) — RequiredString, RequiredGuid, EmailAddress, and more
- [Trellis.Asp](https://www.nuget.org/packages/Trellis.Asp) — ASP.NET Core integration

## License

MIT — see [LICENSE](https://github.com/xavierjohn/Trellis/blob/main/LICENSE) for details.
