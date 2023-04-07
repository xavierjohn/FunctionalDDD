# Functional DDD

Functional-like programming with Domain Driven Design library is based on the
[CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions).

With the following differences.

- Uses an Error object instead of a string to represent errors.
- Validation errors can hold multiple validation errors.
- Aggregate errors can hold multiple errors.
- A way to convert FunctionalDDD.Error to HTTP errors (ActionResult).
- Leverage fluent validation and use it to create domain objects.
- Source code generation for simple domain value objects.
- Ability to run parallel tasks.

Here is a YouTube video explaining several of this library's methods.

[![Functional DDD](https://img.youtube.com/vi/45yk2nuRjj8/0.jpg)](https://youtu.be/45yk2nuRjj8?t=682)

## NuGet Packages

- **Railway Oriented Programming**

  <div>Adds the ability to chain functions.</div>
  
  [![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.RailwayOrientedProgramming.svg)](https://www.nuget.org/packages/FunctionalDDD.RailwayOrientedProgramming)

- **Fluent Validation**

  <div>Extension method to convert fluent validation errors to ROP Result</div>

  [![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.FluentValidation.svg)](https://www.nuget.org/packages/FunctionalDDD.FluentValidation)
  
- **Common Value Objects**

  <div>Helps create simple value objects like Email, Required String & Required Guid</div>

  [![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.CommonValueObjects.svg)](https://www.nuget.org/packages/FunctionalDDD.CommonValueObjects)

- **Common Value Objects Generator**

  <div>Source code generator for boilerplate code needed for Required String & Required Guid</div>

  [![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.CommonValueObjectGenerator.svg)](https://www.nuget.org/packages/FunctionalDDD.CommonValueObjectGenerator)

- **Domain Driven Design**

  <div>Has DDD base type like AggregateRoot & ValueObject</div>

  [![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.DomainDrivenDesign.svg)](https://www.nuget.org/packages/FunctionalDDD.DomainDrivenDesign)

## Examples

Let's look at a few examples:

### Example 1 Async

 ```csharp
await GetCustomerByIdAsync(id)
   .ToResultAsync(Error.NotFound("Customer with such Id is not found: " + id))
   .EnsureAsync(customer => customer.CanBePromoted,
      Error.Validation("The customer has the highest status possible"))
   .TeeAsync(customer => customer.Promote())
   .BindAsync(customer => EmailGateway.SendPromotionNotification(customer.Email))
   .FinallyAsync(ok => "Okay", error => error.Message);
 ```

`GetCustomerByIdAsync` is a repository method that will return a `Customer?`.

If `GetCustomerByIdAsync` returned `null`, then `ToResultAsync` will convert it to `Result` type with the error.

If `GetCustomerByIdAsync` returned a customer, then `EnsureAsync` is called to check if the customer can be promoted.
 If not, return a `Validation` error.

If there is no error, `TeeAsync` will execute the `Promote` method and then send an email.

Finally, `FinallyAsync` will call the given functions with an underlying object or error.

### Example 2 Validation

```csharp
 EmailAddress.New("xavier@somewhere.com")
    .Combine(FirstName.New("Xavier"))
    .Combine(LastName.New("John"))
    .Bind((email, firstName, lastName) =>
       Result.Success(string.Join(" ", firstName, lastName, email)));
 ```

 `Combine` is used to combine multiple `Result` objects. If any of the' Result' objects have failed, it will return a `Result` with all the errors. Avoiding primitive obsession prevents writing parameters out of order.

### Example 3 Fluent Validation

```csharp
 public class User : AggregateRoot<UserId>
{
    public FirstName FirstName { get; }
    public LastName LastName { get; }
    public EmailAddress Email { get; }

    public static Result<User> New(FirstName firstName, LastName lastName, EmailAddress email)
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

### Example 4 Parallel Tasks

```csharp
var r = await _sender.Send(new StudentInformationQuery(studentId)
    .ParallelAsync(_sender.Send(new StudentGradeQuery(studentId))
    .ParallelAsync(_sender.Send(new LibraryCheckedOutBooksQuery(studentId))
    .BindAsync((studentInformation, studentGrades, checkoutBooks)
       => PrepareReport(studentInformation, studentGrades, checkoutBooks));
```

### Example 5 Read HTTP response as Result

```csharp
var result = await _httpClient.GetAsync($"person/{id}")
    .ReadResultWithNotFoundAsync<Person>(Error.NotFound("Person not found"));
```

Or handle errors yourself by using a callback.
  
  ```csharp
  async Task<Error> _callback(HttpResponseMessage response, int personId)
{
    var content = await response.Content.ReadAsStringAsync();
    // Log/Handle error
    return Error.NotFound("Bad request");
}

var result = await task.ReadResultAsync<Person, int>(_callback, 5);

  ```

Look at the [examples folder](https://github.com/xavierjohn/FunctionalDDD/tree/main/Examples) for more examples.
