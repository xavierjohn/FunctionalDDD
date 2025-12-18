# Common Value Objects - Comprehensive Examples

This document provides detailed examples and patterns for using Common Value Objects with source code generation in domain-driven design applications.

## Table of Contents

- [RequiredString Examples](#requiredstring-examples)
  - [Basic Usage](#basic-usage)
  - [Custom Validation](#custom-validation)
  - [Integration with Entities](#integration-with-entities)
- [RequiredGuid Examples](#requiredguid-examples)
  - [Identity Value Objects](#identity-value-objects)
  - [Aggregate IDs](#aggregate-ids)
  - [Parsing and Conversion](#parsing-and-conversion)
- [EmailAddress Examples](#emailaddress-examples)
  - [User Registration](#user-registration)
  - [Email Validation](#email-validation)
- [Generated Code Deep Dive](#generated-code-deep-dive)
  - [RequiredString Generation](#requiredstring-generation)
  - [RequiredGuid Generation](#requiredguid-generation)
  - [IParsable Implementation](#iparsable-implementation)
- [Advanced Patterns](#advanced-patterns)
  - [Combining Value Objects](#combining-value-objects)
  - [Value Object Hierarchies](#value-object-hierarchies)
  - [Custom Error Messages](#custom-error-messages)
- [Integration Examples](#integration-examples)
  - [Entity Framework Core](#entity-framework-core)
  - [ASP.NET Core](#aspnet-core)
  - [JSON Serialization](#json-serialization)

## RequiredString Examples

### Basic Usage

Creating strongly-typed string identifiers:

```csharp
// Define value objects using source code generation
public partial class OrderNumber : RequiredString
{
}

public partial class ProductSKU : RequiredString
{
}

public partial class CustomerName : RequiredString
{
}

// Usage
public class OrderService
{
    public Result<Order> CreateOrder(string orderNumber, string productSKU, string customerName)
    {
        // Validate all strings using TryCreate
        return OrderNumber.TryCreate(orderNumber)
            .Combine(ProductSKU.TryCreate(productSKU))
            .Combine(CustomerName.TryCreate(customerName))
            .Bind((orderNum, sku, custName) =>
                Order.TryCreate(orderNum, sku, custName));
    }
}

// Explicit casting for convenience (throws on failure)
public void ProcessOrder()
{
    var orderNumber = (OrderNumber)"ORD-2024-001";
    var productSKU = (ProductSKU)"PROD-SKU-123";
    var customerName = (CustomerName)"John Doe";
    
    // Use in domain logic
    var order = CreateOrder(orderNumber, productSKU, customerName);
}

// Safe parsing
public void SafeProcessing(string input)
{
    var result = OrderNumber.TryCreate(input);
    
    if (result.IsSuccess)
    {
        Console.WriteLine($"Valid order number: {result.Value}");
    }
    else
    {
        Console.WriteLine($"Invalid: {result.Error.Message}");
        // Output: "Order Number cannot be empty."
    }
}

// IParsable support
public void ParseFromString()
{
    // Parse method (throws FormatException on failure)
    var orderNumber = OrderNumber.Parse("ORD-2024-001", null);
    
    // TryParse method (returns bool)
    if (OrderNumber.TryParse("ORD-2024-001", null, out var result))
    {
        Console.WriteLine($"Parsed: {result}");
    }
}
```

### Custom Validation

Extending RequiredString with custom validation:

```csharp
// While RequiredString only validates non-empty,
// you can add additional validation in your factory methods

public partial class PhoneNumber : RequiredString
{
    // Additional validation logic
    public new static Result<PhoneNumber> TryCreate(string? value) =>
        RequiredString.TryCreate(value)
            .Ensure(
                phone => phone.Length >= 10,
                Error.Validation("Phone number must be at least 10 digits", "phoneNumber"))
            .Ensure(
                phone => phone.All(c => char.IsDigit(c) || c == '-' || c == ' '),
                Error.Validation("Phone number can only contain digits, dashes, and spaces", "phoneNumber"))
            .Map(phone => (PhoneNumber)(object)phone);
}

public partial class ZipCode : RequiredString
{
    public new static Result<ZipCode> TryCreate(string? value) =>
        RequiredString.TryCreate(value)
            .Ensure(
                zip => zip.Length == 5 || zip.Length == 10,
                Error.Validation("Zip code must be 5 or 10 characters (with dash)", "zipCode"))
            .Ensure(
                zip => Regex.IsMatch(zip, @"^\d{5}(-\d{4})?$"),
                Error.Validation("Invalid zip code format (use 12345 or 12345-6789)", "zipCode"))
            .Map(zip => (ZipCode)(object)zip);
}

public partial class ProductCode : RequiredString
{
    public new static Result<ProductCode> TryCreate(string? value) =>
        RequiredString.TryCreate(value)
            .Ensure(
                code => code.Length >= 3 && code.Length <= 20,
                Error.Validation("Product code must be between 3 and 20 characters", "productCode"))
            .Ensure(
                code => Regex.IsMatch(code, @"^[A-Z0-9-]+$"),
                Error.Validation("Product code can only contain uppercase letters, digits, and dashes", "productCode"))
            .Map(code => (ProductCode)(object)code);
}

// Usage
public void ValidateCustom()
{
    // Phone validation
    var phone = PhoneNumber.TryCreate("555-1234");
    // Error: "Phone number must be at least 10 digits"
    
    var validPhone = PhoneNumber.TryCreate("555-123-4567");
    // Success
    
    // Zip code validation
    var zip = ZipCode.TryCreate("12345").Value;
    var zipPlus4 = ZipCode.TryCreate("12345-6789").Value;
    
    // Product code validation
    var productCode = ProductCode.TryCreate("PROD-ABC-123").Value;
    var invalid = ProductCode.TryCreate("prod-abc");
    // Error: "Product code can only contain uppercase letters, digits, and dashes"
}
```

### Integration with Entities

Using RequiredString value objects in entities:

```csharp
public partial class OrderId : RequiredGuid
{
}

public partial class OrderNumber : RequiredString
{
}

public partial class CustomerName : RequiredString
{
}

public partial class ProductSKU : RequiredString
{
}

public class Order : Entity<OrderId>
{
    public OrderNumber OrderNumber { get; }
    public CustomerName CustomerName { get; }
    public ProductSKU ProductSKU { get; }
    public DateTime OrderDate { get; }
    
    private Order(
        OrderId id,
        OrderNumber orderNumber,
        CustomerName customerName,
        ProductSKU productSKU)
        : base(id)
    {
        OrderNumber = orderNumber;
        CustomerName = customerName;
        ProductSKU = productSKU;
        OrderDate = DateTime.UtcNow;
    }
    
    public static Result<Order> TryCreate(
        OrderNumber orderNumber,
        CustomerName customerName,
        ProductSKU productSKU) =>
        new Order(
            OrderId.NewUnique(),
            orderNumber,
            customerName,
            productSKU).ToResult();
    
    // Factory from strings
    public static Result<Order> TryCreate(
        string orderNumber,
        string customerName,
        string productSKU) =>
        OrderNumber.TryCreate(orderNumber)
            .Combine(CustomerName.TryCreate(customerName))
            .Combine(ProductSKU.TryCreate(productSKU))
            .Bind((orderNum, custName, sku) =>
                TryCreate(orderNum, custName, sku));
}
```

## RequiredGuid Examples

### Identity Value Objects

Creating strongly-typed identity value objects:

```csharp
public partial class UserId : RequiredGuid
{
}

public partial class OrderId : RequiredGuid
{
}

public partial class ProductId : RequiredGuid
{
}

public partial class CustomerId : RequiredGuid
{
}

// Usage
public class User : Entity<UserId>
{
    public EmailAddress Email { get; }
    public string FirstName { get; }
    public string LastName { get; }
    
    private User(UserId id, EmailAddress email, string firstName, string lastName)
        : base(id)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }
    
    public static Result<User> TryCreate(
        EmailAddress email,
        string firstName,
        string lastName) =>
        new User(UserId.NewUnique(), email, firstName, lastName).ToResult();
}

// Creating new IDs
public void CreateNewIdentities()
{
    var userId = UserId.NewUnique();
    var orderId = OrderId.NewUnique();
    var productId = ProductId.NewUnique();
    
    Console.WriteLine($"User ID: {userId}");
    // Output: User ID: 550e8400-e29b-41d4-a716-446655440000
}

// Parsing from Guid
public void ParseFromGuid()
{
    var guid = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
    
    var result = UserId.TryCreate(guid);
    if (result.IsSuccess)
    {
        var userId = result.Value;
        Console.WriteLine($"User ID: {userId}");
    }
    
    // Explicit cast
    var userId2 = (UserId)guid;
}

// Parsing from string
public void ParseFromString()
{
    var result = UserId.TryCreate("550e8400-e29b-41d4-a716-446655440000");
    
    if (result.IsSuccess)
    {
        var userId = result.Value;
        Console.WriteLine($"User ID: {userId}");
    }
    else
    {
        Console.WriteLine($"Error: {result.Error.Message}");
    }
    
    // IParsable support
    var userId2 = UserId.Parse("550e8400-e29b-41d4-a716-446655440000", null);
    
    if (UserId.TryParse("550e8400-e29b-41d4-a716-446655440000", null, out var userId3))
    {
        Console.WriteLine($"Parsed: {userId3}");
    }
}
```

### Aggregate IDs

Using RequiredGuid for aggregate root identities:

```csharp
public partial class OrderId : RequiredGuid
{
}

public partial class OrderLineId : RequiredGuid
{
}

public class Order : Aggregate<OrderId>
{
    private readonly List<OrderLine> _lines = new();
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    
    private Order(OrderId id) : base(id)
    {
    }
    
    public static Result<Order> TryCreate() =>
        new Order(OrderId.NewUnique()).ToResult();
    
    public Result<Order> AddLine(ProductId productId, int quantity, decimal price)
    {
        var line = new OrderLine(OrderLineId.NewUnique(), productId, quantity, price);
        _lines.Add(line);
        return this.ToResult();
    }
}

public class OrderLine : Entity<OrderLineId>
{
    public ProductId ProductId { get; }
    public int Quantity { get; }
    public decimal Price { get; }
    
    internal OrderLine(OrderLineId id, ProductId productId, int quantity, decimal price)
        : base(id)
    {
        ProductId = productId;
        Quantity = quantity;
        Price = price;
    }
}
```

### Parsing and Conversion

Comprehensive parsing and conversion examples:

```csharp
public partial class EntityId : RequiredGuid
{
}

public class EntityService
{
    // Parsing from various sources
    public Result<EntityId> ParseFromString(string input)
    {
        // Method 1: TryCreate (returns Result<T>)
        var result1 = EntityId.TryCreate(input);
        
        // Method 2: Parse (throws on failure)
        try
        {
            var result2 = EntityId.Parse(input, null);
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Parse failed: {ex.Message}");
        }
        
        // Method 3: TryParse (returns bool)
        if (EntityId.TryParse(input, null, out var result3))
        {
            return result3.ToResult();
        }
        
        return Error.Validation("Invalid entity ID", "entityId");
    }
    
    // Parsing from Guid
    public Result<EntityId> ParseFromGuid(Guid guid)
    {
        // TryCreate validates that Guid is not empty
        return EntityId.TryCreate(guid);
    }
    
    // Validation scenarios
    public void ValidationScenarios()
    {
        // Empty string fails
        var result1 = EntityId.TryCreate("");
        // Error: "Entity Id cannot be empty."
        
        // Invalid GUID format fails
        var result2 = EntityId.TryCreate("not-a-guid");
        // Error: "Guid should contain 32 digits with 4 dashes..."
        
        // Empty GUID fails
        var result3 = EntityId.TryCreate(Guid.Empty);
        // Error: "Entity Id cannot be empty."
        
        // Valid GUID succeeds
        var result4 = EntityId.TryCreate("550e8400-e29b-41d4-a716-446655440000");
        // Success
        
        // NewUnique always succeeds
        var result5 = EntityId.NewUnique();
        // Success: new unique GUID
    }
}
```

## EmailAddress Examples

### User Registration

Using EmailAddress in user registration:

```csharp
public class User : Entity<UserId>
{
    public EmailAddress Email { get; private set; }
    public string FirstName { get; }
    public string LastName { get; }
    
    private User(UserId id, EmailAddress email, string firstName, string lastName)
        : base(id)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }
    
    public static Result<User> TryCreate(
        string email,
        string firstName,
        string lastName) =>
        EmailAddress.TryCreate(email)
            .Bind(validEmail =>
                new User(UserId.NewUnique(), validEmail, firstName, lastName).ToResult());
    
    public Result<User> UpdateEmail(string newEmail) =>
        EmailAddress.TryCreate(newEmail)
            .Map(validEmail =>
            {
                Email = validEmail;
                return this;
            });
}

public class UserService
{
    private readonly IUserRepository _repository;
    
    public async Task<Result<User>> RegisterUserAsync(
        string email,
        string firstName,
        string lastName,
        CancellationToken cancellationToken)
    {
        return await EmailAddress.TryCreate(email)
            .EnsureAsync(
                async (e, ct) => !await _repository.EmailExistsAsync(e, ct),
                Error.Conflict("Email is already registered"),
                cancellationToken)
            .BindAsync(
                async (validEmail, ct) =>
                    await User.TryCreate(validEmail.Value, firstName, lastName)
                        .TapAsync(
                            async (user, innerCt) => await _repository.AddAsync(user, innerCt),
                            ct),
                cancellationToken);
    }
}
```

### Email Validation

Comprehensive email validation examples:

```csharp
public class EmailValidator
{
    public void ValidateEmails()
    {
        // Valid emails
        var email1 = EmailAddress.TryCreate("user@example.com");
        // Success
        
        var email2 = EmailAddress.TryCreate("john.doe+tag@company.co.uk");
        // Success - supports plus addressing and subdomains
        
        var email3 = EmailAddress.TryCreate("admin@localhost");
        // Success - supports local domains
        
        // Invalid emails
        var invalid1 = EmailAddress.TryCreate("not-an-email");
        // Error: "Email address is not valid."
        
        var invalid2 = EmailAddress.TryCreate("user@");
        // Error: "Email address is not valid."
        
        var invalid3 = EmailAddress.TryCreate("@example.com");
        // Error: "Email address is not valid."
        
        var invalid4 = EmailAddress.TryCreate("");
        // Error: "Email address is not valid."
        
        var invalid5 = EmailAddress.TryCreate(null);
        // Error: "Email address is not valid."
    }
    
    // Email comparison (case-insensitive)
    public void CompareEmails()
    {
        var email1 = EmailAddress.TryCreate("User@Example.COM").Value;
        var email2 = EmailAddress.TryCreate("user@example.com").Value;
        
        // EmailAddress uses case-insensitive comparison
        var areEqual = email1.Equals(email2); // true
    }
    
    // Using in Combine operations
    public Result<User> CreateUserWithMultipleEmails(
        string primaryEmail,
        string alternateEmail)
    {
        return EmailAddress.TryCreate(primaryEmail)
            .Combine(EmailAddress.TryCreate(alternateEmail))
            .Bind((primary, alternate) =>
                User.TryCreate(primary, alternate));
    }
}
```

## Generated Code Deep Dive

### RequiredString Generation

Understanding what gets generated:

```csharp
// Your declaration
public partial class TrackingId : RequiredString
{
}

// Generated code by source generator
public partial class TrackingId : RequiredString, IParsable<TrackingId>
{
    // Error message using class name
    protected static readonly Error CannotBeEmptyError = 
        Error.Validation("Tracking Id cannot be empty.", "trackingId");

    // Private constructor
    private TrackingId(string value) : base(value)
    {
    }

    // Explicit cast operator (throws on failure)
    public static explicit operator TrackingId(string trackingId) => 
        TryCreate(trackingId).Value;

    // IParsable<T>.Parse implementation
    public static TrackingId Parse(string s, IFormatProvider? provider)
    {
        var r = TryCreate(s);
        if (r.IsFailure)
            throw new FormatException(r.Error.Message);
        return r.Value;
    }

    // IParsable<T>.TryParse implementation
    public static bool TryParse(
        [NotNullWhen(true)] string? s, 
        IFormatProvider? provider, 
        [MaybeNullWhen(false)] out TrackingId result)
    {
        var r = TryCreate(s);
        if (r.IsFailure)
        {
            result = default;
            return false;
        }

        result = r.Value;
        return true;
    }

    // TryCreate method returning Result<T>
    public static Result<TrackingId> TryCreate(string? requiredStringOrNothing) =>
        requiredStringOrNothing
            .EnsureNotNullOrWhiteSpace(CannotBeEmptyError)
            .Map(str => new TrackingId(str));
}
```

### RequiredGuid Generation

Understanding GUID generation:

```csharp
// Your declaration
public partial class EmployeeId : RequiredGuid
{
}

// Generated code by source generator
public partial class EmployeeId : RequiredGuid, IParsable<EmployeeId>
{
    // Error message using class name
    protected static readonly Error CannotBeEmptyError = 
        Error.Validation("Employee Id cannot be empty.", "employeeId");

    // Private constructor
    private EmployeeId(Guid value) : base(value)
    {
    }

    // Explicit cast operator (throws on failure)
    public static explicit operator EmployeeId(Guid employeeId) => 
        TryCreate(employeeId).Value;

    // IParsable<T>.Parse implementation
    public static EmployeeId Parse(string s, IFormatProvider? provider)
    {
        var r = TryCreate(s);
        if (r.IsFailure)
            throw new FormatException(r.Error.Message);
        return r.Value;
    }

    // IParsable<T>.TryParse implementation
    public static bool TryParse(
        [NotNullWhen(true)] string? s, 
        IFormatProvider? provider, 
        [MaybeNullWhen(false)] out EmployeeId result)
    {
        var r = TryCreate(s);
        if (r.IsFailure)
        {
            result = default;
            return false;
        }

        result = r.Value;
        return true;
    }

    // Create new unique ID
    public static EmployeeId NewUnique() => new(Guid.NewGuid());

    // TryCreate from Guid
    public static Result<EmployeeId> TryCreate(Guid? requiredGuidOrNothing) =>
        requiredGuidOrNothing
            .ToResult(CannotBeEmptyError)
            .Ensure(x => x != Guid.Empty, CannotBeEmptyError)
            .Map(guid => new EmployeeId(guid));

    // TryCreate from string (parses to Guid first)
    public static Result<EmployeeId> TryCreate(string? stringOrNull)
    {
        Guid parsedGuid = Guid.Empty;
        return stringOrNull
            .ToResult(CannotBeEmptyError)
            .Ensure(
                x => Guid.TryParse(x, out parsedGuid), 
                Error.Validation(
                    "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)", 
                    "employeeId"))
            .Ensure(_ => parsedGuid != Guid.Empty, CannotBeEmptyError)
            .Map(guid => new EmployeeId(parsedGuid));
    }
}
```

### IParsable Implementation

Using IParsable<T> interface:

```csharp
public partial class ProductId : RequiredGuid
{
}

public class ProductService
{
    // Generic parsing using IParsable
    public T ParseGeneric<T>(string input) where T : IParsable<T>
    {
        return T.Parse(input, null);
    }
    
    // Usage
    public void UseGenericParsing()
    {
        var productId = ParseGeneric<ProductId>("550e8400-e29b-41d4-a716-446655440000");
        Console.WriteLine($"Parsed product ID: {productId}");
    }
    
    // TryParse pattern
    public bool TryParseProductId(string input, out ProductId? result)
    {
        return ProductId.TryParse(input, null, out result);
    }
    
    // Using in ASP.NET Core model binding
    [HttpGet("{id}")]
    public ActionResult<Product> GetProduct(ProductId id)
    {
        // Model binder automatically uses IParsable<ProductId>
        var product = _repository.GetById(id);
        return product.ToActionResult(this);
    }
}
```

## Advanced Patterns

### Combining Value Objects

Creating complex validation chains:

```csharp
public partial class FirstName : RequiredString
{
}

public partial class LastName : RequiredString
{
}

public partial class UserId : RequiredGuid
{
}

public class CreateUserRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}

public class UserService
{
    public Result<User> CreateUser(CreateUserRequest request)
    {
        // Combine multiple value object validations
        return FirstName.TryCreate(request.FirstName)
            .Combine(LastName.TryCreate(request.LastName))
            .Combine(EmailAddress.TryCreate(request.Email))
            .Bind((firstName, lastName, email) =>
                User.TryCreate(
                    UserId.NewUnique(),
                    firstName,
                    lastName,
                    email));
    }
    
    // Async version
    public async Task<Result<User>> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        return await FirstName.TryCreate(request.FirstName)
            .Combine(LastName.TryCreate(request.LastName))
            .Combine(EmailAddress.TryCreate(request.Email))
            .BindAsync(
                async (firstName, lastName, email, ct) =>
                    await User.TryCreateAsync(
                        UserId.NewUnique(),
                        firstName,
                        lastName,
                        email,
                        ct),
                cancellationToken);
    }
}
```

### Value Object Hierarchies

Creating domain-specific value object hierarchies:

```csharp
// Base identifiers
public partial class EntityId : RequiredGuid
{
}

// Specific identifiers inherit behavior
public partial class UserId : EntityId
{
}

public partial class OrderId : EntityId
{
}

public partial class ProductId : EntityId
{
}

// String-based identifiers
public partial class Code : RequiredString
{
}

public partial class ProductCode : Code
{
}

public partial class OrderNumber : Code
{
}

public partial class TrackingNumber : Code
{
}

// Usage demonstrates type safety
public class Order : Aggregate<OrderId>
{
    public OrderNumber OrderNumber { get; }
    public TrackingNumber TrackingNumber { get; private set; }
    
    public Result<Order> AssignTracking(TrackingNumber trackingNumber)
    {
        TrackingNumber = trackingNumber;
        return this.ToResult();
    }
    
    // Compile-time type safety prevents mixing IDs
    public Result<Order> AddProduct(ProductId productId, int quantity)
    {
        // Cannot pass OrderId where ProductId is expected
        // Cannot pass UserId where ProductId is expected
        return this.ToResult();
    }
}
```

### Custom Error Messages

Customizing error messages:

```csharp
// Override error message by shadowing the field
public partial class CustomerId : RequiredGuid
{
    // Shadow the generated error with custom message
    protected new static readonly Error CannotBeEmptyError = 
        Error.Validation(
            "A valid customer identifier is required for this operation. Please provide a non-empty GUID.",
            "customerId");
}

public partial class EmailAddress : RequiredString
{
    // EmailAddress already has its own validation, but you can extend it
    public new static Result<EmailAddress> TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation(
                "Email address is required. Please provide a valid email address.",
                "email");
        
        // Use the existing EmailAddress validation
        return FunctionalDDD.CommonValueObjects.EmailAddress.TryCreate(value)
            .Map(email => (EmailAddress)(object)email);
    }
}
```

## Integration Examples

### Entity Framework Core

Configuring value objects with EF Core:

```csharp
public partial class UserId : RequiredGuid
{
}

public partial class OrderId : RequiredGuid
{
}

public class User : Entity<UserId>
{
    public EmailAddress Email { get; private set; }
    public string FirstName { get; }
    public string LastName { get; }
    
    // EF Core requires parameterless constructor
    private User() : base(UserId.NewUnique())
    {
    }
    
    private User(UserId id, EmailAddress email, string firstName, string lastName)
        : base(id)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }
}

// EF Core configuration
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        // Convert UserId to Guid for database
        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value, // To database
                guid => (UserId)guid) // From database
            .IsRequired();
        
        // Convert EmailAddress to string for database
        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                str => EmailAddress.TryCreate(str).Value)
            .HasMaxLength(100)
            .IsRequired();
    }
}
```

### ASP.NET Core

Using value objects in ASP.NET Core:

```csharp
// Model binding automatically uses IParsable<T>
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repository;
    
    // UserId automatically parsed from route parameter
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUserAsync(
        UserId id,
        CancellationToken cancellationToken) =>
        await _repository.GetByIdAsync(id, cancellationToken)
            .ToResultAsync(Error.NotFound($"User {id} not found"))
            .ToActionResultAsync(this);
    
    // Request body with value objects
    public record CreateUserRequest(
        string Email,
        string FirstName,
        string LastName
    );
    
    [HttpPost]
    public async Task<ActionResult<User>> CreateUserAsync(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken) =>
        await EmailAddress.TryCreate(request.Email)
            .BindAsync(
                async (email, ct) =>
                    await User.TryCreateAsync(email, request.FirstName, request.LastName, ct),
                cancellationToken)
            .TapAsync(
                async (user, ct) => await _repository.AddAsync(user, ct),
                cancellationToken)
            .ToActionResultAsync(this);
}
```

### JSON Serialization

Configuring JSON serialization:

```csharp
public partial class UserId : RequiredGuid
{
}

// System.Text.Json converter
public class RequiredGuidConverter<T> : JsonConverter<T> where T : RequiredGuid
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var guid = reader.GetGuid();
        var method = typeof(T).GetMethod("TryCreate", new[] { typeof(Guid?) });
        var result = method?.Invoke(null, new object[] { guid }) as Result<T>;
        
        if (result?.IsSuccess ?? false)
            return result.Value;
        
        throw new JsonException($"Invalid {typeof(T).Name}");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}

// Registration
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new RequiredGuidConverter<UserId>());
                options.JsonSerializerOptions.Converters.Add(new RequiredGuidConverter<OrderId>());
            });
    }
}

// Alternatively, use ToString()/Parse pattern
public record UserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName
)
{
    public static UserDto FromUser(User user) =>
        new(
            user.Id.Value.ToString(),
            user.Email.Value,
            user.FirstName,
            user.LastName);
    
    public Result<User> ToUser() =>
        UserId.TryCreate(Id)
            .Combine(EmailAddress.TryCreate(Email))
            .Bind((userId, email) =>
                User.TryCreate(userId, email, FirstName, LastName));
}
```
