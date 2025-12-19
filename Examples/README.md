# Real-World Examples

This directory contains comprehensive real-world examples demonstrating how to use the FunctionalDDD library in production scenarios. Each example showcases Railway Oriented Programming (ROP), Domain-Driven Design (DDD), and functional programming patterns.

## Available Examples

### 1. 🛒 [E-Commerce Order Processing](./EcommerceExample/)
**Complexity**: 🔵🔵🔵🔵

A complete e-commerce system with order management, payment processing, and inventory control.

**Key Features**:
- Order aggregate with lifecycle management
- Payment processing with retry logic
- Inventory reservation and rollback
- Multi-step workflow orchestration
- Email notifications
- Compensation patterns for failures

**Learn About**:
- Complex domain aggregates
- `Bind`, `Ensure`, `Tap`, `Compensate` in practice
- Async workflows with cancellation tokens
- Parallel validation
- Transaction-like behavior with rollback

**Files**:
- `Aggregates/Order.cs` - Order aggregate root
- `Services/PaymentService.cs` - Payment processing
- `Services/InventoryService.cs` - Stock management
- `Workflows/OrderWorkflow.cs` - Complete order processing flow

---

### 2. 🏦 [Banking Transactions](./BankingExample/)
**Complexity**: 🔵🔵🔵🔵🔵

A banking system with accounts, transfers, fraud detection, and security features.

**Key Features**:
- Account management (checking, savings, money market)
- Fraud detection and pattern analysis
- Daily withdrawal limits
- Overdraft protection
- Interest calculations
- Multi-factor authentication
- Account freeze/unfreeze

**Learn About**:
- Security and fraud prevention
- Complex validation chains
- Parallel fraud detection
- Compensation on security violations
- Audit trail with transaction history
- Status-based state machines

**Files**:
- `Aggregates/BankAccount.cs` - Account aggregate with business rules
- `Services/FraudDetectionService.cs` - Pattern-based fraud detection
- `Workflows/BankingWorkflow.cs` - Secure transaction processing

---

### 3. 👤 [User Management](../SampleUserLibrary/)
**Complexity**: 🔵🔵

Basic user registration and validation with FluentValidation integration.

**Key Features**:
- User aggregate with validation
- Email, FirstName, LastName value objects
- Password complexity requirements
- FluentValidation integration

**Learn About**:
- Basic aggregates and value objects
- FluentValidation with Result pattern
- Simple validation workflows
- Type safety vs primitive obsession

**Files**:
- `Aggregate/User.cs` - User aggregate with validator
- `ValueObject/*.cs` - Value objects for user properties

---

### 4. 🧪 [Unit Test Examples](../Xunit/)
**Complexity**: 🔵🔵

Comprehensive test examples using xUnit.

**Key Features**:
- ValidationExample - Multi-field validation
- AsyncUsageExamples - Async ROP patterns
- MaybeExamples - Optional value handling

**Learn About**:
- Testing ROP workflows
- Async test patterns
- Maybe type usage
- Validation error testing

**Files**:
- `ValidationExample.cs` - Combine and validation testing
- `AsyncUsageExamples.cs` - Async workflow testing
- `MaybeExamples.cs` - Optional value testing

---

### 5. 🌐 [Web API Examples](../SampleWebApplication/)
**Complexity**: 🔵🔵🔵

ASP.NET Core MVC examples showing how to integrate ROP with web APIs.

**Key Features**:
- Controller actions returning Result
- Automatic error-to-HTTP status mapping
- Validation error responses
- Created/NotFound responses

**Learn About**:
- `ToActionResult()` extension
- BadRequest with validation details
- HTTP status code mapping
- API layer integration

**Files**:
- `Controllers/UsersController.cs` - MVC controller with ROP

---

### 6. ⚡ [Minimal API Examples](../SampleMinimalApi/)
**Complexity**: 🔵🔵🔵

ASP.NET Core Minimal API examples with ROP integration.

**Key Features**:
- Minimal API endpoints with Result
- User registration and retrieval
- Todo list management
- HTTP result mapping

**Learn About**:
- `ToHttpResult()` extension
- Minimal API with functional patterns
- Route organization
- Results vs ActionResult

**Files**:
- `API/UserRoutes.cs` - User endpoints
- `API/ToDoRoutes.cs` - Todo endpoints

---

## Common Patterns Across Examples

### 1. **Validation Chain Pattern**
```csharp
EmailAddress.TryCreate(email)
    .Combine(FirstName.TryCreate(firstName))
    .Combine(LastName.TryCreate(lastName))
    .Bind((e, f, l) => User.Create(e, f, l))
```

**Used In**: All examples
**Purpose**: Validate multiple inputs independently and collect all errors

---

### 2. **Async Workflow Pattern**
```csharp
await GetCustomerAsync(id)
    .ToResultAsync(Error.NotFound("Customer not found"))
    .EnsureAsync(c => c.IsActive, Error.Validation("Inactive"))
    .TapAsync(c => LogAccessAsync(c))
    .BindAsync(c => ProcessAsync(c))
```

**Used In**: EcommerceExample, BankingExample
**Purpose**: Chain async operations with error handling

---

### 3. **Compensation Pattern**
```csharp
.Compensate(
    predicate: error => error is UnexpectedError,
    func: () => RetryOperation()
)
```

**Used In**: EcommerceExample (payment retry), BankingExample (account freeze)
**Purpose**: Provide fallback behavior or cleanup on specific errors

---

### 4. **Parallel Validation Pattern**
```csharp
var tasks = items.Select(item => ValidateAsync(item));
var results = await Task.WhenAll(tasks);
// Check results and combine errors
```

**Used In**: EcommerceExample (order lines), BankingExample (transfer validation)
**Purpose**: Validate multiple items concurrently for performance

---

### 5. **Aggregate Pattern with Status**
```csharp
public Result<Order> Submit()
{
    return this.ToResult()
        .Ensure(_ => Status == OrderStatus.Draft, Error.Validation("Cannot submit"))
        .Ensure(_ => Lines.Count > 0, Error.Validation("Empty order"))
        .Tap(_ => Status = OrderStatus.Pending)
        .Map(_ => this);
}
```

**Used In**: EcommerceExample (Order), BankingExample (BankAccount)
**Purpose**: State machine with validation at each transition

---

## Complexity Guide

- 🔵 **Beginner**: Basic ROP concepts, simple validation
- 🔵🔵 **Intermediate**: Value objects, aggregates, basic workflows
- 🔵🔵🔵 **Advanced**: Async patterns, API integration, multiple services
- 🔵🔵🔵🔵 **Expert**: Complex workflows, compensation, parallel operations
- 🔵🔵🔵🔵🔵 **Master**: Security, fraud detection, advanced state management

## Learning Path

### Step 1: Foundation (🔵)
Start with unit test examples to understand basic patterns:
1. Read `ValidationExample.cs` - Learn `Combine` and `Bind`
2. Explore `MaybeExamples.cs` - Understand optional values

### Step 2: Domain Modeling (🔵🔵)
Move to user management:
1. Study `User.cs` aggregate
2. See how FluentValidation integrates
3. Understand value objects

### Step 3: API Integration (🔵🔵🔵)
Learn web integration:
1. Review `UsersController.cs` for MVC pattern
2. Explore Minimal API examples
3. Understand HTTP status mapping

### Step 4: Complex Workflows (🔵🔵🔵🔵)
Dive into e-commerce:
1. Study `Order.cs` aggregate
2. Review `OrderWorkflow.cs` for orchestration
3. Learn compensation patterns

### Step 5: Advanced Features (🔵🔵🔵🔵🔵)
Master banking example:
1. Analyze `BankAccount.cs` state machine
2. Study `FraudDetectionService.cs`
3. Learn parallel validation
4. Understand security patterns

## Running the Examples

### From Console Application
Both E-commerce and Banking examples are standalone console applications:

**Banking Example:**
```bash
cd Examples/BankingExample
dotnet run
```

**E-commerce Example:**
```bash
cd Examples/EcommerceExample
dotnet run
```

### From Visual Studio
1. Right-click the example project (`BankingExample` or `EcommerceExample`)
2. Select "Set as Startup Project"
3. Press F5 or click "Start Debugging"
4. Watch the console output

### From Unit Tests
All examples include test coverage. Run:
```bash
dotnet test
```

### From Web API
Start the web applications:
```bash
cd Examples/SampleWebApplication/src
dotnet run

# Or for Minimal API
cd Examples/SampleMinimalApi
dotnet run
```

### Run Specific Examples

Modify the `Program.cs` in each example project:

**Banking:**
```csharp
using BankingExample;

// Run all examples
await BankingExamples.RunExamplesAsync();

// Or run specific example
await BankingExamples.Example3_FraudDetection();
```

**E-commerce:**
```csharp
using EcommerceExample;

// Run all examples
await EcommerceExamples.RunExamplesAsync();

// Or run specific example
await EcommerceExamples.Example2_CompleteOrderWorkflow();
```

## Key Takeaways

### 1. **Type Safety First**
Every example uses value objects to prevent primitive obsession:
- `OrderId`, `AccountId`, `CustomerId` - Prevent ID confusion
- `Money` - Prevent decimal arithmetic errors
- `EmailAddress`, `FirstName`, `LastName` - Ensure valid data

### 2. **Explicit Error Handling**
No hidden exceptions or null checks:
- All failures return typed `Error` objects
- Errors aggregate when using `Combine`
- Compensation handles specific error types

### 3. **Composable Workflows**
Build complex operations from simple parts:
- Each method does one thing
- Chain with `Bind`, `Ensure`, `Tap`
- Test components independently

### 4. **Async Throughout**
All I/O is async with cancellation support:
- `BindAsync`, `EnsureAsync`, `TapAsync`
- Proper cancellation token propagation
- Parallel operations where beneficial

### 5. **Business Rules in Code**
Domain rules are explicit and enforced:
- Status transitions validated
- Limits checked before operations
- Audit trails automatically maintained

## Additional Resources

- [Railway Oriented Programming README](../../RailwayOrientedProgramming/README.md)
- [DDD Basics](../../docs/docfx_project/articles/basics.md)
- [API Integration Guide](../../Asp/README.md)
- [Value Objects Guide](../../CommonValueObjects/README.md)

## Contributing Examples

Want to add a new example? Follow this structure:


## Questions?

- Check the [main documentation](../../README.md)
- Review [examples article](../../docs/docfx_project/articles/examples.md)
- Open an issue on GitHub
