# Functional DDD

## Code and Doc in flux so not useable yet.

Functional-like programming with Domain Driven Design library is based on the 
[CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions).

With the following differences.

- Uses an Error object instead of a string to represent errors.
- Validation error can hold multiple validation errors.
- Aggregate error can hold multiple errors.
- A way to convert errors to HTTP errors.
- Leverage fluent validation and use it to create domain objects.
- A place to put common domain objects.
- Ability to run parallel tasks.
    
 Let's look at a few examples:
 
 ### Example 1 Async
 ```csharp
 await GetCustomerByIdAsync(id)
       .ToResultAsync(Error.NotFound("Customer with such Id is not found: " + id))
       .EnsureAsync(customer => customer.CanBePromoted, Error.Validation("The customer has the highest status possible"))
       .IfOkAsync(customer => customer.Promote())
       .IfOkAsync(customer => EmailGateway.SendPromotionNotification(customer.Email))
       .UnwrapAsync(ok => "Okay", error => error.Message);
 ```

 

 `GetCustomerByIdAsync` is a repository method that will `Maybe` return a Customer. 
 The repository layer does not know the context so it cannot decide on a resonable error message.
 The domain layer has the context so it converts `null` object to an error with `ToResultAsync`. 
 The followed error types have been predefined.
 
- Validation (400)
- Unauthorized (401)
- Forbidden (403)
- NotFound (404)
- Conflict (409)
- Aggregate (500)
- Unexpected (500)
 
 The next step `EnsureAsync` fails if the predicate `customer.CanBePromoted` is false.
 
 `TapAsync` is used to call functions that does not return `Result` or the return value is not important.
 
 `IfOkAsync` is used to call functions that returns `Result` and the return value is important.
 
 `UnwrapAsync` is used to return a value. It is used to convert `Result` to a value. It get called in success and failed cases.

 ### Example 2 Validation
 ```csharp
 EmailAddress.New("xavier@somewhere.com")
            .Combine(FirstName.New("Xavier"))
            .Combine(LastName.New("John"))
            .Combine(EmailAddress.New("xavier@somewhereelse.com"))
            .IfOk((email, firstName, lastName, anotherEmail) => Result.Success(string.Join(" ", firstName, lastName, email, anotherEmail)));
 ```

 `Combine` is used to combine multiple `Result` objects. It will return a `Result` with all the errors if any of the `Result` objects are `Failure`.
 
 ### Example 3 Fluent Validation
 ```csharp
 public class User : AggregateRoot<UserId>
{
    public FirstName FirstName { get; }
    public LastName LastName { get; }
    public EmailAddress Email { get; }
    public string Password { get; }

    public static Result<User> New(FirstName firstName, LastName lastName, EmailAddress email, string password)
    {
        var user = new User(firstName, lastName, email, password);
        return s_validator.ValidateToResult(user);
    }


    private User(FirstName firstName, LastName lastName, EmailAddress email, string password) : base(UserId.CreateUnique())
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = password;
    }

    // Fluent Validation
    static readonly InlineValidator<User> s_validator = new()
    {
        v => v.RuleFor(x => x.FirstName).NotNull(),
        v => v.RuleFor(x => x.LastName).NotNull(),
        v => v.RuleFor(x => x.Email).NotNull(),
        v => v.RuleFor(x => x.Password).NotEmpty()
    };
}
 ```

 Look at the [examples folder](https://github.com/xavierjohn/FunctionalDDD/tree/main/Examples) for more examples.