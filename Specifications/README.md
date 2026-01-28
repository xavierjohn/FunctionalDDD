# FunctionalDdd.Specifications

Expression-based, composable query specifications for Entity Framework Core with Result pattern integration.

## Features

| Feature | Description |
|---------|-------------|
| **Type-safe** | Works with strongly-typed IDs and value objects |
| **Composable** | AND/OR/NOT with fluent extensions |
| **Immutable** | Each operation returns a new specification |
| **EF Core native** | Compiles to SQL, supports includes/ordering/paging |
| **Result-integrated** | `FirstOrNotFoundAsync` returns `Result<T>` |
| **Testable** | `IsSatisfiedBy` for unit testing without database |

## Installation

```bash
dotnet add package FunctionalDdd.Specifications
```

## Quick Start

### 1. Create a Specification (Subclass)

```csharp
public sealed class ActiveOrdersSpec : Specification<Order>
{
    public ActiveOrdersSpec()
        : base(o => o.Status == OrderStatus.Active)
    {
        AddInclude(o => o.Lines);
        AddOrderByDescending(o => o.CreatedAt);
        ApplyAsNoTracking();
    }
}

// Usage
var orders = await context.Orders.ToListAsync(new ActiveOrdersSpec());
```

### 2. Create a Parameterized Specification

```csharp
public sealed class OrdersByCustomerSpec : Specification<Order>
{
    public OrdersByCustomerSpec(CustomerId customerId)
        : base(o => o.CustomerId == customerId)
    {
        AddInclude(o => o.Lines);
    }
}

// Usage
var orders = await context.Orders.ToListAsync(new OrdersByCustomerSpec(customerId));
```

### 3. Inline Specification

```csharp
var highValueOrders = Spec.For<Order>(o => o.Total > 1000m);
var orders = await context.Orders.ToListAsync(highValueOrders);

// Match all entities
var allOrders = Spec.All<Order>();
```

### 4. Composition

```csharp
var activeSpec = new ActiveOrdersSpec();
var highValueSpec = Spec.For<Order>(o => o.Total > 1000m);

var combinedSpec = activeSpec
    .And(highValueSpec)
    .OrderByDescending(o => o.Total)
    .Paginate(pageNumber: 1, pageSize: 10);

var orders = await context.Orders.ToListAsync(combinedSpec);
```

### 5. Result Integration

```csharp
public async Task<Result<Order>> GetOrderAsync(OrderId orderId)
{
    var spec = Spec.For<Order>(o => o.Id == orderId)
        .Include(o => o.Lines)
        .AsNoTracking();

    return await _context.Orders.FirstOrNotFoundAsync(spec, "Order");
}

// Chain with ROP
var result = await GetOrderAsync(orderId)
    .EnsureAsync(o => o.Status == OrderStatus.Active, Error.Validation("Order is not active"))
    .BindAsync(o => ProcessOrderAsync(o));
```

## Specification Base Class

The `Specification<T>` class provides protected methods to configure your specification:

```csharp
public sealed class OrdersReadyForShippingSpec : Specification<Order>
{
    public OrdersReadyForShippingSpec(DateOnly shippingDate)
        : base(o => o.Status == OrderStatus.Confirmed
                 && o.PaymentStatus == PaymentStatus.Paid
                 && o.RequestedShipDate <= shippingDate)
    {
        // Eagerly load navigation properties
        AddInclude(o => o.Customer);
        AddInclude(o => o.Lines);
        AddInclude("Lines.Product");  // Nested include via string
        
        // Ordering
        AddOrderBy(o => o.RequestedShipDate);
        
        // Performance options
        ApplyAsSplitQuery();
        ApplyAsNoTracking();
        
        // Pagination (skip, take)
        ApplyPaging(skip: 0, take: 100);
    }
}
```

## Composition Methods

### AND

```csharp
var combinedSpec = activeSpec.And(highValueSpec);
```

### OR

```csharp
var combinedSpec = activeSpec.Or(cancelledSpec);
```

### NOT

```csharp
var notActiveSpec = activeSpec.Not();
```

## Fluent Extensions

All extensions return a new immutable specification:

```csharp
var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active)
    .Include(o => o.Lines)
    .Include("Lines.Product")
    .OrderBy(o => o.CustomerName)
    .OrderByDescending(o => o.Total)
    .Paginate(pageNumber: 2, pageSize: 10)
    .AsNoTracking()
    .AsSplitQuery();
```

## DbSet Extensions

| Method | Returns | Description |
|--------|---------|-------------|
| `ToListAsync` | `List<T>` | All matching entities |
| `FirstOrNotFoundAsync` | `Result<T>` | First match or NotFound error |
| `SingleOrNotFoundAsync` | `Result<T>` | Single match, NotFound, or Conflict error |
| `AnyAsync` | `bool` | Whether any entity matches |
| `CountAsync` | `int` | Count of matching entities |

## Testing Support

Test specifications without a database using `IsSatisfiedBy`:

```csharp
using FunctionalDdd.Testing;

var spec = new ActiveOrdersSpec();
var order = Order.Create(...);

// Test single entity
spec.IsSatisfiedBy(order).Should().BeTrue();

// Test collection
var orders = new[] { order1, order2, order3 };
spec.IsSatisfiedByAll(orders).Should().BeTrue();
spec.IsSatisfiedByAny(orders).Should().BeTrue();

// Filter in-memory
var matching = spec.Filter(orders).ToList();

// Count matches
var count = spec.Count(orders);
```

## Direct Evaluator Usage

For custom scenarios, use `SpecificationEvaluator` directly:

```csharp
var query = SpecificationEvaluator.Apply(spec, context.Orders);

// Add custom operations
var result = await query
    .Select(o => new OrderDto(o.Id, o.Total))
    .ToListAsync();
```

## Best Practices

### ✅ DO

- Create named specification classes for reusable business rules
- Use parameterized constructors for dynamic criteria
- Compose specifications for complex queries
- Use `AsNoTracking()` for read-only queries
- Use `AsSplitQuery()` with multiple includes to avoid cartesian explosion

### ❌ DON'T

- Don't use `IsSatisfiedBy` for filtering large datasets (use EF Core)
- Don't create specifications with side effects
- Don't modify specifications after creation (they're immutable by design)

## Examples

### Repository Pattern Integration

```csharp
public interface IRepository<T> where T : class
{
    Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<Result<T>> FirstOrNotFoundAsync(ISpecification<T> spec, CancellationToken ct = default);
}

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;
    
    public Repository(DbContext context) => _context = context;
    
    public Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default)
        => _context.Set<T>().ToListAsync(spec, ct);
    
    public Task<Result<T>> FirstOrNotFoundAsync(ISpecification<T> spec, CancellationToken ct = default)
        => _context.Set<T>().FirstOrNotFoundAsync(spec, ct: ct);
}
```

### API Endpoint

```csharp
app.MapGet("/api/orders", async (
    [FromQuery] int page,
    [FromQuery] int pageSize,
    [FromQuery] OrderStatus? status,
    OrderDbContext context) =>
{
    var spec = status.HasValue
        ? Spec.For<Order>(o => o.Status == status.Value)
        : Spec.All<Order>();
    
    var pagedSpec = spec
        .OrderByDescending(o => o.CreatedAt)
        .Paginate(page, pageSize)
        .AsNoTracking();
    
    var orders = await context.Orders.ToListAsync(pagedSpec);
    return Results.Ok(orders);
});
```

### Domain Service

```csharp
public class OrderService
{
    private readonly OrderDbContext _context;
    
    public async Task<Result<Order>> ShipOrderAsync(OrderId orderId)
    {
        var spec = Spec.For<Order>(o => o.Id == orderId)
            .Include(o => o.Lines);
        
        return await _context.Orders
            .FirstOrNotFoundAsync(spec, "Order")
            .EnsureAsync(
                o => o.Status == OrderStatus.Confirmed,
                Error.Validation("Order must be confirmed before shipping"))
            .TapAsync(o => o.Ship())
            .TapAsync(_ => _context.SaveChangesAsync());
    }
}
```
