# FluentValidation Integration

**Level:** Intermediate 📚 | **Time:** 30-40 min | **Prerequisites:** [Basics](basics.md)

Integrate FluentValidation with Railway-Oriented Programming using the **FunctionalDDD.FluentValidation** adapter. This package provides seamless conversion from FluentValidation results to `Result<T>`, enabling you to use FluentValidation's powerful validation framework within your ROP workflows.

> **Note:** FunctionalDDD.FluentValidation is an **adapter library** that bridges FluentValidation and Railway-Oriented Programming. It does not replace or extend FluentValidation—it simply converts FluentValidation's validation results to `Result<T>`. For comprehensive FluentValidation documentation, see the [official FluentValidation docs](https://docs.fluentvalidation.net/).

## Table of Contents

- [Installation](#installation)
- [What the Adapter Provides](#what-the-adapter-provides)
- [Basic Usage](#basic-usage)
- [Inline Validators](#inline-validators)
- [Separate Validator Classes](#separate-validator-classes)
- [Async Validation](#async-validation)
- [Dependency Injection](#dependency-injection)
- [Best Practices](#best-practices)

## Installation

```bash
dotnet add package FluentValidation
dotnet add package FunctionalDdd.FluentValidation
```

## What the Adapter Provides

The **FunctionalDdd.FluentValidation** adapter provides extension methods to convert FluentValidation results to `Result<T>`:

### Core Extension Methods

```csharp
// Synchronous validation
Result<T> ValidateToResult<T>(this IValidator<T> validator, T instance);

// Asynchronous validation
Task<Result<T>> ValidateToResultAsync<T>(
    this IValidator<T> validator, 
    T instance, 
    CancellationToken ct);
```

**What happens:**
- ✅ **Success**: Returns `Result.Success(instance)` with the validated object
- ❌ **Failure**: Converts FluentValidation errors to `ValidationError` with field-level details
- 🔄 **Automatic Mapping**: FluentValidation's `ValidationFailure` → `ValidationError.FieldError`

### Conversion Details

```csharp
// FluentValidation result
var validationResult = validator.Validate(command);

// Manual conversion (what the adapter does internally)
if (validationResult.IsValid)
{
    return Result.Success(command);
}
else
{
    var fieldErrors = validationResult.Errors
        .GroupBy(e => e.PropertyName)
        .Select(g => new ValidationError.FieldError(
            FieldName: g.Key,
            Details: g.Select(e => e.ErrorMessage).ToArray()))
        .ToArray();
    
    return Result.Failure<Command>(new ValidationError(fieldErrors));
}

// Adapter does this automatically
return validator.ValidateToResult(command);
```

## Basic Usage

### Example: Command Validation

```csharp
using FluentValidation;
using FunctionalDdd;

// 1. Define your FluentValidation validator (standard FluentValidation)
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Age).GreaterThanOrEqualTo(18);
    }
}

// 2. Use the adapter to convert validation results to Result<T>
public class UserService
{
    private readonly IValidator<CreateUserCommand> _validator;

    public UserService(IValidator<CreateUserCommand> validator)
    {
        _validator = validator;
    }

    public Result<User> CreateUser(CreateUserCommand command)
    {
        // ValidateToResult converts FluentValidation results to Result<T>
        return _validator.ValidateToResult(command)
            .Bind(validCommand => User.Create(validCommand))
            .Tap(user => _repository.Add(user));
    }
}
```

**HTTP Response (validation failure):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["'Email' must not be empty."],
    "Age": ["'Age' must be greater than or equal to '18'."]
  }
}
```

## Inline Validators

Use FluentValidation's `InlineValidator` for simple validation within aggregates:

```csharp
using FluentValidation;
using FunctionalDdd;

public class User : Aggregate<UserId>
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
        
        // ValidateToResult converts FluentValidation results to Result<User>
        return Validator.ValidateToResult(user);
    }

    private User(
        FirstName firstName, 
        LastName lastName, 
        EmailAddress email, 
        int age)
        : base(UserId.NewUnique())
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Age = age;
    }

    // Standard FluentValidation InlineValidator
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

**What you get:**
- ✅ Domain validation stays with the aggregate
- ✅ Automatic conversion to `Result<T>` via the adapter
- ✅ FluentValidation's rich rule set (see [FluentValidation docs](https://docs.fluentvalidation.net/))
- ✅ Error messages formatted as `ValidationError`

## Separate Validator Classes

For complex validation, use standard FluentValidation `AbstractValidator` classes:

```csharp
// Standard FluentValidation validator
public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new OrderItemValidator());
        RuleFor(x => x.ShippingAddress).SetValidator(new AddressValidator());
        RuleFor(x => x.TotalAmount).GreaterThan(0);
    }
}

// Use the adapter in your service
public class OrderService : IOrderService
{
    private readonly IValidator<CreateOrderCommand> _validator;
    private readonly IOrderRepository _repository;

    public OrderService(
        IValidator<CreateOrderCommand> validator,
        IOrderRepository repository)
    {
        _validator = validator;
        _repository = repository;
    }

    public Result<Order> CreateOrder(CreateOrderCommand command)
    {
        // Adapter converts FluentValidation result to Result<T>
        return _validator.ValidateToResult(command)
            .Bind(validCommand => Order.Create(validCommand))
            .Tap(order => _repository.Add(order));
    }
}
```

> **Tip:** For FluentValidation syntax and built-in validators (like `NotEmpty()`, `EmailAddress()`, `GreaterThan()`, etc.), see the [official FluentValidation documentation](https://docs.fluentvalidation.net/en/latest/built-in-validators.html).

## Async Validation

The adapter supports async validation with `ValidateToResultAsync`:

```csharp
// Standard FluentValidation async validator
public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    private readonly IUserRepository _repository;

    public RegisterUserValidator(IUserRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(BeUniqueEmailAsync)
            .WithMessage("Email is already registered");
        
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(3, 50)
            .MustAsync(BeUniqueUsernameAsync)
            .WithMessage("Username is already taken");
    }

    private async Task<bool> BeUniqueEmailAsync(string email, CancellationToken ct)
    {
        var exists = await _repository.ExistsByEmailAsync(email, ct);
        return !exists;
    }

    private async Task<bool> BeUniqueUsernameAsync(string username, CancellationToken ct)
    {
        var exists = await _repository.ExistsByUsernameAsync(username, ct);
        return !exists;
    }
}
```

### Async Usage with the Adapter

```csharp
public async Task<Result<User>> RegisterUserAsync(
    RegisterUserCommand command,
    CancellationToken ct)
{
    // ValidateToResultAsync converts async FluentValidation results to Result<T>
    return await _validator.ValidateToResultAsync(command, ct)
        .BindAsync((validCommand, cancellationToken) => 
            User.CreateAsync(validCommand, cancellationToken), ct)
        .TapAsync(async (user, cancellationToken) => 
            await _repository.SaveAsync(user, cancellationToken), ct);
}
```

**Key Points:**
- ✅ `ValidateToResultAsync` is the async adapter method
- ✅ Converts async FluentValidation results to `Result<T>`
- ✅ Supports `CancellationToken` propagation
- ✅ Works with FluentValidation's `MustAsync`, `CustomAsync`, etc.

## Dependency Injection

Register FluentValidation validators with ASP.NET Core DI as normal:

```csharp
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Register all FluentValidation validators from assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Or register specific validators
builder.Services.AddScoped<IValidator<CreateOrderCommand>, CreateOrderValidator>();
builder.Services.AddScoped<IValidator<RegisterUserCommand>, RegisterUserValidator>();

// Register your services that use the adapter
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();
```

### Inject Validators into Services

```csharp
public class UserService : IUserService
{
    private readonly IValidator<RegisterUserCommand> _registerValidator;
    private readonly IValidator<UpdateUserCommand> _updateValidator;
    private readonly IUserRepository _repository;

    public UserService(
        IValidator<RegisterUserCommand> registerValidator,
        IValidator<UpdateUserCommand> updateValidator,
        IUserRepository repository)
    {
        _registerValidator = registerValidator;
        _updateValidator = updateValidator;
        _repository = repository;
    }

    public async Task<Result<User>> RegisterAsync(
        RegisterUserCommand command,
        CancellationToken ct)
        // Adapter converts FluentValidation result to Result<T>
        => await _registerValidator.ValidateToResultAsync(command, ct)
            .BindAsync((cmd, cancellationToken) => 
                User.CreateAsync(cmd, cancellationToken), ct)
            .TapAsync(async (user, cancellationToken) => 
                await _repository.SaveAsync(user, cancellationToken), ct);

    public async Task<Result<User>> UpdateAsync(
        UpdateUserCommand command,
        CancellationToken ct)
        // Adapter converts FluentValidation result to Result<T>
        => await _updateValidator.ValidateToResultAsync(command, ct)
            .BindAsync(async (cmd, cancellationToken) => 
                await _repository.GetByIdAsync(cmd.UserId, cancellationToken), ct)
            .Bind(user => user.Update(command))
            .TapAsync(async (user, cancellationToken) => 
                await _repository.SaveAsync(user, cancellationToken), ct);
}
```

## Best Practices

### 1. Validate Early in the Pipeline

Use the adapter at the application service layer to validate before business logic:

```csharp
public async Task<Result<Order>> CreateOrderAsync(
    CreateOrderCommand command,
    CancellationToken ct)
{
    // Validate first with adapter, fail fast
    return await _validator.ValidateToResultAsync(command, ct)
        .BindAsync((validCmd, cancellationToken) => 
            ProcessOrderAsync(validCmd, cancellationToken), ct);
}
```

### 2. Separate Domain and Application Validation

- **Domain Validators (InlineValidator)**: Use for invariants that must always be true
- **Application Validators (AbstractValidator)**: Use for context-specific rules (uniqueness, external dependencies)

```csharp
// Domain validator - invariants (using adapter)
private static readonly InlineValidator<EmailAddress> DomainValidator = new()
{
    v => v.RuleFor(x => x.Value).NotEmpty().EmailAddress()
};

public static Result<EmailAddress> TryCreate(string value)
    => DomainValidator.ValidateToResult(new EmailAddress(value));

// Application validator - context rules (using adapter)
public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator(IUserRepository repository)
    {
        RuleFor(x => x.Email)
            .MustAsync(async (email, ct) => 
                !await repository.ExistsByEmailAsync(email, ct))
            .WithMessage("Email already registered");
    }
}
```

### 3. Always Pass CancellationToken

Support graceful cancellation in async validation:

```csharp
public async Task<Result<User>> ProcessAsync(
    CreateUserCommand command,
    CancellationToken ct)  // ✅ Accept token
    => await _validator.ValidateToResultAsync(command, ct)  // ✅ Pass to adapter
        .BindAsync((cmd, cancellationToken) => 
            User.CreateAsync(cmd, cancellationToken), ct);  // ✅ Pass through
```

### 4. Leverage FluentValidation Features

The adapter works with all FluentValidation features:

```csharp
public class CreatePaymentValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentValidator()
    {
        // Conditional validation
        When(x => x.PaymentMethod == PaymentMethod.CreditCard, () =>
        {
            RuleFor(x => x.CreditCardNumber).CreditCard();
            RuleFor(x => x.ExpiryDate).GreaterThan(DateTime.UtcNow);
        });

        // Cascade mode
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress();

        // Custom validators
        RuleFor(x => x.Password).SetValidator(new StrongPasswordValidator());
    }
}

// Adapter works seamlessly with all FluentValidation features
var result = _validator.ValidateToResult(command);
```

> **Learn More:** For comprehensive FluentValidation documentation on validators, rules, and patterns, see:
> - [Built-in Validators](https://docs.fluentvalidation.net/en/latest/built-in-validators.html)
> - [Custom Validators](https://docs.fluentvalidation.net/en/latest/custom-validators.html)
> - [Async Validation](https://docs.fluentvalidation.net/en/latest/async.html)
> - [Conditional Validation](https://docs.fluentvalidation.net/en/latest/conditions.html)

## Error Format

The adapter automatically converts FluentValidation errors to `ValidationError`:

```csharp
// FluentValidation failure
var validationResult = validator.Validate(command);
// Errors: [
//   { PropertyName: "Email", ErrorMessage: "'Email' must not be empty." },
//   { PropertyName: "Age", ErrorMessage: "'Age' must be greater than or equal to '18'." }
// ]
