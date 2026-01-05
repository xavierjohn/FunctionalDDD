# API Documentation for FunctionalDdd Library

Welcome to the FunctionalDDD API reference. This library brings Railway-Oriented Programming (ROP) and Domain-Driven Design (DDD) patterns to C#, enabling you to write robust, maintainable code with explicit error handling.

## Core Packages

### Railway-Oriented Programming

> **New to Railway-Oriented Programming?** The concept uses a railway track analogy where operations flow along a success track or switch to an error track. For a gentle introduction to the philosophy, see [this introductory article](https://blog.logrocket.com/what-is-railway-oriented-programming/). Our documentation provides a comprehensive, production-ready C# implementation with type safety, async support, and real-world patterns.

#### [Result](xref:FunctionalDdd.Result`1)

The Result type used in functional programming languages to represent a success value or an error. Core to Railway-Oriented Programming, it allows you to chain operations where each step can succeed or fail, automatically handling the error path.

**Key operations:** `Bind`, `Map`, `Tap`, `Ensure`, `Match`, `Combine`, `Compensate`

#### [Maybe](xref:FunctionalDdd.Maybe`1)

The Maybe type represents an optional value that may or may not exist. Use it to eliminate null reference exceptions and make optionality explicit in your type system.

**Key operations:** `Map`, `Bind`, `Match`, `GetValueOrDefault`, `ToResult`

#### [Error Types](xref:FunctionalDdd.Error)

A hierarchy of error types for representing different failure scenarios:
- `ValidationError` - Input validation failures with field-level details
- `NotFoundError` - Resource not found (404)
- `UnauthorizedError` - Authentication required (401)
- `ForbiddenError` - Insufficient permissions (403)
- `ConflictError` - Resource conflicts (409)
- `DomainError` - Business rule violations (422)
- `UnexpectedError` - Unexpected system errors (500)
- `ServiceUnavailableError` - Service unavailable (503)

---

## Domain-Driven Design

### [Aggregate](xref:FunctionalDdd.Aggregate`1)

A DDD aggregate is a cluster of domain objects that can be treated as a single unit. An aggregate will have one of its component objects be the aggregate root. Any references from outside the aggregate should only go to the aggregate root. The root can thus ensure the integrity of the aggregate as a whole.

**Key features:** Encapsulation, consistency boundaries, transactional boundaries

[Read more about DDD Aggregates](https://martinfowler.com/bliki/DDD_Aggregate.html)

### [Entity](xref:FunctionalDdd.Entity`1)

A domain object that has a unique identity that runs through time and different representations. Two entities with the same identity are considered the same, even if their attributes differ.

### [ValueObject](xref:FunctionalDdd.ValueObject)

A value object is an object that represents a descriptive aspect of the domain with no conceptual identity. It is a small, simple object that encapsulates a concept from your problem domain. Unlike an aggregate, a value object does not have a unique identity and is immutable. Value objects support and enrich the ubiquitous language of your domain.

**Key characteristics:** Immutability, equality by value, no identity

---

## Integration Packages

### ASP.NET Core Integration

#### [ToActionResult Extensions](xref:FunctionalDdd.ToActionResultExtensions)

Convert `Result<T>` to ASP.NET Core action results for MVC controllers.

**Features:** Automatic status code mapping, Problem Details (RFC 7807) format, field-level validation errors

#### [ToHttpResult Extensions](xref:FunctionalDdd.ToHttpResultExtensions)

Convert `Result<T>` to `IResult` for Minimal API endpoints.

**Features:** Fluent error matching, pagination support, Unit type handling (204 No Content)

### HTTP Client Integration

#### [HttpResponseExtensions](xref:FunctionalDdd.HttpResponseExtensions)

Extension methods for `HttpResponseMessage` that enable functional HTTP communication with Result types.

**Key methods:**
- `HandleNotFound` / `HandleUnauthorized` / `HandleForbidden` / `HandleConflict` - Handle specific status codes
- `HandleClientError` / `HandleServerError` - Handle error ranges (4xx, 5xx)
- `EnsureSuccess` - Functional alternative to `EnsureSuccessStatusCode()`
- `ReadResultFromJsonAsync` - Deserialize JSON to `Result<T>`
- `ReadResultMaybeFromJsonAsync` - Deserialize JSON to `Result<Maybe<T>>`

### FluentValidation Integration

#### [ValidateToResult Extensions](xref:FunctionalDdd.ValidateToResultExtensions)

Convert FluentValidation results to `Result<T>` for seamless integration with Railway-Oriented Programming.

**Features:** Automatic `ValidationError` creation, field-level error mapping, async validation support

---

## Common Value Objects

### [EmailAddress](xref:FunctionalDdd.EmailAddress)

A validated email address value object. Ensures email format correctness at construction.

### [RequiredString](xref:FunctionalDdd.RequiredString)

A non-null, non-empty string value object. Prevents primitive obsession for required text fields.

### [RequiredGuid](xref:FunctionalDdd.RequiredGuid)

A validated GUID value object that cannot be empty (Guid.Empty).

### [ScalarValueObject](xref:FunctionalDdd.ScalarValueObject`1)

Base class for creating custom value objects that wrap a single primitive value.

**Common examples:** `FirstName`, `LastName`, `OrderId`, `UserId`, `ProductCode`

---

## Observability

### [OpenTelemetry Tracing](xref:FunctionalDdd.RopTracerProviderBuilderExtensions)

Built-in distributed tracing support for Railway-Oriented Programming operations.

**Features:**
- Automatic span creation for `Bind`, `Map`, `Tap`, `Ensure` operations
- Error tracking and status codes
- Trace correlation across async operations
- Integration with Jaeger, Zipkin, Application Insights

---

## Next Steps

- **Get Started:** Read the [Introduction](~/articles/intro.md) guide
- **Learn Basics:** Explore [Core Concepts](~/articles/basics.md)
- **Integration:** See [ASP.NET Core](~/articles/integration-aspnet.md), [HTTP Client](~/articles/integration-http.md), and [FluentValidation](~/articles/integration-fluentvalidation.md) guides
- **Examples:** Browse [Real-World Examples](~/articles/examples.md)

---

## Package Reference

| Package | Description | NuGet |
|---------|-------------|-------|
| FunctionalDDD.RailwayOrientedProgramming | Core Result/Maybe types | [![NuGet](https://img.shields.io/nuget/v/FunctionalDDD.RailwayOrientedProgramming.svg)](https://www.nuget.org/packages/FunctionalDDD.RailwayOrientedProgramming) |
| FunctionalDDD.Asp | ASP.NET Core integration | [![NuGet](https://img.shields.io/nuget/v/FunctionalDDD.Asp.svg)](https://www.nuget.org/packages/FunctionalDDD.Asp) |
| FunctionalDDD.Http | HTTP client extensions | [![NuGet](https://img.shields.io/nuget/v/FunctionalDDD.Http.svg)](https://www.nuget.org/packages/FunctionalDDD.Http) |
| FunctionalDDD.FluentValidation | FluentValidation integration | [![NuGet](https://img.shields.io/nuget/v/FunctionalDDD.FluentValidation.svg)](https://www.nuget.org/packages/FunctionalDDD.FluentValidation) |
| FunctionalDDD.CommonValueObjects | Reusable value objects | [![NuGet](https://img.shields.io/nuget/v/FunctionalDDD.CommonValueObjects.svg)](https://www.nuget.org/packages/FunctionalDDD.CommonValueObjects) |
| FunctionalDDD.DomainDrivenDesign | DDD building blocks | [![NuGet](https://img.shields.io/nuget/v/FunctionalDDD.DomainDrivenDesign.svg)](https://www.nuget.org/packages/FunctionalDDD.DomainDrivenDesign) |
