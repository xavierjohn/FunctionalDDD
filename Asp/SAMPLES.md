# ASP Extension - Comprehensive Examples

This document provides detailed examples and advanced patterns for using the ASP extension with Railway Oriented Programming in ASP.NET Core applications.

## Table of Contents

- [MVC Controllers](#mvc-controllers)
  - [Basic CRUD Operations](#basic-crud-operations)
  - [Async Operations](#async-operations)
  - [Complex Validation Scenarios](#complex-validation-scenarios)
  - [Custom Status Codes](#custom-status-codes)
  - [Pagination Support](#pagination-support)
- [Minimal API](#minimal-api)
  - [Basic Endpoints](#basic-endpoints)
  - [Async Operations](#async-operations-1)
  - [Custom Status Codes](#custom-status-codes-in-minimal-api)
  - [Advanced Patterns](#advanced-patterns)
- [Error Handling Patterns](#error-handling-patterns)
  - [Validation Errors](#validation-errors)
  - [Authorization Errors](#authorization-errors)
  - [Not Found Handling](#not-found-handling)
  - [Conflict Resolution](#conflict-resolution)
- [Integration Patterns](#integration-patterns)
  - [Service Layer Integration](#service-layer-integration)
  - [Repository Pattern](#repository-pattern)
  - [Unit of Work](#unit-of-work)
- [Advanced Scenarios](#advanced-scenarios)
  - [Batch Operations](#batch-operations)
  - [Conditional Processing](#conditional-processing)
  - [Side Effects with Tap](#side-effects-with-tap)

## MVC Controllers

### Basic CRUD Operations

Complete CRUD controller demonstrating all HTTP verbs:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repository;
    private readonly IEmailService _emailService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserRepository repository,
        IEmailService emailService,
        ILogger<UsersController> logger)
    {
        _repository = repository;
        _emailService = emailService;
        _logger = logger;
    }

    // CREATE with 201 Created
    [HttpPost]
    public ActionResult<User> Create([FromBody] CreateUserRequest request) =>
        FirstName.TryCreate(request.FirstName)
            .Combine(LastName.TryCreate(request.LastName))
            .Combine(EmailAddress.TryCreate(request.Email))
            .Bind((firstName, lastName, email) => 
                User.TryCreate(firstName, lastName, email, request.Password))
            .Tap(user => _repository.Add(user))
            .Finally(
                ok => CreatedAtAction(nameof(GetById), new { id = ok.Id }, ok),
                err => err.ToErrorActionResult<User>(this));

    // READ single resource
    [HttpGet("{id}")]
    public ActionResult<User> GetById(string id) =>
        _repository.GetById(id)
            .ToResult(Error.NotFound($"User {id} not found"))
            .ToActionResult(this);

    // READ collection
    [HttpGet]
    public ActionResult<IEnumerable<User>> GetAll() =>
        _repository.GetAll()
            .ToActionResult(this);

    // UPDATE
    [HttpPut("{id}")]
    public ActionResult<User> Update(
        string id,
        [FromBody] UpdateUserRequest request) =>
        _repository.GetById(id)
            .ToResult(Error.NotFound($"User {id} not found"))
            .Bind(user => user.UpdateName(request.FirstName, request.LastName))
            .Bind(user => user.UpdateEmail(request.Email))
            .Tap(user => _repository.Update(user))
            .ToActionResult(this);

    // DELETE with 204 No Content
    [HttpDelete("{id}")]
    public ActionResult<Unit> Delete(string id) =>
        _repository.GetById(id)
            .ToResult(Error.NotFound($"User {id} not found"))
            .Ensure(user => user.CanDelete, Error.Conflict("User has active orders"))
            .Tap(user => _repository.Delete(user))
            .Map(_ => Result.Success())
            .ToActionResult(this);
}
```

### Async Operations

Best practices for async operations:

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IInventoryService _inventoryService;
    private readonly IPaymentService _paymentService;
    private readonly IShippingService _shippingService;
    private readonly IEmailService _emailService;

    // Async CREATE with side effects
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrderAsync(
        [FromBody] CreateOrderRequest request) =>
        await ValidateOrderAsync(request)
            .BindAsync(validatedRequest => _orderService.CreateAsync(validatedRequest))
            .TapAsync(order => _inventoryService.ReserveAsync(order.Items))
            .TapAsync(order => _emailService.SendOrderConfirmationAsync(order))
            .ToActionResultAsync(this);

    // Async READ with complex validation
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrderAsync(string id) =>
        await _orderService.GetByIdAsync(id)
            .ToResultAsync(Error.NotFound($"Order {id} not found"))
            .EnsureAsync(
                order => CanAccessOrderAsync(User, order),
                Error.Forbidden("Access denied to this order"))
            .ToActionResultAsync(this);

    // Async UPDATE with multiple steps
    [HttpPut("{id}/submit")]
    public async Task<ActionResult<Order>> SubmitOrderAsync(string id) =>
        await _orderService.GetByIdAsync(id)
            .ToResultAsync(Error.NotFound($"Order {id} not found"))
            .BindAsync(order => order.SubmitAsync())
            .BindAsync(order => _paymentService.ProcessAsync(order))
            .TapAsync(order => _orderService.SaveAsync(order))
            .TapAsync(order => _shippingService.ScheduleAsync(order))
            .ToActionResultAsync(this);

    // Async DELETE with cleanup
    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> CancelOrderAsync(string id) =>
        await _orderService.GetByIdAsync(id)
            .ToResultAsync(Error.NotFound($"Order {id} not found"))
            .EnsureAsync(
                order => order.CanCancelAsync(),
                Error.Conflict("Order cannot be cancelled"))
            .TapAsync(order => _inventoryService.ReleaseAsync(order.Items))
            .TapAsync(order => _paymentService.RefundAsync(order))
            .TapAsync(order => _orderService.DeleteAsync(order))
            .Map(_ => Result.Success())
            .ToActionResultAsync(this);

    private async Task<Result<CreateOrderRequest>> ValidateOrderAsync(
        CreateOrderRequest request) =>
        await _inventoryService.CheckAvailabilityAsync(request.Items)
            .EnsureAsync(
                available => Task.FromResult(available),
                Error.Conflict("Some items are out of stock"))
            .Map(_ => request);

    private async Task<bool> CanAccessOrderAsync(
        ClaimsPrincipal user,
        Order order) =>
        await Task.FromResult(
            user.IsInRole("Admin") || 
            user.FindFirst(ClaimTypes.NameIdentifier)?.Value == order.UserId);
}
```

### Complex Validation Scenarios

Handling multiple validation steps with clear error messages:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;
    private readonly ICategoryRepository _categoryRepository;

    [HttpPost]
    public ActionResult<Product> Create([FromBody] CreateProductRequest request) =>
        // Validate all value objects
        ProductName.TryCreate(request.Name)
            .Combine(ProductDescription.TryCreate(request.Description))
            .Combine(Price.TryCreate(request.Price))
            .Combine(Quantity.TryCreate(request.Quantity))
            // Validate category exists
            .Bind((name, description, price, quantity) =>
                _categoryRepository.GetById(request.CategoryId)
                    .ToResult(Error.NotFound($"Category {request.CategoryId} not found"))
                    .Map(category => (name, description, price, quantity, category)))
            // Validate unique product name
            .Ensure(
                tuple => !_repository.ExistsByName(tuple.name),
                Error.Conflict("A product with this name already exists"))
            // Create product
            .Bind(tuple => Product.TryCreate(
                tuple.name,
                tuple.description,
                tuple.price,
                tuple.quantity,
                tuple.category))
            // Save
            .Tap(product => _repository.Add(product))
            .Finally(
                ok => CreatedAtAction(nameof(GetById), new { id = ok.Id }, ok),
                err => err.ToErrorActionResult<Product>(this));

    [HttpGet("{id}")]
    public ActionResult<Product> GetById(string id) =>
        _repository.GetById(id)
            .ToResult(Error.NotFound($"Product {id} not found"))
            .ToActionResult(this);

    [HttpPut("{id}/price")]
    public ActionResult<Product> UpdatePrice(
        string id,
        [FromBody] UpdatePriceRequest request) =>
        _repository.GetById(id)
            .ToResult(Error.NotFound($"Product {id} not found"))
            .Bind(product => Price.TryCreate(request.NewPrice)
                .Bind(newPrice => product.UpdatePrice(newPrice)))
            .Tap(product => _repository.Update(product))
            .ToActionResult(this);
}
```

### Custom Status Codes

Using `Finally` for specific HTTP status codes:

```csharp
[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    // 202 Accepted for async processing
    [HttpPost]
    public ActionResult<JobStatus> StartJob([FromBody] StartJobRequest request) =>
        _jobService.QueueJob(request)
            .Finally(
                ok => Accepted(new { jobId = ok.Id, status = "queued" }),
                err => err.ToErrorActionResult<JobStatus>(this));

    // 201 Created with Location header
    [HttpPost("batch")]
    public ActionResult<BatchJob> CreateBatch([FromBody] CreateBatchRequest request) =>
        _jobService.CreateBatch(request)
            .Finally(
                ok => CreatedAtAction(
                    nameof(GetBatchStatus),
                    new { batchId = ok.Id },
                    ok),
                err => err.ToErrorActionResult<BatchJob>(this));

    // 204 No Content for successful operation without content
    [HttpPost("{id}/cancel")]
    public ActionResult<Unit> CancelJob(string id) =>
        _jobService.GetById(id)
            .ToResult(Error.NotFound($"Job {id} not found"))
            .Bind(job => job.Cancel())
            .Map(_ => Result.Success())
            .ToActionResult(this);

    // Custom error codes
    [HttpPost("{id}/retry")]
    public ActionResult<Job> RetryJob(string id) =>
        _jobService.GetById(id)
            .ToResult(Error.NotFound($"Job {id} not found"))
            .Ensure(
                job => job.CanRetry,
                Error.Conflict("Job cannot be retried"))
            .Bind(job => job.Retry())
            .ToActionResult(this);

    [HttpGet("batch/{batchId}/status")]
    public ActionResult<BatchStatus> GetBatchStatus(string batchId) =>
        _jobService.GetBatchStatus(batchId)
            .ToResult(Error.NotFound($"Batch {batchId} not found"))
            .ToActionResult(this);
}
```

### Pagination Support

RFC9110-compliant pagination with Content-Range headers:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repository;

    // Basic pagination
    [HttpGet]
    public ActionResult<IEnumerable<User>> GetUsers(
        [FromQuery] int? from = null,
        [FromQuery] int? to = null)
    {
        var totalCount = _repository.Count();
        var fromIndex = from ?? 0;
        var toIndex = to ?? Math.Min(fromIndex + 49, totalCount - 1);

        return _repository.GetAll()
            .Map(users => users
                .Skip(fromIndex)
                .Take(toIndex - fromIndex + 1))
            .ToActionResult(this, toIndex, fromIndex, totalCount);
    }

    // Pagination with filtering
    [HttpGet("search")]
    public ActionResult<IEnumerable<User>> SearchUsers(
        [FromQuery] string query,
        [FromQuery] int? from = null,
        [FromQuery] int? to = null)
    {
        var filtered = _repository.Search(query);
        var totalCount = filtered.Count();
        var fromIndex = from ?? 0;
        var toIndex = to ?? Math.Min(fromIndex + 49, totalCount - 1);

        return filtered
            .Skip(fromIndex)
            .Take(toIndex - fromIndex + 1)
            .ToResult()
            .ToActionResult(this, toIndex, fromIndex, totalCount);
    }

    // Pagination with sorting
    [HttpGet("sorted")]
    public ActionResult<IEnumerable<User>> GetSortedUsers(
        [FromQuery] string sortBy = "name",
        [FromQuery] bool descending = false,
        [FromQuery] int? from = null,
        [FromQuery] int? to = null)
    {
        var sorted = sortBy.ToLower() switch
        {
            "email" => descending
                ? _repository.GetAll().OrderByDescending(u => u.Email)
                : _repository.GetAll().OrderBy(u => u.Email),
            "created" => descending
                ? _repository.GetAll().OrderByDescending(u => u.CreatedAt)
                : _repository.GetAll().OrderBy(u => u.CreatedAt),
            _ => descending
                ? _repository.GetAll().OrderByDescending(u => u.Name)
                : _repository.GetAll().OrderBy(u => u.Name)
        };

        var totalCount = sorted.Count();
        var fromIndex = from ?? 0;
        var toIndex = to ?? Math.Min(fromIndex + 49, totalCount - 1);

        return sorted
            .Skip(fromIndex)
            .Take(toIndex - fromIndex + 1)
            .ToResult()
            .ToActionResult(this, toIndex, fromIndex, totalCount);
    }
}
```

**Response Headers Example:**
- Partial: `206 Partial Content` with `Content-Range: items 0-49/1000`
- Complete: `200 OK` with no Content-Range header

## Minimal API

### Basic Endpoints

Simple CRUD operations using Minimal API:

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var userApi = app.MapGroup("/api/users");

// CREATE
userApi.MapPost("/", (
    CreateUserRequest request,
    IUserRepository repository) =>
    FirstName.TryCreate(request.FirstName)
        .Combine(LastName.TryCreate(request.LastName))
        .Combine(EmailAddress.TryCreate(request.Email))
        .Bind((firstName, lastName, email) => 
            User.TryCreate(firstName, lastName, email, request.Password))
        .Tap(user => repository.Add(user))
        .ToHttpResult());

// READ
userApi.MapGet("/{id}", (
    string id,
    IUserRepository repository) =>
    repository.GetById(id)
        .ToResult(Error.NotFound($"User {id} not found"))
        .ToHttpResult());

// UPDATE
userApi.MapPut("/{id}", (
    string id,
    UpdateUserRequest request,
    IUserRepository repository) =>
    repository.GetById(id)
        .ToResult(Error.NotFound($"User {id} not found"))
        .Bind(user => user.UpdateName(request.FirstName, request.LastName))
        .Bind(user => user.UpdateEmail(request.Email))
        .Tap(user => repository.Update(user))
        .ToHttpResult());

// DELETE
userApi.MapDelete("/{id}", (
    string id,
    IUserRepository repository) =>
    repository.GetById(id)
        .ToResult(Error.NotFound($"User {id} not found"))
        .Ensure(user => user.CanDelete, Error.Conflict("User has active orders"))
        .Tap(user => repository.Delete(user))
        .Map(_ => Result.Success())
        .ToHttpResult());

app.Run();
```

### Async Operations

Async endpoints:

```csharp
var orderApi = app.MapGroup("/api/orders");

// Async CREATE
orderApi.MapPost("/", async (
    CreateOrderRequest request,
    IOrderService orderService,
    IInventoryService inventoryService,
    IEmailService emailService) =>
    await orderService.CreateAsync(request)
        .TapAsync(order => inventoryService.ReserveAsync(order.Items))
        .TapAsync(order => emailService.SendConfirmationAsync(order))
        .ToHttpResultAsync());

// Async READ
orderApi.MapGet("/{id}", async (
    string id,
    IOrderService orderService) =>
    await orderService.GetByIdAsync(id)
        .ToResultAsync(Error.NotFound($"Order {id} not found"))
        .ToHttpResultAsync());

// Async UPDATE
orderApi.MapPut("/{id}/submit", async (
    string id,
    IOrderService orderService,
    IPaymentService paymentService) =>
    await orderService.GetByIdAsync(id)
        .ToResultAsync(Error.NotFound($"Order {id} not found"))
        .BindAsync(order => order.SubmitAsync())
        .BindAsync(order => paymentService.ProcessAsync(order))
        .TapAsync(order => orderService.SaveAsync(order))
        .ToHttpResultAsync());

// Async DELETE
orderApi.MapDelete("/{id}", async (
    string id,
    IOrderService orderService,
    IInventoryService inventoryService,
    IPaymentService paymentService) =>
    await orderService.GetByIdAsync(id)
        .ToResultAsync(Error.NotFound($"Order {id} not found"))
        .TapAsync(order => inventoryService.ReleaseAsync(order.Items))
        .TapAsync(order => paymentService.RefundAsync(order))
        .TapAsync(order => orderService.DeleteAsync(order))
        .Map(_ => Result.Success())
        .ToHttpResultAsync());
```

### Custom Status Codes in Minimal API

Using `Finally` for specific HTTP responses:

```csharp
var productApi = app.MapGroup("/api/products");

// 201 Created with Location
productApi.MapPost("/", (
    CreateProductRequest request,
    IProductRepository repository) =>
    ProductName.TryCreate(request.Name)
        .Combine(Price.TryCreate(request.Price))
        .Bind((name, price) => Product.TryCreate(name, price))
        .Tap(product => repository.Add(product))
        .Finally(
            ok => Results.CreatedAtRoute(
                "GetProductById",
                new { id = ok.Id },
                ok),
            err => err.ToErrorResult()))
    .WithName("GetProductById");

// 202 Accepted
productApi.MapPost("/import", (
    ImportRequest request,
    IImportService importService) =>
    importService.QueueImport(request)
        .Finally(
            ok => Results.Accepted($"/api/import/{ok.JobId}", ok),
            err => err.ToErrorResult()));

// 204 No Content
productApi.MapDelete("/{id}", (
    string id,
    IProductRepository repository) =>
    repository.Delete(id)
        .Map(_ => Result.Success())
        .Finally(
            _ => Results.NoContent(),
            err => err.ToErrorResult()));

// Custom validation response
productApi.MapPost("/validate", (
    ValidateProductRequest request,
    IProductValidator validator) =>
    validator.Validate(request)
        .Finally(
            ok => Results.Ok(new { valid = true, product = ok }),
            err => err.ToErrorResult()));
```

### Advanced Patterns

Complex orchestration in Minimal API:

```csharp
// Batch processing with Traverse
var batchApi = app.MapGroup("/api/batch");

batchApi.MapPost("/users", async (
    List<CreateUserRequest> requests,
    IUserRepository repository) =>
    await requests.TraverseAsync(
        async request =>
            await FirstName.TryCreate(request.FirstName)
                .Combine(LastName.TryCreate(request.LastName))
                .Combine(EmailAddress.TryCreate(request.Email))
                .BindAsync(async (firstName, lastName, email) =>
                    await User.TryCreateAsync(firstName, lastName, email, request.Password))
                .TapAsync(user => repository.AddAsync(user)))
    .ToHttpResultAsync());

// Complex validation chain
var validationApi = app.MapGroup("/api/validate");

validationApi.MapPost("/order", async (
    CreateOrderRequest request,
    IInventoryService inventoryService,
    ICustomerService customerService,
    IPricingService pricingService) =>
    await customerService.ValidateCustomerAsync(request.CustomerId)
        .BindAsync(async customer => 
            (await inventoryService.ValidateItemsAsync(request.Items))
                .Map(_ => customer))
        .BindAsync(async customer =>
            (await pricingService.CalculateTotalAsync(request.Items))
                .Map(total => (customer, total)))
        .Map(tuple => new OrderValidationResult(
            tuple.customer,
            tuple.total,
            IsValid: true))
        .ToHttpResultAsync());

// Conditional processing
var processingApi = app.MapGroup("/api/process");

processingApi.MapPost("/order/{id}/complete", async (
    string id,
    IOrderService orderService,
    IPaymentService paymentService,
    IShippingService shippingService) =>
    await orderService.GetByIdAsync(id)
        .ToResultAsync(Error.NotFound($"Order {id} not found"))
        .BindAsync(async order =>
        {
            if (order.RequiresPayment)
                return await paymentService.ProcessAsync(order);
            return order.ToResult();
        })
        .BindAsync(async order =>
        {
            if (order.RequiresShipping)
                return await shippingService.ScheduleAsync(order);
            return order.ToResult();
        })
        .TapAsync(order => orderService.SaveAsync(order))
        .ToHttpResultAsync());
```

## Error Handling Patterns

### Validation Errors

Collecting multiple validation errors:

```csharp
[HttpPost]
public ActionResult<Order> CreateOrder([FromBody] CreateOrderRequest request) =>
    // Combine all validations - collects ALL errors
    CustomerId.TryCreate(request.CustomerId)
        .Combine(ShippingAddress.TryCreate(request.ShippingAddress))
        .Combine(BillingAddress.TryCreate(request.BillingAddress))
        .Combine(request.Items.Traverse(item =>
            ProductId.TryCreate(item.ProductId)
                .Combine(Quantity.TryCreate(item.Quantity))
                .Combine(Price.TryCreate(item.Price))
                .Bind((productId, quantity, price) =>
                    OrderLine.TryCreate(productId, quantity, price))))
        .Bind((customerId, shipping, billing, items) =>
            Order.TryCreate(customerId, shipping, billing, items))
        .ToActionResult(this);
```

**Validation Error Response:**
```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "customerId": ["Customer ID is required"],
        "shippingAddress.zipCode": ["Invalid ZIP code format"],
        "items[0].quantity": ["Quantity must be positive"],
        "items[2].price": ["Price must be greater than zero"]
    }
}
```

### Authorization Errors

Handling authentication and authorization:

```csharp
[HttpGet("{id}")]
[Authorize]
public async Task<ActionResult<Order>> GetOrderAsync(string id) =>
    await _orderService.GetByIdAsync(id)
        .ToResultAsync(Error.NotFound($"Order {id} not found"))
        // Check ownership
        .EnsureAsync(
            order => IsOwnerAsync(User, order),
            Error.Forbidden("You don't have permission to view this order"))
        // Check subscription level
        .EnsureAsync(
            order => HasRequiredSubscriptionAsync(User, order),
            Error.Forbidden("Your subscription level doesn't allow viewing this order type"))
        .ToActionResultAsync(this);

private async Task<bool> IsOwnerAsync(
    ClaimsPrincipal user,
    Order order)
{
    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    return await Task.FromResult(
        user.IsInRole("Admin") || userId == order.UserId);
}

private async Task<bool> HasRequiredSubscriptionAsync(
    ClaimsPrincipal user,
    Order order)
{
    var subscription = await _subscriptionService.GetUserSubscriptionAsync(
        user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    return subscription.Level >= order.RequiredSubscriptionLevel;
}
```

### Not Found Handling

Consistent 404 handling patterns:

```csharp
[HttpGet("{id}")]
public ActionResult<Product> GetProduct(string id) =>
    _repository.GetById(id)
        .ToResult(Error.NotFound($"Product {id} not found"))
        .ToActionResult(this);

[HttpGet("category/{categoryId}/products")]
public ActionResult<IEnumerable<Product>> GetProductsByCategory(string categoryId) =>
    _categoryRepository.GetById(categoryId)
        .ToResult(Error.NotFound($"Category {categoryId} not found"))
        .Bind(category => _productRepository.GetByCategory(category))
        .Ensure(
            products => products.Any(),
            Error.NotFound($"No products found in category {categoryId}"))
        .ToActionResult(this);

// Multiple resources
[HttpGet("order/{orderId}/items/{itemId}")]
public ActionResult<OrderItem> GetOrderItem(string orderId, string itemId) =>
    _orderRepository.GetById(orderId)
        .ToResult(Error.NotFound($"Order {orderId} not found"))
        .Bind(order => order.GetItem(itemId)
            .ToResult(Error.NotFound($"Item {itemId} not found in order {orderId}")))
        .ToActionResult(this);
```

### Conflict Resolution

Handling 409 Conflict errors:

```csharp
[HttpPost]
public ActionResult<User> Register([FromBody] RegisterRequest request) =>
    EmailAddress.TryCreate(request.Email)
        .Ensure(
            email => !_userRepository.ExistsByEmail(email),
            Error.Conflict("A user with this email already exists"))
        .Bind(email => User.TryCreate(
            request.FirstName,
            request.LastName,
            email,
            request.Password))
        .ToActionResult(this);

[HttpPut("{id}/email")]
public async Task<ActionResult<User>> UpdateEmailAsync(
    string id,
    [FromBody] UpdateEmailRequest request) =>
    await _userRepository.GetByIdAsync(id)
        .ToResultAsync(Error.NotFound($"User {id} not found"))
        .BindAsync(async user =>
            await EmailAddress.TryCreate(request.NewEmail)
                .EnsureAsync(
                    async email => !await _userRepository.ExistsByEmailAsync(email),
                    Error.Conflict("Email already in use"))
                .BindAsync(email => user.UpdateEmailAsync(email)))
        .TapAsync(user => _userRepository.SaveAsync(user))
        .ToActionResultAsync(this);

[HttpDelete("{id}")]
public ActionResult<Unit> DeleteUser(string id) =>
    _userRepository.GetById(id)
        .ToResult(Error.NotFound($"User {id} not found"))
        .Ensure(
            user => !user.HasActiveOrders,
            Error.Conflict("Cannot delete user with active orders"))
        .Ensure(
            user => !user.HasPendingPayments,
            Error.Conflict("Cannot delete user with pending payments"))
        .Tap(user => _userRepository.Delete(user))
        .Map(_ => Result.Success())
        .ToActionResult(this);
```

## Integration Patterns

### Service Layer Integration

Clean separation between API and service layers:

```csharp
// Service Layer
public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IInventoryService _inventoryService;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public async Task<Result<Order>> CreateAsync(CreateOrderRequest request) =>
        await ValidateCustomerAsync(request.CustomerId)
            .BindAsync(async customer => 
                (await CreateOrderLinesAsync(request.Items))
                    .Map(items => (customer, items)))
            .BindAsync(async tuple => 
                await Order.TryCreateAsync(tuple.customer.Id, tuple.items))
            .TapAsync(order => _repository.AddAsync(order))
            .TapAsync(order => _eventDispatcher.DispatchAsync(order.DomainEvents));

    private async Task<Result<Customer>> ValidateCustomerAsync(string customerId) =>
        await _customerRepository.GetByIdAsync(customerId)
            .ToResultAsync(Error.NotFound($"Customer {customerId} not found"))
            .EnsureAsync(
                customer => customer.IsActiveAsync(),
                Error.Conflict("Customer account is inactive"));

    private async Task<Result<IEnumerable<OrderLine>>> CreateOrderLinesAsync(
        IEnumerable<OrderLineRequest> requests) =>
        await requests.TraverseAsync(
            async request =>
                await _inventoryService.GetProductAsync(request.ProductId)
                    .ToResultAsync(Error.NotFound($"Product {request.ProductId} not found"))
                    .BindAsync(product => 
                        OrderLine.TryCreateAsync(
                            product.Id,
                            product.Name,
                            product.Price,
                            request.Quantity)));
}

// Controller Layer
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrderAsync(
        [FromBody] CreateOrderRequest request) =>
        await _orderService.CreateAsync(request)
            .ToActionResultAsync(this);

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrderAsync(string id) =>
        await _orderService.GetByIdAsync(id)
            .ToActionResultAsync(this);
}
```

### Repository Pattern

Repository returning Result<T>:

```csharp
public class UserRepository : IUserRepository
{
    private readonly DbContext _context;

    public Result<User> GetById(string id)
    {
        var user = _context.Users.Find(id);
        return user is not null
            ? user.ToResult()
            : Error.NotFound($"User {id} not found");
    }

    public async Task<Result<User>> GetByIdAsync(string id)
    {
        var user = await _context.Users.FindAsync(id);
        return user is not null
            ? user.ToResult()
            : Error.NotFound($"User {id} not found");
    }

    public Result<User> GetByEmail(EmailAddress email)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        return user is not null
            ? user.ToResult()
            : Error.NotFound($"User with email {email} not found");
    }

    public Result<IEnumerable<User>> GetAll() =>
        _context.Users.ToList().ToResult();

    public Result<Unit> Add(User user)
    {
        try
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Error.Unexpected("Failed to add user", ex);
        }
    }

    public async Task<Result<Unit>> AddAsync(User user)
    {
        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Error.Unexpected("Failed to add user", ex);
        }
    }
}
```

### Unit of Work

Transactional operations with UnitOfWork:

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;

    public async Task<Result<Unit>> CommitAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return Error.Conflict("Concurrency conflict detected", ex);
        }
        catch (DbUpdateException ex)
        {
            return Error.Unexpected("Database update failed", ex);
        }
    }
}

// Usage in controller
[HttpPost("transfer")]
public async Task<ActionResult<TransferResult>> TransferFundsAsync(
    [FromBody] TransferRequest request) =>
    await _accountRepository.GetByIdAsync(request.FromAccountId)
        .ToResultAsync(Error.NotFound($"Account {request.FromAccountId} not found"))
        .BindAsync(async fromAccount =>
            (await _accountRepository.GetByIdAsync(request.ToAccountId))
                .ToResult(Error.NotFound($"Account {request.ToAccountId} not found"))
                .Map(toAccount => (fromAccount, toAccount)))
        .BindAsync(async tuple =>
            (await tuple.fromAccount.WithdrawAsync(request.Amount))
                .BindAsync(_ => tuple.toAccount.DepositAsync(request.Amount))
                .Map(_ => (tuple.fromAccount, tuple.toAccount)))
        .BindAsync(async tuple => 
            (await _unitOfWork.CommitAsync())
                .Map(_ => new TransferResult(tuple.fromAccount.Id, tuple.toAccount.Id, request.Amount)))
        .ToActionResultAsync(this);
```

## Advanced Scenarios

### Batch Operations

Processing multiple items with Traverse:

```csharp
[HttpPost("batch/users")]
public async Task<ActionResult<BatchResult<User>>> CreateUsersAsync(
    [FromBody] List<CreateUserRequest> requests) =>
    await requests.TraverseAsync(
        async request =>
            await FirstName.TryCreate(request.FirstName)
                .Combine(LastName.TryCreate(request.LastName))
                .Combine(EmailAddress.TryCreate(request.Email))
                .BindAsync(async (firstName, lastName, email) =>
                    await User.TryCreateAsync(firstName, lastName, email, request.Password))
                .TapAsync(user => _repository.AddAsync(user)))
    .Map(users => new BatchResult<User>(users.Count(), users))
    .ToActionResultAsync(this);

[HttpPut("batch/activate")]
public async Task<ActionResult<Unit>> ActivateUsersAsync(
    [FromBody] List<string> userIds) =>
    await userIds.TraverseAsync(
        async id =>
            await _repository.GetByIdAsync(id)
                .ToResultAsync(Error.NotFound($"User {id} not found"))
                .BindAsync(user => user.ActivateAsync())
                .TapAsync(user => _repository.UpdateAsync(user)))
    .Map(_ => Result.Success())
    .ToActionResultAsync(this);
```

### Conditional Processing

Conditional logic in pipelines:

```csharp
[HttpPost("{id}/process")]
public async Task<ActionResult<Order>> ProcessOrderAsync(string id) =>
    await _orderRepository.GetByIdAsync(id)
        .ToResultAsync(Error.NotFound($"Order {id} not found"))
        // Conditionally apply discount
        .BindAsync(async order =>
        {
            if (order.Customer.IsVIP)
                return await order.ApplyDiscountAsync(0.1m);
            return order.ToResult();
        })
        // Conditionally process payment
        .BindAsync(async order =>
        {
            if (order.Total > 0)
                return await _paymentService.ProcessAsync(order);
            return order.ToResult();
        })
        // Conditionally schedule shipping
        .BindAsync(async order =>
        {
            if (order.RequiresShipping)
                return await _shippingService.ScheduleAsync(order);
            return order.ToResult();
        })
        .TapAsync(order => _orderRepository.SaveAsync(order))
        .ToActionResultAsync(this);
```

### Side Effects with Tap

Executing side effects that shouldn't affect the pipeline:

```csharp
[HttpPost]
public async Task<ActionResult<User>> RegisterAsync(
    [FromBody] RegisterRequest request) =>
    await FirstName.TryCreate(request.FirstName)
        .Combine(LastName.TryCreate(request.LastName))
        .Combine(EmailAddress.TryCreate(request.Email))
        .BindAsync(async (firstName, lastName, email) =>
            await User.TryCreateAsync(firstName, lastName, email, request.Password))
        // Save user (critical)
        .TapAsync(user => _repository.AddAsync(user))
        // Send welcome email (non-critical, failures don't affect result)
        .TapAsync(async user =>
        {
            try
            {
                await _emailService.SendWelcomeEmailAsync(user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send welcome email to {Email}", user.Email);
            }
        })
        // Log analytics (non-critical)
        .TapAsync(async user =>
        {
            try
            {
                await _analyticsService.TrackUserRegistrationAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to track registration for {UserId}", user.Id);
            }
        })
        .ToActionResultAsync(this);

[HttpPut("{id}")]
public async Task<ActionResult<Product>> UpdateProductAsync(
    string id,
    [FromBody] UpdateProductRequest request) =>
    await _repository.GetByIdAsync(id)
        .ToResultAsync(Error.NotFound($"Product {id} not found"))
        .BindAsync(product => product.UpdateAsync(request))
        // Save changes (critical)
        .TapAsync(product => _repository.UpdateAsync(product))
        // Invalidate cache (best effort)
        .TapAsync(async product =>
        {
            try
            {
                await _cache.InvalidateAsync($"product:{product.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate cache for product {ProductId}", product.Id);
            }
        })
        // Notify subscribers (best effort)
        .TapAsync(async product =>
        {
            try
            {
                await _notificationService.NotifyProductUpdateAsync(product);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to notify subscribers for product {ProductId}", product.Id);
            }
        })
        .ToActionResultAsync(this);
