# FDDD007: Use Create instead of TryCreate().Value

## Cause

Using `.Value` immediately after calling `TryCreate()`, which provides unclear intent and poor error messages.

## Rule Description

When creating value objects, there are two factory methods:
- **`TryCreate()`** - Returns `Result<T>` for scenarios where failure is expected and should be handled
- **`Create()`** - Throws an exception for scenarios where the value is known to be valid

Using `TryCreate().Value` combines the worst of both worlds:
- ❌ Throws an exception like `Create()` (not handling the Result)
- ❌ But with a generic "Result is in failure state" message instead of the specific validation error

## How to Fix Violations

### Use Create() when you expect the value to be valid

```csharp
// ❌ Bad - Unclear intent, poor error message
var email = EmailAddress.TryCreate("test@example.com").Value;
// Throws: "Result is in failure state" (generic message)

// ✅ Good - Clear intent, detailed error message
var email = EmailAddress.Create("test@example.com");
// Throws: "Failed to create EmailAddress: Email address is not valid." (specific message)
```

### Or handle the Result properly

```csharp
// ✅ Good - Proper error handling
var result = EmailAddress.TryCreate(userInput);
if (result.IsFailure)
    return BadRequest(result.Error);

var email = result.Value;
```

## When to Use Each Pattern

### Use `Create()` when:
- ✅ Value comes from constants or configuration
- ✅ Value is already validated
- ✅ Failure would be a programming error
- ✅ In test code with known-valid values

```csharp
// ✅ Good - constant value
var defaultCurrency = CurrencyCode.Create("USD");

// ✅ Good - already validated
var total = Money.Create(item1.Amount + item2.Amount, "USD");

// ✅ Good - test data
[Fact]
public void Should_CreateOrder()
{
    var customer = Customer.Create("test@example.com", "John");
    // ...
}
```

### Use `TryCreate()` when:
- ✅ Value comes from user input
- ✅ Value comes from external systems
- ✅ Failure is a expected business scenario
- ✅ You want to handle errors gracefully

```csharp
// ✅ Good - user input
return EmailAddress.TryCreate(dto.Email)
    .Bind(email => Customer.Create(email, dto.Name))
    .Map(customer => customer.ToDto())
    .Match(
        onSuccess: dto => Ok(dto),
        onFailure: error => BadRequest(error));
```

## Code Fix

This diagnostic offers an automatic code fix that replaces `TryCreate().Value` with `Create()`:

### Example Code Fix Transformation

**Before:**
```csharp
var name = Name.TryCreate("John").Value;
var email = EmailAddress.TryCreate("test@example.com").Value;
var customer = Customer.Create(name, email);
```

**After (automatic):**
```csharp
var name = Name.Create("John");
var email = EmailAddress.Create("test@example.com");
var customer = Customer.Create(name, email);
```

## Error Message Comparison

**Using `TryCreate().Value` (bad):**
```
Unhandled exception: System.InvalidOperationException: Result is in failure state.
   at FunctionalDdd.Result`1.get_Value()
   at MyApp.CreateCustomer()
```

**Using `Create()` (good):**
```
Unhandled exception: System.InvalidOperationException: Failed to create EmailAddress: Email address is not valid.
   at EmailAddress.Create()
   at MyApp.CreateCustomer()
```

The `Create()` version includes the actual validation error details!

## Related Rules

- [FDDD003](FDDD003.md) - Unsafe access to Result.Value
