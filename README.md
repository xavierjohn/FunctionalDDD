# Functional Domain Driven Design

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

## FunctionalDdd Libray

![Build](https://github.com/xavierjohn/FunctionalDDD/actions/workflows/build.yml/badge.svg)

This library facilitates railway-oriented programming, generates standard HTTP errors, and includes common error classes.
It also supports fluent validation for validating the domain model and includes a source code generator for common types.


Here is a YouTube video explaining several of this library's methods. That video was not created by me, but it does a good job of explaining the concepts behind this library.

[![Functional DDD](https://img.youtube.com/vi/45yk2nuRjj8/0.jpg)](https://youtu.be/45yk2nuRjj8?t=682)

## NuGet Packages

- **Railway Oriented Programming**

  Adds the ability to chain functions.

  [![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.RailwayOrientedProgramming.svg)](https://www.nuget.org/packages/FunctionalDDD.RailwayOrientedProgramming)

- **Fluent Validation**

  Extension method to convert fluent validation errors to ROP Result.

  [![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.FluentValidation.svg)](https://www.nuget.org/packages/FunctionalDDD.FluentValidation)
  
- **Common Value Objects**

  Helps create simple value objects like Email, Required String & Required Guid.

  [![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.CommonValueObjects.svg)](https://www.nuget.org/packages/FunctionalDDD.CommonValueObjects)

- **Common Value Objects Generator**

  Source code generator for boilerplate code needed for Required String & Required Guid.

  [![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.CommonValueObjectGenerator.svg)](https://www.nuget.org/packages/FunctionalDDD.CommonValueObjectGenerator)

- **Domain Driven Design**

  Has DDD base type like Aggregate & ValueObject.

  [![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.DomainDrivenDesign.svg)](https://www.nuget.org/packages/FunctionalDDD.DomainDrivenDesign)

  **ASP.NET**

  Convert Result object to HTTP result.

  [![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.Asp.svg)](https://www.nuget.org/packages/FunctionalDDD.Asp)

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
   .FinallyAsync(ok => "Okay", error => error.Message);
 ```

`GetCustomerByIdAsync` is a repository method that will return a `Customer?`.

If `GetCustomerByIdAsync` returns `null`, then `ToResultAsync` will convert it to a `Result` type which contains the error.

If `GetCustomerByIdAsync` returned a customer, then `EnsureAsync` is called to check if the customer can be promoted.
 If not, return a `Validation` error.

If there is no error, `TapAsync` will execute the `Promote` method and then send an email.

Finally, `FinallyAsync` will call the given functions with an underlying object or error.

### Multi-Expression Evaluation

```csharp"sal
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
var r = await _sender.Send(new StudentInformationQuery(studentId)
    .ParallelAsync(_sender.Send(new StudentGradeQuery(studentId))
    .ParallelAsync(_sender.Send(new LibraryCheckedOutBooksQuery(studentId))
    .AwaitAsync()
    .BindAsync((studentInformation, studentGrades, checkoutBooks)
       => PrepareReport(studentInformation, studentGrades, checkoutBooks));
```

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
    .ToOkActionResult(this);

  ```

#### Minimal API
  ```csharp
userApi.MapPost("/register", (RegisterUserRequest request) =>
    FirstName.TryCreate(request.firstName)
    .Combine(LastName.TryCreate(request.lastName))
    .Combine(EmailAddress.TryCreate(request.email))
    .Bind((firstName, lastName, email) => User.TryCreate(firstName, lastName, email, request.password))
    .ToOkResult());

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

Look at the [examples folder](https://github.com/xavierjohn/FunctionalDDD/tree/main/Examples) for more sample use cases.

## Related project
[CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions) Functional Extensions for C#. This library was inspired by several of the training materials created by Vladimir Khorikov.
