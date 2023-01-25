# Functional DDD

## Code and Doc in flux so not useable yet.

Functional-like programming with Domain Driven Design library is based on the 
[CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions).

I wanted the following
- A reasonable error object that can be returned from functions.
- A way to handle errors in a functional way.
- A way to return multiple errors.
- A way to convert errors to HTTP errors.
- Leverage fluent validation and use it to create domain objects.
- A place to put common domain objects.
- Railway Oriented programming with parallel tasks.
    
 Let's look at a few examples:
 ```csharp
 await GetCustomerByIdAsync(id)
       .ToResultAsync(Error.NotFound("Customer with such Id is not found: " + id))
       .EnsureAsync(customer => customer.CanBePromoted, Error.Validation("The customer has the highest status possible"))
       .TapAsync(customer => customer.Promote())
       .BindAsync(customer => EmailGateway.SendPromotionNotification(customer.Email))
       .FinallyAsync(result => result.IsSuccess ? "Okay" : result.Error.Message);
 ```

 

 `GetCustomerByIdAsync` is a repository method that will `Maybe` return a Customer. 
 The repository layer does not know the context so it cannot decide on a resonable error message.
 The domain layer has the context so it converts `null` object to an error with `ToResultAsync`. 
 The followed error types have been predefined.
 
- BadRequest (400)
- Unauthorized (401)
- Forbidden (403)
- NotFound (404)
- Validation (400)
- Conflict (409)
- Unexpected (500)
 
 The next step `EnsureAsync` fails if the predicate `customer.CanBePromoted` is false.
 
 `TapAsync` is used to call functions that does not return `Result` or the return value is not important.
 
 `BindAsync` is used to call functions that returns `Result` and the return value is important.
 
 `FinallyAsync` is used to return a value. It is used to convert `Result` to a value. It get called in success and failed cases.