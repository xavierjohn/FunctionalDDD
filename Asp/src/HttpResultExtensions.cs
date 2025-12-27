namespace FunctionalDdd;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Provides extension methods to convert Result types to ASP.NET Core Minimal API <see cref="IResult"/> responses.
/// These methods bridge Railway Oriented Programming with ASP.NET Core Minimal APIs.
/// </summary>
/// <remarks>
/// <para>
/// These extensions enable clean, functional-style Minimal API endpoints by automatically mapping
/// domain Result types to appropriate HTTP responses. They provide the same functionality as
/// <see cref="ActionResultExtensions"/> but for the Minimal API programming model (ASP.NET Core 6+).
/// </para>
/// <para>
/// Key features:
/// <list type="bullet">
/// <item>Automatic HTTP status code selection based on error type</item>
/// <item>Problem Details (RFC 7807) formatting for errors</item>
/// <item>Validation error formatting with field-level details</item>
/// <item>Unit result to 204 No Content conversion</item>
/// <item>Clean, declarative endpoint definitions</item>
/// </list>
/// </para>
/// <para>
/// Usage pattern in Minimal APIs:
/// <code>
/// app.MapGet("/users/{id}", (string id, IUserService userService) =>
///     UserId.TryCreate(id)
///         .Bind(userService.GetUser)
///         .Map(user => new UserDto(user))
///         .ToHttpResult()
/// );
/// </code>
/// </para>
/// </remarks>
public static class HttpResultExtensions
{
    /// <summary>
    /// Converts a <see cref="Result{TValue}"/> to an <see cref="IResult"/> with appropriate HTTP status code.
    /// </summary>
    /// <typeparam name="TValue">The type of the value contained in the result.</typeparam>
    /// <param name="result">The result object to convert.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item>200 OK with value if result is successful (except for Unit)</item>
    /// <item>204 No Content if result is successful and TValue is Unit</item>
    /// <item>Appropriate error status code (400-599) based on error type if result is failure</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is the primary method for converting domain results to HTTP responses in Minimal APIs.
    /// It automatically selects the appropriate status code based on the error type.
    /// </para>
    /// <para>
    /// Special handling for <see cref="Unit"/>: Since Unit represents "no value",
    /// successful Unit results return 204 No Content instead of 200 OK.
    /// This is appropriate for operations like DELETE or state-changing operations
    /// that don't return data.
    /// </para>
    /// </remarks>
    /// <example>
    /// Simple GET endpoint:
    /// <code>
    /// app.MapGet("/users/{id}", (Guid id, IUserRepository repository) =>
    ///     UserId.TryCreate(id)
    ///         .Bind(repository.GetAsync)
    ///         .Map(user => new UserDto(user))
    ///         .ToHttpResult());
    /// 
    /// // Success: 200 OK with UserDto
    /// // Not found: 404 Not Found with Problem Details
    /// // Validation error: 400 Bad Request with validation details
    /// </code>
    /// </example>
    /// <example>
    /// POST endpoint with validation:
    /// <code>
    /// app.MapPost("/users", (CreateUserRequest request, IUserService userService) =>
    ///     EmailAddress.TryCreate(request.Email)
    ///         .Combine(FirstName.TryCreate(request.FirstName))
    ///         .Bind((email, name) => userService.CreateUser(email, name))
    ///         .Map(user => new UserDto(user))
    ///         .ToHttpResult());
    /// 
    /// // Success: 200 OK with UserDto
    /// // Validation error: 400 Bad Request with field-level errors
    /// // Conflict: 409 Conflict if user already exists
    /// </code>
    /// </example>
    /// <example>
    /// DELETE endpoint returning Unit:
    /// <code>
    /// app.MapDelete("/users/{id}", (Guid id, IUserRepository repository) =>
    ///     UserId.TryCreate(id)
    ///         .Bind(repository.DeleteAsync)
    ///         .ToHttpResult());
    /// 
    /// // Success: 204 No Content (automatic for Unit)
    /// // Not found: 404 Not Found
    /// </code>
    /// </example>
    /// <example>
    /// Complex endpoint with multiple operations:
    /// <code>
    /// app.MapPost("/orders", async (
    ///     CreateOrderRequest request,
    ///     IOrderService orderService,
    ///     IEventBus eventBus) =>
    ///     await CustomerId.TryCreate(request.CustomerId)
    ///         .BindAsync(orderService.GetCustomerAsync)
    ///         .BindAsync(customer => orderService.CreateOrderAsync(customer, request.Items))
    ///         .TapAsync(async order => await eventBus.PublishAsync(new OrderCreatedEvent(order.Id)))
    ///         .MapAsync(order => new OrderDto(order))
    ///         .ToHttpResultAsync());
    /// </code>
    /// </example>
    public static Microsoft.AspNetCore.Http.IResult ToHttpResult<TValue>(this Result<TValue> result)
    {
        if (result.IsSuccess)
        {
            // If TValue is Unit, return 204 No Content
            if (typeof(TValue) == typeof(Unit))
                return Results.NoContent();
            
            return Results.Ok(result.Value);
        }
        
        return result.Error.ToHttpResult();
    }

    /// <summary>
    /// Converts a domain <see cref="Error"/> to an <see cref="IResult"/> with appropriate HTTP status code and Problem Details format.
    /// </summary>
    /// <param name="error">The domain error to convert.</param>
    /// <returns>
    /// An IResult with Problem Details (RFC 7807) response containing:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Domain Error Type</term>
    ///         <description>HTTP Status Code</description>
    ///     </listheader>
    ///     <item>
    ///         <term><see cref="ValidationError"/></term>
    ///         <description>400 Bad Request with validation problem details and field errors</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="BadRequestError"/></term>
    ///         <description>400 Bad Request</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="UnauthorizedError"/></term>
    ///         <description>401 Unauthorized</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="ForbiddenError"/></term>
    ///         <description>403 Forbidden</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="NotFoundError"/></term>
    ///         <description>404 Not Found</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="ConflictError"/></term>
    ///         <description>409 Conflict</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="DomainError"/></term>
    ///         <description>422 Unprocessable Entity</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="RateLimitError"/></term>
    ///         <description>429 Too Many Requests</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="UnexpectedError"/></term>
    ///         <description>500 Internal Server Error</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="ServiceUnavailableError"/></term>
    ///         <description>503 Service Unavailable</description>
    ///     </item>
    ///     <item>
    ///         <term>Unknown types</term>
    ///         <description>500 Internal Server Error</description>
    ///     </item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// All responses use Problem Details format (RFC 7807) which provides a standard way to
    /// communicate errors in HTTP APIs. The format includes:
    /// <list type="bullet">
    /// <item><c>type</c>: A URI reference identifying the problem type</item>
    /// <item><c>title</c>: A short human-readable summary</item>
    /// <item><c>status</c>: The HTTP status code</item>
    /// <item><c>detail</c>: A human-readable explanation (from error.Detail)</item>
    /// <item><c>instance</c>: A URI reference identifying the specific occurrence (from error.Instance)</item>
    /// </list>
    /// </para>
    /// <para>
    /// For <see cref="ValidationError"/>, the response includes an additional <c>errors</c> object
    /// containing field-level validation messages grouped by field name.
    /// </para>
    /// </remarks>
    /// <example>
    /// Example Problem Details response for a validation error:
    /// <code>
    /// {
    ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "detail": "User data validation failed",
    ///   "instance": "/api/users",
    ///   "errors": {
    ///     "email": ["Email address is invalid", "Email is required"],
    ///     "age": ["Age must be 18 or older"]
    ///   }
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// Using custom error types in domain logic:
    /// <code>
    /// public class UserService
    /// {
    ///     public Result&lt;User&gt; GetUser(UserId id)
    ///     {
    ///         var user = _repository.FindById(id);
    ///         if (user == null)
    ///             return Error.NotFound($"User {id} not found", $"/users/{id}");
    ///             
    ///         return user;
    ///     }
    ///     
    ///     public Result&lt;User&gt; CreateUser(EmailAddress email, FirstName name)
    ///     {
    ///         if (_repository.ExistsByEmail(email))
    ///             return Error.Conflict("User with this email already exists");
    ///             
    ///         var user = User.Create(email, name);
    ///         _repository.Add(user);
    ///         return user;
    ///     }
    /// }
    /// 
    /// // Minimal API endpoint
    /// app.MapGet("/users/{id}", (string id, UserService service) =>
    ///     UserId.TryCreate(id)
    ///         .Bind(service.GetUser)
    ///         .ToHttpResult());
    ///         
    /// // Error automatically mapped to 404 Not Found with Problem Details
    /// </code>
    /// </example>
    public static Microsoft.AspNetCore.Http.IResult ToHttpResult(this Error error)
    {
        if (error is ValidationError validationError)
        {
            Dictionary<string, string[]> errors = validationError.FieldErrors
                .GroupBy(x => x.FieldName)
                .ToDictionary(x => x.Key, x => x.SelectMany(y => y.Details).ToArray());

            return Results.ValidationProblem(errors, validationError.Detail, validationError.Instance);
        }

        var status = error switch
        {
            NotFoundError => StatusCodes.Status404NotFound,
            BadRequestError => StatusCodes.Status400BadRequest,
            ConflictError => StatusCodes.Status409Conflict,
            UnauthorizedError => StatusCodes.Status401Unauthorized,
            ForbiddenError => StatusCodes.Status403Forbidden,
            DomainError => StatusCodes.Status422UnprocessableEntity,
            RateLimitError => StatusCodes.Status429TooManyRequests,
            UnexpectedError => StatusCodes.Status500InternalServerError,
            ServiceUnavailableError => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };
        return Results.Problem(error.Detail, error.Instance, status);
    }
}
