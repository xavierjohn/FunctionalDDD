# FunctionalDdd.Testing

[![NuGet Package](https://img.shields.io/nuget/v/FunctionalDdd.Testing.svg)](https://www.nuget.org/packages/FunctionalDdd.Testing)

Testing utilities and FluentAssertions extensions for **FunctionalDdd** - Write expressive, maintainable tests for Railway-Oriented Programming patterns.

## Installation

```bash
dotnet add package FunctionalDdd.Testing
```

## Features

✅ **FluentAssertions Extensions** - Expressive assertions for `Result<T>`, `Maybe<T>`, and `Error` types  
🏗️ **Test Builders** - Fluent builders for creating test data  
🎭 **Fake Implementations** - In-memory fakes for repositories and infrastructure  
📖 **Readable Tests** - Write tests that read like specifications  
💡 **IntelliSense Support** - Discover test utilities through IntelliSense

## Quick Start

### Result Assertions

```csharp
using FunctionalDdd.Testing;

[Fact]
public void Should_Return_Success()
{
    var result = EmailAddress.TryCreate("user@example.com");

    result.Should()
        .BeSuccess()
        .Which.Value.Should().Contain("@");
}

[Fact]
public void Should_Return_NotFound_Error()
{
    var result = _repository.GetByIdAsync(userId, ct);

    result.Should()
        .BeFailureOfType<NotFoundError>()
        .Which.Should()
        .HaveDetail("User not found");
}
```

### Validation Error Assertions

```csharp
[Fact]
public void Should_Have_Multiple_Validation_Errors()
{
    var result = CreateUser("", "invalid-email", 15);

    result.Should()
        .BeFailureOfType<ValidationError>()
        .Which.Should()
        .HaveFieldCount(3)
        .And.HaveFieldError("firstName")
        .And.HaveFieldError("email")
        .And.HaveFieldErrorWithDetail("age", "Must be 18 or older");
}
```

### Maybe Assertions

```csharp
[Fact]
public void Should_Have_Value()
{
    var maybe = Maybe.From("hello");

    maybe.Should()
        .HaveValue()
        .Which.Should().Be("hello");
}

[Fact]
public void Should_Be_None()
{
    var maybe = Maybe.None<string>();

    maybe.Should().BeNone();
}
```

### Test Builders

```csharp
[Fact]
public void Should_Handle_NotFound()
{
    var result = ResultBuilder.NotFound<User>("User not found");

    result.Should()
        .BeFailureOfType<NotFoundError>();
}

[Fact]
public void Should_Build_Complex_Validation_Error()
{
    var error = ValidationErrorBuilder.Create()
        .WithFieldError("email", "Email is required")
        .WithFieldError("email", "Invalid email format")
        .WithFieldError("age", "Must be 18 or older")
        .Build();

    error.Should()
        .HaveFieldCount(2)
        .And.HaveFieldErrorWithDetail("email", "Email is required");
}
```

### Fake Repository

```csharp
public class UserServiceTests
{
    private readonly FakeRepository<User, UserId> _fakeRepository;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _fakeRepository = new FakeRepository<User, UserId>();
        _sut = new UserService(_fakeRepository);
    }

    [Fact]
    public async Task Should_Save_User_And_Publish_Event()
    {
        // Arrange
        var command = new CreateUserCommand("John", "Doe", "john@example.com");

        // Act
        var result = await _sut.CreateUserAsync(command, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        
        _fakeRepository.Exists(result.Value.Id).Should().BeTrue();
        _fakeRepository.PublishedEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserCreatedEvent>();
    }
}
```

## API Reference

### Result Assertions

| Method | Description |
|--------|-------------|
| `BeSuccess()` | Asserts the result is a success |
| `BeFailure()` | Asserts the result is a failure |
| `BeFailureOfType<TError>()` | Asserts the result failed with a specific error type |
| `HaveValue(expected)` | Asserts the success value equals the expected value |
| `HaveValueMatching(predicate)` | Asserts the success value satisfies a predicate |

### Error Assertions

| Method | Description |
|--------|-------------|
| `HaveCode(code)` | Asserts the error has the specified code |
| `HaveDetail(detail)` | Asserts the error has the specified detail message |
| `HaveDetailContaining(substring)` | Asserts the error detail contains a substring |
| `BeOfType<TError>()` | Asserts the error is of a specific type |

### ValidationError Assertions

| Method | Description |
|--------|-------------|
| `HaveFieldError(fieldName)` | Asserts the validation error contains a field error |
| `HaveFieldErrorWithDetail(field, detail)` | Asserts a field has a specific error detail |
| `HaveFieldCount(count)` | Asserts the number of field errors |

### Maybe Assertions

| Method | Description |
|--------|-------------|
| `HaveValue()` | Asserts the Maybe has a value |
| `BeNone()` | Asserts the Maybe has no value |
| `HaveValueEqualTo(expected)` | Asserts the value equals the expected value |

## Benefits

| Before | After |
|--------|-------|
| `result.IsSuccess.Should().BeTrue()` | `result.Should().BeSuccess()` |
| `result.Error.Should().BeOfType<NotFoundError>()` | `result.Should().BeFailureOfType<NotFoundError>()` |
| `maybe.HasValue.Should().BeTrue()` | `maybe.Should().HaveValue()` |
| Manual repository setup with mocks | `new FakeRepository<User, UserId>()` |

**Advantages:**
- ✂️ **Less Code** - More concise assertions
- 💬 **Better Error Messages** - Detailed failure descriptions
- 💡 **IntelliSense Guided** - Discover test patterns through IDE
- 📖 **Readable Tests** - Tests read like specifications
- 🔒 **Type Safe** - Compiler-enforced correctness

## Related Packages

- **FunctionalDdd.RailwayOrientedProgramming** - Core Result/Maybe types
- **FunctionalDdd.DomainDrivenDesign** - DDD building blocks

## License

MIT

## Contributing

Contributions are welcome! See the main repository for guidelines.
