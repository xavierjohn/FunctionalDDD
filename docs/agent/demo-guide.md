# ?? FunctionalDDD Clean Architecture Agent - Demo Guide

This guide demonstrates how to use GitHub Copilot to automatically scaffold complete .NET clean architecture projects using FunctionalDDD best practices.

## ? **The Jaw-Dropping Demo**

Watch as GitHub Copilot transforms a simple YAML specification into a complete, production-ready .NET application in minutes!

### **Demo Scenario: E-Commerce Order System**

We'll create a complete e-commerce order management system with:
- ?? 3 aggregates (Order, Customer, Product)
- ?? 6 value objects with validation
- ?? 4 queries
- ?? 4 commands
- ?? 3 API controllers with versioning
- ? Comprehensive tests
- ?? OpenTelemetry observability
- ?? Swagger documentation

**Time to complete**: ~5 minutes ??

---

## ?? **Demo Steps**

### **Step 1: Create New Repository** (30 seconds)

```bash
# Create new repository on GitHub
gh repo create demo-ecommerce --public --description "E-commerce system with FunctionalDDD"

# Clone it locally
gh repo clone demo-ecommerce
cd demo-ecommerce
```

### **Step 2: Add Project Specification** (2 minutes)

1. Create `.github/project-spec.yml`:

```bash
mkdir -p .github
```

2. Copy the demo specification:

```yaml
# See .github/project-spec-demo.yml for complete example
project:
  name: ECommerceOrderSystem
  namespace: DemoCompany.ECommerce
  description: E-commerce order management system

domain:
  aggregates:
    - name: Order
      id: OrderId
      properties:
        - name: CustomerId
          type: CustomerId
        - name: Status
          type: OrderStatus
          enum: [Draft, Submitted, Processing, Shipped]
      behaviors:
        - name: Submit
          validates: Status == Draft
    
    - name: Customer
      id: CustomerId
      # ... more properties
  
  valueObjects:
    - name: OrderId
      type: RequiredGuid
    - name: Money
      type: ScalarValueObject<decimal>

application:
  queries:
    - name: GetOrderById
      parameters:
        - orderId: OrderId
      returns: Order
  
  commands:
    - name: CreateOrder
      returns: Order

api:
  version: "2024-01-15"
  endpoints:
    - resource: orders
      operations: [GET, POST]
```

3. Commit and push:

```bash
git add .github/project-spec.yml
git commit -m "Add project specification"
git push
```

### **Step 3: Create GitHub Issue** (30 seconds)

Create an issue with the `copilot-scaffold` label:

```bash
gh issue create \
  --title "Scaffold E-Commerce Order System" \
  --label "copilot-scaffold" \
  --body "Please scaffold the E-Commerce Order System based on .github/project-spec.yml"
```

### **Step 4: Watch the Magic! ?** (3-5 minutes)

GitHub Copilot will automatically:

1. **Read the specification** from `.github/project-spec.yml`
2. **Create a new branch** `copilot-scaffold/{issue-number}-ECommerceOrderSystem`
3. **Generate complete project**:
   ```
   ECommerceOrderSystem/
   ??? Domain/
   ?   ??? src/
   ?   ?   ??? Aggregates/
   ?   ?   ?   ??? Order.cs
   ?   ?   ?   ??? Customer.cs
   ?   ?   ?   ??? Product.cs
   ?   ?   ??? ValueObjects/
   ?   ?   ?   ??? OrderId.cs
   ?   ?   ?   ??? CustomerId.cs
   ?   ?   ?   ??? Money.cs
   ?   ?   ?   ??? Address.cs
   ?   ?   ??? Domain.csproj
   ?   ??? tests/
   ?       ??? Domain.Tests.csproj
   ??? Application/
   ?   ??? src/
   ?   ?   ??? Queries/
   ?   ?   ?   ??? GetOrderByIdQuery.cs
   ?   ?   ?   ??? GetOrderByIdQueryHandler.cs
   ?   ?   ??? Commands/
   ?   ?   ?   ??? CreateOrderCommand.cs
   ?   ?   ?   ??? CreateOrderCommandHandler.cs
   ?   ?   ??? Abstractions/
   ?   ?   ?   ??? IOrderRepository.cs
   ?   ?   ?   ??? IEmailService.cs
   ?   ?   ??? Application.csproj
   ?   ??? tests/
   ??? Acl/
   ?   ??? src/
   ?   ?   ??? OrderRepository.cs
   ?   ?   ??? EmailService.cs
   ?   ?   ??? AntiCorruptionLayer.csproj
   ?   ??? tests/
   ??? Api/
   ?   ??? src/
   ?   ?   ??? 2024-01-15/
   ?   ?   ?   ??? Controllers/
   ?   ?   ?       ??? OrdersController.cs
   ?   ?   ?       ??? CustomersController.cs
   ?   ?   ?       ??? ProductsController.cs
   ?   ?   ??? Middleware/
   ?   ?   ?   ??? ErrorHandlingMiddleware.cs
   ?   ?   ??? Program.cs
   ?   ?   ??? Api.csproj
   ?   ??? tests/
   ??? Directory.Build.props
   ??? Directory.Packages.props
   ??? global.json
   ??? .editorconfig
   ??? ECommerceOrderSystem.sln
   ```
4. **Create Pull Request** with detailed description
5. **Comment on issue** with completion status

### **Step 5: Review and Merge** (1 minute)

```bash
# Review the PR
gh pr view --web

# Run tests locally
git fetch
git checkout copilot-scaffold/1-ECommerceOrderSystem
dotnet test

# Start API
cd Api/src
dotnet run

# Open browser to https://localhost:5001 - Swagger UI appears!

# Merge when satisfied
gh pr merge --squash
```

---

## ?? **What Gets Generated**

### **Domain Layer**

**Aggregates** with business logic:
```csharp
public class Order : Aggregate<OrderId>
{
    public CustomerId CustomerId { get; }
    public OrderStatus Status { get; private set; }
    public Money Total { get; private set; }
    
    public static Result<Order> TryCreate(CustomerId customerId)
    {
        var order = new Order(customerId);
        return s_validator.ValidateToResult(order);
    }
    
    public Result<Order> Submit()
    {
        return this.ToResult()
            .Ensure(_ => Status == OrderStatus.Draft, 
                   Error.Validation("Can only submit draft orders"))
            .Ensure(_ => Lines.Count > 0, 
                   Error.Validation("Cannot submit empty order"))
            .Tap(_ => {
                Status = OrderStatus.Submitted;
                DomainEvents.Add(new OrderSubmittedEvent(Id, Total, DateTime.UtcNow));
            });
    }
    
    private static readonly InlineValidator<Order> s_validator = new()
    {
        v => v.RuleFor(x => x.CustomerId).NotNull()
    };
}
```

**Value Objects** with validation:
```csharp
// Simple (source-generated)
public partial class OrderId : RequiredGuid { }

// Complex with custom validation
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    public static Result<Money> TryCreate(decimal amount, string currency = "USD")
    {
        return (amount, currency).ToResult()
            .Ensure(x => x.amount >= 0, Error.Validation("Amount cannot be negative"))
            .Ensure(x => x.currency.Length == 3, Error.Validation("Invalid currency"))
            .Map(x => new Money(x.amount, x.currency));
    }
    
    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

### **Application Layer**

**CQRS Queries**:
```csharp
public class GetOrderByIdQuery : IQuery<Result<Order>>
{
    public OrderId OrderId { get; }
    
    public static Result<GetOrderByIdQuery> TryCreate(OrderId orderId)
        => s_validator.ValidateToResult(new GetOrderByIdQuery(orderId));
    
    private GetOrderByIdQuery(OrderId orderId) => OrderId = orderId;
    
    private static readonly InlineValidator<GetOrderByIdQuery> s_validator = new()
    {
        v => v.RuleFor(x => x.OrderId).NotNull()
    };
}

public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, Result<Order>>
{
    private readonly IOrderRepository _repository;
    
    public async ValueTask<Result<Order>> Handle(
        GetOrderByIdQuery query, 
        CancellationToken ct)
        => await _repository.GetByIdAsync(query.OrderId, ct)
            .ToResultAsync(Error.NotFound($"Order {query.OrderId} not found"));
}
```

**Service Abstractions**:
```csharp
public interface IOrderRepository
{
    ValueTask<Order?> GetByIdAsync(OrderId orderId, CancellationToken ct);
    ValueTask AddAsync(Order order, CancellationToken ct);
    ValueTask UpdateAsync(Order order, CancellationToken ct);
}
```

### **ACL Layer**

**Repository Implementation**:
```csharp
public class OrderRepository : IOrderRepository
{
    private readonly Dictionary<OrderId, Order> _orders = new();
    
    public ValueTask<Order?> GetByIdAsync(OrderId orderId, CancellationToken ct)
    {
        _orders.TryGetValue(orderId, out var order);
        return ValueTask.FromResult(order);
    }
    
    public ValueTask AddAsync(Order order, CancellationToken ct)
    {
        _orders[order.Id] = order;
        return ValueTask.CompletedTask;
    }
}
```

**Dependency Injection**:
```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddAntiCorruptionLayer(
        this IServiceCollection services)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IEmailService, EmailService>();
        return services;
    }
}
```

### **API Layer**

**Railway-Oriented Controllers**:
```csharp
[ApiController]
[ApiVersion("2024-01-15")]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;
    
    [HttpGet("{id}")]
    public async ValueTask<ActionResult<OrderDto>> GetById(
        string id, 
        CancellationToken ct)
        => await OrderId.TryCreate(Guid.Parse(id))
            .Bind(orderId => GetOrderByIdQuery.TryCreate(orderId))
            .BindAsync(query => _sender.Send(query, ct))
            .MapAsync(order => order.Adapt<OrderDto>())
            .ToActionResultAsync(this);
    
    [HttpPost]
    public async ValueTask<ActionResult<OrderDto>> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken ct)
        => await CustomerId.TryCreate(request.CustomerId)
            .Bind(customerId => CreateOrderCommand.TryCreate(customerId))
            .BindAsync(command => _sender.Send(command, ct))
            .MapAsync(order => order.Adapt<OrderDto>())
            .ToActionResultAsync(this);
}
```

**Program.cs** with all wiring:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPresentation()
    .AddApplication()
    .AddAntiCorruptionLayer();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### **Tests**

```csharp
public class OrderTests
{
    [Fact]
    public void CreateOrder_WithValidCustomerId_Succeeds()
    {
        // Arrange
        var customerId = CustomerId.NewUnique();
        
        // Act
        var result = Order.TryCreate(customerId);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CustomerId.Should().Be(customerId);
        result.Value.Status.Should().Be(OrderStatus.Draft);
    }
    
    [Fact]
    public void SubmitOrder_WhenDraft_Succeeds()
    {
        // Arrange
        var order = Order.TryCreate(CustomerId.NewUnique()).Value;
        var productId = ProductId.NewUnique();
        var price = Money.TryCreate(10m).Value;
        order.AddLine(productId, "Test Product", 1, price);
        
        // Act
        var result = order.Submit();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(OrderStatus.Submitted);
    }
    
    [Fact]
    public void SubmitEmptyOrder_Fails()
    {
        // Arrange
        var order = Order.TryCreate(CustomerId.NewUnique()).Value;
        
        // Act
        var result = order.Submit();
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Detail.Should().Contain("Cannot submit empty order");
    }
}
```

---

## ?? **Before vs After Comparison**

### **Traditional Approach** ?

```
Time to build: 2-3 weeks
Lines of code to write manually: ~5,000
Files to create: ~50
Chances of inconsistency: High
```

### **With FunctionalDDD Agent** ?

```
Time to build: 5 minutes
Lines of code to write: ~100 (YAML spec)
Files created automatically: ~50
Consistency guaranteed: 100%
```

---

## ?? **Demo Script for Presentations**

### **Opening** (1 minute)

> "Let me show you something incredible. I'm going to create a complete, production-ready e-commerce system in just 5 minutes using nothing but a YAML file and GitHub Copilot."

### **Setup** (1 minute)

1. Create new GitHub repository
2. Show the project specification YAML file
3. Highlight key sections: aggregates, value objects, queries, commands

### **The Magic** (1 minute)

1. Create GitHub issue with `copilot-scaffold` label
2. Show automated workflow kick off
3. Watch in real-time as PR is created

### **The Reveal** (2 minutes)

1. Open the generated PR
2. Browse through generated code:
   - Domain aggregates with business logic
   - Value objects with validation
   - CQRS queries and commands
   - Railway-oriented controllers
   - Comprehensive tests
3. Clone and run: `dotnet test` - all green!
4. Start API: `dotnet run`
5. Open Swagger UI - full API documentation

### **Closing**

> "That's it. From a simple specification to a complete, tested, production-ready application in 5 minutes. This is the power of combining Clean Architecture, FunctionalDDD, and GitHub Copilot."

---

## ?? **Advanced Demo Scenarios**

### **Scenario 1: Todo List (Minimal)**

Perfect for showing the simplicity:

```yaml
project:
  name: TodoList
  namespace: MyApp.TodoList
  
domain:
  aggregates:
    - name: TodoItem
      id: TodoId
      properties:
        - name: Title
          type: string
        - name: IsCompleted
          type: bool
      behaviors:
        - name: Complete
```

**Demo time**: 2 minutes
**Generated files**: ~20

### **Scenario 2: Banking System (Complex)**

Show handling of complex business rules:

```yaml
domain:
  aggregates:
    - name: BankAccount
      behaviors:
        - name: Withdraw
          validates:
            - amount > 0
            - Balance >= amount
            - DailyWithdrawalLimit not exceeded
        - name: Deposit
        - name: Transfer
```

**Demo time**: 5 minutes
**Generated files**: ~60

### **Scenario 3: Multi-Tenant SaaS**

Demonstrate enterprise patterns:

```yaml
domain:
  aggregates:
    - name: Tenant
    - name: User
    - name: Subscription
  valueObjects:
    - name: TenantId
    - name: SubscriptionTier
      enum: [Free, Pro, Enterprise]
```

---

## ?? **Tips for a Great Demo**

1. **Pre-create the repository** to save time
2. **Have the YAML ready** in a gist or file
3. **Use GitHub CLI** (`gh`) for speed
4. **Keep terminal window large** for visibility
5. **Show the PR immediately** - don't wait for completion
6. **Have backup slides** in case of network issues
7. **Emphasize the testing** - run `dotnet test` and show all green
8. **Open Swagger** - visual proof of working API

---

## ?? **Key Messages**

- ? **From specification to working code in minutes**
- ? **Clean Architecture enforced automatically**
- ? **Railway-Oriented Programming throughout**
- ? **Comprehensive tests generated**
- ? **Production-ready from day one**
- ? **Consistent patterns across all layers**
- ? **Best practices built-in**

---

## ?? **Resources**

- [Project Specification Template](.github/PROJECT_SPEC_TEMPLATE.md)
- [Demo Specification](.github/project-spec-demo.yml)
- [Agent Instructions](.github/copilot-instructions.md)
- [FunctionalDDD Documentation](https://github.com/xavierjohn/FunctionalDDD)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

**Ready to create your own jaw-dropping demo?** ??

Try it now:
```bash
gh repo create my-awesome-project
cd my-awesome-project
# Copy project-spec-demo.yml to .github/project-spec.yml
gh issue create --label copilot-scaffold --title "Scaffold My Project"
# Watch the magic! ?
```
