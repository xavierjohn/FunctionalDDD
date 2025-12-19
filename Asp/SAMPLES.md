# ASP Extension - Comprehensive Examples

This document provides detailed examples and advanced patterns for using the ASP extension with Railway Oriented Programming in ASP.NET Core applications.

## Table of Contents

- [MVC Controllers](#mvc-controllers)
  - [Basic CRUD Operations](#basic-crud-operations)
  - [Async Operations with CancellationToken](#async-operations-with-cancellationtoken)
  - [Complex Validation Scenarios](#complex-validation-scenarios)
  - [Custom Status Codes](#custom-status-codes)
  - [Pagination Support](#pagination-support)
- [Minimal API](#minimal-api)
  - [Basic Endpoints](#basic-endpoints)
  - [Async Operations](#async-operations)
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

### Async Operations with CancellationToken

Best practices for async operations with proper cancellation support:

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IInventoryService _inventoryService;
    private readonly IPaymentService _paymentService;
    private readonly IShippingService _shippingService;

    // Async CREATE with side effects
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrderAsync(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken) =>
        await ValidateOrderAsync(request, cancellationToken)
            .BindAsync(
                async (validatedRequest, ct) => await _orderService.CreateAsync(validatedRequest, ct),
                cancellationToken)
            .TapAsync(
                async (order, ct) => await _inventoryService.ReserveAsync(order.Items, ct),
                cancellationToken)
            .TapAsync(
                async (order, ct) => await _emailService.SendOrderConfirmationAsync(order, ct),
                cancellationToken)
            .ToActionResultAsync(this);

    // Async READ with complex validation
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrderAsync(
        string id,
        CancellationToken cancellationToken) =>
        await _orderService.GetByIdAsync(id, cancellationToken)
            .ToResultAsync(Error.NotFound($"Order {id} not found"))
            .EnsureAsync(
                async (order, ct) => await CanAccessOrderAsync(User, order, ct),
                Error.Forbidden("Access denied to this order"),
                cancellationToken)
            .ToActionResultAsync(this);

    // Async UPDATE with multiple steps
    [HttpPut("{id}/submit")]
    public async Task<ActionResult<Order>> SubmitOrderAsync(
        string id,
        CancellationToken cancellationToken) =>
        await _orderService.GetByIdAsync(id, cancellationToken)
            .ToResultAsync(Error.NotFound($"Order {id} not found"))
            .BindAsync(
                async (order, ct) => await order.SubmitAsync(ct),
                cancellationToken)
            .BindAsync(
                async (order, ct) => await _paymentService.ProcessAsync(order, ct),
                cancellationToken)
            .TapAsync(
                async (order, ct) => await _orderService.SaveAsync(order, ct),
                cancellationToken)
            .TapAsync(
                async (order, ct) => await _shippingService.ScheduleAsync(order, ct),
                cancellationToken)
            .ToActionResultAsync(this);

    // Async DELETE with cleanup
    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> CancelOrderAsync(
        string id,
        CancellationToken cancellationToken) =>
        await _orderService.GetByIdAsync(id, cancellationToken)
            .ToResultAsync(Error.NotFound($"Order {id} not found"))
            .EnsureAsync(
                async (order, ct) => await order.CanCancelAsync(ct),
                Error.Conflict("Order cannot be cancelled"),
                cancellationToken)
            .TapAsync(
                async (order, ct) => await _inventoryService.ReleaseAsync(order.Items, ct),
                cancellationToken)
            .TapAsync(
                async (order, ct) => await _paymentService.RefundAsync(order, ct),
                cancellationToken)
            .TapAsync(
                async (order, ct) => await _orderService.DeleteAsync(order, ct),
                cancellationToken)
            .Map(_ => Result.Success())
            .ToActionResultAsync(this);

    private async Task<Result<CreateOrderRequest>> ValidateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken) =>
        await _inventoryService.CheckAvailabilityAsync(request.Items, cancellationToken)
            .EnsureAsync(
                async (available, ct) => available,
                Error.Conflict("Some items are out of stock"),
                cancellationToken)
            .Map(_ => request);

    private async Task<bool> CanAccessOrderAsync(
        ClaimsPrincipal user,
        Order order,
        CancellationToken cancellationToken) =>
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

Async endpoints with proper CancellationToken support:

```csharp
var orderApi = app.MapGroup("/api/orders");

// Async CREATE
orderApi.MapPost("/", async (
    CreateOrderRequest request,
    IOrderService orderService,
    IInventoryService inventoryService,
    IEmailService emailService,
    CancellationToken cancellationToken) =>
    await orderService.CreateAsync(request, cancellationToken)
        .TapAsync(
            async (order, ct) => await inventoryService.ReserveAsync(order.Items, ct),
            cancellationToken)
        .TapAsync(
            async (order, ct) => await emailService.SendConfirmationAsync(order, ct),
            cancellationToken)
        .ToHttpResultAsync());

// Async READ
orderApi.MapGet("/{id}", async (
    string id,
    IOrderService orderService,
    CancellationToken cancellationToken) =>
    await orderService.GetByIdAsync(id, cancellationToken)
        .ToResultAsync(Error.NotFound($"Order {id} not found"))
        .ToHttpResultAsync());

// Async UPDATE
orderApi.MapPut("/{id}/submit", async (
    string id,
    IOrderService orderService,
    IPaymentService paymentService,
    CancellationToken cancellationToken) =>
    await orderService.GetByIdAsync(id, cancellationToken)
        .ToResultAsync(Error.NotFound($"Order {id} not found"))
        .BindAsync(
            async (order, ct) => await order.SubmitAsync(ct),
            cancellationToken)
        .BindAsync(
            async (order, ct) => await paymentService.ProcessAsync(order, ct),
            cancellationToken)
        .TapAsync(
            async (order, ct) => await orderService.SaveAsync(order, ct),
            cancellationToken)
        .ToHttpResultAsync());

// Async DELETE
orderApi.MapDelete("/{id}", async (
    string id,
    IOrderService orderService,
    IInventoryService inventoryService,
    IPaymentService paymentService,
    CancellationToken cancellationToken) =>
    await orderService.GetByIdAsync(id, cancellationToken)
        .ToResultAsync(Error.NotFound($"Order {id} not found"))
        .TapAsync(
            async (order, ct) => await inventoryService.ReleaseAsync(order.Items, ct),
            cancellationToken)
        .TapAsync(
            async (order, ct) => await paymentService.RefundAsync(order, ct),
            cancellationToken)
        .TapAsync(
            async (order, ct) => await orderService.DeleteAsync(order, ct),
            cancellationToken)
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
    IUserRepository repository,
    CancellationToken cancellationToken) =>
    await requests.TraverseAsync(
        async (request, ct) =>
            await FirstName.TryCreate(request.FirstName)
                .Combine(LastName.TryCreate(request.LastName))
                .Combine(EmailAddress.TryCreate(request.Email))
                .BindAsync(
                    async (firstName, lastName, email, innerCt) =>
                        await User.TryCreateAsync(firstName, lastName, email, request.Password, innerCt),
                    ct)
                .TapAsync(
                    async (user, innerCt) => await repository.AddAsync(user, innerCt),
                    ct),
        cancellationToken)
    .ToHttpResultAsync());

// Complex validation chain
var validationApi = app.MapGroup("/api/validate");

validationApi.MapPost("/order", async (
    CreateOrderRequest request,
    IInventoryService inventoryService,
    ICustomerService customerService,
    IPricingService pricingService,
    CancellationToken cancellationToken) =>
    await customerService.ValidateCustomerAsync(request.CustomerId, cancellationToken)
        .BindAsync(
            async (customer, ct) => await inventoryService.ValidateItemsAsync(request.Items, ct)
                .Map(_ => customer),
            cancellationToken)
        .BindAsync(
            async (customer, ct) => await pricingService.CalculateTotalAsync(request.Items, ct)
                .Map(total => (customer, total)),
            cancellationToken)
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
    IShippingService shippingService,
    CancellationToken cancellationToken) =>
    await orderService.GetByIdAsync(id, cancellationToken)
        .ToResultAsync(Error.NotFound($"Order {id} not found"))
        .BindAsync(
            async (order, ct) =>
            {
                if (order.RequiresPayment)
                    return await paymentService.ProcessAsync(order, ct);
                return order.ToResult();
            },
            cancellationToken)
        .BindAsync(
            async (order, ct) =>
            {
                if (order.RequiresShipping)
                    return await shippingService.ScheduleAsync(order, ct);
                return order.ToResult();
            },
            cancellationToken)
        .TapAsync(
            async (order, ct) => await orderService.SaveAsync(order, ct),
            cancellationToken)
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
public async Task<ActionResult<Order>> GetOrderAsync(
    string id,
    CancellationToken cancellationToken) =>
    await _orderService.GetByIdAsync(id, cancellationToken)
        .ToResultAsync(Error.NotFound($"Order {id} not found"))
        // Check ownership
        .EnsureAsync(
            async (order, ct) => await IsOwnerAsync(User, order, ct),
            Error.Forbidden("You don't have permission to view this order"),
            cancellationToken)
        // Check subscription level
        .EnsureAsync(
            async (order, ct) => await HasRequiredSubscriptionAsync(User, order, ct),
            Error.Forbidden("Your subscription level doesn't allow viewing this order type"),
            cancellationToken)
        .ToActionResultAsync(this);

private async Task<bool> IsOwnerAsync(
    ClaimsPrincipal user,
    Order order,
    CancellationToken cancellationToken)
{
    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    return await Task.FromResult(
        user.IsInRole("Admin") || userId == order.UserId);
}

private async Task<bool> HasRequiredSubscriptionAsync(
    ClaimsPrincipal user,
    Order order,
    CancellationToken cancellationToken)
{
    var subscription = await _subscriptionService.GetUserSubscriptionAsync(
        user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        cancellationToken);
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
    [FromBody] UpdateEmailRequest request,
    CancellationToken cancellationToken) =>
    await _userRepository.GetByIdAsync(id, cancellationToken)
        .ToResultAsync(Error.NotFound($"User {id} not found"))
        .BindAsync(
            async (user, ct) =>
                await EmailAddress.TryCreate(request.NewEmail)
                    .EnsureAsync(
                        async (email, innerCt) => !await _userRepository.ExistsByEmailAsync(email, innerCt),
                        Error.Conflict("Email already in use"),
                        ct)
                    .BindAsync(
                        async (email, innerCt) => await user.UpdateEmailAsync(email, innerCt),
                        ct),
            cancellationToken)
        .TapAsync(
            async (user, ct) => await _userRepository.SaveAsync(user, ct),
            cancellationToken)
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
    private readonly IInventoryService _inventoryService;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public async Task<Result<Order>> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken) =>
        await ValidateCustomerAsync(request.CustomerId, cancellationToken)
            .BindAsync(
                async (customer, ct) => await CreateOrderLinesAsync(request.Items, ct)
                    .Map(items => (customer, items)),
                cancellationToken)
            .BindAsync(
                async (tuple, ct) => await Order.TryCreateAsync(
                    tuple.customer.Id,
                    tuple.items,
                    ct),
                cancellationToken)
            .TapAsync(
                async (order, ct) => await _repository.AddAsync(order, ct),
                cancellationToken)
            .TapAsync(
                async (order, ct) => await _eventDispatcher.DispatchAsync(order.DomainEvents, ct),
                cancellationToken);

    private async Task<Result<Customer>> ValidateCustomerAsync(
        string customerId,
        CancellationToken cancellationToken) =>
        await _customerRepository.GetByIdAsync(customerId, cancellationToken)
            .ToResultAsync(Error.NotFound($"Customer {customerId} not found"))
            .EnsureAsync(
                async (customer, ct) => await customer.IsActiveAsync(ct),
                Error.Conflict("Customer account is inactive"),
                cancellationToken);

    private async Task<Result<IEnumerable<OrderLine>>> CreateOrderLinesAsync(
        IEnumerable<OrderLineRequest> requests,
        CancellationToken cancellationToken) =>
        await requests.TraverseAsync(
            async (request, ct) =>
                await _inventoryService.GetProductAsync(request.ProductId, ct)
                    .ToResultAsync(Error.NotFound($"Product {request.ProductId} not found"))
                    .BindAsync(
                        async (product, innerCt) => await OrderLine.TryCreateAsync(
                            product.Id,
                            product.Name,
                            product.Price,
                            request.Quantity,
                            innerCt),
                        ct),
            cancellationToken);
}

// Controller Layer
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrderAsync(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken) =>
        await _orderService.CreateAsync(request, cancellationToken)
            .ToActionResultAsync(this);

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrderAsync(
        string id,
        CancellationToken cancellationToken) =>
        await _orderService.GetByIdAsync(id, cancellationToken)
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

    public async Task<Result<User>> GetByIdAsync(
        string id,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
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

    public async Task<Result<Unit>> AddAsync(
        User user,
        CancellationToken cancellationToken)
    {
        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);
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

    public async Task<Result<Unit>> CommitAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
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
    [FromBody] TransferRequest request,
    CancellationToken cancellationToken) =>
    await _accountRepository.GetByIdAsync(request.FromAccountId, cancellationToken)
        .ToResultAsync(Error.NotFound($"Account {request.FromAccountId} not found"))
        .BindAsync(
            async (fromAccount, ct) =>
                await _accountRepository.GetByIdAsync(request.ToAccountId, ct)
                    .ToResultAsync(Error.NotFound($"Account {request.ToAccountId} not found"))
                    .Map(toAccount => (fromAccount, toAccount)),
            cancellationToken)
        .BindAsync(
            async (tuple, ct) =>
                await tuple.fromAccount.WithdrawAsync(request.Amount, ct)
                    .BindAsync(
                        async (_, innerCt) => await tuple.toAccount.DepositAsync(request.Amount, innerCt),
                        ct)
                    .Map(_ => (tuple.fromAccount, tuple.toAccount)),
            cancellationToken)
        .BindAsync(
            async (tuple, ct) => await _unitOfWork.CommitAsync(ct)
                .Map(_ => new TransferResult(tuple.fromAccount.Id, tuple.toAccount.Id, request.Amount)),
            cancellationToken)
        .ToActionResultAsync(this);
```

## Advanced Scenarios

### Batch Operations

Processing multiple items with Traverse:

```csharp
[HttpPost("batch/users")]
public async Task<ActionResult<BatchResult<User>>> CreateUsersAsync(
    [FromBody] List<CreateUserRequest> requests,
    CancellationToken cancellationToken) =>
    await requests.TraverseAsync(
        async (request, ct) =>
            await FirstName.TryCreate(request.FirstName)
                .Combine(LastName.TryCreate(request.LastName))
                .Combine(EmailAddress.TryCreate(request.Email))
                .BindAsync(
                    async (firstName, lastName, email, innerCt) =>
                        await User.TryCreateAsync(firstName, lastName, email, request.Password, innerCt),
                    ct)
                .TapAsync(
                    async (user, innerCt) => await _repository.AddAsync(user, innerCt),
                    ct),
        cancellationToken)
    .Map(users => new BatchResult<User>(users.Count(), users))
    .ToActionResultAsync(this);

[HttpPut("batch/activate")]
public async Task<ActionResult<Unit>> ActivateUsersAsync(
    [FromBody] List<string> userIds,
    CancellationToken cancellationToken) =>
    await userIds.TraverseAsync(
        async (id, ct) =>
            await _repository.GetByIdAsync(id, ct)
                .ToResultAsync(Error.NotFound($"User {id} not found"))
                .BindAsync(
                    async (user, innerCt) => await user.ActivateAsync(innerCt),
                    ct)
                .TapAsync(
                    async (user, innerCt) => await _repository.UpdateAsync(user, innerCt),
                    ct),
        cancellationToken)
    .Map(_ => Result.Success())
    .ToActionResultAsync(this);
```

### Conditional Processing

Conditional logic in pipelines:

```csharp
[HttpPost("{id}/process")]
public async Task<ActionResult<Order>> ProcessOrderAsync(
    string id,
    CancellationToken cancellationToken) =>
    await _orderRepository.GetByIdAsync(id, cancellationToken)
        .ToResultAsync(Error.NotFound($"Order {id} not found"))
        // Conditionally apply discount
        .BindAsync(
            async (order, ct) =>
            {
                if (order.Customer.IsVIP)
                    return await order.ApplyDiscountAsync(0.1m, ct);
                return order.ToResult();
            },
            cancellationToken)
        // Conditionally process payment
        .BindAsync(
            async (order, ct) =>
            {
                if (order.Total > 0)
                    return await _paymentService.ProcessAsync(order, ct);
                return order.ToResult();
            },
            cancellationToken)
        // Conditionally schedule shipping
        .BindAsync(
            async (order, ct) =>
            {
                if (order.RequiresShipping)
                    return await _shippingService.ScheduleAsync(order, ct);
                return order.ToResult();
            },
            cancellationToken)
        .TapAsync(
            async (order, ct) => await _orderRepository.SaveAsync(order, ct),
            cancellationToken)
        .ToActionResultAsync(this);
```

### Side Effects with Tap

Executing side effects that shouldn't affect the pipeline:

```csharp
[HttpPost]
public async Task<ActionResult<User>> RegisterAsync(
    [FromBody] RegisterRequest request,
    CancellationToken cancellationToken) =>
    await FirstName.TryCreate(request.FirstName)
        .Combine(LastName.TryCreate(request.LastName))
        .Combine(EmailAddress.TryCreate(request.Email))
        .BindAsync(
            async (firstName, lastName, email, ct) =>
                await User.TryCreateAsync(firstName, lastName, email, request.Password, ct),
            cancellationToken)
        // Save user (critical)
        .TapAsync(
            async (user, ct) => await _repository.AddAsync(user, ct),
            cancellationToken)
        // Send welcome email (non-critical, failures don't affect result)
        .TapAsync(
            async (user, ct) =>
            {
                try
                {
                    await _emailService.SendWelcomeEmailAsync(user.Email, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send welcome email to {Email}", user.Email);
                }
            },
            cancellationToken)
        // Log analytics (non-critical)
        .TapAsync(
            async (user, ct) =>
            {
                try
                {
                    await _analyticsService.TrackUserRegistrationAsync(user, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to track registration for {UserId}", user.Id);
                }
            },
            cancellationToken)
        .ToActionResultAsync(this);

[HttpPut("{id}")]
public async Task<ActionResult<Product>> UpdateProductAsync(
    string id,
    [FromBody] UpdateProductRequest request,
    CancellationToken cancellationToken) =>
    await _repository.GetByIdAsync(id, cancellationToken)
        .ToResultAsync(Error.NotFound($"Product {id} not found"))
        .BindAsync(
            async (product, ct) => await product.UpdateAsync(request, ct),
            cancellationToken)
        // Save changes (critical)
        .TapAsync(
            async (product, ct) => await _repository.UpdateAsync(product, ct),
            cancellationToken)
        // Invalidate cache (best effort)
        .TapAsync(
            async (product, ct) =>
            {
                try
                {
                    await _cache.InvalidateAsync($"product:{product.Id}", ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to invalidate cache for product {ProductId}", product.Id);
                }
            },
            cancellationToken)
        // Notify subscribers (best effort)
        .TapAsync(
            async (product, ct) =>
            {
                try
                {
                    await _notificationService.NotifyProductUpdateAsync(product, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to notify subscribers for product {ProductId}", product.Id);
                }
            },
            cancellationToken)
        .ToActionResultAsync(this);
```
