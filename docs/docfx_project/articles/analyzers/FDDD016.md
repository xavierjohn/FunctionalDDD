# FDDD016: Error message should not be empty

## Cause

Creating an `Error` using factory methods like `Error.Validation()`, `Error.NotFound()`, etc. with an empty or whitespace-only message.

## Rule Description

Error messages provide crucial context for:
- **Debugging**: Understanding what went wrong
- **User feedback**: Showing meaningful messages to users
- **Logging**: Recording actionable information
- **API responses**: Returning helpful error details

Empty error messages make it impossible to understand failures without diving into the code.

## How to Fix Violations

Provide a meaningful message that describes the error:

```csharp
// ❌ Bad - Empty message
Error.Validation("");
Error.Validation("   ");
Error.NotFound(string.Empty);

// ✅ Good - Meaningful message
Error.Validation("Email address is required", "email");
Error.Validation("Password must be at least 8 characters", "password");
Error.NotFound("Customer not found");
```

## Examples

### Example 1: Validation Error

```csharp
// ❌ Bad
public static Result<Email> TryCreate(string value)
{
    if (string.IsNullOrWhiteSpace(value))
        return Error.Validation("");  // What's the error?
    // ...
}

// ✅ Good
public static Result<Email> TryCreate(string value)
{
    if (string.IsNullOrWhiteSpace(value))
        return Error.Validation("Email address is required", "email");
    // ...
}
```

### Example 2: Not Found Error

```csharp
// ❌ Bad
public async Task<Result<Customer>> GetCustomerAsync(Guid id)
{
    var customer = await _repository.FindAsync(id);
    if (customer is null)
        return Error.NotFound("");  // What wasn't found?

    return customer;
}

// ✅ Good
public async Task<Result<Customer>> GetCustomerAsync(Guid id)
{
    var customer = await _repository.FindAsync(id);
    if (customer is null)
        return Error.NotFound($"Customer with ID {id} not found");

    return customer;
}
```

### Example 3: Multiple Validation Errors

```csharp
// ❌ Bad
var errors = new List<Error>();
if (string.IsNullOrEmpty(firstName))
    errors.Add(Error.Validation(""));  // Which field?
if (string.IsNullOrEmpty(lastName))
    errors.Add(Error.Validation(""));  // Same empty message!

// ✅ Good
var errors = new List<Error>();
if (string.IsNullOrEmpty(firstName))
    errors.Add(Error.Validation("First name is required", "firstName"));
if (string.IsNullOrEmpty(lastName))
    errors.Add(Error.Validation("Last name is required", "lastName"));
```

## Best Practices for Error Messages

1. **Be specific**: "Email must contain @" not "Invalid email"
2. **Include field names**: Use the `fieldName` parameter for validation errors
3. **Be user-friendly**: Messages may be shown to end users
4. **Avoid technical jargon**: "Invalid format" not "Regex match failed"
5. **Include relevant values** (but not sensitive data): "Order {orderId} not found"

## Related Rules

- [FDDD010](FDDD010.md) - Use specific error type instead of base Error class

## See Also

- [Error Handling Best Practices](../error-handling.md)
