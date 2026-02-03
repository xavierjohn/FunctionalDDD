# FDDD012: Maybe is double-wrapped

## Cause

A `Maybe<Maybe<T>>` type is detected, indicating that a `Maybe` is wrapped inside another `Maybe`.

## Rule Description

`Maybe<Maybe<T>>` is almost always unintended and indicates a logic error. This typically happens when using `Map` with a function that returns `Maybe<T>`.

Unlike `Result<T>`, `Maybe<T>` doesn't have a `Bind` operation, which makes composition more challenging. This is by design - `Maybe` is for optional values without context, while `Result` is for operations that can fail with error information.

## How to Fix Violations

### Option 1: Convert to Result for better composability

```csharp
// ❌ Bad - Maybe<Maybe<Customer>>
Maybe<string> maybeEmail = GetEmail();
Maybe<Maybe<Customer>> doubleWrapped = maybeEmail.Map(email => FindCustomer(email));
//                                                              ^^^^^^^^^^^^^^^^^^
//                                                         Returns Maybe<Customer>

// ✅ Good - Use Result instead
Result<Customer> result = GetEmail()
    .ToResult(Error.Validation("Email is required"))
    .Bind(email => FindCustomer(email)
        .ToResult(Error.NotFound("Customer not found")));
```

### Option 2: Flatten manually (not recommended)

```csharp
// ⚠️ Works but not recommended
Maybe<string> maybeEmail = GetEmail();
Maybe<Maybe<Customer>> doubleWrapped = maybeEmail.Map(email => FindCustomer(email));
Maybe<Customer> flattened = doubleWrapped.HasValue && doubleWrapped.Value.HasValue
    ? doubleWrapped.Value
    : Maybe<Customer>.None;
```

## Why Maybe Doesn't Have Bind

`Maybe<T>` is intentionally limited to avoid complex compositions:
- Use `Maybe<T>` for simple optional values (configuration, nullable references)
- Use `Result<T>` for operations that need composition and error context

## Example Migration

```csharp
// ❌ Bad - Trying to compose with Maybe
public Maybe<Order> CreateOrder(Guid customerId, decimal amount)
{
    Maybe<Customer> maybeCustomer = FindCustomer(customerId);
    // Can't easily compose - would create Maybe<Maybe<Order>>
}

// ✅ Good - Use Result for composition
public Result<Order> CreateOrder(Guid customerId, decimal amount)
{
    return FindCustomer(customerId)
        .ToResult(Error.NotFound($"Customer {customerId} not found"))
        .Bind(customer => Order.Create(customer, amount));
}

private Maybe<Customer> FindCustomer(Guid id) =>
    repository.FindById(id);
```

## When to Use Maybe vs Result

| Use `Maybe<T>` | Use `Result<T>` |
|----------------|-----------------|
| Optional configuration | Validation that can fail |
| Nullable reference alternative | Database operations |
| Simple presence/absence | API calls |
| No error context needed | Need to explain failures |

## When to Suppress Warnings

Do not suppress this warning. If you have `Maybe<Maybe<T>>`, restructure your code to use `Result<T>` instead.

## Related Rules

- [FDDD008](FDDD008.md) - Result is double-wrapped
- [FDDD009](FDDD009.md) - Maybe.ToResult called without error parameter
