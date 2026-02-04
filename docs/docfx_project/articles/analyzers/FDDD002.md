# FDDD002: Use Bind instead of Map when lambda returns Result

## Cause

A `Map` operation is used with a lambda function that returns `Result<T>`, which creates a double-wrapped `Result<Result<T>>`.

## Rule Description

In Railway Oriented Programming:
- **Map** transforms the value inside a `Result<T>` while keeping the `Result` wrapper
- **Bind** (also called flatMap) is used when the transformation itself returns a `Result<T>`

Using `Map` when the lambda returns a `Result<T>` creates `Result<Result<T>>`, which is almost always unintended.

## How to Fix Violations

Replace `Map` with `Bind` when the transformation function returns a `Result<T>`:

```csharp
// ❌ Bad - Creates Result<Result<Customer>>
return emailResult.Map(email => Customer.Create(email, name));
//                 ^^^                          ^^^^^^^^^^^^^^
//                 Map                    Returns Result<Customer>

// ✅ Good - Properly flattens to Result<Customer>
return emailResult.Bind(email => Customer.Create(email, name));
//                 ^^^^
//                 Bind
```

## Example

```csharp
public Result<Order> CreateOrder(string emailStr, decimal amount)
{
    // ❌ Bad - Result<Result<Order>>
    return Email.TryCreate(emailStr)
        .Map(email => Order.Create(email, amount));
        // Order.Create returns Result<Order>
        // Map wraps it again → Result<Result<Order>>

    // ✅ Good - Result<Order>
    return Email.TryCreate(emailStr)
        .Bind(email => Order.Create(email, amount));
        // Bind flattens → Result<Order>
}
```

## Code Fix

This diagnostic offers an automatic code fix that replaces `Map` with `Bind`.

## When to Suppress Warnings

Do not suppress this warning. If you actually need `Result<Result<T>>` (extremely rare), make it explicit with a comment explaining why.

## Related Rules

- [FDDD008](FDDD008.md) - Result is double-wrapped
