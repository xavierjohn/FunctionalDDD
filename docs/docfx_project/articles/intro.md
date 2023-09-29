# Why use this library?


### Functional programming

Railway Oriented Programming is a coding concept that involves using a library's built-in functions to control program execution flow based on success or error track. By doing so, functional code can be written that allows for chaining of functions without the need for error checking.

### Domain Driven Design

Using the library's classes, it is possible to create Aggregate, Entity, and ValueObject classes. These classes can be validated using Fluent Validation to ensure that all domain properties are in a valid state. For simple ValueObjects with a single value, ScalarValueObject can be used. If the only requirement for a ValueObject is that the value is not null or empty, RequiredString can be utilized.

### Error classes

The library includes a set of common error classes that can be returned by your domain. Additionally, the library supports automatic mapping of these errors to corresponding HTTP errors.

- ValidationError
- NotFoundError
- ForbiddenError
- UnauthorizedError
- ConflictError
- BadRequestError
- UnauthorizedError
- AggregateError

### Reuse Domain object validation rules at the API layer

As far as I know, this library is unique in its ability to reuse domain validation rules at the presentation layer to return errors in HTTP standard format. If the domain returns a validation failure, the library will translate it to HTTP BadRequest with the failure details. Additionally, if the domain returns a record not found, the library will translate it to HTTP NotFound.

### Pagination over several objects

When pagination is required for a response, the library will automatically set the HTTP headers and return either an HTTP Ok (200) status or an HTTP PartialContent (206) status in accordance with [RFC 9110](https://www.rfc-editor.org/rfc/rfc9110#field.content-range).

### Avoid primitive obsession

To avoid passing around strings, it is recommended to use RequiredString to obtain strongly typed properties. The source code generator will automate the implementation process. There are also other ValueObject classes available to use.

### Parallel Execution

Need to fetch data from several sources in parallel while maintaining ROP style of programming? This library can do that.
