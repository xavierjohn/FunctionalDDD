# ASP Extension

This library will help convert Error objects to ASP.NET Core ActionResult.

- ToOkActionResult
- ToErrorActionResult

## ToOkActionResult

Use this method to convert `Result` to `OkObjectResult` or various failed results.

## ToErrorActionResult

Use this method to convert `Error` to various failed results.
The mapping is as follows

```csharp
    NotFoundError => (ActionResult<T>)controllerBase.NotFound(error),
    ValidationError validation => ValidationErrors<T>(validation, controllerBase),
    ConflictError => (ActionResult<T>)controllerBase.Conflict(error),
    UnauthorizedError => (ActionResult<T>)controllerBase.Unauthorized(error),
    ForbiddenError => (ActionResult<T>)controllerBase.Forbid(error.Message),
    UnexpectedError => (ActionResult<T>)controllerBase.StatusCode(500, error),
    _ => throw new NotImplementedException($"Unknown error {error.Code}"),
```

## Example

Simple case.

```csharp
[HttpPost("[action]")]
public ActionResult<User> Register([FromBody] RegisterRequest request) =>
    FirstName.TryCreate(request.firstName)
    .Combine(LastName.TryCreate(request.lastName))
    .Combine(EmailAddress.TryCreate(request.email))
    .Bind((firstName, lastName, email) => SampleWebApplication.User.TryCreate(firstName, lastName, email, request.password))
    .ToOkActionResult(this);
```

To control the return type

```csharp
[HttpPost("[action]")]
public ActionResult<User> RegisterCreated2([FromBody] RegisterRequest request) =>
    FirstName.TryCreate(request.firstName)
    .Combine(LastName.TryCreate(request.lastName))
    .Combine(EmailAddress.TryCreate(request.email))
    .Bind((firstName, lastName, email) => SampleWebApplication.User.TryCreate(firstName, lastName, email, request.password))
    .Finally(
        ok => CreatedAtAction("Get", new { name = ok.FirstName }, ok),
        err => err.ToErrorActionResult<User>(this));
```

## ToPartialOrOkActionResult
ToPartialOrOkActionResult can be used to support pagination.
The function takes in three parameters to, from and length and based on the values
will return PartialContent (206) or Okay(200) per [RFC9110](https://www.rfc-editor.org/rfc/rfc9110#field.content-range)