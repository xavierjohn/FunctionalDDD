# Enum Value Objects (SmartEnum)

Enum Value Objects are type-safe enumerations with behavior, providing a powerful alternative to C# enums for domain modeling. Also known as "Smart Enums" in the .NET community or "Standard Types" in DDD terminology.

## Why Enum Value Objects?

C# enums have limitations that can lead to bugs:

```csharp
// C# enum problems
public enum OrderStatus { Draft, Confirmed, Shipped }

var status = (OrderStatus)999;  // Valid! No compile or runtime error
var invalid = Enum.Parse<OrderStatus>("Invalid");  // Throws at runtime
```

Enum Value Objects solve these issues:

```csharp
// Enum Value Object - type-safe, with behavior
public class OrderState : EnumValueObject<OrderState>
{
    public static readonly OrderState Draft = new();
    public static readonly OrderState Confirmed = new();
    public static readonly OrderState Shipped = new();
    
    private OrderState() { }
}

// Cannot create invalid values - Name is auto-derived from field name
var result = OrderState.TryFromName("Invalid");  // Returns Result.Failure
var state = OrderState.Draft;  // state.Name == "Draft"
```

## Basic Usage

### Defining an Enum Value Object

The Name is automatically derived from the field name - pure domain, no strings needed:

```csharp
using FunctionalDdd;

public class PaymentMethod : EnumValueObject<PaymentMethod>
{
    // Name auto-derived: "CreditCard", "DebitCard", etc.
    public static readonly PaymentMethod CreditCard = new();
    public static readonly PaymentMethod DebitCard = new();
    public static readonly PaymentMethod BankTransfer = new();
    public static readonly PaymentMethod Crypto = new();

    private PaymentMethod() { }
}
```

### Creating Instances

```csharp
// From name (case-insensitive, returns Result<T>)
var result = PaymentMethod.TryFromName("creditcard");
if (result.IsSuccess)
    Console.WriteLine(result.Value.Name);  // "CreditCard"

// Direct access when known valid
var method = PaymentMethod.FromName("CreditCard");  // Throws if invalid

// Check membership
if (payment.Is(PaymentMethod.CreditCard, PaymentMethod.DebitCard))
    ApplyCardFee();

if (payment.IsNot(PaymentMethod.Crypto))
    ProcessTraditionalPayment();

// Enumerate all values
foreach (var m in PaymentMethod.GetAll())
    Console.WriteLine($"{m.Value}: {m.Name}");
```

## Adding Behavior

Enum Value Objects can have properties and methods:

```csharp
public class OrderState : EnumValueObject<OrderState>
{
    // Name auto-derived from field name
    public static readonly OrderState Draft = new(canModify: true, canCancel: true, isTerminal: false);
    public static readonly OrderState Confirmed = new(canModify: false, canCancel: true, isTerminal: false);
    public static readonly OrderState Shipped = new(canModify: false, canCancel: false, isTerminal: false);
    public static readonly OrderState Delivered = new(canModify: false, canCancel: false, isTerminal: true);
    public static readonly OrderState Cancelled = new(canModify: false, canCancel: false, isTerminal: true);

    public bool CanModify { get; }
    public bool CanCancel { get; }
    public bool IsTerminal { get; }

    private OrderState(bool canModify, bool canCancel, bool isTerminal)
    {
        CanModify = canModify;
        CanCancel = canCancel;
        IsTerminal = isTerminal;
    }
}

// Usage
if (order.State.CanModify)
    order.AddLine(product, quantity);

if (order.State.CanCancel)
    order.Cancel();
```

## State Machine Pattern

Enum Value Objects excel at modeling state machines with valid transitions:

```csharp
public class OrderState : EnumValueObject<OrderState>
{
    // ... members defined above ...

    public IReadOnlyList<OrderState> AllowedTransitions => this switch
    {
        _ when this == Draft => [Confirmed, Cancelled],
        _ when this == Confirmed => [Shipped, Cancelled],
        _ when this == Shipped => [Delivered],
        _ => []  // Terminal states have no transitions
    };

    public bool CanTransitionTo(OrderState newState) =>
        AllowedTransitions.Contains(newState);

    public Result<OrderState> TryTransitionTo(OrderState newState)
    {
        if (CanTransitionTo(newState))
            return newState;

        return Error.Validation(
            $"Cannot transition from '{Name}' to '{newState.Name}'. " +
            $"Allowed: {string.Join(", ", AllowedTransitions.Select(s => s.Name))}");
    }
}
```

## JSON Serialization

### Using JsonConverter Attribute

```csharp
using System.Text.Json.Serialization;

[JsonConverter(typeof(EnumValueObjectJsonConverter<OrderState>))]
public class OrderState : EnumValueObject<OrderState>
{
    // ...
}

// Serializes to: "Confirmed" (the Name)
var json = JsonSerializer.Serialize(OrderState.Confirmed);

// Deserializes from string
var state = JsonSerializer.Deserialize<OrderState>("\"Confirmed\"");
```

### Using Converter Factory

Register once to handle all Enum Value Objects:

```csharp
var options = new JsonSerializerOptions
{
    Converters = { new EnumValueObjectJsonConverterFactory() }
};

// All EnumValueObject types now serialize/deserialize automatically
var json = JsonSerializer.Serialize(order, options);
```

## Entity Framework Core

Store Enum Value Objects using the auto-generated Value property:

```csharp
// In DbContext.OnModelCreating
modelBuilder.Entity<Order>(builder =>
{
    // Store as int using auto-generated Value (0, 1, 2, ...)
    builder.Property(o => o.State)
        .HasConversion(
            state => state.Value,
            value => OrderState.GetAll().First(s => s.Value == value))
        .IsRequired();
});
```

**Note:** The Value is assigned based on field declaration order (0, 1, 2, ...). 
If you reorder fields, database values will change. For existing databases, 
use explicit mapping instead.

## API Reference

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Auto-derived from field name |
| `Value` | `int` | Auto-generated based on declaration order (0, 1, 2, ...) |

### Static Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `GetAll()` | `IReadOnlyCollection<T>` | All defined members |
| `TryFromName(string)` | `Result<T>` | Find by name (case-insensitive) |
| `TryFromName(string, out T)` | `bool` | Try-pattern for name lookup |
| `FromName(string)` | `T` | Find by name, throws if invalid |

### Instance Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Is(params T[])` | `bool` | Check if instance is one of the specified values |
| `IsNot(params T[])` | `bool` | Check if instance is not one of the specified values |

### Operators

- Equality: `==`, `!=`
- Comparison: `<`, `<=`, `>`, `>=` (based on Value)
- Implicit conversion to `string` (returns Name)

## Best Practices

1. **Use private constructor** - Prevent external instantiation
2. **Define members as static readonly** - Ensures single instances
3. **No strings in domain** - Name is auto-derived from field name
4. **Add behavior for domain logic** - Encapsulate rules in the enum
5. **Use TryFromName** - For user input validation
6. **Use FromName** - For known-valid values (tests, constants)
7. **Model state machines** - When values have valid transitions
8. **Use Is() and IsNot()** - For readable membership checks

## See Also

- [Domain-Driven Design](intro.md#domain-driven-design) - Overview of DDD patterns
- [Entity Framework Core Integration](integration-ef.md) - Persistence patterns
- [Clean Architecture](clean-architecture.md) - Using Enum Value Objects in layered applications
