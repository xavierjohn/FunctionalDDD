# Specification Pattern Integration

**TL;DR:** FunctionalDDD integrates seamlessly with Ardalis.Specification, combining the power of type-safe queries with Railway-Oriented Programming.

## Table of Contents

- [What is the Specification Pattern?](#what-is-the-specification-pattern)
- [FunctionalDDD Integration](#functionalddd-integration)
- [Type-Safe Specifications with Value Objects](#type-safe-specifications-with-value-objects)
- [ROP Extensions for Repository Queries](#rop-extensions-for-repository-queries)
- [Combining Specifications with ROP Pipelines](#combining-specifications-with-rop-pipelines)
- [Best Practices](#best-practices)
- [Real-World Example](#real-world-example)

## What is the Specification Pattern?

The Specification pattern encapsulates query logic into reusable, composable objects. It's particularly useful for:

- **Dynamic query building** - Build complex database queries programmatically
- **Query reuse** - Share query logic across your application
- **Separation of concerns** - Keep query logic separate from business logic
- **Type safety** - Strongly-typed query parameters

**Traditional Repository Pattern:**
```csharp
// ❌ Query logic mixed with business logic
var products = await _dbContext.Products
    .Where(p => p.IsActive)
    .Where(p => p.Category == category)
    .OrderBy(p => p.Name)
    .ToListAsync();
```

**Specification Pattern:**
```csharp
// ✅ Query logic encapsulated in reusable specification
public class ActiveProductsByCategorySpec : Specification<Product>
{
    public ActiveProductsByCategorySpec(CategoryName category) =>
        Query
            .Where(p => p.IsActive)
            .Where(p => p.Category == category)
            .OrderBy(p => p.Name);
}

// Usage
var products = await repository.ToListAsync(new ActiveProductsByCategorySpec(category));
```

## FunctionalDDD Integration

FunctionalDDD provides **seamless integration** with [Ardalis.Specification](https://github.com/ardalis/Specification) through the `FunctionalDDD.ArdalisSpecification` package.

### Installation

```bash
dotnet add package FunctionalDDD.ArdalisSpecification
```

### Key Features

✅ **Result-returning repository methods** - Returns `Result<T>` instead of throwing exceptions  
✅ **Type-safe value object queries** - Specifications accept value objects, not primitives  
✅ **ROP pipeline integration** - Chain specifications with `Ensure`, `Bind`, `Map`, etc.  
✅ **Automatic error handling** - `NotFoundError` and `ConflictError` instead of exceptions

## Type-Safe Specifications with Value Objects

FunctionalDDD specifications work seamlessly with **strongly-typed value objects**, preventing primitive obsession:

### Example: Product Specifications

```csharp
/// <summary>
/// Find a product by its unique SKU value object.
/// </summary>
public sealed class ProductBySkuSpec : SingleResultSpecification<Product>
{
    public ProductBySkuSpec(Sku sku) =>  // ✅ Accepts Sku value object, not string
        Query.Where(p => p.Sku == sku);
}

/// <summary>
/// Find products within a price range using Money value objects.
/// </summary>
public sealed class ProductsByPriceRangeSpec : Specification<Product>
{
    public ProductsByPriceRangeSpec(Money minPrice, Money maxPrice) =>  // ✅ Money value objects
        Query
            .Where(p => p.IsActive)
            .Where(p => p.Price.Amount >= minPrice.Amount && p.Price.Amount <= maxPrice.Amount)
            .OrderBy(p => p.Price.Amount);
}

/// <summary>
/// Find active products in a category.
/// </summary>
public sealed class ActiveProductsByCategorySpec : Specification<Product>
{
    public ActiveProductsByCategorySpec(CategoryName category) =>  // ✅ CategoryName value object
        Query
            .Where(p => p.IsActive)
            .Where(p => p.Category == category)
            .OrderBy(p => p.Name);
}
```

### Type Safety Benefits

```csharp
// ✅ Compiler enforces correct types
var sku = Sku.Create("LAPTOP-001");
var spec = new ProductBySkuSpec(sku);  // ✅ Type-safe

// ❌ Compiler error - can't pass string!
var wrongSpec = new ProductBySkuSpec("LAPTOP-001");  // ❌ Compile error!
```

## ROP Extensions for Repository Queries

FunctionalDDD extends Ardalis.Specification repositories with **Result-returning methods**:

### `FirstOrNotFoundAsync<T>`

Returns the first entity matching the specification, or `NotFoundError`:

```csharp
public static async Task<Result<T>> FirstOrNotFoundAsync<T>(
    this IRepositoryBase<T> repository,
    ISpecification<T> specification,
    string? entityName = null,
    CancellationToken ct = default) where T : class
```

**Usage:**
```csharp
var result = await repository.FirstOrNotFoundAsync(
    new ActiveProductsByCategorySpec(category),
    entityName: "Product");

// Returns Result<Product>:
// - Success(product) if found
// - Failure(NotFoundError) if none exist
```

### `SingleOrNotFoundAsync<T>`

Returns the single entity matching the specification, or an error:

```csharp
public static async Task<Result<T>> SingleOrNotFoundAsync<T>(
    this IRepositoryBase<T> repository,
    ISingleResultSpecification<T> specification,
    string? entityName = null,
    CancellationToken ct = default) where T : class
```

**Usage:**
```csharp
var result = await repository.SingleOrNotFoundAsync(
    new ProductBySkuSpec(sku),
    entityName: "Product");

// Returns Result<Product>:
// - Success(product) if exactly one found
// - Failure(NotFoundError) if none found
// - Failure(ConflictError) if multiple found
```

### `ToListAsync<T>`

Returns a list of entities (standard Ardalis.Specification method):

```csharp
var products = await repository.ToListAsync(new LowStockProductsSpec(threshold: 10));
// Returns List<Product> directly
```

## Combining Specifications with ROP Pipelines

The real power comes from **chaining specifications with ROP operations**:

### Pattern 1: Validate Input → Query → Transform

```csharp
// User input
string userInputSku = "LAPTOP-001";

// Validate → Query → Business Rules → Transform
var result = await Sku.TryCreate(userInputSku, "sku")
    .BindAsync(async sku => 
        await repository.SingleOrNotFoundAsync(new ProductBySkuSpec(sku)))
    .Ensure(p => p.IsActive, Error.Validation("Product is not active"))
    .Ensure(p => p.StockQuantity > 0, Error.Validation("Product is out of stock"))
    .Map(p => new ProductDto(p.Id, p.Name, p.Price, p.StockQuantity));

// Returns Result<ProductDto>:
// - If SKU validation fails → Failure(ValidationError)
// - If product not found → Failure(NotFoundError)
// - If product inactive → Failure(ValidationError)
// - If out of stock → Failure(ValidationError)
// - Otherwise → Success(ProductDto)
```

### Pattern 2: Query → Complex Business Logic

```csharp
var orderPipeline = await repository
    .SingleOrNotFoundAsync(new ProductBySkuSpec(sku))
    .Ensure(p => p.IsActive, Error.Validation("Product is not active"))
    .Ensure(p => p.StockQuantity >= quantity, Error.Validation("Insufficient stock"))
    .Bind(p => CreateOrderLine(p, quantity))
    .TapAsync(async line => await _orderRepository.AddLineAsync(line));

// Returns Result<OrderLine>
```

### Pattern 3: Multiple Queries Combined

```csharp
var combinedResult = await repository
    .SingleOrNotFoundAsync(new ProductByIdSpec(productId))
    .Combine(
        await _customerRepository.SingleOrNotFoundAsync(new CustomerByIdSpec(customerId)))
    .Bind((product, customer) => ValidateOrderEligibility(product, customer))
    .Tap((product, customer) => CreateOrder(product, customer));

// Returns Result<(Product, Customer)>
```

### Pattern 4: Specification + Async Validation

```csharp
var result = await repository
    .SingleOrNotFoundAsync(new CustomerByEmailSpec(email))
    .EnsureAsync(
        async c => !await _blacklistService.IsBlacklistedAsync(c.Id),
        Error.Validation("Customer is blacklisted"))
    .TapAsync(async c => await SendWelcomeEmailAsync(c.Email));

// Returns Result<Customer>
```

## Best Practices

### ✅ Use Specifications For Queries

```csharp
// ✅ Good - Specification encapsulates query logic
public class ActiveCustomersByRegionSpec : Specification<Customer>
{
    public ActiveCustomersByRegionSpec(string region) =>
        Query
            .Where(c => c.IsActive)
            .Where(c => c.Region == region)
            .OrderBy(c => c.Name);
}

var customers = await repository.ToListAsync(new ActiveCustomersByRegionSpec("West"));
```

### ✅ Use ROP For Business Logic

```csharp
// ✅ Good - ROP enforces business rules after query
var result = await repository
    .SingleOrNotFoundAsync(new CustomerByIdSpec(customerId))
    .Ensure(c => c.CreditLimit >= orderTotal,
           Error.Validation("Insufficient credit limit"))
    .Ensure(c => !c.IsBlacklisted,
           Error.Validation("Customer is blacklisted"))
    .Tap(c => PlaceOrder(c, orderTotal));
```

### ✅ Use Value Objects in Specifications

```csharp
// ✅ Good - Type-safe with value objects
public class ProductBySkuSpec : SingleResultSpecification<Product>
{
    public ProductBySkuSpec(Sku sku) =>  // ← Sku value object
        Query.Where(p => p.Sku == sku);
}

// ❌ Bad - Primitive obsession
public class ProductBySkuSpec : SingleResultSpecification<Product>
{
    public ProductBySkuSpec(string sku) =>  // ← string primitive
        Query.Where(p => p.Sku.Value == sku);
}
```

### ✅ Validate Before Querying

```csharp
// ✅ Good - Validate input first, then query
var result = await Sku.TryCreate(userInput, "sku")
    .BindAsync(async sku => 
        await repository.SingleOrNotFoundAsync(new ProductBySkuSpec(sku)));

// ❌ Bad - Query with potentially invalid input
var product = await repository.SingleOrDefaultAsync(spec);
if (product is null) return Error.NotFound("Product not found");
```

### ✅ Use `SingleResultSpecification<T>` for Unique Queries

```csharp
// ✅ Good - Indicates single result expected
public class ProductByIdSpec : SingleResultSpecification<Product>
{
    public ProductByIdSpec(ProductId id) =>
        Query.Where(p => p.Id == id);
}

// Use with SingleOrNotFoundAsync
var result = await repository.SingleOrNotFoundAsync(new ProductByIdSpec(id));
// Returns Result<Product> with proper error handling
```

### ✅ Chain Specifications with ROP

```csharp
// ✅ Good - Query → Validate → Transform pipeline
var dto = await repository
    .FirstOrNotFoundAsync(new RecentOrdersSpec(customerId))
    .Ensure(o => o.Status == OrderStatus.Pending,
           Error.Validation("Only pending orders can be modified"))
    .Map(o => new OrderDto(o.Id, o.Total, o.Status));

// Returns Result<OrderDto>
```

## Real-World Example

### E-Commerce Order Processing

```csharp
public class OrderService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Order> _orderRepository;

    public async Task<Result<Order>> CreateOrderAsync(
        string customerEmail,
        string productSku,
        int quantity)
    {
        // 1. Validate and create value objects
        return await EmailAddress.TryCreate(customerEmail, "email")
            .Combine(Sku.TryCreate(productSku, "sku"))
            
            // 2. Query using specifications
            .BindAsync(async (email, sku) =>
                await _customerRepository.SingleOrNotFoundAsync(
                    new CustomerByEmailSpec(email),
                    entityName: "Customer")
                .Combine(
                    await _productRepository.SingleOrNotFoundAsync(
                        new ProductBySkuSpec(sku),
                        entityName: "Product"))
                .Map((customer, product) => (customer, product, quantity)))
            
            // 3. Apply business rules
            .Ensure(tuple => tuple.customer.IsActive,
                   Error.Validation("Customer account is inactive"))
            .Ensure(tuple => tuple.product.IsActive,
                   Error.Validation("Product is not available"))
            .Ensure(tuple => tuple.product.StockQuantity >= tuple.quantity,
                   Error.Validation("Insufficient stock"))
            .Ensure(tuple => tuple.customer.CreditLimit >= tuple.product.Price.Amount * tuple.quantity,
                   Error.Validation("Insufficient credit limit"))
            
            // 4. Create and persist order
            .BindAsync(async tuple => 
                await Order.TryCreate(tuple.customer.Id, tuple.product.Id, tuple.quantity))
            .TapAsync(async order => await _orderRepository.AddAsync(order))
            
            // 5. Side effects
            .TapAsync(async order => await SendOrderConfirmationAsync(order));
    }
}
```

## Summary

### When to Use Each Pattern

| Use Case | Pattern | Why |
|----------|---------|-----|
| **Database queries** | Specifications | Encapsulates EF Core query logic |
| **Reusable query logic** | Specifications | DRY principle for queries |
| **Dynamic filtering** | Specifications | Build queries programmatically |
| **Complex joins** | Specifications | Handle complex EF Core queries |
| **Business rule validation** | ROP (`Ensure`, `Bind`) | Type-safe error handling |
| **Data transformation** | ROP (`Map`) | Functional composition |
| **Side effects** | ROP (`Tap`) | Controlled side-effect execution |
| **Error propagation** | ROP (automatic) | Railway track automatic failure routing |

### The Perfect Combination

**Specifications + ROP** gives you the best of both worlds:

✅ **Type-safe queries** with value objects  
✅ **Reusable query logic** through specifications  
✅ **Rich error handling** through `Result<T>`  
✅ **Composable pipelines** combining queries and business logic  
✅ **No exceptions** for expected failures  
✅ **Self-documenting code** that reads like English  

**The railway stays on track, powered by specifications.** 🚂✨

