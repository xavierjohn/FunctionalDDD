# FDDD013: Consider using GetValueOrDefault or Match

## Cause

Using the ternary operator pattern `result.IsSuccess ? result.Value : defaultValue` when `GetValueOrDefault()` or `Match()` provides a more functional and safer alternative.

## Rule Description

The pattern `result.IsSuccess ? result.Value : defaultValue` is verbose and can be replaced with more idiomatic functional methods:
- `GetValueOrDefault()` for simple fallback values
- `Match()` for transformations or side effects

## How to Fix Violations

### Option 1: Use GetValueOrDefault

```csharp
// ❌ Verbose - Ternary operator
var customer = result.IsSuccess ? result.Value : defaultCustomer;

// ✅ Concise - GetValueOrDefault
var customer = result.GetValueOrDefault(defaultCustomer);
```

### Option 2: Use Match

```csharp
// ❌ Verbose - Ternary with transformation
var dto = result.IsSuccess ? result.Value.ToDto() : DefaultDto();

// ✅ Better - Match with transformation
var dto = result.Match(
    onSuccess: customer => customer.ToDto(),
    onFailure: _ => DefaultDto());
```

## Examples

### Example 1: Simple Fallback

```csharp
// ❌ Ternary
var email = emailResult.IsSuccess 
    ? emailResult.Value 
    : EmailAddress.Create("noreply@example.com");

// ✅ GetValueOrDefault
var email = emailResult.GetValueOrDefault(
    EmailAddress.Create("noreply@example.com"));
```

### Example 2: Fallback to Default Struct Value

```csharp
// ❌ Ternary
var id = idResult.IsSuccess ? idResult.Value : Guid.Empty;

// ✅ GetValueOrDefault with default
var id = idResult.GetValueOrDefault();  // Uses default(Guid) = Guid.Empty
```

### Example 3: With Transformation

```csharp
// ❌ Ternary with transformation
var displayName = nameResult.IsSuccess 
    ? nameResult.Value.ToString() 
    : "Unknown";

// ✅ Match
var displayName = nameResult.Match(
    onSuccess: name => name.ToString(),
    onFailure: _ => "Unknown");
```

### Example 4: Error-Dependent Fallback

```csharp
// ❌ Can't easily access error details
var message = result.IsSuccess 
    ? "Success" 
    : "Failed";  // Lost error information

// ✅ Match with error details
var message = result.Match(
    onSuccess: _ => "Success",
    onFailure: error => $"Failed: {error.Detail}");
```

## GetValueOrDefault Overloads

```csharp
// Uses default(T) as fallback
result.GetValueOrDefault()

// Uses provided value as fallback
result.GetValueOrDefault(customDefault)

// Uses factory function as fallback (lazily evaluated)
result.GetValueOrDefault(() => CreateDefault())
```

## Match Variants

```csharp
// Match for transformations
var output = result.Match(
    onSuccess: value => TransformValue(value),
    onFailure: error => HandleError(error));

// Match for side effects (void return)
result.Match(
    onSuccess: value => Console.WriteLine(value),
    onFailure: error => Logger.LogError(error.Detail));

// MatchAsync for async transformations
var output = await result.MatchAsync(
    onSuccess: async value => await TransformAsync(value),
    onFailure: error => Task.FromResult(default));
```

## Benefits

### Null Safety

```csharp
// ❌ Ternary - Can accidentally access Value on failure
var name = result.IsSuccess ? result.Value : defaultName;
// If you mistype IsFailure, you get an exception!

// ✅ GetValueOrDefault - Can't access Value incorrectly
var name = result.GetValueOrDefault(defaultName);
```

### Functional Composition

```csharp
// ✅ Chainable
return GetCustomer(id)
    .Match(
        onSuccess: customer => customer.Email,
        onFailure: _ => EmailAddress.Create("noreply@example.com"))
    .ToString();
```

## When to Use Ternary

Use the ternary operator when:
- You're checking other properties (not just `IsSuccess`)
- The logic is complex and doesn't fit `Match`

```csharp
// Ternary appropriate - checking different property
var count = list.Count > 0 ? list.Count : defaultCount;
```

## When to Suppress Warnings

This is a suggestion-level diagnostic. Suppress it if:
- You prefer explicit ternary operators for clarity
- The pattern doesn't fit `GetValueOrDefault` or `Match`
- You're working with legacy code

## Related Rules

- [FDDD003](FDDD003.md) - Unsafe access to Result.Value
