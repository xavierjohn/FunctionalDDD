# Functional DDD

Functional-like programming with Domain Driven Design library is based on the
[CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions).

With the following differences.

- Uses an Error object instead of a string to represent errors.
- Validation error can hold multiple validation errors.
- Aggregate error can hold multiple errors.
- A way to convert errors to HTTP errors.
- Leverage fluent validation and use it to create domain objects.
- Source code generation for simple domain value objects.
- Ability to run parallel tasks.
- More intuitive function names.

## Examples

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

`GetCustomerByIdAsync` is a repository method that will return a `Maybe<Customer>`.

If `Maybe<Customer>` returned `None`, then `ToResultAsync` will convert it to `Result` type with the error.

If `Maybe<Customer>` returned a customer, then `EnsureAsync` is called to check if the customer can be promoted.
 If not return a `Validation` error.

If there is no error, `IfOkAsync` will execute the `Promote` method and then send an email.

Finally `UnwrapAsync` will call the given functions with underlying object or error.

### Example 2 Validation

```csharp
 EmailAddress.New("xavier@somewhere.com")
    .Combine(FirstName.New("Xavier"))
    .Combine(LastName.New("John"))
    .Combine(EmailAddress.New("xavier@somewhereelse.com"))
    .IfOk((email, firstName, lastName, anotherEmail) => Result.Success(string.Join(" ", firstName, lastName, email, anotherEmail)));
 ```

 `Combine` is used to combine multiple `Result` objects. It will return a `Result` with all the errors if any of the `Result` objects have failed.

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

`InlineValidator` does the [FluentValidation](https://docs.fluentvalidation.net)

Look at the [examples folder](https://github.com/xavierjohn/FunctionalDDD/tree/main/Examples) for more examples.
