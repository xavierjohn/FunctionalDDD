# FDDD017: Don't compare Result or Maybe to null

## Cause

Comparing a `Result<T>` or `Maybe<T>` value to `null` using `==`, `!=`, `is null`, or `is not null`.

## Rule Description

`Result<T>` and `Maybe<T>` are **structs** (value types), not classes. They cannot be null. Comparing them to null:

- **Always produces a compile error** (CS0019 or CS9135)
- **Indicates a misunderstanding** of how these types work
- **Should use proper state checks** instead

## How to Fix Violations

Use the appropriate state-checking properties:

### For Result<T>

```csharp
// ❌ Bad - Result is a struct, can't be null
if (result == null) { }
if (result is null) { }
if (result != null) { }
if (result is not null) { }

// ✅ Good - Use state properties
if (result.IsSuccess) { }
if (result.IsFailure) { }
```

### For Maybe<T>

```csharp
// ❌ Bad - Maybe is a struct, can't be null
if (maybe == null) { }
if (maybe is null) { }

// ✅ Good - Use state properties
if (maybe.HasValue) { }
if (maybe.HasNoValue) { }
```

## Examples

### Example 1: Checking Result Success

```csharp
// ❌ Bad - Compiler error + wrong pattern
public IActionResult Get(Guid id)
{
    var result = GetCustomer(id);
    if (result == null)  // CS0019: Operator '==' cannot be applied
        return NotFound();
    return Ok(result.Value);
}

// ✅ Good
public IActionResult Get(Guid id)
{
    var result = GetCustomer(id);
    if (result.IsFailure)
        return result.Error.ToHttpResult();
    return Ok(result.Value);
}
```

### Example 2: Checking Maybe Value

```csharp
// ❌ Bad
public string GetDisplayName(Maybe<User> maybeUser)
{
    if (maybeUser is null)  // Compiler error
        return "Guest";
    return maybeUser.Value.Name;
}

// ✅ Good
public string GetDisplayName(Maybe<User> maybeUser)
{
    if (maybeUser.HasNoValue)
        return "Guest";
    return maybeUser.Value.Name;
}

// ✅ Better - Use Match
public string GetDisplayName(Maybe<User> maybeUser) =>
    maybeUser.Match(
        onValue: user => user.Name,
        onNoValue: () => "Guest");
```

### Example 3: Pattern Matching

```csharp
// ❌ Bad - Wrong pattern
var message = result is not null ? "Has value" : "No value";

// ✅ Good
var message = result.IsSuccess ? "Success" : "Failure";

// ✅ Better - Use Match
var message = result.Match(
    onSuccess: _ => "Success",
    onFailure: _ => "Failure");
```

## Why Structs Can't Be Null

In C#, structs are value types that:
- Are stored on the stack (or inline in other objects)
- Always have a value (default is `default(T)`, not `null`)
- Cannot be assigned `null` without using `Nullable<T>` (`T?`)

`Result<T>` and `Maybe<T>` are intentionally designed as structs to:
- Avoid null reference exceptions
- Provide better performance (no heap allocation)
- Force explicit handling of success/failure states

## Related Rules

- [FDDD003](FDDD003.md) - Unsafe access to Result.Value
- [FDDD006](FDDD006.md) - Unsafe access to Maybe.Value

## See Also

- [C# Value Types](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/value-types)
