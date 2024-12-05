# ASP Extension

This library will help convert Error objects to ASP.NET Core ActionResult

## MVC

### ToActionResult

Use this method to convert `Result` to `OkObjectResult` or various failed results.

It supports pagination by passing three parameters to, from and length. Based on the values it
will return PartialContent (206) or Okay(200) per [RFC9110](https://www.rfc-editor.org/rfc/rfc9110#field.content-range)

Failed state is based on the `Error` object.
The mapping is as follows

```csharp
    NotFoundError => (ActionResult<T>)controllerBase.NotFound(error),
    ValidationError validation => ValidationErrors<T>(validation, controllerBase),
    ConflictError => (ActionResult<T>)controllerBase.Conflict(error),
    UnauthorizedError => (ActionResult<T>)controllerBase.Unauthorized(error),
    ForbiddenError => (ActionResult<T>)controllerBase.StatusCode(StatusCodes.Status403Forbidden, error),
    UnexpectedError => (ActionResult<TValue>)controllerBase.StatusCode(StatusCodes.Status500InternalServerError, error),
    _ => (ActionResult<TValue>)controllerBase.StatusCode(StatusCodes.Status500InternalServerError, error),
```

### MVC Example

Simple case.

```csharp
[HttpPost("[action]")]
public ActionResult<User> Register([FromBody] RegisterRequest request) =>
    FirstName.TryCreate(request.firstName)
    .Combine(LastName.TryCreate(request.lastName))
    .Combine(EmailAddress.TryCreate(request.email))
    .Bind((firstName, lastName, email) => SampleWebApplication.User.TryCreate(firstName, lastName, email, request.password))
    .ToActionResult(this);
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

## Minimal API

### ToHttpResult

Use this method to convert `Result` to `Http.IResult` or various failed results.

### Minimal API Example

Simple case.

```csharp
userApi.MapPost("/register", (RegisterUserRequest request) =>
    FirstName.TryCreate(request.firstName)
    .Combine(LastName.TryCreate(request.lastName))
    .Combine(EmailAddress.TryCreate(request.email))
    .Bind((firstName, lastName, email) => User.TryCreate(firstName, lastName, email, request.password))
    .ToHttpResult());
```

To control the return type

```csharp
userApi.MapPost("/registerCreated", (RegisterUserRequest request) =>
    FirstName.TryCreate(request.firstName)
    .Combine(LastName.TryCreate(request.lastName))
    .Combine(EmailAddress.TryCreate(request.email))
    .Bind((firstName, lastName, email) => User.TryCreate(firstName, lastName, email, request.password))
    .Map(user => new RegisterUserResponse(user.Id, user.FirstName, user.LastName, user.Email, user.Password))
    .Finally(
            ok => Results.CreatedAtRoute("GetUserById", new RouteValueDictionary { { "name", ok.firstName } }, ok),
            err => err.ToErrorResult()));
```
