# EF Core Example

This example demonstrates seamless integration of FunctionalDDD primitive value objects with Entity Framework Core using an in-memory database.

## Features Demonstrated

- **RequiredUlid** - Time-ordered, lexicographically sortable identifiers (`OrderId`, `CustomerId`)
- **RequiredGuid** - Traditional GUID identifiers (`ProductId`)
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
│   ├── Customer.cs         # Customer entity with ULID ID
│   ├── Product.cs          # Product entity with GUID ID
│   ├── Order.cs            # Order aggregate with ULID ID
│   └── OrderLine.cs        # Order line entity
└── ValueObjects/
    ├── OrderId.cs          # RequiredUlid<OrderId>
    ├── CustomerId.cs       # RequiredUlid<CustomerId>
    ├── ProductId.cs        # RequiredGuid<ProductId>
    ├── ProductName.cs      # RequiredString<ProductName>
    └── CustomerName.cs     # RequiredString<CustomerName>
```

## Key Concepts

### Defining Value Objects

```csharp
// ULID-based identifier (time-ordered, sortable)
public partial class OrderId : RequiredUlid<OrderId> { }

// GUID-based identifier (traditional)
public partial class ProductId : RequiredGuid<ProductId> { }

// Non-empty string
public partial class CustomerName : RequiredString<CustomerName> { }
```

### EF Core Value Converters

```csharp
// RequiredUlid -> string (26-char Crockford Base32)
builder.Property(o => o.Id)
    .HasConversion(
        id => id.Value.ToString(),
        str => OrderId.Create(Ulid.Parse(str, CultureInfo.InvariantCulture)))
    .HasMaxLength(26);

// RequiredGuid -> Guid
builder.Property(p => p.Id)
    .HasConversion(
        id => id.Value,
        guid => ProductId.Create(guid));

// EmailAddress -> string
builder.Property(c => c.Email)
    .HasConversion(
        email => email.Value,
        str => EmailAddress.Create(str))
    .HasMaxLength(254);
```

### Railway-Oriented Entity Creation

```csharp
public static Result<Customer> TryCreate(string? name, string? email) =>
    CustomerName.TryCreate(name, nameof(name))
        .Combine(EmailAddress.TryCreate(email, nameof(email)))
        .Map((customerName, emailAddress) => new Customer(
            CustomerId.NewUnique(),
            customerName,
            emailAddress));
```

### ULID Benefits for Database IDs

| Feature | ULID | GUID |
|---------|------|------|
| **Index Performance** | ✅ Sequential (better) | ❌ Random (fragmentation) |
| **Natural Ordering** | ✅ By creation time | ❌ Random |
| **Format** | 26 chars (URL-safe) | 36 chars (with dashes) |
| **Query Ordering** | `ORDER BY Id` = chronological | Requires separate timestamp |

## Sample Output

```
╔══════════════════════════════════════════════════════════════════╗
║  EF Core Example with FunctionalDDD Primitive Value Objects      ║
╚══════════════════════════════════════════════════════════════════╝

📦 Creating Products...
  ✓ Created: MacBook Pro 16" (ID: 75324c82-304b-4ef3-9b04-f7a370d80ce1)
  ✓ Created: iPhone 15 Pro (ID: a9a84432-96ae-4d37-88b8-f67ff77b42cd)

👤 Creating Customer...
  ✓ Created: John Doe
             ID: 01KFW56SWA55Q98FB07AABA4SY
             Email: john.doe@example.com
             Note: ULID naturally encodes creation time!

🔒 Demonstrating Validation...
  ✗ Validation failed: Email address is not valid.
  ✗ Validation failed: Customer Name cannot be empty.

🛒 Creating Order...
  ✓ Order Created and Confirmed!
             Order ID: 01KFW56SWNXZ4B0C66KF8MZW6Y
             Total: $5,649.94

📊 Demonstrating ULID Ordering (Time-based Sortability)...
  Orders sorted by ULID (natural chronological order):
    01KFW56SWNXZ4B0C66KF8MZW6Y - Created: 03:22:54.229
    01KFW56T0K5PR7XXPRWN1F4YTY - Created: 03:22:54.355
    01KFW56T13K8C30G4A1A726SXT - Created: 03:22:54.371
```

## Related Documentation

- [Primitive Value Objects README](../../PrimitiveValueObjects/README.md)
- [EF Core Integration Guide](../../docs/docfx_project/articles/integration-ef.md)
- [Railway-Oriented Programming Basics](../../docs/docfx_project/articles/basics.md)
