# FunctionalDdd.ArdalisSpecification

Integration package that adds Railway Oriented Programming (ROP) extensions to [Ardalis.Specification](https://github.com/ardalis/Specification) repositories.

## Overview

This package bridges Ardalis.Specification's repository pattern with FunctionalDDD's `Result<T>` type, enabling fluent, type-safe error handling when querying data.

## Installation

```bash
dotnet add package FunctionalDdd.ArdalisSpecification
```

## Features

### Result-Returning Repository Extensions

| Method | Returns | Description |
|--------|---------|-------------|
| `FirstOrNotFoundAsync` | `Result<T>` | Returns first matching entity or `NotFoundError` |
| `SingleOrNotFoundAsync` | `Result<T>` | Returns single matching entity, `NotFoundError` if none, `ConflictError` if multiple |
| `ToListAsync` | `List<T>` | Returns all matching entities |
| `AnyAsync` | `bool` | Checks if any entity matches |

### Supported Repository Interfaces

- `IRepositoryBase<T>` - Full CRUD repository
- `IReadRepositoryBase<T>` - Read-only repository

## Usage Examples

### Basic Query with Error Handling

```csharp
public class GetOrderHandler
{
    private readonly IRepository<Order> _repository;

    public async Task<Result<OrderDto>> Handle(GetOrderQuery query, CancellationToken ct)
    {
        return await _repository
            .FirstOrNotFoundAsync(new OrderByIdSpec(query.OrderId), ct: ct)
            .MapAsync(order => new OrderDto(order));
    }
}
```

### Chaining with ROP Extensions

```csharp
public async Task<Result<OrderConfirmation>> PlaceOrder(PlaceOrderCommand cmd, CancellationToken ct)
{
    return await _customerRepository
        .FirstOrNotFoundAsync(new CustomerByIdSpec(cmd.CustomerId), ct: ct)
        .BindAsync(customer => _productRepository
            .FirstOrNotFoundAsync(new ProductBySkuSpec(cmd.Sku), ct: ct)
            .MapAsync(product => (customer, product)))
        .BindAsync(tuple => CreateOrder(tuple.customer, tuple.product))
        .TapAsync(order => _orderRepository.AddAsync(order, ct))
        .MapAsync(order => new OrderConfirmation(order.Id));
}
```

### Single Result Specification

Use `ISingleResultSpecification<T>` for `SingleOrNotFoundAsync`:

```csharp
public class ActiveOrderByCustomerSpec : SingleResultSpecification<Order>
{
    public ActiveOrderByCustomerSpec(CustomerId customerId)
    {
        Query
            .Where(o => o.CustomerId == customerId)
            .Where(o => o.Status == OrderStatus.Active);
    }
}

// Usage
var result = await _repository.SingleOrNotFoundAsync(
    new ActiveOrderByCustomerSpec(customerId),
    entityName: "Active order", // Custom error message
    ct: cancellationToken);
```

### Custom Entity Names in Errors

```csharp
// Default: "Order not found"
var result = await _repository.FirstOrNotFoundAsync(spec, ct: ct);

// Custom: "Customer order not found"  
var result = await _repository.FirstOrNotFoundAsync(
    spec, 
    entityName: "Customer order", 
    ct: ct);
```

### Error Type Mapping

| Scenario | Error Type | Example Message |
|----------|------------|-----------------|
| No entity found | `NotFoundError` | "Order not found" |
| Multiple entities (SingleOrNotFoundAsync) | `ConflictError` | "Multiple Order entities found" |

## API Reference

### FirstOrNotFoundAsync

```csharp
Task<Result<T>> FirstOrNotFoundAsync<T>(
    this IRepositoryBase<T> repository,
    ISpecification<T> specification,
    string? entityName = null,
    CancellationToken ct = default)
```

Returns the first entity matching the specification, or `NotFoundError` if none exists.

### SingleOrNotFoundAsync

```csharp
Task<Result<T>> SingleOrNotFoundAsync<T>(
    this IRepositoryBase<T> repository,
    ISingleResultSpecification<T> specification,
    string? entityName = null,
    CancellationToken ct = default)
```

Returns the single entity matching the specification. Returns:
- `Success(entity)` - exactly one entity found
- `Failure(NotFoundError)` - no entity found
- `Failure(ConflictError)` - multiple entities found

### ToListAsync

```csharp
Task<List<T>> ToListAsync<T>(
    this IRepositoryBase<T> repository,
    ISpecification<T> specification,
    CancellationToken ct = default)
```

Returns all entities matching the specification.

### AnyAsync

```csharp
Task<bool> AnyAsync<T>(
    this IRepositoryBase<T> repository,
    ISpecification<T> specification,
    CancellationToken ct = default)
```

Returns `true` if any entity matches the specification.

## Integration with Minimal APIs

```csharp
app.MapGet("/orders/{id}", async (
    Guid id,
    IRepository<Order> repository,
    CancellationToken ct) =>
{
    return await repository
        .FirstOrNotFoundAsync(new OrderByIdSpec(id), ct: ct)
        .MapAsync(order => new OrderDto(order))
        .ToHttpResult();
});
```

## Integration with MVC Controllers

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetOrder(Guid id, CancellationToken ct)
{
    return await _repository
        .FirstOrNotFoundAsync(new OrderByIdSpec(id), ct: ct)
        .MapAsync(order => new OrderDto(order))
        .ToActionResult();
}
```

## Best Practices

### 1. Use Meaningful Entity Names

```csharp
// ✅ Good - provides context
await _repository.FirstOrNotFoundAsync(spec, entityName: "Active subscription", ct: ct);

// ❌ Less helpful
await _repository.FirstOrNotFoundAsync(spec, ct: ct);
```

### 2. Prefer SingleOrNotFoundAsync for Unique Constraints

```csharp
// When expecting exactly one result
var result = await _repository.SingleOrNotFoundAsync(
    new UserByEmailSpec(email), 
    ct: ct);

// When first match is acceptable
var result = await _repository.FirstOrNotFoundAsync(
    new OrdersByCustomerSpec(customerId), 
    ct: ct);
```

### 3. Chain Operations Fluently

```csharp
// ✅ Fluent ROP chain
return await _repository
    .FirstOrNotFoundAsync(spec, ct: ct)
    .EnsureAsync(entity => entity.IsActive, Error.Validation("Entity is inactive"))
    .MapAsync(entity => MapToDto(entity));
```

## Dependencies

- [Ardalis.Specification](https://www.nuget.org/packages/Ardalis.Specification) v9.3.1+
- [Ardalis.Specification.EntityFrameworkCore](https://www.nuget.org/packages/Ardalis.Specification.EntityFrameworkCore) v9.3.1+
- [FunctionalDdd.RailwayOrientedProgramming](https://www.nuget.org/packages/FunctionalDdd.RailwayOrientedProgramming)

## License

This project is licensed under the MIT License.
