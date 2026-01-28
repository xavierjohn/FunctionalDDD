# Specification Pattern Example

This example demonstrates the **Specification Pattern** for building type-safe, composable, and reusable query specifications with EF Core integration.

## What's Demonstrated

### 1. Subclass Specifications (Reusable Business Rules)

```csharp
public sealed class OrdersReadyForProcessingSpec : Specification<Order>
{
    public OrdersReadyForProcessingSpec()
        : base(o => o.Status == OrderStatus.Confirmed && o.PaymentStatus == PaymentStatus.Paid)
    {
        AddInclude(o => o.Lines);
        AddOrderBy(o => o.CreatedAt);
        ApplyAsNoTracking();
    }
}

// Usage
var orders = await context.Orders.ToListAsync(new OrdersReadyForProcessingSpec());
```

### 2. Inline Specifications

```csharp
// Ad-hoc query
var shippedSpec = Spec.For<Order>(o => o.Status == OrderStatus.Shipped);
var shippedOrders = await context.Orders.ToListAsync(shippedSpec);

// Match all
var allOrders = await context.Orders.ToListAsync(Spec.All<Order>());
```

### 3. Composition (And, Or, Not)

```csharp
// High-value AND priority orders
var spec = new HighValueOrdersSpec(500m).And(new PriorityOrdersSpec());

// Confirmed OR Processing orders
var activeSpec = new OrdersByStatusSpec(OrderStatus.Confirmed)
    .Or(new OrdersByStatusSpec(OrderStatus.Processing));

// NOT cancelled orders
var notCancelled = new CancelledOrdersSpec().Not();
```

### 4. Fluent Extensions

```csharp
var spec = Spec.For<Order>(o => o.Status != OrderStatus.Cancelled)
    .Include(o => o.Lines)
    .OrderByDescending(o => o.Total)
    .Paginate(pageNumber: 1, pageSize: 10)
    .AsNoTracking();
```

### 5. Result Pattern Integration

```csharp
public async Task<Result<Order>> GetOrderByIdAsync(OrderId orderId)
{
    var spec = Spec.For<Order>(o => o.Id == orderId)
        .Include(o => o.Lines)
        .AsNoTracking();

    return await _context.Orders.FirstOrNotFoundAsync(spec, "Order");
}

// Returns NotFoundError if not found
// Returns ConflictError if multiple found (SingleOrNotFoundAsync)
```

### 6. Railway Oriented Programming

```csharp
public async Task<Result<Order>> ProcessOrderAsync(OrderId orderId)
{
    var spec = Spec.For<Order>(o => o.Id == orderId).Include(o => o.Lines);

    return await _context.Orders
        .FirstOrNotFoundAsync(spec, "Order")
        .EnsureAsync(
            o => o.Status == OrderStatus.Confirmed,
            Error.Validation("Order must be confirmed before processing"))
        .EnsureAsync(
            o => o.PaymentStatus == PaymentStatus.Paid,
            Error.Validation("Order must be paid before processing"))
        .TapAsync(o =>
        {
            o.Status = OrderStatus.Processing;
            return _context.SaveChangesAsync();
        });
}
```

### 7. Testing Without Database

```csharp
using FunctionalDdd.Testing;

var spec = new HighValueOrdersSpec(500m);
var order = Order.Create(...);

// Test single entity
spec.IsSatisfiedBy(order).Should().BeTrue();

// Test collection
spec.IsSatisfiedByAll(orders).Should().BeTrue();

// Filter in-memory
var filtered = spec.Filter(orders).ToList();
```

## Running the Example

```bash
cd Examples/SpecificationExample
dotnet run
```

## Output

```
╔══════════════════════════════════════════════════════════════════╗
║  Specification Pattern Example with FunctionalDDD                ║
╚══════════════════════════════════════════════════════════════════╝

📦 Seeding Sample Orders...
────────────────────────────────────────────────────────────────────
  ✓ Created 9 orders for 2 customers

🔍 Using Simple Specifications...
────────────────────────────────────────────────────────────────────
  Orders ready for processing: 2
    • Alice Johnson: $750.00 (Priority: True)
    • Bob Smith: $2,500.00 (Priority: True)

🔗 Composing Specifications...
────────────────────────────────────────────────────────────────────
  High-value priority orders: 2
  Active orders (confirmed or processing): 4
  Non-cancelled orders: 8

...and more!
```

## Key Benefits

| Benefit | Description |
|---------|-------------|
| **Encapsulation** | Query logic lives with the domain, not scattered in services |
| **Testability** | Specifications can be tested without a database |
| **Reusability** | Compose complex queries from simple building blocks |
| **Type Safety** | Works seamlessly with strongly-typed value objects |
| **Separation** | Clean separation between query definition and execution |
