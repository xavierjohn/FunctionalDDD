# Functional Domain Driven Design

[![Build](https://github.com/xavierjohn/FunctionalDDD/actions/workflows/build.yml/badge.svg)](https://github.com/xavierjohn/FunctionalDDD/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/FunctionalDDD.RailwayOrientedProgramming.svg)](https://www.nuget.org/packages/FunctionalDDD.RailwayOrientedProgramming)
[![NuGet Downloads](https://img.shields.io/nuget/dt/FunctionalDDD.RailwayOrientedProgramming.svg)](https://www.nuget.org/packages/FunctionalDDD.RailwayOrientedProgramming)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download)
[![C#](https://img.shields.io/badge/C%23-14.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![GitHub Stars](https://img.shields.io/github/stars/xavierjohn/FunctionalDDD?style=social)](https://github.com/xavierjohn/FunctionalDDD/stargazers)

## Table of Contents

- [Overview](#overview)
- [FunctionalDdd Library](#functionalddd-library)
- [What's New](#whats-new)
- [NuGet Packages](#nuget-packages)
- [Quick Start](#quick-start)
- [Performance](#performance)
- [Examples](#examples)
  - [Compose Operations](#compose-multiple-operations-in-a-single-chain)
  - [CancellationToken Support](#with-cancellationtoken-support)
  - [Multi-Expression Evaluation](#multi-expression-evaluation)
  - [Fluent Validation](#fluent-validation)
  - [Parallel Tasks](#running-parallel-tasks)
  - [HTTP Integration](#read-http-response-as-result)
  - [ASP.NET Core](#convert-result-to-http-response)
  - [Tracing](#tracing)
  - [Error Matching](#discriminated-error-matching)
  - [Pattern Matching](#pattern-matching-with-tuples)
- [Contributing](#contributing)
- [License](#license)
- [Related Projects](#related-projects)

## Overview

Functional programming, railway-oriented programming, and domain-driven design are three concepts that can work together to create robust and reliable software.

Functional programming is a programming paradigm that emphasizes the use of pure functions,
which are functions that take in inputs and produce outputs without any side effects.
This approach can lead to code that is easier to understand, test, and maintain.
To get to know more about the principles behind it, check out the [Applying Functional Principles in C# Pluralsight course](https://enterprisecraftsmanship.com/ps-func).

Railway-oriented programming is an approach to error handling that is based on the idea of a railway track.
In this approach, the code is divided into a series of functions that represent different steps along the railway track.
Each function either succeeds and moves the code along the track, or fails and sends the code down a different track.
This approach can make error handling more explicit and easier to reason about.

Domain-driven design is an approach to software development that focuses on understanding the problem domain and creating a model that accurately represents it.
This model is then used to guide the design and implementation of the software.
By focusing on the problem domain, developers can create software that is more closely aligned with the needs of the users and the business.
To learn more about DDD, check out the course [Domain-Driven Design in Practice](https://app.pluralsight.com/library/courses/domain-driven-design-in-practice/table-of-contents).

When combined, functional programming, railway-oriented programming, and domain-driven design can lead to software that is both robust and reliable.
By using pure functions, developers can create code that is easier to reason about and test.
By using railway-oriented programming, developers can make error handling more explicit and easier to reason about.
By focusing on the problem domain, developers can create software that is more closely aligned with the needs of the users and the business.

Overall, functional programming with railway-oriented programming and domain-driven design can be a powerful approach to software development that can lead to more robust and reliable software.

## FunctionalDdd Library

This library facilitates railway-oriented programming, generates standard HTTP errors, and includes common error classes.
It also supports fluent validation for validating the domain model and includes a source code generator for common types.

### What's New

**Recent enhancements:**
- ? **Discriminated Error Matching**: Pattern match on specific error types (ValidationError, NotFoundError, etc.) using `MatchError`
- ?? **Comprehensive CancellationToken Support**: All async operations now support cancellation tokens for graceful shutdown and timeouts
- ?? **Tuple Destructuring**: Automatically destructure tuples in Match/Switch operations for cleaner code
- ?? **Enhanced Documentation**: Comprehensive READMEs for all packages with detailed examples

For detailed documentation, see the [Railway Oriented Programming README](RailwayOrientedProgramming/README.md).

Here is a YouTube video explaining several of this library's methods. That video was not created by me, but it does a good job of explaining the concepts behind this library.

[![Functional DDD](https://img.youtube.com/vi/45yk2nuRjj8/0.jpg)](https://youtu.be/45yk2nuRjj8?t=682)

## NuGet Packages

- **Railway Oriented Programming**

  Comprehensive railway-oriented programming with Result/Maybe types, error handling, and async support.

  ?? [View Documentation](RailwayOrientedProgramming/README.md)

  [![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.RailwayOrientedProgramming.svg)](https://www.nuget.org/packages/FunctionalDDD.RailwayOrientedProgramming)

- **Fluent Validation**

  Seamlessly integrate FluentValidation with Railway Oriented Programming.

  ?? [View Documentation](FluentValidation/README.md)

  [![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.FluentValidation.svg)](https://www.nuget.org/packages/FunctionalDDD.FluentValidation)
  
- **Common Value Objects**

  Create strongly-typed value objects like EmailAddress, RequiredString & RequiredGuid.

  ?? [View Documentation](CommonValueObjects/README.md)

  [![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.CommonValueObjects.svg)](https://www.nuget.org/packages/FunctionalDDD.CommonValueObjects)

- **Common Value Objects Generator**

  Source code generator for boilerplate code needed for RequiredString & RequiredGuid.

  [![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.CommonValueObjectGenerator.svg)](https://www.nuget.org/packages/FunctionalDDD.CommonValueObjectGenerator)

- **Domain Driven Design**

  DDD building blocks: Aggregate, Entity, ValueObject, ScalarValueObject, and Domain Events.

  ?? [View Documentation](DomainDrivenDesign/README.md)

  [![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.DomainDrivenDesign.svg)](https://www.nuget.org/packages/FunctionalDDD.DomainDrivenDesign)

- **ASP.NET**

  Convert Result objects to HTTP responses for MVC and Minimal APIs.

  ?? [View Documentation](Asp/README.md)

  [![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.Asp.svg)](https://www.nuget.org/packages/FunctionalDDD.Asp)

## Quick Start

### Installation

Install the core railway-oriented programming package:

```bash
dotnet add package FunctionalDDD.RailwayOrientedProgramming
```

For ASP.NET Core integration:

```bash
dotnet add package FunctionalDDD.Asp
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
    onFailure: error => $"Error: {error.Message}"
);

// Chain multiple operations
var result = await GetUserAsync(userId)
    .ToResultAsync(Error.NotFound("User not found"))
    .BindAsync(user => SaveUserAsync(user))
    .TapAsync(user => SendEmailAsync(user.Email));
```

?? **Next Steps**: See the [Examples](#examples) section below or explore the [Railway Oriented Programming documentation](RailwayOrientedProgramming/README.md) for comprehensive guidance.

## Performance

### ? **Negligible Overhead, Maximum Clarity**

FunctionalDDD is designed with performance in mind. Comprehensive benchmarks on **.NET 10** show that railway-oriented programming adds only **~11-16 nanoseconds** of overhead compared to imperative code—less than **0.002%** of typical I/O operations.

**Test Environment**: Intel Core i7-1185G7 @ 3.00GHz, Windows 11, .NET 10.0.1

#### Key Performance Metrics

| Operation | ROP Time | Imperative Time | Overhead | Memory |
|-----------|----------|-----------------|----------|--------|
| **Happy Path** | 147 ns | 131 ns | **16 ns** (12%) | 144 B (identical) |
| **Error Path** | 99 ns | 88 ns | **11 ns** (13%) | 184 B (identical) |
| **Combine (2 results)** | 7 ns | - | - | 0 B |
| **Combine (5 results)** | 58 ns | - | - | 0 B |
| **Bind (single)** | 9 ns | - | - | 0 B |
| **Bind (5 chains)** | 63 ns | - | - | 0 B |
| **Map (single)** | 4.6 ns | - | - | 0 B |
| **Map (5 transforms)** | 44.5 ns | - | - | 0 B |
| **Tap (single)** | 3 ns | - | - | 0 B |
| **Tap (5 actions)** | 37.4 ns | - | - | 64 B |
| **Ensure (single)** | 22.5 ns | - | - | 152 B |
| **Ensure (5 checks)** | 175 ns | - | - | 760 B |

#### Real-World Context

```
Database Query:   1,000,000 ns (1 ms)
HTTP Request:    10,000,000 ns (10 ms)
ROP Overhead:            16 ns (0.000016 ms)
                         ?
                    0.0016% overhead
```

**The overhead is 1/62,500th of a single database query!**

#### Benefits Without Sacrifice

? **Same Memory Usage** - No additional allocations vs imperative code  
? **Blazing Fast** - Single-digit to low double-digit nanosecond overhead  
? **Better Code** - Cleaner, more testable, and maintainable  
? **Explicit Errors** - Clear error propagation and aggregation  

?? **[View Detailed Benchmarks ?](BENCHMARKS.md)**

Run benchmarks yourself:
```bash
dotnet run --project Benchmark/Benchmark.csproj -c Release
```

## Examples

Let's look at a few examples:

### Compose multiple operations in a single chain

 ```csharp
await GetCustomerByIdAsync(id)
   .ToResultAsync(Error.NotFound("Customer with such Id is not found: " + id))
   .EnsureAsync(customer => customer.CanBePromoted,
      Error.Validation("The customer has the highest status possible"))
   .TapAsync(customer => customer.Promote())
   .BindAsync(customer => EmailGateway.SendPromotionNotification(customer.Email))
   .MatchAsync(ok => "Okay", error => error.Message);
 ```

`GetCustomerByIdAsync` is a repository method that will return a `Customer?`.

If `GetCustomerByIdAsync` returns `null`, then `ToResultAsync` will convert it to a `Result` type which contains the error.

If `GetCustomerByIdAsync` returned a customer, then `EnsureAsync` is called to check if the customer can be promoted.
 If not, return a `Validation` error.

If there is no error, `TapAsync` will execute the `Promote` method and then send an email.

Finally, `MatchAsync` will call the given functions based on success or failure.

### With CancellationToken Support

```csharp
await GetCustomerByIdAsync(id, cancellationToken)
   .ToResultAsync(Error.NotFound("Customer with such Id is not found: " + id))
   .EnsureAsync(
      (customer, ct) => customer.CanBePromotedAsync(ct),
      Error.Validation("The customer has the highest status possible"),
      cancellationToken)
   .TapAsync(
      async (customer, ct) => await customer.PromoteAsync(ct),
      cancellationToken)
   .BindAsync(
      (customer, ct) => EmailGateway.SendPromotionNotificationAsync(customer.Email, ct),
      cancellationToken)
   .MatchAsync(ok => "Okay", error => error.Message);
```

This allows graceful cancellation of long-running operations and supports request timeouts in web applications.

### Multi-Expression Evaluation

```csharp
 EmailAddress.TryCreate("xavier@somewhere.com")
    .Combine(FirstName.TryCreate("Xavier"))
    .Combine(LastName.TryCreate("John"))
    .Bind((email, firstName, lastName) =>
       Result.Success(string.Join(" ", firstName, lastName, email)));
 ```

 `Combine` is used to combine multiple `Result` objects. If any of the `Result` objects have failed, it will return a `Result` containing each of the errors which arose during evaluation. Avoiding primitive obsession prevents writing parameters out of order.

### Fluent Validation

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


    private User(FirstName firstName, LastName lastName, EmailAddress email)
    : base(UserId.NewUnique())
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    // Fluent Validation
    private static readonly InlineValidator<User> Validator = new()
    {
        v => v.RuleFor(x => x.FirstName).NotNull(),
        v => v.RuleFor(x => x.LastName).NotNull(),
        v => v.RuleFor(x => x.Email).NotNull(),
    };
}
 ```

`InlineValidator` does the [FluentValidation](https://docs.fluentvalidation.net)

### Running Parallel Tasks

```csharp
var r = await _sender.Send(new StudentInformationQuery(studentId))
    .ParallelAsync(_sender.Send(new StudentGradeQuery(studentId))
    .ParallelAsync(_sender.Send(new LibraryCheckedOutBooksQuery(studentId))
    .AwaitAsync()
    .BindAsync((studentInformation, studentGrades, checkoutBooks)
       => PrepareReport(studentInformation, studentGrades, checkoutBooks));
```

### With CancellationToken Support

```csharp
var r = await _sender.Send(new StudentInformationQuery(studentId), cancellationToken)
    .ParallelAsync(_sender.Send(new StudentGradeQuery(studentId), cancellationToken))
    .ParallelAsync(_sender.Send(new LibraryCheckedOutBooksQuery(studentId), cancellationToken))
    .AwaitAsync()
    .BindAsync(
        (studentInformation, studentGrades, checkoutBooks, ct) =>
            PrepareReportAsync(studentInformation, studentGrades, checkoutBooks, ct),
        cancellationToken);
```

This allows cancellation to propagate through all parallel operations and the final report generation.

### Read HTTP response as Result

```csharp
var result = await _httpClient.GetAsync($"person/{id}")
    .ReadResultWithNotFoundAsync<Person>(Error.NotFound("Person not found"));
```

Or handle errors yourself by using a callback.
  
  ```csharp
async Task<Error> FailureHandling(HttpResponseMessage response, int personId)
{
    var content = await response.Content.ReadAsStringAsync();
    // Log/Handle error
    _logger.LogError("Person API Failed: code :{code}, message:{message}", response.StatusCode, content);
    return Error.NotFound("Person not found");
}

var result = await _httpClient.GetAsync($"person/{id}")
    .ReadResultAsync<Person, int>(FailureHandling, 5);

  ```

### Convert Result to HTTP response

#### MVC

  ```csharp
[HttpPost("[action]")]
public ActionResult<User> Register([FromBody] RegisterUserRequest request) =>
    FirstName.TryCreate(request.firstName)
    .Combine(LastName.TryCreate(request.lastName))
    .Combine(EmailAddress.TryCreate(request.email))
    .Bind((firstName, lastName, email) => SampleUserLibrary.User.TryCreate(firstName, lastName, email, request.password))
    .ToActionResult(this);

  ```

#### Minimal API

  ```csharp
userApi.MapPost("/register", (RegisterUserRequest request) =>
    FirstName.TryCreate(request.firstName)
    .Combine(LastName.TryCreate(request.lastName))
    .Combine(EmailAddress.TryCreate(request.email))
    .Bind((firstName, lastName, email) => User.TryCreate(firstName, lastName, email, request.password))
    .ToHttpResult());

  ```

Sample Error:

  ```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "lastName": [
            "Last Name cannot be empty."
        ],
        "email": [
            "Email address is not valid."
        ]
    }
}
  ```

### Tracing

Tracing can be enabled by adding `AddFunctionalDddRopInstrumentation()` for ROP code or `AddFunctionalDddCvoInstrumentation()` for Common Value Objects.

```csharp
var builder = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("FunctionDddExample"))
    .AddFunctionalDddCvoInstrumentation()
    .AddOtlpExporter();
```

### Discriminated Error Matching

Match on specific error types for precise error handling:

```csharp
var result = ProcessOrder(order)
    .MatchError(
        onValidationError: validationErr => 
            Results.BadRequest(new { errors = validationErr.Details }),
        onNotFoundError: notFoundErr => 
            Results.NotFound(new { message = notFoundErr.Message }),
        onConflictError: conflictErr => 
            Results.Conflict(new { message = conflictErr.Message }),
        onUnauthorizedError: _ => 
            Results.Unauthorized(),
        onSuccess: order => 
            Results.Ok(order)
    );
```

### Pattern Matching with Tuples

Automatically destructure tuples in Match operations:

```csharp
var result = EmailAddress.TryCreate(email)
    .Combine(UserId.TryCreate(userId))
    .Combine(OrderId.TryCreate(orderId))
    .Match(
        // Tuple automatically destructured into parameters
        (emailAddr, user, order) => $"Order {order} for {emailAddr}",
        error => $"Error: {error.Message}"
    );
```

Look at the [examples folder](https://github.com/xavierjohn/FunctionalDDD/tree/main/Examples) for more sample use cases.

## Contributing

Contributions are welcome! This project follows standard GitHub workflow:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

Please ensure:
- All tests pass (`dotnet test`)
- Code follows existing style conventions
- New features include appropriate tests and documentation
- Commit messages are clear and descriptive

For major changes, please open an issue first to discuss what you would like to change.

## License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

## Related Projects

[CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions) Functional Extensions for C#. This library was inspired by several of the training materials created by Vladimir Khorikov.
