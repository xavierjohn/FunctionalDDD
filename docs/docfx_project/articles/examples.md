# Examples

This page provides quick code snippets to get you started. For comprehensive real-world examples, see the [Examples Directory](https://github.com/xavierjohn/FunctionalDDD/tree/main/Examples).

## Real-World Examples

The repository includes production-ready examples demonstrating complete systems:

### 🛒 [E-Commerce Order Processing](https://github.com/xavierjohn/FunctionalDDD/tree/main/Examples/EcommerceExample)
Complete order processing with payment, inventory management, and email notifications. Demonstrates complex workflows, compensation patterns, and transaction-like behavior.

**Key Concepts**: Aggregate lifecycle, compensation, parallel validation, async workflows

### 🏦 [Banking Transactions](https://github.com/xavierjohn/FunctionalDDD/tree/main/Examples/BankingExample)
Banking system with fraud detection, daily limits, overdraft protection, and interest calculations. Shows security patterns and state machines.

**Key Concepts**: Fraud detection, parallel fraud checks, MFA, account freeze, audit trail

### 👤 [User Management](https://github.com/xavierjohn/FunctionalDDD/tree/main/Examples/SampleUserLibrary)
User registration with FluentValidation integration and value objects.

**Key Concepts**: Aggregates, FluentValidation, value objects, type safety

### 🌐 [Web API Integration](https://github.com/xavierjohn/FunctionalDDD/tree/main/Examples/SampleWebApplication)
ASP.NET Core MVC and Minimal API examples with automatic error-to-HTTP status mapping.

**Key Concepts**: ToActionResult, ToHttpResult, API integration, HTTP status codes

See the [Examples README](https://github.com/xavierjohn/FunctionalDDD/tree/main/Examples/README.md) for a complete guide including complexity ratings, learning paths, and common patterns.

---

## Quick Code Snippets

## Compose multiple operations in a single chain

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

`MatchAsync` will terminate the chain and return a `string` if there is no error, otherwise it will return the error message.

## Multi-Expression Evaluation

```csharp
 EmailAddress.TryCreate("xavier@somewhere.com")
    .Combine(FirstName.TryCreate("Xavier"))
    .Combine(LastName.TryCreate("John"))
    .Bind((email, firstName, lastName) =>
       Result.Success(string.Join(" ", firstName, lastName, email)));
 ```

 `Combine` is used to combine multiple `Result` objects. If any of the `Result` objects have failed, it will return a `Result` containing each of the errors which arose during evaluation. Avoiding primitive obsession prevents using parameters out of order.

## Validation

This library supports validation using [FluentValidation](https://docs.fluentvalidation.net).
The API layer can reuse the Domain validation logic to return `BadRequest` with the validation errors.

```csharp
 public class User : Aggregate<UserId>
{
    public FirstName FirstName { get; }
    public LastName LastName { get; }

    public static Result<User> TryCreate(FirstName firstName, LastName lastName)
    {
        var user = new User(firstName, lastName);
        return Validator.ValidateToResult(user);
    }


    private User(FirstName firstName, LastName lastName)
    : base(UserId.NewUnique())
    {
        FirstName = firstName;
        LastName = lastName;
    }

    // Fluent Validation
    private static readonly InlineValidator<User> Validator = new()
    {
        v => v.RuleFor(x => x.FirstName).NotNull(),
        v => v.RuleFor(x => x.LastName).NotNull(),
    };
}
 ```

`InlineValidator` does the [FluentValidation](https://docs.fluentvalidation.net)

Calling the API with missing LastName will return a `BadRequest` with the error message.

```
HTTP/1.1 400 Bad Request
Connection: close
Content-Type: application/problem+json; charset=utf-8
Date: Thu, 21 Sep 2023 16:40:27 GMT
Server: Kestrel
Transfer-Encoding: chunked

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "00-c86cd9b34ca9435b688ec3a6b905b8e4-5f4c286ce90f99cb-00",
  "errors": {
    "lastName": [
      "Last Name cannot be empty."
    ]
  }
}
```

### Running Parallel Tasks

```csharp
var r = await _sender.Send(new StudentInformationQuery(studentId)
    .ParallelAsync(_sender.Send(new StudentGradeQuery(studentId))
    .ParallelAsync(_sender.Send(new LibraryCheckedOutBooksQuery(studentId))
    .BindAsync((studentInformation, studentGrades, checkoutBooks)
       => PrepareReport(studentInformation, studentGrades, checkoutBooks));
```

## Read HTTP response as Result

Handles HTTP NotFound and throws for all other failures.

```csharp
var result = await _httpClient.GetAsync($"person/{id}")
    .ReadResultWithNotFoundAsync<Person>(Error.NotFound("Person not found"));
```

Or handle the errors yourself by using a callback.
  
  ```csharp
async Task<Error> FailureCallback(HttpResponseMessage response, int personId)
{
    var content = await response.Content.ReadAsStringAsync();
    // Log/Handle error
    _logger.LogError("Person API Failed: code :{code}, message:{message}", response.StatusCode, content);
    return Error.NotFound("Person not found");
}

var result = await _httpClient.GetAsync($"person/{id}")
    .ReadResultAsync<Person, int>(FailureCallback, 5);

  ```

Look at the [examples folder](https://github.com/xavierjohn/FunctionalDDD/tree/main/Examples) for more sample use cases.