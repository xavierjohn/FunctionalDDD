# Fluent Validation Extension

[![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.FluentValidation.svg)](https://www.nuget.org/packages/FunctionalDDD.FluentValidation)

This library seamlessly integrates [FluentValidation](https://docs.fluentvalidation.net) with Railway Oriented Programming, converting FluentValidation errors into FunctionalDDD `ValidationError` objects.

## Table of Contents

- [Installation](#installation)
- [Why Use This Extension](#why-use-this-extension)
- [Quick Start](#quick-start)
  - [Inline Validator](#inline-validator)
  - [Separate Validator Class](#separate-validator-class)
- [Core Concepts](#core-concepts)
- [Best Practices](#best-practices)
- [Resources](#resources)

## Installation

Install both packages via NuGet:

```bash
dotnet add package FunctionalDDD.FluentValidation
dotnet add package FluentValidation
```

## Why Use This Extension

**Without this extension**, you would manually convert validation errors:

```csharp
var validator = new UserValidator();
var validationResult = validator.Validate(user);
if (!validationResult.IsValid)
{
    var errors = validationResult.Errors
        .Select(e => Error.Validation(e.ErrorMessage, e.PropertyName));
    return Result.Failure<User>(Error.Aggregate(errors));
}
return Result.Success(user);
```

**With this extension**, validation integrates seamlessly with ROP:

```csharp
return Validator.ValidateToResult(user);
```

## Quick Start

### Inline Validator

The simplest approach using `InlineValidator`:

```csharp
public partial class User : Aggregate<UserId>
{
    public FirstName FirstName { get; }
    public LastName LastName { get; }
    public EmailAddress Email { get; }
    public int Age { get; }

    public static Result<User> TryCreate(
        FirstName firstName, 
        LastName lastName, 
        EmailAddress email,
        int age)
    {
        var user = new User(firstName, lastName, email, age);
        return Validator.ValidateToResult(user);
    }

    private User(FirstName firstName, LastName lastName, EmailAddress email, int age)
        : base(UserId.NewUnique())
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Age = age;
    }

    // Fluent Validation
    private static readonly InlineValidator<User> Validator = new()
    {
        v => v.RuleFor(x => x.FirstName).NotNull(),
        v => v.RuleFor(x => x.LastName).NotNull(),
        v => v.RuleFor(x => x.Email).NotNull(),
        v => v.RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18)
            .WithMessage("Must be 18 or older")
    };
}
```

### Separate Validator Class

For complex validation logic, create a dedicated validator:

```csharp
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.FirstName)
            .NotNull()
            .WithMessage("First name is required");
            
        RuleFor(x => x.LastName)
            .NotNull()
            .WithMessage("Last name is required");
            
        RuleFor(x => x.Email)
            .NotNull()
            .EmailAddress()
            .WithMessage("Valid email address is required");
            
        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18)
            .WithMessage("Must be 18 or older")
            .LessThan(120)
            .WithMessage("Age must be realistic");
    }
}

public class User
{
    private static readonly UserValidator Validator = new();
    
    public static Result<User> TryCreate(
        FirstName firstName, 
        LastName lastName, 
        EmailAddress email,
        int age)
    {
        var user = new User(firstName, lastName, email, age);
        return Validator.ValidateToResult(user);
    }
}
```

## Core Concepts

| Feature | Use Case | Example |
|---------|----------|----------|
| **Sync Validation** | Simple validation rules | `Validator.ValidateToResult(user)` |
| **Async Validation** | Database/API checks | `await Validator.ValidateToResultAsync(user, ct)` |
| **Combine Integration** | Multi-step validation | `Email.TryCreate().Combine(Name.TryCreate()).Bind(...)` |
| **Conditional Rules** | Context-dependent validation | `RuleFor(x => x.Address).When(x => x.NeedsShipping)` |
| **Custom Messages** | Property-specific errors | `WithMessage(x => $"Value {x.Price} invalid")` |
| **RuleSets** | Different scenarios | `IncludeRuleSets("Create")` |

## Best Practices

1. **Use InlineValidator for simple cases**, AbstractValidator for complex logic with dependencies.

2. **Validate value objects separately from aggregates**  
   First validate value objects, then validate aggregate invariants.

3. **Use async validation only when necessary**  
   Database checks and API calls justify async, simple format checks do not.

4. **Provide meaningful error messages with context**  
   Use `WithMessage(x => ...)` to include property values in error messages.

5. **Use When() for conditional validation**  
   Apply rules based on object state or external conditions.

6. **Leverage RuleSets for different validation scenarios**  
   Separate validation rules for Create, Update, Delete operations.

7. **Combine FluentValidation with ROP**  
   Use FluentValidation for structure/format, ROP Ensure for business rules.

## Resources

- [SAMPLES.md](SAMPLES.md) - Comprehensive examples and advanced patterns
- [FluentValidation Documentation](https://docs.fluentvalidation.net) - Official FluentValidation docs
- [Railway Oriented Programming](../RailwayOrientedProgramming/README.md) - Core Result<T> concepts
- [Domain-Driven Design](../DomainDrivenDesign/README.md) - Entity and value object patterns