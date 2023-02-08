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
    FirstName.New(request.firstName)
    .Combine(LastName.New(request.lastName))
    .Combine(EmailAddress.New(request.email))
    .OnOk((firstName, lastName, email) => SampleWebApplication.User.New(firstName, lastName, email, request.password))
    .ToOkActionResult(this);
```

To control the return type

```csharp
[HttpPost("[action]")]
public ActionResult<User> RegisterCreated2([FromBody] RegisterRequest request) =>
    FirstName.New(request.firstName)
    .Combine(LastName.New(request.lastName))
    .Combine(EmailAddress.New(request.email))
    .OnOk((firstName, lastName, email) => SampleWebApplication.User.New(firstName, lastName, email, request.password))
    .Unwrap(
        ok => CreatedAtAction("Get", new { name = ok.FirstName }, ok),
        err => err.ToErrorActionResult<User>(this));
```
