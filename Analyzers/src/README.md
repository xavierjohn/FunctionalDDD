# FunctionalDDD.Analyzers

Roslyn analyzers for the FunctionalDDD library. These analyzers help enforce proper Result and Maybe handling patterns at compile time.

## Installation

```
dotnet add package FunctionalDdd.Analyzers
```

Or add to your project file:

```xml
<PackageReference Include="FunctionalDdd.Analyzers" Version="x.x.x" PrivateAssets="all" />
```

## Diagnostic Rules

| ID | Severity | Description |
|----|----------|-------------|
| FDDD001 | Warning | Result return value is not handled |
| FDDD002 | Info | Use Bind instead of Map when lambda returns Result |
| FDDD003 | Warning | Unsafe access to Result.Value |
| FDDD004 | Warning | Unsafe access to Result.Error |
| FDDD005 | Info | Consider using MatchError for error type discrimination |
| FDDD006 | Warning | Unsafe access to Maybe.Value |
| FDDD007 | Info | Use Create instead of TryCreate().Value |

## FDDD001: Result return value is not handled

This analyzer warns when a method returning `Result<T>` is called but the return value is discarded. This can lead to silent error handling issues.

```csharp
// Warning: Result not handled
UserService.CreateUser(name, email);

// OK: Result is handled
var result = UserService.CreateUser(name, email);

// OK: Result is chained
UserService.CreateUser(name, email)
    .Bind(user => SendWelcomeEmail(user));
```

## FDDD002: Use Bind instead of Map

This analyzer suggests using `Bind` instead of `Map` when the lambda returns a `Result<T>`. Using `Map` in this case would produce `Result<Result<T>>` which is likely not intended.

```csharp
// Info: Use Bind instead of Map
result.Map(user => ValidateUser(user)); // Returns Result<Result<User>>

// OK: Use Bind for Result-returning lambdas
result.Bind(user => ValidateUser(user)); // Returns Result<User>
```

## FDDD003: Unsafe access to Result.Value

This analyzer warns when accessing `Result<T>.Value` without first checking `IsSuccess`. Accessing `Value` on a failed result throws an exception.

```csharp
// Warning: Unsafe access
var user = result.Value;

// OK: Guarded access
if (result.IsSuccess)
{
    var user = result.Value;
}

// OK: Using TryGetValue
if (result.TryGetValue(out var user))
{
    // use user
}

// OK: Using Match
result.Match(
    onSuccess: user => HandleUser(user),
    onFailure: error => HandleError(error));
```

## FDDD004: Unsafe access to Result.Error

Similar to FDDD003, this analyzer warns when accessing `Result<T>.Error` without first checking `IsFailure`.

```csharp
// Warning: Unsafe access
var error = result.Error;

// OK: Guarded access
if (result.IsFailure)
{
    var error = result.Error;
}
```

## FDDD006: Unsafe access to Maybe.Value

This analyzer warns when accessing `Maybe<T>.Value` without first checking `HasValue`.

```csharp
// Warning: Unsafe access
var value = maybe.Value;

// OK: Guarded access
if (maybe.HasValue)
{
    var value = maybe.Value;
}

// OK: Using GetValueOrDefault
var value = maybe.GetValueOrDefault(defaultValue);

// OK: Using TryGetValue
if (maybe.TryGetValue(out var value))
{
    // use value
}

// OK: Converting to Result
maybe.ToResult(Error.NotFound("Not found"))
    .Bind(value => ProcessValue(value));
```

## FDDD007: Use Create instead of TryCreate().Value

This analyzer detects when `.Value` is accessed directly on a `TryCreate()` result for scalar value objects implementing `IScalarValue<TSelf, TPrimitive>`. This pattern is unclear and defeats the purpose of using `TryCreate`. Both `TryCreate().Value` and `Create()` throw the same exception on invalid input, but `Create()` shows clearer intent.

```csharp
// Info: Unclear usage (for types implementing IScalarValue)
var email = EmailAddress.TryCreate("test@example.com").Value;

// OK: Use Create when you expect the value to be valid
var email = EmailAddress.Create("test@example.com");

// OK: Or properly handle the Result
var result = EmailAddress.TryCreate(userInput);
if (result.IsFailure)
    return result.ToHttpResult();
var email = result.Value;
```

**Note:** This analyzer only applies to scalar value objects that implement `IScalarValue<TSelf, TPrimitive>`, which guarantees both `TryCreate` and `Create` methods exist with the documented behavior.

## FDDD008: Result is double-wrapped

This analyzer detects when a `Result` is wrapped inside another `Result`, creating `Result<Result<T>>`. This is almost always unintended and indicates misuse of `Map` instead of `Bind`, or unnecessary wrapping of an existing Result.

```csharp
// Warning: Double wrapping in type declaration
Result<Result<User>> user;
public Result<Result<Order>> GetOrder() { }

// Warning: Wrapping an existing Result
Result<int> existingResult = GetValue();
var wrapped = Result.Success(existingResult); // Creates Result<Result<int>>

// OK: Single wrapping
Result<User> user;
public Result<Order> GetOrder() { }
var result = Result.Success(42); // Result<int>

// OK: Use Bind for Result-returning functions (see FDDD002)
result.Bind(x => ValidateUser(x)); // Returns Result<User>, not Result<Result<User>>
```

**Common causes:**
1. Using `Map` instead of `Bind` when the lambda returns a `Result` (also caught by FDDD002)
2. Calling `Result.Success()` or `Result.Failure()` on a value that's already a `Result`
3. Declaring variables, properties, or return types with `Result<Result<T>>`

## Suppressing Diagnostics

If you need to suppress a diagnostic for a specific case, you can use:

```csharp
#pragma warning disable FDDD001
SomeMethodReturningResult();
#pragma warning restore FDDD001
```

Or use the `[SuppressMessage]` attribute:

```csharp
[SuppressMessage("FunctionalDDD", "FDDD001", Justification = "Result is intentionally ignored")]
public void MyMethod()
{
    // ...
}
```
