# Functional Domain Driven Design

[![Build](https://github.com/xavierjohn/FunctionalDDD/actions/workflows/build.yml/badge.svg)](https://github.com/xavierjohn/FunctionalDDD/actions/workflows/build.yml)
[![codecov](https://codecov.io/gh/xavierjohn/FunctionalDDD/branch/main/graph/badge.svg)](https://codecov.io/gh/xavierjohn/FunctionalDDD)
[![NuGet](https://img.shields.io/nuget/v/FunctionalDDD.RailwayOrientedProgramming.svg)](https://www.nuget.org/packages/FunctionalDDD.RailwayOrientedProgramming)
[![NuGet Downloads](https://img.shields.io/nuget/dt/FunctionalDDD.RailwayOrientedProgramming.svg)](https://www.nuget.org/packages/FunctionalDDD.RailwayOrientedProgramming)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download)
[![C#](https://img.shields.io/badge/C%23-14.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![GitHub Stars](https://img.shields.io/github/stars/xavierjohn/FunctionalDDD?style=social)](https://github.com/xavierjohn/FunctionalDDD/stargazers)
[![Documentation](https://img.shields.io/badge/docs-online-blue.svg)](https://xavierjohn.github.io/FunctionalDDD/)

> **Write 60% less code that reads like English** using Railway-Oriented Programming and Domain-Driven Design

Transform error-prone imperative code into readable, succinct functional pipelines—with zero performance overhead.

```csharp
// ? Before: 20 lines of nested error checking
var firstName = ValidateFirstName(input.FirstName);
if (firstName == null) return BadRequest("Invalid first name");
var lastName = ValidateLastName(input.LastName);
if (lastName == null) return BadRequest("Invalid last name");
// ... 15 more lines of repetitive checks

// ? After: 8 lines that read like a story
return FirstName.TryCreate(input.FirstName)
    .Combine(LastName.TryCreate(input.LastName))
    .Combine(EmailAddress.TryCreate(input.Email))
    .Bind((first, last, email) => User.TryCreate(first, last, email))
    .Ensure(user => !_repository.EmailExists(user.Email), Error.Conflict("Email exists"))
    .Tap(user => _repository.Save(user))
    .Tap(user => _emailService.SendWelcome(user.Email))
    .Match(onSuccess: user => Ok(user), onFailure: error => BadRequest(error.Detail));
```

**Key Benefits:**
- ?? **60% less boilerplate** - Write less, understand more
- ?? **Self-documenting** - Code reads like English: "Create ? Validate ? Save ? Notify"
- ?? **Compiler-enforced** - Impossible to skip error handling
- ? **Zero overhead** - Only 11-16ns (0.002% of I/O operations)
- ? **Production-ready** - Type-safe, testable, maintainable

---

## Table of Contents

- [Why Use This?](#why-use-this)
- [Quick Start](#quick-start)
- [Key Features](#key-features)
- [NuGet Packages](#nuget-packages)
- [Performance](#performance)
- [Documentation](#documentation)
- [Examples](#examples)
- [What's New](#whats-new)
- [Contributing](#contributing)
- [License](#license)

## Why Use This?

**The Problem:**
Traditional error handling in C# creates verbose, error-prone code with nested if-statements that obscure business logic and make errors easy to miss.

**The Solution:**
Railway-Oriented Programming (ROP) treats your code like railway tracks—operations flow along the success track or automatically switch to the error track. **You write what should happen, not what could go wrong.**

**Real-World Impact:**
- ? **Teams report 40-60% reduction** in error-handling boilerplate
- ? **Bugs caught at compile-time** instead of runtime
- ? **New developers understand code faster** thanks to readable chains
- ? **Zero performance penalty** - same speed as imperative code

?? **[Read the full introduction](https://xavierjohn.github.io/FunctionalDDD/articles/intro.html)**

---

## Quick Start

### Installation

Install the core railway-oriented programming package:

```bash
dotnet add package FunctionalDdd.RailwayOrientedProgramming
```

For ASP.NET Core integration:

```bash
dotnet add package FunctionalDdd.Asp
```

### Basic Usage

```csharp
using FunctionalDdd;

// Create a Result with validation
var emailResult = EmailAddress.TryCreate("user@example.com")
    .Ensure(email => email.Domain != "spam.com", 
            Error.Validation("Email domain not allowed"))
    .Tap(email => Console.WriteLine($"Valid email: {email}"));

// Handle success or failure
var message = emailResult.Match(
    onSuccess: email => $"Welcome {email}!",
    onFailure: error => $"Error: {error.Detail}"
);

// Chain multiple operations
var result = await GetUserAsync(userId)
    .ToResultAsync(Error.NotFound("User not found"))
    .BindAsync(user => SaveUserAsync(user))
    .TapAsync(user => SendEmailAsync(user.Email));
```

#### ?? [Quick Start Guide](Examples/QUICKSTART.md)


?? **Next Steps**: Browse the [Examples](#examples) section or explore the [complete documentation](https://xavierjohn.github.io/FunctionalDDD/)

?? **Need help debugging?** Check out the [Debugging ROP Chains guide](https://xavierjohn.github.io/FunctionalDDD/articles/debugging.html)

---

## Key Features

### ?? Railway-Oriented Programming
Chain operations that automatically handle success/failure paths—no more nested if-statements.

```csharp
return GetUserAsync(id)
    .ToResultAsync(Error.NotFound("User not found"))
    .BindAsync(user => UpdateUserAsync(user))
    .TapAsync(user => AuditLogAsync(user))
    .MatchAsync(user => Ok(user), error => NotFound(error.Detail));
```

### ?? Type-Safe Value Objects
Prevent primitive obsession and parameter mix-ups with strongly-typed domain objects.

```csharp
// ? Compiler catches this mistake
CreateUser(lastName, firstName);  // Error: Wrong parameter types!

// ? This compiles but has a bug
CreateUser(lastNameString, firstNameString);  // Swapped, but compiler can't tell
```

### ? Discriminated Error Matching
Pattern match on specific error types for precise error handling.

```csharp
return ProcessOrder(order).MatchError(
    onValidation: err => BadRequest(err.FieldErrors),
    onNotFound: err => NotFound(err.Detail),
    onConflict: err => Conflict(err.Detail),
    onSuccess: order => Ok(order)
);
```

### ? Async & Parallel Operations
Full support for async/await and parallel execution.

```csharp
var result = await GetUserAsync(id)
    .ParallelAsync(GetOrdersAsync(id))
    .ParallelAsync(GetPreferencesAsync(id))
    .WhenAllAsync()
    .MapAsync((user, orders, prefs) => new UserProfile(user, orders, prefs));
```

### ?? Built-in Tracing
OpenTelemetry integration for automatic distributed tracing.

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddRailwayOrientedProgrammingInstrumentation()
        .AddOtlpExporter());
```

?? **[View all features](https://xavierjohn.github.io/FunctionalDDD/articles/intro.html)**

---

## Overview

Functional programming, railway-oriented programming, and domain-driven design combine to create robust, reliable software.

### ?? Functional Programming
**Pure functions** take inputs and produce outputs without side effects, making code **predictable, testable, and composable**.

?? [Applying Functional Principles in C# (Pluralsight)](https://enterprisecraftsmanship.com/ps-func)

### ?? Railway-Oriented Programming
Handle errors using a **railway track metaphor**: operations flow along the success track or automatically switch to the error track. This makes error handling **explicit and visual**.

**Key insight:** *Write what should happen, not what could go wrong.*

### ??? Domain-Driven Design
Focus on understanding the problem domain and creating an accurate model. Use **Aggregates**, **Entities**, and **Value Objects** to enforce business rules and maintain valid state.

?? [Domain-Driven Design in Practice (Pluralsight)](https://app.pluralsight.com/library/courses/domain-driven-design-in-practice/table-of-contents)

### Why They Work Together

```
Pure Functions          Clear business logic
     +                        ?
Railway-Oriented    ?   Explicit error handling
     +                        ?
Type Safety         ?   Compiler-enforced correctness
     +                        ?
Domain Model        ?   Business rule enforcement
     =                        ?
Robust, Maintainable Software
```

---

## What's New

**Recent enhancements:**
- ?? **NEW: RequiredEnum** - Type-safe enumerations with behavior, state machine support, and JSON serialization. Prevents invalid values unlike C# enums. Source-generated `IScalarValue` support for ASP.NET Core auto-validation.
- ?? **NEW: Roslyn Analyzers** - 14 compile-time diagnostics to enforce ROP best practices and prevent common mistakes with Result/Maybe types
- ? **ASP.NET Core Auto-Validation** - Value objects automatically validate in requests (route params, query strings, JSON bodies) via `AddScalarValueObjectValidation()`
- ?? **11 New Value Objects** - Ready-to-use: `Url`, `PhoneNumber`, `Percentage`, `Currency`, `IpAddress`, `Hostname`, `Slug`, `CountryCode`, `LanguageCode`, `Age`, plus `RequiredInt`/`RequiredDecimal`
- ? **Discriminated Error Matching** - Pattern match on specific error types (ValidationError, NotFoundError, etc.) using `MatchError`
- ? **Tuple Destructuring** - Automatically destructure tuples in Match/Switch for cleaner code
- ?? **Enhanced Documentation** - [Complete documentation site](https://xavierjohn.github.io/FunctionalDDD/) with tutorials, examples, and API reference
- ? **Performance Optimizations** - Reduced allocation and improved throughput
- ?? **OpenTelemetry Tracing** - Built-in distributed tracing support

?? **[View changelog](CHANGELOG.md)**

---

## NuGet Packages

| Package | Version | Description | Documentation |
|---------|---------|-------------|---------------|
| **[RailwayOrientedProgramming](https://www.nuget.org/packages/FunctionalDdd.RailwayOrientedProgramming)** | [![NuGet](https://img.shields.io/nuget/v/FunctionalDdd.RailwayOrientedProgramming.svg)](https://www.nuget.org/packages/FunctionalDdd.RailwayOrientedProgramming) | Core Result/Maybe types, error handling, async support | [?? Docs](RailwayOrientedProgramming/README.md) |
| **[Asp](https://www.nuget.org/packages/FunctionalDdd.Asp)** | [![NuGet](https://img.shields.io/nuget/v/FunctionalDdd.Asp.svg)](https://www.nuget.org/packages/FunctionalDdd.Asp) | Convert Result ? HTTP responses (MVC & Minimal API) | [?? Docs](Asp/README.md) |
| **[Http](https://www.nuget.org/packages/FunctionalDdd.Http)** | [![NuGet](https://img.shields.io/nuget/v/FunctionalDdd.Http.svg)](https://www.nuget.org/packages/FunctionalDdd.Http) | HTTP client extensions for Result/Maybe with status code handling | [?? Docs](Http/README.md) |
| **[FluentValidation](https://www.nuget.org/packages/FunctionalDdd.FluentValidation)** | [![NuGet](https://img.shields.io/nuget/v/FunctionalDdd.FluentValidation.svg)](https://www.nuget.org/packages/FunctionalDdd.FluentValidation) | Integrate FluentValidation with ROP | [?? Docs](FluentValidation/README.md) |
| **[PrimitiveValueObjects](https://www.nuget.org/packages/FunctionalDdd.PrimitiveValueObjects)** | [![NuGet](https://img.shields.io/nuget/v/FunctionalDdd.PrimitiveValueObjects.svg)](https://www.nuget.org/packages/FunctionalDdd.PrimitiveValueObjects) | Base classes (RequiredString, RequiredGuid, RequiredUlid, RequiredInt, RequiredDecimal) + 11 ready-to-use VOs | [?? Docs](PrimitiveValueObjects/README.md) |
| **[PrimitiveValueObjectGenerator](https://www.nuget.org/packages/FunctionalDdd.PrimitiveValueObjectGenerator)** | [![NuGet](https://img.shields.io/nuget/v/FunctionalDdd.PrimitiveValueObjectGenerator.svg)](https://www.nuget.org/packages/FunctionalDdd.PrimitiveValueObjectGenerator) | Source generator for value object boilerplate | [?? Docs](PrimitiveValueObjects/generator/README.md) |
| **[Analyzers](https://www.nuget.org/packages/FunctionalDdd.Analyzers)** | [![NuGet](https://img.shields.io/nuget/v/FunctionalDdd.Analyzers.svg)](https://www.nuget.org/packages/FunctionalDdd.Analyzers) | **NEW!** Roslyn analyzers for compile-time ROP safety (14 rules) | [?? Docs](Analyzers/README.md) |
| **[DomainDrivenDesign](https://www.nuget.org/packages/FunctionalDdd.DomainDrivenDesign)** | [![NuGet](https://img.shields.io/nuget/v/FunctionalDdd.DomainDrivenDesign.svg)](https://www.nuget.org/packages/FunctionalDdd.DomainDrivenDesign) | Aggregate, Entity, ValueObject, Domain Events | [?? Docs](DomainDrivenDesign/README.md) |
| **[Testing](https://www.nuget.org/packages/FunctionalDdd.Testing)** | [![NuGet](https://img.shields.io/nuget/v/FunctionalDdd.Testing.svg)](https://www.nuget.org/packages/FunctionalDdd.Testing) | FluentAssertions extensions, test builders, fakes | [?? Docs](Testing/README.md) |

---

## Performance

### ? Negligible Overhead, Maximum Clarity

Comprehensive benchmarks on **.NET 10** show ROP adds only **11-16 nanoseconds** of overhead—less than **0.002%** of typical I/O operations.

| Operation | Time | Overhead | Memory |
|-----------|------|----------|--------|
| **Happy Path** | 147 ns | **16 ns** (12%) | 144 B |
| **Error Path** | 99 ns | **11 ns** (13%) | 184 B |
| **Combine (5 results)** | 58 ns | - | 0 B |
| **Bind chain (5)** | 63 ns | - | 0 B |

**Real-world context:**
```
Database Query: 1,000,000 ns (1 ms)
ROP Overhead:          16 ns
                       ?
            0.0016% of DB query time
```

**The overhead is 1/62,500th of a single database query!**

? Same memory usage as imperative code  
? Single-digit to low double-digit nanosecond operations  
?? **[View detailed benchmarks](BENCHMARKS.md)**

Run benchmarks yourself:
```bash
dotnet run --project Benchmark/Benchmark.csproj -c Release
```

---

## Documentation

?? **[Complete Documentation Site](https://xavierjohn.github.io/FunctionalDDD/)**

### Learning Paths

**?? Beginner** (2-3 hours)
- [Introduction](https://xavierjohn.github.io/FunctionalDDD/articles/intro.html) - Why use ROP?
- [Basics Tutorial](https://xavierjohn.github.io/FunctionalDDD/articles/basics.html) - Core concepts
- [Examples](https://xavierjohn.github.io/FunctionalDDD/articles/examples.html) - Real-world patterns

**?? Integration** (1-2 hours)
- [ASP.NET Core](https://xavierjohn.github.io/FunctionalDDD/articles/integration-aspnet.html)
- [FluentValidation](https://xavierjohn.github.io/FunctionalDDD/articles/integration-fluentvalidation.html)
- [Entity Framework Core](https://xavierjohn.github.io/FunctionalDDD/articles/integration-ef.html)

**?? Advanced** (3-4 hours)
- [Clean Architecture](https://xavierjohn.github.io/FunctionalDDD/articles/clean-architecture.html) - CQRS patterns
- [Advanced Features](https://xavierjohn.github.io/FunctionalDDD/articles/advanced-features.html) - LINQ, parallelization
- [Error Handling](https://xavierjohn.github.io/FunctionalDDD/articles/error-handling.html) - Custom errors, aggregation

### Quick References
- [Debugging Guide](https://xavierjohn.github.io/FunctionalDDD/articles/debugging.html)
- [Performance Tips](https://xavierjohn.github.io/FunctionalDDD/articles/performance.html)
- [API Reference](https://xavierjohn.github.io/FunctionalDDD/api/)

---

## Examples

### Basic Usage

```csharp
// Chain operations with automatic error handling
var result = EmailAddress.TryCreate("user@example.com")
    .Ensure(email => email.Domain != "spam.com", Error.Validation("Domain not allowed"))
    .Tap(email => _logger.LogInformation("Validated: {Email}", email))
    .Match(
        onSuccess: email => $"Welcome {email}!",
        onFailure: error => $"Error: {error.Detail}"
    );
```

### Real-World Scenarios

<details>
<summary><b>User Registration with Validation</b></summary>

```csharp
[HttpPost]
public ActionResult<User> Register([FromBody] RegisterUserRequest request) =>
    FirstName.TryCreate(request.FirstName)
        .Combine(LastName.TryCreate(request.LastName))
        .Combine(EmailAddress.TryCreate(request.Email))
        .Bind((first, last, email) => User.TryCreate(first, last, email, request.Password))
        .Ensure(user => !_repository.EmailExists(user.Email), Error.Conflict("Email exists"))
        .Tap(user => _repository.Save(user))
        .Tap(user => _emailService.SendWelcome(user.Email))
        .ToActionResult(this);
```

</details>

<details>
<summary><b>Async Operations</b></summary>

```csharp
public async Task<IResult> ProcessOrderAsync(int orderId)
{
    return await GetOrderAsync(orderId)
        .ToResultAsync(Error.NotFound($"Order {orderId} not found"))
        .EnsureAsync(
            order => order.CanProcessAsync(),
            Error.Validation("Order cannot be processed"))
        .TapAsync(order => ValidateInventoryAsync(order))
        .BindAsync(order => ChargePaymentAsync(order))
        .TapAsync(order => SendConfirmationAsync(order))
        .MatchAsync(
            order => Results.Ok(order),
            error => Results.BadRequest(error.Detail));
}
```

</details>

<details>
<summary><b>Parallel Operations</b></summary>

```csharp
// Fetch data from multiple sources in parallel
var result = await GetUserAsync(userId)
    .ParallelAsync(GetOrdersAsync(userId))
    .ParallelAsync(GetPreferencesAsync(userId))
    .WhenAllAsync()
    .BindAsync(
        (user, orders, preferences) =>
            CreateProfileAsync(user, orders, preferences),
        ct);
```

</details>

<details>
<summary><b>Discriminated Error Matching</b></summary>

```csharp
return ProcessOrder(order).MatchError(
    onValidation: err => Results.BadRequest(new { errors = err.FieldErrors }),
    onNotFound: err => Results.NotFound(new { message = err.Detail }),
    onConflict: err => Results.Conflict(new { message = err.Detail }),
    onUnauthorized: _ => Results.Unauthorized(),
    onSuccess: order => Results.Ok(order)
);
```

</details>

<details>
<summary><b>HTTP Integration</b></summary>

```csharp
// Read HTTP response as Result with status code handling
var result = await _httpClient.GetAsync($"api/users/{userId}", ct)
    .HandleNotFoundAsync(Error.NotFound("User not found"))
    .HandleUnauthorizedAsync(Error.Unauthorized("Please login"))
    .HandleServerErrorAsync(code => Error.ServiceUnavailable($"API error: {code}"))
    .ReadResultFromJsonAsync(UserContext.Default.User, ct)
    .TapAsync(user => _logger.LogInformation("Retrieved user: {UserId}", user.Id));

// Or use EnsureSuccess for generic error handling
var product = await _httpClient.GetAsync($"api/products/{productId}", ct)
    .EnsureSuccessAsync(code => Error.Unexpected($"Failed to get product: {code}"))
    .ReadResultFromJsonAsync(ProductContext.Default.Product, ct);
```

</details>

<details>
<summary><b>FluentValidation Integration</b></summary>

```csharp
public class User : Aggregate<UserId>
{
    public FirstName FirstName { get; }
    public LastName LastName { get; }
    public EmailAddress Email { get; }

    public static Result<User> TryCreate(FirstName firstName, LastName lastName, EmailAddress email)
    {
        var user = new User(firstName, lastName, email);
        return Validator.ValidateToResult(user);
    }

    private static readonly InlineValidator<User> Validator = new()
    {
        v => v.RuleFor(x => x.FirstName).NotNull(),
        v => v.RuleFor(x => x.LastName).NotNull(),
        v => v.RuleFor(x => x.Email).NotNull(),
    };
}
```

</details>

?? **[Browse all examples](Examples/)** | ?? **[Complete documentation](https://xavierjohn.github.io/FunctionalDDD/articles/examples.html)**

---

## Contributing

Contributions are welcome! This project follows standard GitHub workflow:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

### Guidelines

Please ensure:
- ? All tests pass (`dotnet test`)
- ? Code follows existing style conventions
- ? New features include tests and documentation
- ? Commit messages are clear and descriptive

For major changes, please open an issue first to discuss what you would like to change.

---

## License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## Related Projects

- **[CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions)** - Functional Extensions for C# by Vladimir Khorikov. This library was inspired by Vladimir's excellent training materials and takes a complementary approach with enhanced DDD support and comprehensive documentation.

---

## Community & Support

- ?? **[Documentation](https://xavierjohn.github.io/FunctionalDDD/)**
- ?? **[Discussions](https://github.com/xavierjohn/FunctionalDDD/discussions)** - Ask questions, share ideas
- ?? **[Issues](https://github.com/xavierjohn/FunctionalDDD/issues)** - Report bugs or request features
- ? **[Star this repo](https://github.com/xavierjohn/FunctionalDDD)** if you find it useful!

### Learning Resources

- ?? **[YouTube: Functional DDD Explanation](https://youtu.be/45yk2nuRjj8?t=682)** - Third-party video explaining the library concepts
- ?? **[Pluralsight: Applying Functional Principles in C#](https://enterprisecraftsmanship.com/ps-func)**
- ?? **[Pluralsight: Domain-Driven Design in Practice](https://app.pluralsight.com/library/courses/domain-driven-design-in-practice/table-of-contents)**

---

<div align="center">

**[? Back to Top](#functional-domain-driven-design)**

Made with ?? by the FunctionalDDD community

</div>
