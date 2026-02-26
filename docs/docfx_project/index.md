# Trellis

> **Structured building blocks for AI-driven enterprise software**

Write less code that reads like English using Railway-Oriented Programming and Domain-Driven Design for .NET 10.

## Before & After

### Traditional Approach

```csharp
// 20 lines of repetitive error checking — easy to miss a check!
var firstName = ValidateFirstName(input.FirstName);
if (firstName == null) return BadRequest("Invalid first name");

var lastName = ValidateLastName(input.LastName);
if (lastName == null) return BadRequest("Invalid last name");

var email = ValidateEmail(input.Email);
if (email == null) return BadRequest("Invalid email");

var user = CreateUser(firstName, lastName, email);
if (user == null) return BadRequest("Cannot create user");

if (!_repository.EmailExists(email))
    return Conflict("Email already registered");

_repository.Save(user);
_emailService.SendWelcome(user.Email);
return Ok(user);
```

### With Trellis

```csharp
// 8 lines — reads like a story: validate → create → check → save → notify
return FirstName.TryCreate(input.FirstName)
    .Combine(LastName.TryCreate(input.LastName))
    .Combine(EmailAddress.TryCreate(input.Email))
    .Bind((first, last, email) => User.TryCreate(first, last, email))
    .Ensure(user => !_repository.EmailExists(user.Email), Error.Conflict("Email registered"))
    .Tap(user => _repository.Save(user))
    .Tap(user => _emailService.SendWelcome(user.Email))
    .Match(onSuccess: user => Ok(user), onFailure: error => BadRequest(error.Detail));
```

**60% less code. Reads like English. Impossible to skip error handling.**

---

## What You Get

- **Result\<T\> and Maybe\<T\>** — composable error handling and optional values. No exceptions, no null. [Learn more](articles/basics.md)
- **Type-safe value objects** — `FirstName`, `EmailAddress`, `OrderId` generated from one-line declarations. If it exists, it's valid. [Learn more](articles/required-enum.md)
- **Aggregates and entities** — DDD building blocks with domain events and consistency boundaries. [Learn more](articles/aggregate-factory-pattern.md)
- **10 discriminated error types** — `ValidationError`, `NotFoundError`, `ConflictError`, etc. Map automatically to HTTP status codes. [Learn more](articles/error-handling.md)
- **9 pipeline operations** — `Bind`, `Map`, `Tap`, `Ensure`, `Combine`, `Match`, and more. All with async variants. [Learn more](articles/basics.md)
- **18 Roslyn analyzers** — catch unsafe `.Value` access, forgotten results, and anti-patterns at compile time. [Learn more](articles/analyzers/index.md)
- **State machine integration** — `FireResult()` returns `Result<T>` instead of throwing. [Learn more](articles/state-machines.md)
- **AI-ready patterns** — structured building blocks that AI can generate correctly. [Learn more](articles/ai-code-generation.md)

---

## Quick Start

```bash
dotnet add package Trellis.Results
dotnet add package Trellis.Primitives
dotnet add package Trellis.Primitives.Generator
dotnet add package Trellis.Analyzers
```

```csharp
using Trellis;

// Define a value object — one line
public partial class EmailAddress : RequiredString { }

// Use it — invalid values are impossible
Result<EmailAddress> result = EmailAddress.TryCreate("user@example.com");

result.Match(
    onSuccess: email => Console.WriteLine($"Valid: {email}"),
    onFailure: error => Console.WriteLine($"Error: {error.Detail}")
);
```

See the [Introduction](articles/intro.md) for a full walkthrough, or jump to [Basics](articles/basics.md) for all pipeline operations.

---

## Packages

### Core

| Package | Purpose |
|---------|---------|
| `Trellis.Results` | Result\<T\>, Maybe\<T\>, error types, pipeline operations, async support |
| `Trellis.DomainDrivenDesign` | Aggregate, Entity, ValueObject, Domain Events |
| `Trellis.Primitives` | RequiredString, RequiredGuid, RequiredInt, RequiredDecimal + 11 ready-to-use value objects |
| `Trellis.Primitives.Generator` | Source generator for value object boilerplate |
| `Trellis.Analyzers` | 18 Roslyn analyzers enforcing ROP best practices at compile time |

### Integration

| Package | Purpose |
|---------|---------|
| `Trellis.Asp` | Result → HTTP responses: `ToActionResult()` for MVC, `ToHttpResult()` for Minimal API, [configurable error mapping](articles/integration-aspnet.md) |
| `Trellis.Http` | [HttpClient extensions](articles/integration-http.md) returning Result\<T\> |
| `Trellis.FluentValidation` | [Bridge FluentValidation errors](articles/integration-fluentvalidation.md) to Result\<T\> |
| `Trellis.Testing` | FluentAssertions extensions, test builders, fakes |
| `Trellis.Stateless` | Wraps Stateless state machine [Fire() to return Result\<T\>](articles/state-machines.md) |

---

## Performance

Trellis adds **11–16 nanoseconds** per operation — **0.002%** of a typical database query. Zero extra allocations on Combine.

See [Performance & Benchmarks](articles/performance.md) for detailed analysis.

---

## The Vision

Trellis is designed so that both humans and AI can produce correct, maintainable, enterprise-grade code by following the structure the framework provides.

1. A human writes a **specification** describing business requirements in plain language.
2. An AI produces enterprise software using Trellis as the structural foundation.
3. Domain terms map directly to Trellis constructs — aggregates, value objects, entities, domain events, state machines.
4. The **type system and compiler enforce correctness** — impossible to skip error handling or make illegal state transitions.
5. When requirements change, changes propagate correctly through the type system.

See [Trellis for AI Code Generation](articles/ai-code-generation.md) for details on spec-to-code mapping.

---

## Learn More

| Path | Start Here |
|------|------------|
| **New to Trellis** | [Introduction](articles/intro.md) → [Basics](articles/basics.md) → [Examples](articles/examples.md) |
| **Integrating** | [ASP.NET Core](articles/integration-aspnet.md) · [HTTP Client](articles/integration-http.md) · [EF Core](articles/integration-ef.md) · [FluentValidation](articles/integration-fluentvalidation.md) |
| **Architecture** | [Clean Architecture](articles/clean-architecture.md) · [Aggregate Factory Pattern](articles/aggregate-factory-pattern.md) |
| **Reference** | [API Documentation](api/index.md) · [Analyzer Rules](articles/analyzers/index.md) · [Debugging](articles/debugging.md) |
| **Migrating** | [Migration Guide from FunctionalDDD](articles/migration.md) |
