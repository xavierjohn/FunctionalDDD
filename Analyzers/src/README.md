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
