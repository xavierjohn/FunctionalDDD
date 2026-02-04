# FDDD010: Use specific error type instead of base Error class

## Cause

Instantiating the base `Error` class directly instead of using specific error factory methods like `Error.Validation()`, `Error.NotFound()`, etc.

## Rule Description

FunctionalDDD provides specific error types (ValidationError, NotFoundError, UnauthorizedError, etc.) that enable type-safe error handling with `MatchError`.

Using the base `Error` class directly:
- Prevents type-safe pattern matching
- Loses semantic information about the error category
- Makes it harder to handle errors appropriately

## How to Fix Violations

Use specific error factory methods:

```csharp
// ❌ Bad - Base Error class
return Result.Failure<Customer>(new Error("ERR001", "Customer not found"));

// ✅ Good - Specific error type
return Result.Failure<Customer>(Error.NotFound("Customer not found"));
```

## Available Error Types

### Validation Errors
```csharp
Error.Validation("Invalid email format")
Error.Validation("Age must be positive", fieldName: "age")
```

### Not Found Errors
```csharp
Error.NotFound("Customer not found")
Error.NotFound($"Order {id} does not exist")
```

### Unauthorized Errors
```csharp
Error.Unauthorized("Invalid credentials")
Error.Unauthorized("Access denied to resource")
```

### Conflict Errors
```csharp
Error.Conflict("Email already exists")
Error.Conflict("Duplicate order number")
```

### Unexpected Errors
```csharp
Error.Unexpected("Database connection failed")
Error.Unexpected("External service unavailable")
```

## Benefits of Specific Error Types

### Type-Safe Error Handling

```csharp
return result.MatchError(
    onValidationError: ve => BadRequest(ve.Detail),
    onNotFoundError: nfe => NotFound(nfe.Detail),
    onUnauthorizedError: ue => Unauthorized(ue.Detail),
    onConflictError: ce => Conflict(ce.Detail),
    onOtherError: e => StatusCode(500, e.Detail));
```

### Clear Semantics

```csharp
// ❌ Unclear - What kind of error is this?
Error.Create("ERR001", "Operation failed")

// ✅ Clear - Immediately obvious this is a validation error
Error.Validation("Email is required", "email")
```

## Example

```csharp
public class Customer : ValueObject
{
    private Customer(EmailAddress email, string name)
    {
        Email = email;
        Name = name;
    }

    public EmailAddress Email { get; }
    public string Name { get; }

    public static Result<Customer> Create(string emailStr, string name)
    {
        // ❌ Bad - Generic error
        if (string.IsNullOrWhiteSpace(name))
            return new Error("INVALID", "Name is required");

        // ✅ Good - Specific validation error
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("Name is required", fieldName: "name");

        return EmailAddress.TryCreate(emailStr)
            .Map(email => new Customer(email, name));
    }
}
```

## When to Suppress Warnings

This is a suggestion-level diagnostic. Suppress it if:
- You're creating a custom error type that doesn't fit the standard categories
- You're wrapping errors from external libraries

However, consider creating a custom error type that inherits from Error instead.

## Related Rules

- [FDDD005](FDDD005.md) - Consider using MatchError for error type discrimination
