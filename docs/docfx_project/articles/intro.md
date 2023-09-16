# Why use this library?


### Functional programming

Functional code can be written using the library's provided functions to control program execution flow based on success or error track.


### Domain Driven Design

The library's classes can be utilized to create AggregateRoot and ValueObject classes, with Fluent Validation available to validate all domain properties and ensure they remain in a valid state.


### Common error classes

The library can automatically translate common domain failures to their respective HTTP errors using its provided error classes.


### Reuse Domain object validation rules at the API layer

As far as I know, this library is unique in its ability to reuse domain validation rules at the presentation layer to return errors in HTTP standard format. If the domain returns a validation failure, the library will translate it to HTTP BadRequest with the failure details. Additionally, if the domain returns a record not found, the library will translate it to HTTP NotFound.


### Pagination over several objects

When pagination is required for a response, the library will automatically set the HTTP headers and return either an HTTP Ok (200) status or an HTTP PartialContent (206) status in accordance with [RFC 9110](https://www.rfc-editor.org/rfc/rfc9110#field.content-range).


### Avoid primitive obsession

To avoid passing around strings, it is recommended to use RequiredString to obtain strongly typed properties. The source code generator will automate the implementation process. There are also other ValueObject classes available to use.


### Parallel Execution

Need to fetch data from several sources in parallel while maintaining ROP style of programming? This library can do that.
