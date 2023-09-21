# Examples

Here are a few examples:

## Compose multiple operations in a single chain

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

If `GetCustomerByIdAsync` returns `null`, then `ToResultAsync` will convert it to a `Result` type which contains the error.

If `GetCustomerByIdAsync` returned a customer, then `EnsureAsync` is called to check if the customer can be promoted.
If not, return a `Validation` error.

If there is no error, `TeeAsync` will execute the `Promote` method and then send an email.

`FinallyAsync` will terminate the chain and return a `string` if there is no error, otherwise it will return the error message.

## Multi-Expression Evaluation

```csharp
 EmailAddress.New("xavier@somewhere.com")
    .Combine(FirstName.New("Xavier"))
    .Combine(LastName.New("John"))
    .Bind((email, firstName, lastName) =>
       Result.Success(string.Join(" ", firstName, lastName, email)));
 ```

 `Combine` is used to combine multiple `Result` objects. If any of the `Result` objects have failed, it will return a `Result` containing each of the errors which arose during evaluation. Avoiding primitive obsession prevents using parameters out of order.

## Fluent Validation

The API layer can reuse the Domain validation logic to return `BadRequest` with the validation errors.

```csharp
 public class User : Aggregate<UserId>
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

Look at the [examples folder](https://github.com/xavierjohn/FunctionalDDD/tree/main/Examples) for more sample use cases.