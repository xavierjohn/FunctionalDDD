# FluentValidation Integration

[![NuGet Package](https://img.shields.io/nuget/v/Trellis.FluentValidation.svg)](https://www.nuget.org/packages/Trellis.FluentValidation)

Seamlessly convert [FluentValidation](https://docs.fluentvalidation.net) errors into Trellis `ValidationError` objects for Railway Oriented Programming pipelines.

## Installation

```bash
dotnet add package Trellis.FluentValidation
dotnet add package FluentValidation
```

## Quick Start

**Without this extension:**

```csharp
var validationResult = validator.Validate(user);
if (!validationResult.IsValid)
{
    var errors = validationResult.Errors
        .Select(e => Error.Validation(e.ErrorMessage, e.PropertyName));
    return Result.Failure<User>(Error.Aggregate(errors));
}
return Result.Success(user);
```

**With this extension:**

```csharp
return Validator.ValidateToResult(user);
```

## Usage

### Inline Validator

```csharp
public partial class User : Aggregate<UserId>
{
    public FirstName FirstName { get; }
    public int Age { get; }

    public static Result<User> TryCreate(FirstName firstName, int age)
    {
        var user = new User(firstName, age);
        return Validator.ValidateToResult(user);
    }

    private static readonly InlineValidator<User> Validator = new()
    {
        v => v.RuleFor(x => x.FirstName).NotNull(),
        v => v.RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18)
            .WithMessage("Must be 18 or older")
    };
}
```

### Separate Validator Class

```csharp
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.FirstName).NotNull().WithMessage("First name is required");
        RuleFor(x => x.Age).GreaterThanOrEqualTo(18).WithMessage("Must be 18 or older");
    }
}

// Use it
private static readonly UserValidator Validator = new();
return Validator.ValidateToResult(user);
```

### Async Validation

```csharp
return await Validator.ValidateToResultAsync(user, cancellationToken: cancellationToken);
```

## API

| Method | Use Case |
|--------|----------|
| `ValidateToResult(instance)` | Sync validation → `Result<T>` |
| `ValidateToResultAsync(instance, cancellationToken: ct)` | Async validation (DB/API checks) → `Task<Result<T>>` |
| `validationResult.ToResult(value)` | Convert `FluentValidation.ValidationResult` → `Result<T>` |

## Best Practices

1. **Use InlineValidator for simple cases**, AbstractValidator for complex logic
2. **Validate value objects separately** — First validate VOs, then aggregate invariants
3. **Use async only when needed** — DB/API checks justify async, format checks do not
4. **Combine with ROP** — FluentValidation for structure/format, `Ensure` for business rules

## Related Packages

- [Trellis.Results](https://www.nuget.org/packages/Trellis.Results) — Core `Result<T>` type
- [Trellis.DomainDrivenDesign](https://www.nuget.org/packages/Trellis.DomainDrivenDesign) — Entity and aggregate patterns
- [Trellis.Primitives](https://www.nuget.org/packages/Trellis.Primitives) — Type-safe value objects

## License

MIT — see [LICENSE](https://github.com/xavierjohn/Trellis/blob/main/LICENSE) for details.
