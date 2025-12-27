# Basics

This guide demonstrates core concepts using primitive obsession avoidance as a practical example.

## Avoiding Primitive Obsession

Passing strings as parameters can cause errors. Consider this example where first and last names could be swapped:

```csharp
Person CreatePerson(string firstName, string lastName)
{
    return new Person(firstName, lastName);
}

var firstName = "John";
var lastName = "Smith";
var person = CreatePerson(lastName, firstName);
```

This would result in a person with first name "Smith" and last name "John".

### Creating Type-Safe Parameters

Create dedicated classes for each domain type (FirstName, LastName). In Domain-Driven Design, objects must maintain valid state, requiring parameter validation before instantiation. For simple null/empty checks, use the `RequiredString` class:

```csharp
public partial class FirstName : RequiredString
{
}

public partial class LastName : RequiredString
{
}

Person CreatePerson(FirstName firstName, LastName lastName)
{
    return new Person(firstName, lastName);
}
```

The class must be partial to allow source code generation of the `TryCreate` method:

```csharp
Result<FirstName> firstNameResult = FirstName.TryCreate("John");
```

`TryCreate` returns a `Result` type that is either `Success` or `Failure`. Handle the failure case:

```csharp
Result<FirstName> firstNameResult = FirstName.TryCreate("John");
if (firstNameResult.IsFailure)
{
    Console.WriteLine(firstNameResult.Error);
    return;
}

Result<LastName> lastNameResult = LastName.TryCreate("Smith");
if (lastNameResult.IsFailure)
{
    Console.WriteLine(lastNameResult.Error);
    return;
}

var person = CreatePerson(firstNameResult.Value, lastNameResult.Value);
```

The compiler will catch parameter order mistakes.

## Result<TValue> Type

Result is a generic type that holds either a value or an error. Extension methods eliminate tedious failure handling after each call.

First, let us look at the definition of the `Result` class:

[!code-csharp[](../../../RailwayOrientedProgramming/src/Result/Result{TValue}.cs#L11-L32)]

Result enables function chaining on success or error paths—the core concept of [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/).

**Important:** Accessing `Value` on failure or `Error` on success throws an exception.

## Core Extension Methods

### Combine

Combine multiple results to validate all inputs together:

```csharp
var result = FirstName.TryCreate("John")
    .Combine(LastName.TryCreate("Smith"));
```

Returns either validation errors from FirstName/LastName, or a tuple containing both values on success.

### Bind

Chain operations that return Results:

```csharp
var result = FirstName.TryCreate("John")
    .Combine(LastName.TryCreate("Smith"))
    .Bind((firstName, lastName) => CreatePerson(firstName, lastName));
```

Returns validation errors from FirstName/LastName, a Person object on success, or any error from CreatePerson.

### Match

Unwrap the Result to extract the final value:

```csharp
string result = FirstName.TryCreate("John")
    .Combine(LastName.TryCreate("Smith"))
    .Bind((firstName, lastName) => CreatePerson(firstName, lastName))
    .Match(ok => "Okay: Person created", error => error.Detail);
```

Match accepts two functions and calls the appropriate one based on Result state.

## Additional Core Operations

### Map - Transform Values

Use `Map` when you want to transform a successful value without changing it to another Result:

```csharp
var result = EmailAddress.TryCreate("user@example.com")
    .Map(email => email.ToString().ToUpper());
// Result<string> containing "USER@EXAMPLE.COM" or an error
```

**Key difference:** `Map` transforms the value (returns `T`), while `Bind` chains operations (returns `Result<T>`).

### Tap - Execute Side Effects

Use `Tap` to perform side effects (like logging) without changing the Result:

```csharp
var result = FirstName.TryCreate("John")
    .Tap(name => Console.WriteLine($"Created name: {name}"))
    .Tap(name => _logger.LogInformation("Name validated"));
// Result<FirstName> - unchanged, but side effects executed
```

**Common uses:** Logging, auditing, sending notifications, updating UI

### Ensure - Add Validation

Use `Ensure` to add additional validation conditions:

```csharp
var result = EmailAddress.TryCreate("user@spam.com")
    .Ensure(email => !email.Domain.Contains("spam"),
           Error.Validation("Spam domains not allowed"));
// Fails if email is from spam domain
```

You can chain multiple `Ensure` calls for complex validation:

```csharp
var result = Age.TryCreate(25)
    .Ensure(age => age >= 18, Error.Validation("Must be 18 or older"))
    .Ensure(age => age <= 120, Error.Validation("Invalid age"));
```

### Compensate - Recover from Errors

Use `Compensate` to provide fallback values or recovery logic:

```csharp
var result = GetUserFromCache(id)
    .Compensate(error => GetUserFromDatabase(id));
// Try cache first, fallback to database on error
```

### Working with Async Operations

All operations have async variants with `Async` suffix:

```csharp
var result = await GetUserAsync(id)
    .BindAsync(user => GetOrdersAsync(user.Id))
    .TapAsync(orders => LogOrderCountAsync(orders.Count))
    .EnsureAsync(orders => orders.Any(),
                Error.NotFound("No orders found"))
    .MapAsync(orders => orders.ToDto())
    .MatchAsync(
        onSuccess: dto => Results.Ok(dto),
        onFailure: error => Results.BadRequest(error.Detail)
    );
```

### Summary of Core Operations

| Operation | Purpose | Input Function Returns | Example Use Case |
|-----------|---------|----------------------|------------------|
| **Bind** | Chain operations that return Result | `Result<T>` | Calling another validation/business operation |
| **Map** | Transform successful values | `T` | Converting types, formatting |
| **Tap** | Execute side effects | `void` | Logging, notifications |
| **Ensure** | Add validation | `bool` | Business rule validation |
| **Combine** | Merge multiple Results | N/A | Validating multiple inputs together |
| **Compensate** | Error recovery | `Result<T>` | Fallback logic, retry |
| **Match** | Unwrap and handle both cases | `TResult` | Final result handling |

## Conclusion

Use strongly-typed classes that maintain valid state to prevent parameter assignment errors. Railway-oriented programming enables clean, readable code with explicit error handling through core operations: `Bind`, `Map`, `Tap`, `Ensure`, and `Combine`.
