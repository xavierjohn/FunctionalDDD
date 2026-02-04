# Enum Value Objects

Enum Value Objects are type-safe enumerations with behavior, providing a powerful alternative to C# enums for domain modeling.

## Why Enum Value Objects?

C# enums have limitations that can lead to bugs:

```csharp
// ? C# enum problems
public enum OrderStatus { Draft, Confirmed, Shipped }

var status = (OrderStatus)999;  // Valid! No compile or runtime error
var invalid = Enum.Parse<OrderStatus>("Invalid");  // Throws at runtime
```

Enum Value Objects solve these issues:

```csharp
// ? Enum Value Object benefits
public class OrderState : EnumValueObject<OrderState>
{
    public static readonly OrderState Draft = new(1, "Draft");
    public static readonly OrderState Confirmed = new(2, "Confirmed");
    
    private OrderState(int value, string name) : base(value, name) { }
}

// Cannot create invalid values
var result = OrderState.TryFromValue(999);  // Returns Result.Failure
var state = OrderState.TryFromName("Invalid");  // Returns Result.Failure
```

## Basic Usage

### Defining a Enum Value Object

```csharp
using FunctionalDdd;

public class PaymentMethod : EnumValueObject<PaymentMethod>
{
    public static readonly PaymentMethod CreditCard = new(1, "CreditCard");
    public static readonly PaymentMethod DebitCard = new(2, "DebitCard");
    public static readonly PaymentMethod BankTransfer = new(3, "BankTransfer");
    public static readonly PaymentMethod Crypto = new(4, "Crypto");

    private PaymentMethod(int value, string name) : base(value, name) { }
}
```

### Creating Instances

```csharp
// From value (returns Result<T>)
var result = PaymentMethod.TryFromValue(1);
if (result.IsSuccess)
    Console.WriteLine(result.Value.Name);  // "CreditCard"

// From name (case-insensitive)
var result2 = PaymentMethod.TryFromName("creditcard");  // Success

// Direct access when known valid
var method = PaymentMethod.FromValue(1);  // Throws if invalid
var method2 = PaymentMethod.FromName("CreditCard");  // Throws if invalid

// Enumerate all values
foreach (var method in PaymentMethod.GetAll())
    Console.WriteLine($"{method.Value}: {method.Name}");
```

## Adding Behavior

Enum Value Objects can have properties and methods:

```csharp
public class OrderState : EnumValueObject<OrderState>
{
    public static readonly OrderState Draft = new(1, "Draft", 
        canModify: true, canCancel: true, isTerminal: false);
    public static readonly OrderState Confirmed = new(2, "Confirmed", 
        canModify: false, canCancel: true, isTerminal: false);
    public static readonly OrderState Shipped = new(3, "Shipped", 
        canModify: false, canCancel: false, isTerminal: false);
    public static readonly OrderState Delivered = new(4, "Delivered", 
        canModify: false, canCancel: false, isTerminal: true);
    public static readonly OrderState Cancelled = new(5, "Cancelled", 
        canModify: false, canCancel: false, isTerminal: true);

    public bool CanModify { get; }
    public bool CanCancel { get; }
    public bool IsTerminal { get; }

    private OrderState(int value, string name, bool canModify, bool canCancel, bool isTerminal)
        : base(value, name)
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

// Usage in aggregate
public class Order : Aggregate<OrderId>
{
    public OrderState State { get; private set; } = OrderState.Draft;

    public Result<Order> Confirm() =>
        this.ToResult()
            .Ensure(_ => Lines.Count > 0, Error.Validation("Order must have items"))
            .Bind(_ => State.TryTransitionTo(OrderState.Confirmed))
            .Tap(newState => State = newState)
            .Map(_ => this);

    public Result<Order> Ship() =>
        State.TryTransitionTo(OrderState.Shipped)
            .Tap(newState => State = newState)
            .Map(_ => this);
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

// Serializes to: "Confirmed"
var json = JsonSerializer.Serialize(OrderState.Confirmed);

// Deserializes from string or int
var state1 = JsonSerializer.Deserialize<OrderState>("\"Confirmed\"");
var state2 = JsonSerializer.Deserialize<OrderState>("2");
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

### ASP.NET Core Configuration

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new EnumValueObjectJsonConverterFactory());
    });
```

## Entity Framework Core

Store Enum Value Objects as string or int:

```csharp
// In DbContext.OnModelCreating
modelBuilder.Entity<Order>(builder =>
{
    // Store as string (human-readable)
    builder.Property(o => o.State)
        .HasConversion(
            state => state.Name,
            name => OrderState.FromName(name))
        .HasMaxLength(20)
        .IsRequired();

    // Or store as int (efficient)
    builder.Property(o => o.State)
        .HasConversion(
            state => state.Value,
            value => OrderState.FromValue(value))
        .IsRequired();
});
```

## Polymorphic Behavior

For complex behavior, use inheritance:

```csharp
public abstract class PaymentMethod : EnumValueObject<PaymentMethod>
{
    public static readonly PaymentMethod CreditCard = new CreditCardPayment();
    public static readonly PaymentMethod BankTransfer = new BankTransferPayment();
    public static readonly PaymentMethod Crypto = new CryptoPayment();

    private PaymentMethod(int value, string name) : base(value, name) { }

    public abstract decimal CalculateFee(decimal amount);
    public abstract TimeSpan EstimatedProcessingTime { get; }

    private sealed class CreditCardPayment : PaymentMethod
    {
        public CreditCardPayment() : base(1, "CreditCard") { }
        public override decimal CalculateFee(decimal amount) => amount * 0.029m + 0.30m;
        public override TimeSpan EstimatedProcessingTime => TimeSpan.FromSeconds(5);
    }

    private sealed class BankTransferPayment : PaymentMethod
    {
        public BankTransferPayment() : base(2, "BankTransfer") { }
        public override decimal CalculateFee(decimal amount) => 0.50m;
        public override TimeSpan EstimatedProcessingTime => TimeSpan.FromDays(3);
    }

    private sealed class CryptoPayment : PaymentMethod
    {
        public CryptoPayment() : base(3, "Crypto") { }
        public override decimal CalculateFee(decimal amount) => amount * 0.01m;
        public override TimeSpan EstimatedProcessingTime => TimeSpan.FromMinutes(30);
    }
}

// Usage
var fee = PaymentMethod.CreditCard.CalculateFee(100.00m);  // $3.20
var time = PaymentMethod.BankTransfer.EstimatedProcessingTime;  // 3 days
```

## API Reference

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Value` | `int` | Integer value for persistence |
| `Name` | `string` | String name for display |

### Static Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `GetAll()` | `IReadOnlyCollection<T>` | All defined members |
| `TryFromValue(int)` | `Result<T>` | Find by value with validation |
| `TryFromName(string)` | `Result<T>` | Find by name (case-insensitive) |
| `FromValue(int)` | `T` | Find by value, throws if invalid |
| `FromName(string)` | `T` | Find by name, throws if invalid |
| `TryFromValue(int, out T)` | `bool` | Try-pattern for value lookup |
| `TryFromName(string, out T)` | `bool` | Try-pattern for name lookup |

### Operators

- Equality: `==`, `!=`
- Comparison: `<`, `<=`, `>`, `>=`
- Implicit conversion to `int` and `string`

## Best Practices

1. **Use private constructor** - Prevent external instantiation
2. **Define members as `static readonly`** - Ensures single instances
3. **Add behavior for domain logic** - Encapsulate rules in the enum
4. **Use `TryFromValue`/`TryFromName`** - For user input validation
5. **Use `FromValue`/`FromName`** - For known-valid values (tests, constants)
6. **Model state machines** - When values have valid transitions

## See Also

- [Domain-Driven Design](intro.md#domain-driven-design) - Overview of DDD patterns
- [Entity Framework Core Integration](integration-ef.md) - Persistence patterns
- [Clean Architecture](clean-architecture.md) - Using Enum Value Objects in layered applications
