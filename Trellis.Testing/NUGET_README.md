# Testing Utilities

[![NuGet Package](https://img.shields.io/nuget/v/Trellis.Testing.svg)](https://www.nuget.org/packages/Trellis.Testing)

FluentAssertions extensions, test builders, and fake implementations for testing Railway Oriented Programming patterns with Trellis.

## Installation

```bash
dotnet add package Trellis.Testing
```

## Quick Start

### Result Assertions

```csharp
using Trellis.Testing;

// Success
result.Should().BeSuccess()
    .Which.Value.Should().Be(expected);

// Failure by type
result.Should().BeFailureOfType<NotFoundError>()
    .Which.Should().HaveDetail("User not found");
```

### Validation Error Assertions

```csharp
result.Should()
    .BeFailureOfType<ValidationError>()
    .Which.Should()
    .HaveFieldCount(3)
    .And.HaveFieldError("firstName")
    .And.HaveFieldError("email")
    .And.HaveFieldErrorWithDetail("age", "Must be 18 or older");
```

### Maybe Assertions

```csharp
maybe.Should().HaveValue().Which.Should().Be("hello");
maybe.Should().BeNone();
```

### Test Builders

```csharp
using Trellis.Testing.Builders;

// Quick Result creation
var result = ResultBuilder.NotFound<User>("User not found");

// Complex validation errors
var error = ValidationErrorBuilder.Create()
    .WithFieldError("email", "Email is required")
    .WithFieldError("age", "Must be 18 or older")
    .Build();
```

### Fake Repository

```csharp
using Trellis.Testing.Fakes;

var repo = new FakeRepository<User, UserId>();
var sut = new UserService(repo);

var result = await sut.CreateUserAsync(command, CancellationToken.None);

result.Should().BeSuccess();
repo.Exists(result.Value.Id).Should().BeTrue();
repo.PublishedEvents.Should().ContainSingle()
    .Which.Should().BeOfType<UserCreatedEvent>();
```

### TestActorProvider — Scoped Actor Switching

Eliminates `try/finally` boilerplate in authorization tests. `WithActor` temporarily switches the actor and restores the previous one on dispose.

```csharp
using Trellis.Testing.Fakes;

var actorProvider = new TestActorProvider("admin", "Orders.Read", "Orders.Write");

// Temporarily switch to a restricted user
await using var scope = actorProvider.WithActor("user-1", "Orders.Read");
var result = await mediator.Send(new CreateOrderCommand());
result.Should().BeFailure();
// scope disposes → actor reverts to admin
```

### ReplaceResourceLoader — DI Registration Replacement

Replaces existing `IResourceLoader<TCommand, TResource>` registrations in one call — no manual `RemoveAll` needed. Registered as scoped, matching the production lifetime.

```csharp
using Trellis.Testing;

// Stateless fake — capture a pre-created instance
var fakeLoader = new FakeOrderResourceLoader(fakeRepo);
builder.ConfigureServices(services =>
{
    services.ReplaceResourceLoader<CancelOrderCommand, Order>(_ => fakeLoader);
});

// Scoped dependency — resolve from the container
builder.ConfigureServices(services =>
{
    services.ReplaceResourceLoader<CancelOrderCommand, Order>(
        sp => new FakeOrderResourceLoader(sp.GetRequiredService<AppDbContext>()));
});
```

### CreateClientWithActor — Integration Test HttpClient

Creates an `HttpClient` with the `X-Test-Actor` header pre-set for authorization integration tests.

```csharp
using Trellis.Testing;

var client = _factory.CreateClientWithActor("user-1", "Orders.Create", "Orders.Read");
var response = await client.PostAsync("/api/orders", content);
```

## Assertion API Reference

### Result

| Method | Description |
|--------|-------------|
| `BeSuccess()` | Assert result is success |
| `BeFailure()` | Assert result is failure |
| `BeFailureOfType<TError>()` | Assert failure with specific error type |
| `HaveValue(expected)` | Assert success value equals expected |
| `HaveValueMatching(predicate)` | Assert success value satisfies predicate |
| `HaveValueEquivalentTo(expected)` | Assert success value using structural comparison |
| `HaveErrorCode(code)` | Assert failure has specific error code |
| `HaveErrorDetail(detail)` | Assert failure has specific error detail |
| `HaveErrorDetailContaining(substring)` | Assert failure error detail contains substring |

Async variants (`BeSuccessAsync`, `BeFailureAsync`, `BeFailureOfTypeAsync`) are available for `Task<Result<T>>` and `ValueTask<Result<T>>`.

### Error

| Method | Description |
|--------|-------------|
| `Be(expected)` | Assert error equals expected (by Code) |
| `HaveCode(code)` | Assert error code |
| `HaveDetail(detail)` | Assert error detail message |
| `HaveDetailContaining(substring)` | Assert error detail contains substring |
| `HaveInstance(instance)` | Assert error instance identifier |
| `BeOfType<TError>()` | Assert error is of a specific type |

### ValidationError

| Method | Description |
|--------|-------------|
| `HaveFieldError(fieldName)` | Assert field has error |
| `HaveFieldErrorWithDetail(field, detail)` | Assert field has specific error |
| `HaveFieldCount(count)` | Assert number of field errors |

### Maybe

| Method | Description |
|--------|-------------|
| `HaveValue()` | Assert Maybe has a value |
| `BeNone()` | Assert Maybe has no value |
| `HaveValueEqualTo(expected)` | Assert value equals expected |
| `HaveValueMatching(predicate)` | Assert value satisfies predicate |
| `HaveValueEquivalentTo(expected)` | Assert value using structural comparison |

## Before & After

| Before | After |
|--------|-------|
| `result.IsSuccess.Should().BeTrue()` | `result.Should().BeSuccess()` |
| `result.Error.Should().BeOfType<NotFoundError>()` | `result.Should().BeFailureOfType<NotFoundError>()` |
| `maybe.HasValue.Should().BeTrue()` | `maybe.Should().HaveValue()` |

## Related Packages

- [Trellis.Results](https://www.nuget.org/packages/Trellis.Results) — Core `Result<T>` and `Maybe<T>` types
- [Trellis.DomainDrivenDesign](https://www.nuget.org/packages/Trellis.DomainDrivenDesign) — DDD building blocks
- [Trellis.Authorization](https://www.nuget.org/packages/Trellis.Authorization) — `Actor`, `IActorProvider`, authorization primitives

## License

MIT — see [LICENSE](https://github.com/xavierjohn/Trellis/blob/main/LICENSE) for details.
