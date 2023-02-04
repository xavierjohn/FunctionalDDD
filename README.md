# Functional DDD

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

 ## Documentation
 
 ### [Result](RailwayOrientedProgramming/src/Result/Result{TOk,TErr}.cs)
 Result object holds the result of an operation. It can be either `Ok` or `Failure`.
 It is defined as
 
 ```csharp
 public readonly struct Result<TOk, TErr>
{
    public TOk Ok => IsError ? throw new ResultFailureException<TErr>(Error) : _ok!;
    public TErr Error => _error ?? throw new ResultSuccessException();

    public bool IsError { get; }
    public bool IsOk => !IsError;
    
    public static implicit operator Result<TOk, TErr>(TOk value) => Result.Success<TOk, TErr>(value);

    public static implicit operator Result<TOk, TErr>(TErr errors) => Result.Failure<TOk, TErr>(errors);
 }
 ```

 ### IfOk
 IfOk calls the given function if the result is in `Ok` state.

 ### IfError
 IfError calls the given function if the result is in `Error` state.

 ### Ensure
 Ensure calls the given function if the result is in `Ok` state.
 If the function return false, the attached error is returned.

 ### Map
 Map calls the given function if the result is in `Ok` state.
 The return value is wrapped in `Result` as Ok.
 
 ### Combine
 Combine combines multiple `Result` objects. It will return a `Result` with all the errors if any of the `Result` objects are `Failure`.
 If all the errors are of type `ValidationError`, then it will return a `ValidationError` with all the errors. 
 Otherwise it will return an `AggregatedError`
 
 ### ParallelAsync
 Parallel runs multiple tasks in parallel and returns multiple tasks. `IfOkAsync` will await all the task and call the given function.

 ### Unwarp
 Unwrap unwraps the `Result` and returns the `Ok` value or the error.

 ### Maybe
 Maybe states if it contains a value or not. 
 It has the following methods:
 
 - HasValue - returns true if it has a value.
 - HasNoValue - returns true if it does not have a value.
 - Value - returns the value if it has a value. Otherwise `InvalidOperationException`