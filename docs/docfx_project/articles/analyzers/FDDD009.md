# FDDD009: Maybe.ToResult called without error parameter

## Cause

Converting a `Maybe<T>` to `Result<T>` using `ToResult()` without providing an error parameter to handle the `None` case.

## Rule Description

When converting from `Maybe<T>` to `Result<T>`, you must provide an `Error` to represent the `None` case. Without this error, there's no way to communicate why the value is absent.

## How to Fix Violations

Provide an error parameter to `ToResult()`:

```csharp
// ❌ Bad - No error parameter
return maybeCustomer.ToResult();
// Compilation error: ToResult requires an error parameter

// ✅ Good - Error describes the None case
return maybeCustomer.ToResult(
    Error.NotFound("Customer not found"));
```

## Example

```csharp
public Result<CustomerDto> GetCustomerDto(Guid id)
{
    // ❌ Bad - What error should be returned if customer not found?
    return repository.FindById(id)  // Returns Maybe<Customer>
        .ToResult()  // ❌ Missing error parameter
        .Map(customer => customer.ToDto());

    // ✅ Good - Clear error for the None case
    return repository.FindById(id)
        .ToResult(Error.NotFound($"Customer {id} not found"))
        .Map(customer => customer.ToDto());
}
```

## Best Practices

Choose an appropriate error type:

```csharp
// Not found scenario
maybe.ToResult(Error.NotFound("Resource not found"))

// Validation scenario
maybe.ToResult(Error.Validation("Value is required", "fieldName"))

// Unauthorized scenario
maybe.ToResult(Error.Unauthorized("Access denied"))
```

## When to Use Maybe vs Result

**Use `Maybe<T>` when:**
- Absence of a value is normal and doesn't need an explanation
- Example: Optional configuration values

**Use `Result<T>` when:**
- You need to explain why an operation failed
- Example: Database lookups, API calls, validation

**Convert Maybe to Result when:**
- You need to surface the "not found" case to the caller
- You're returning from an API endpoint or service method

## Related Rules

- [FDDD006](FDDD006.md) - Unsafe access to Maybe.Value
