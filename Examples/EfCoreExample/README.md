# EF Core Example

This example demonstrates seamless integration of Trellis primitive value objects with Entity Framework Core using an in-memory database.

## Features Demonstrated

- **RequiredGuid** - Strongly-typed GUID identifiers (`OrderId`, `CustomerId`, `ProductId`)
- **RequiredString** - Non-empty string validation (`ProductName`, `CustomerName`)
- **EmailAddress** - RFC 5322 email validation
- **EF Core Value Converters** - Seamless database persistence
- **Railway-Oriented Programming** - Validation with `TryCreate`, `Bind`, `Map`, `Tap`

## Running the Example

```bash
dotnet run --project Examples/EfCoreExample/EfCoreExample.csproj
```

## Project Structure

```
EfCoreExample/
├── Program.cs              # Main demo showcasing all features
├── Data/
│   └── AppDbContext.cs     # EF Core configuration with value converters
├── Entities/
│   ├── Customer.cs         # Customer entity with GUID ID
│   ├── Product.cs          # Product entity with GUID ID
│   ├── Order.cs            # Order aggregate with GUID ID
│   └── OrderLine.cs        # Order line entity
├── Enums/
│   └── OrderState.cs       # RequiredEnum<OrderState>
└── ValueObjects/
    ├── OrderId.cs          # RequiredGuid<OrderId>
    ├── CustomerId.cs       # RequiredGuid<CustomerId>
    ├── ProductId.cs        # RequiredGuid<ProductId>
    ├── ProductName.cs      # RequiredString<ProductName>
    └── CustomerName.cs     # RequiredString<CustomerName>
```

## Key Concepts

### Defining Value Objects

```csharp
// GUID-based identifiers
public partial class OrderId : RequiredGuid<OrderId> { }
public partial class CustomerId : RequiredGuid<CustomerId> { }
public partial class ProductId : RequiredGuid<ProductId> { }

// Non-empty string
public partial class CustomerName : RequiredString<CustomerName> { }
```

### EF Core Value Converters

Trellis.EntityFrameworkCore eliminates all `HasConversion()` boilerplate. A single line in `ConfigureConventions` registers converters for every value object automatically:

```csharp
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
    configurationBuilder.ApplyTrellisConventions(typeof(CustomerId).Assembly);

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // No HasConversion() needed — just configure keys, indexes, constraints
    modelBuilder.Entity<Customer>(builder =>
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(254).IsRequired();
    });
}
```

### Railway-Oriented Entity Creation

```csharp
public static Result<Customer> TryCreate(string? name, string? email) =>
    CustomerName.TryCreate(name, nameof(name))
        .Combine(EmailAddress.TryCreate(email, nameof(email)))
        .Map((customerName, emailAddress) => new Customer(
            CustomerId.NewUniqueV7(),
            customerName,
            emailAddress));
```

### GUID V7 Benefits for Database IDs

Using `NewUniqueV7()` generates time-ordered GUIDs that provide natural chronological ordering and better index performance compared to random GUIDs.

| Feature | GUID V7 | GUID V4 |
|---------|---------|----------|
| **Index Performance** | ✅ Sequential (better) | ❌ Random (fragmentation) |
| **Natural Ordering** | ✅ By creation time | ❌ Random |
| **Format** | 36 chars (standard GUID) | 36 chars (standard GUID) |
| **Query Ordering** | `ORDER BY Id` = chronological | Requires separate timestamp |

## Sample Output

```
╔══════════════════════════════════════════════════════════════════╗
║  EF Core Example with Trellis Primitive Value Objects            ║
╚══════════════════════════════════════════════════════════════════╝

📦 Creating Products...
  ✓ Created: MacBook Pro 16" (ID: 75324c82-304b-4ef3-9b04-f7a370d80ce1)
  ✓ Created: iPhone 15 Pro (ID: a9a84432-96ae-4d37-88b8-f67ff77b42cd)

👤 Creating Customer...
  ✓ Created: John Doe
             ID: 019505a3-b1e0-7c6a-8b4d-2f1a3e5c7d9f
             Email: john.doe@example.com

🔒 Demonstrating Validation...
  ✗ Validation failed: Email address is not valid.
  ✗ Validation failed: Customer Name cannot be empty.

🛒 Creating Order...
  ✓ Order Created and Confirmed!
             Order ID: 019505a3-b1e1-7d2b-9c5e-3a2b4f6d8e0a
             Total: $5,649.94
```

## Related Documentation

- [Primitive Value Objects README](../../Trellis.Primitives/README.md)
- [EF Core Integration Guide](../../docs/docfx_project/articles/integration-ef.md)
- [Railway-Oriented Programming Basics](../../docs/docfx_project/articles/basics.md)
