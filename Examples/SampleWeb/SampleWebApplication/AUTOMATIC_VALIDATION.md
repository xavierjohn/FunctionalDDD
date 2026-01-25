# Automatic Value Object Validation

This example demonstrates the new automatic validation feature for scalar value objects in ASP.NET Core.

## Setup

Add one line to your `Program.cs`:

```csharp
using FunctionalDdd;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddScalarValueObjectValidation(); // ← Add this line
```

## Before vs After

### ❌ Before: Manual Validation with Result.Combine()

```csharp
public record RegisterUserRequest(
    string firstName,
    string lastName,
    string email,
    string password
);

[HttpPost("register")]
public ActionResult<User> Register([FromBody] RegisterUserRequest request) =>
    FirstName.TryCreate(request.firstName)
    .Combine(LastName.TryCreate(request.lastName))
    .Combine(EmailAddress.TryCreate(request.email))
    .Bind((firstName, lastName, email) =>
        User.TryCreate(firstName, lastName, email, request.password))
    .ToActionResult(this);
```

**Problems:**
- Manual `TryCreate()` calls for each field
- Verbose `Combine()` chaining
- Error-prone - easy to forget a field
- No compile-time safety

### ✅ After: Automatic Validation with Value Objects in DTO

```csharp
public record RegisterUserDto
{
    public FirstName FirstName { get; init; } = null!;
    public LastName LastName { get; init; } = null!;
    public EmailAddress Email { get; init; } = null!;
    public string Password { get; init; } = null!;
}

[HttpPost("RegisterWithAutoValidation")]
public ActionResult<User> RegisterWithAutoValidation([FromBody] RegisterUserDto dto)
{
    // If we reach here, all value objects are already validated!
    // The [ApiController] attribute automatically returns 400 if validation fails.

    Result<User> userResult = User.TryCreate(
        dto.FirstName,
        dto.LastName,
        dto.Email,
        dto.Password);

    return userResult.ToActionResult(this);
}
```

**Benefits:**
- ✅ No manual `TryCreate()` calls
- ✅ No `Combine()` chaining
- ✅ Validation happens automatically during model binding
- ✅ Compile-time safety - can't forget a field
- ✅ Clean, readable code
- ✅ Standard ASP.NET Core validation pipeline

## How It Works

1. **Model Binding**: When a request comes in, ASP.NET Core uses the `ScalarValueObjectModelBinder`
2. **Automatic Validation**: The binder calls `TryCreate()` on each value object automatically
3. **Error Collection**: Validation errors are added to `ModelState`
4. **Automatic 400 Response**: The `[ApiController]` attribute returns 400 Bad Request if `ModelState` is invalid
5. **Your Controller**: Only executed if all validations pass

## Testing with .http File

See [Register.http](Requests/Register.http) for example requests:

```http
### Success - All validations pass
POST {{host}}/users/RegisterWithAutoValidation
Content-Type: application/json

{
    "firstName": "Xavier",
    "lastName": "John",
    "email": "xavier@example.com",
    "password": "SecurePass123!"
}

### Invalid Email - Returns 400 automatically
POST {{host}}/users/RegisterWithAutoValidation
Content-Type: application/json

{
    "firstName": "Xavier",
    "lastName": "John",
    "email": "not-an-email",
    "password": "SecurePass123!"
}
```

## Response Examples

### Success Response (200 OK)
```json
{
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "firstName": "Xavier",
    "lastName": "John",
    "email": "xavier@example.com"
}
```

### Validation Error Response (400 Bad Request)
```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Email": [
            "Email address must contain an @ symbol"
        ]
    }
}
```

### Multiple Validation Errors (400 Bad Request)
```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "FirstName": [
            "Value cannot be null or empty"
        ],
        "Email": [
            "Email address must contain an @ symbol"
        ]
    }
}
```

## Key Points

- **Zero Reflection for TryCreate**: The CRTP pattern enables direct interface calls
- **Reflection Only for Discovery**: Used only to detect which types need validation
- **Opt-In**: Only works when you add `.AddScalarValueObjectValidation()`
- **Works with Any Value Object**: Supports `ScalarValueObject<TSelf, T>`, `RequiredString<TSelf>`, `RequiredGuid<TSelf>`, etc.
- **Compatible with Existing Code**: Old manual approach still works fine

## Technical Details

The automatic validation uses:
1. **IScalarValueObject<TSelf, T>** interface with static abstract `TryCreate(T)` method
2. **ScalarValueObjectModelBinder** that calls `TryCreate()` during model binding
3. **ScalarValueObjectModelBinderProvider** that detects value object types
4. **ScalarValueObjectJsonConverter** for JSON serialization/deserialization
5. **CRTP (Curiously Recurring Template Pattern)** for compile-time type safety

See the [implementation plan](../../IMPLEMENTATION_PLAN.md) for full technical details.
