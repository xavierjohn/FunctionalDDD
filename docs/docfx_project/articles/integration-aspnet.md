# ASP.NET Core Integration

**Level:** Intermediate ?? | **Time:** 20-30 min | **Prerequisites:** [Basics](basics.md)

Integrate Railway-Oriented Programming with ASP.NET Core using the **FunctionalDDD.Asp** package. This package provides extension methods to convert `Result<T>` to HTTP responses with automatic error-to-status-code mapping and Problem Details (RFC 7807) support.

> **Note:** This guide focuses on **ASP.NET Core integration only**. For validation, see [FluentValidation Integration](integration-fluentvalidation.md). For data access, see [Entity Framework Core Integration](integration-ef.md).

## Table of Contents

- [Installation](#installation)
- [What the Package Provides](#what-the-package-provides)
- [MVC Controllers](#mvc-controllers)
- [Minimal API](#minimal-api)
- [Automatic Error Mapping](#automatic-error-mapping)
- [Custom Error Responses](#custom-error-responses)
- [Pagination Support](#pagination-support)
- [Best Practices](#best-practices)

## Installation

```bash
dotnet add package FunctionalDDD.Asp
```

## What the Package Provides

The **FunctionalDDD.Asp** package provides extension methods to convert `Result<T>` to HTTP responses:

### Core Extension Methods

**For MVC Controllers:**
```csharp
ActionResult<T> ToActionResult<T>(this Result<T> result, ControllerBase controller);
Task<ActionResult<T>> ToActionResultAsync<T>(this Task<Result<T>> resultTask, ControllerBase controller);

// Pagination support
ActionResult<T> ToActionResult<T>(
    this Result<T> result, 
    ControllerBase controller,
    long from, 
    long to, 
    long totalCount);
```

**For Minimal API:**
```csharp
IResult ToHttpResult<T>(this Result<T> result);
Task<IResult> ToHttpResultAsync<T>(this Task<Result<T>> resultTask);
```

**What happens:**
- ? **Success**: Returns appropriate HTTP status (200 OK, 201 Created, 204 No Content)
- ? **Failure**: Converts error types to HTTP status codes with Problem Details format
- ?? **Pagination**: Returns 206 Partial Content with Content-Range headers

## MVC Controllers

Use `ToActionResult` to convert `Result<T>` to `ActionResult<T>`:

### Simple Example

```csharp
using FunctionalDdd;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken ct)
        => await _userService.CreateUserAsync(request, ct)
            .MapAsync(user => new UserDto(user))
            .ToActionResultAsync(this);  // Converts Result<UserDto> ? ActionResult<UserDto>

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(
        string id,
        CancellationToken ct)
        => await _userService.GetUserByIdAsync(id, ct)
            .MapAsync(user => new UserDto(user))
            .ToActionResultAsync(this);

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(
        string id,
        [FromBody] UpdateUserRequest request,
        CancellationToken ct)
        => await _userService.UpdateUserAsync(id, request, ct)
            .MapAsync(user => new UserDto(user))
            .ToActionResultAsync(this);

    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteUser(
        string id,
        CancellationToken ct)
        => await _userService.DeleteUserAsync(id, ct)
            .ToActionResultAsync(this);  // Returns 204 No Content on success
}
```

**Key Points:**
- Controller accepts requests and calls service layer
- Service returns `Result<T>` (success or failure)
- `ToActionResultAsync` converts `Result<T>` ? `ActionResult<T>` at the API boundary
- Automatic error-to-HTTP status mapping (see [Automatic Error Mapping](#automatic-error-mapping))

> **Note:** The service layer (`IUserService`) can use any architecture you prefer. See [Examples](examples.md) for complete application examples with different architectural patterns.

## Minimal API

Use `ToHttpResult` for Minimal API endpoints:

```csharp
using FunctionalDdd;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

var userApi = app.MapGroup("/api/users")
    .WithTags("Users")
    .WithOpenApi();

userApi.MapPost("/", async (
    CreateUserRequest request,
    IUserService userService,
    CancellationToken ct) =>
    await userService.CreateUserAsync(request, ct)
        .MapAsync(user => new UserDto(user))
        .ToHttpResultAsync())  // Converts Result<UserDto> ? IResult
    .WithName("CreateUser")
    .Produces<UserDto>(StatusCodes.Status200OK)
    .ProducesValidationProblem()
    .ProducesProblem(StatusCodes.Status409Conflict);

userApi.MapGet("/{id}", async (
    string id,
    IUserService userService,
    CancellationToken ct) =>
    await userService.GetUserByIdAsync(id, ct)
        .MapAsync(user => new UserDto(user))
        .ToHttpResultAsync())
    .WithName("GetUser")
    .Produces<UserDto>()
    .ProducesProblem(StatusCodes.Status404NotFound);

userApi.MapPut("/{id}", async (
    string id,
    UpdateUserRequest request,
    IUserService userService,
    CancellationToken ct) =>
    await userService.UpdateUserAsync(id, request, ct)
        .MapAsync(user => new UserDto(user))
        .ToHttpResultAsync())
    .WithName("UpdateUser")
    .Produces<UserDto>()
    .ProducesValidationProblem()
    .ProducesProblem(StatusCodes.Status404NotFound);

userApi.MapDelete("/{id}", async (
    string id,
    IUserService userService,
    CancellationToken ct) =>
    await userService.DeleteUserAsync(id, ct)
        .ToHttpResultAsync())  // Returns 204 No Content on success
    .WithName("DeleteUser")
    .Produces(StatusCodes.Status204NoContent)
    .ProducesProblem(StatusCodes.Status404NotFound);

app.Run();
```

## Automatic Error Mapping

The package automatically maps error types to HTTP status codes:

| Error Type | HTTP Status | Example Use Case |
|------------|-------------|------------------|
| `ValidationError` | 400 Bad Request | Invalid email format, required field missing |
| `BadRequestError` | 400 Bad Request | Malformed request, invalid query parameters |
| `UnauthorizedError` | 401 Unauthorized | Missing authentication token |
| `ForbiddenError` | 403 Forbidden | Insufficient permissions for action |
| `NotFoundError` | 404 Not Found | User not found, resource doesn't exist |
| `ConflictError` | 409 Conflict | Duplicate email, concurrent modification |
| `DomainError` | 422 Unprocessable Entity | Business rule violation |
| `RateLimitError` | 429 Too Many Requests | API rate limit exceeded |
| `UnexpectedError` | 500 Internal Server Error | Database connection failed |
| `ServiceUnavailableError` | 503 Service Unavailable | Service under maintenance |
| `AggregateError` | Varies | Multiple errors (uses first error's status) |

**Key Features:**
- ? **Automatic Status Codes** - No manual mapping required
- ? **Problem Details (RFC 7807)** - Standard error response format
- ? **Validation Error Formatting** - Field-level errors
- ? **Unit Type Support** - `Result<Unit>` returns 204 No Content
- ? **Async Support** - Full async/await with `CancellationToken`

### Example: Validation Error Response

**Request:**
```http
POST /api/users HTTP/1.1
Content-Type: application/json

{
  "email": "",
  "firstName": "John",
  "lastName": "",
  "age": 15
}
```

**Response:**
```http
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "User registration validation failed",
  "instance": "/api/users",
  "errors": {
    "email": ["Email is required"],
    "lastName": ["Last name is required"],
    "age": ["Must be 18 or older"]
  }
}
```

### Example: Not Found Error Response

**Request:**
```http
GET /api/users/12345 HTTP/1.1
```

**Response:**
```http
HTTP/1.1 404 Not Found
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "User 12345 not found",
  "instance": "/api/users/12345"
}
```

## Custom Error Responses

Use `MatchError` for custom error handling when you need more control than automatic mapping provides:

```csharp
app.MapPost("/orders", async (
    CreateOrderRequest request,
    IOrderService orderService,
    CancellationToken ct) =>
{
    return await orderService.ProcessOrderAsync(request, ct)
        .MatchErrorAsync(
            onValidation: err => Results.BadRequest(new 
            { 
                message = "Validation failed",
                errors = err.FieldErrors
                    .ToDictionary(
                        f => f.FieldName, 
                        f => f.Details.ToArray())
            }),
            onNotFound: err => 
                Results.NotFound(new { message = err.Detail }),
            onConflict: err => 
                Results.Conflict(new { message = err.Detail }),
            onDomain: err =>
                Results.Problem(
                    detail: err.Detail,
                    statusCode: StatusCodes.Status422UnprocessableEntity),
            onSuccess: order => 
                Results.Created($"/orders/{order.Id}", order),
            cancellationToken: ct
        );
});
```

**Use `MatchError` when:**
- You need custom response payloads
- You want different error handling per endpoint
- You need to add custom headers or cookies
- Default Problem Details format doesn't fit your needs

**Use `ToActionResult`/`ToHttpResult` when:**
- Standard Problem Details format is sufficient
- You want consistent error responses across your API
- You don't need custom error logic per endpoint

## Pagination Support

The package provides built-in support for HTTP 206 Partial Content responses with Content-Range headers:

### Basic Pagination

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersAsync(
    [FromQuery] int page = 0,
    [FromQuery] int pageSize = 25,
    CancellationToken ct)
{
    var from = page * pageSize;
    var to = from + pageSize - 1;
    
    var result = await _userService.GetPagedUsersAsync(from, pageSize, ct);
    
    // Automatically returns:
    // - 200 OK if all items fit in one page (to >= totalCount - 1)
    // - 206 Partial Content with Content-Range header if partial results
    return result
        .Map(pagedData => (pagedData.Items, pagedData.TotalCount))
        .Map(x => x.Items.Select(u => new UserDto(u)))
        .ToActionResult(this, from, to, result.Value.TotalCount);
}
```

**Response (partial content):**
```http
HTTP/1.1 206 Partial Content
Content-Range: items 0-24/100
Content-Type: application/json

[
  { "id": "1", "email": "user1@example.com", ... },
  { "id": "2", "email": "user2@example.com", ... },
  ...
]
```

**Response (complete):**
```http
HTTP/1.1 200 OK
Content-Type: application/json

[
  { "id": "1", "email": "user1@example.com", ... },
  ...
]
```

### Advanced Pagination with Custom Range Extraction

```csharp
public record PagedResult<T>(
    IEnumerable<T> Items, 
    long From, 
    long To, 
    long TotalCount);

[HttpGet]
public ActionResult<IEnumerable<UserDto>> GetUsers(
    [FromQuery] int page = 0,
    [FromQuery] int pageSize = 25)
{
    return _userService
        .GetPagedUsers(page, pageSize)
        .ToActionResult(
            this,
            funcRange: pagedResult => new ContentRangeHeaderValue(
                pagedResult.From,
                pagedResult.To,
                pagedResult.TotalCount)
            {
                Unit = "items"
            },
            funcValue: pagedResult => pagedResult.Items.Select(u => new UserDto(u))
        );
}
```

## Best Practices

### 1. Convert at API Boundaries Only

Keep `Result<T>` types internal to your application. Convert to HTTP responses only at the controller/endpoint level.

```csharp
// ? Good - Result stays in application/domain layer
public class UserService
{
    public async Task<Result<User>> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken ct)
    {
        return await EmailAddress.TryCreate(request.Email)
            .Combine(FirstName.TryCreate(request.FirstName))
            .BindAsync(async (email, first) => 
                await User.CreateAsync(email, first, ct), ct);
    }
}

[HttpPost]
public async Task<ActionResult<UserDto>> CreateUser(
    CreateUserRequest request,
    CancellationToken ct) =>
    await _userService.CreateUserAsync(request, ct)
        .MapAsync(user => new UserDto(user))
        .ToActionResultAsync(this);  // ? Convert at boundary

// ? Bad - exposing Result in controller return type
public async Task<Result<User>> CreateUser(...)
```

### 2. Always Pass CancellationToken

Support graceful cancellation in async operations:

```csharp
[HttpPost]
public async Task<ActionResult<Order>> ProcessOrder(
    CreateOrderRequest request,
    CancellationToken ct)  // ? Accept CancellationToken
    => await _orderService.ProcessOrderAsync(request, ct)
        .ToActionResultAsync(this);
```

### 3. Use Unit for Side-Effect Operations

Operations that don't return data should return `Result<Unit>`:

```csharp
[HttpDelete("{id}")]
public async Task<ActionResult<Unit>> DeleteUser(
    string id,
    CancellationToken ct) =>
    await _userService.DeleteUserAsync(id, ct)
        .ToActionResultAsync(this);
// ? Automatically returns 204 No Content on success
```

### 4. Use Consistent Error Messages

Structure error messages with context for better Problem Details responses:

```csharp
// ? Good - includes context
Error.NotFound($"User {userId} not found", userId.ToString())
Error.Validation("Email format is invalid", "email")
Error.Conflict("Email already in use", $"email:{email}")

// ? Bad - generic, no context
Error.NotFound("Not found")
Error.Validation("Invalid")
```

### 5. Prefer Automatic Mapping Over Custom Logic

Use `ToActionResult`/`ToHttpResult` for consistent error responses. Only use `MatchError` when you need custom logic:

```csharp
// ? Good - consistent Problem Details across API
[HttpPost]
public async Task<ActionResult<User>> CreateUser(CreateUserRequest request, CancellationToken ct)
    => await _userService.CreateUserAsync(request, ct)
        .ToActionResultAsync(this);

// ?? Use only when necessary - custom error handling
app.MapPost("/special-endpoint", async (request, service, ct) =>
    await service.ProcessAsync(request, ct)
        .MatchErrorAsync(
            onValidation: err => CustomValidationResponse(err),
            onSuccess: result => CustomSuccessResponse(result),
            cancellationToken: ct));
```

## Next Steps

- Learn about [FluentValidation Integration](integration-fluentvalidation.md) for validation before HTTP conversion
- See [Entity Framework Core Integration](integration-ef.md) for repository patterns that return `Result<T>`
- Review [Observability](integration-observability.md) for OpenTelemetry tracing and Problem Details correlation
- Check [Error Handling](error-handling.md) for working with different error types
- See [Examples](examples.md) for complete working applications
