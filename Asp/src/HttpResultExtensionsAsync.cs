namespace FunctionalDdd;

/// <summary>
/// Provides asynchronous extension methods to convert Task/ValueTask-wrapped Result types to ASP.NET Core Minimal API <see cref="Microsoft.AspNetCore.Http.IResult"/> responses.
/// These methods enable clean async patterns in Minimal API endpoints while maintaining Railway Oriented Programming benefits.
/// </summary>
/// <remarks>
/// <para>
/// These extensions are async variants of <see cref="HttpResultExtensions"/>, designed for use with
/// async service methods in Minimal APIs. They support both <see cref="Task{TResult}"/> and <see cref="ValueTask{TResult}"/>
/// for maximum flexibility and performance.
/// </para>
/// <para>
/// Key benefits:
/// <list type="bullet">
/// <item>Clean async Minimal API endpoint code without manual awaiting</item>
/// <item>Automatic HTTP status code selection based on error type</item>
/// <item>Support for both Task and ValueTask for performance optimization</item>
/// <item>Consistent error handling across async operations</item>
/// <item>Seamless integration with async Railway Oriented Programming chains</item>
/// <item>Reduced boilerplate in endpoint definitions</item>
/// </list>
/// </para>
/// <para>
/// Usage pattern in async Minimal APIs:
/// <code>
/// app.MapGet("/users/{id}", async (string id, IUserService userService) =>
///     await UserId.TryCreate(id)
///         .BindAsync(userService.GetUserAsync)
///         .MapAsync(user => new UserDto(user))
///         .ToHttpResultAsync()
/// );
/// </code>
/// </para>
/// </remarks>
public static class HttpResultExtensionsAsync
{

    /// <summary>
    /// Converts a Task-wrapped <see cref="Result{TValue}"/> to an <see cref="Microsoft.AspNetCore.Http.IResult"/> with appropriate HTTP status code.
    /// </summary>
    /// <typeparam name="TValue">The type of the value contained in the result.</typeparam>
    /// <param name="resultTask">The task containing the result object to convert.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing:
    /// <list type="bullet">
    /// <item>200 OK with value if result is successful (except for Unit)</item>
    /// <item>204 No Content if result is successful and TValue is Unit</item>
    /// <item>Appropriate error status code (400-599) based on error type if result is failure</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is the primary async method for converting domain results to HTTP responses in Minimal APIs.
    /// It awaits the result task and delegates to <see cref="HttpResultExtensions.ToHttpResult{TValue}(Result{TValue})"/>.
    /// </para>
    /// <para>
    /// For performance-critical scenarios where the operation frequently completes synchronously,
    /// consider using the ValueTask overload instead.
    /// </para>
    /// </remarks>
    /// <example>
    /// Async GET endpoint with database query:
    /// <code>
    /// app.MapGet("/users/{id}", async (Guid id, IUserRepository repository) =>
    ///     await UserId.TryCreate(id)
    ///         .BindAsync(repository.GetByIdAsync)
    ///         .MapAsync(user => new UserDto(user))
    ///         .ToHttpResultAsync());
    /// 
    /// // Success: 200 OK with UserDto
    /// // Not found: 404 Not Found with Problem Details
    /// // Validation error: 400 Bad Request
    /// </code>
    /// </example>
    /// <example>
    /// Async POST endpoint with multiple operations:
    /// <code>
    /// app.MapPost("/orders", async (
    ///     CreateOrderRequest request,
    ///     IOrderService orderService,
    ///     IEventBus eventBus) =>
    ///     await CustomerId.TryCreate(request.CustomerId)
    ///         .BindAsync(orderService.GetCustomerAsync)
    ///         .BindAsync(customer => orderService.CreateOrderAsync(customer, request.Items))
    ///         .TapAsync(async order => 
    ///             await eventBus.PublishAsync(new OrderCreatedEvent(order.Id)))
    ///         .MapAsync(order => new OrderDto(order))
    ///         .ToHttpResultAsync());
    /// 
    /// // Success: 200 OK with OrderDto
    /// // Validation error: 400 Bad Request
    /// // Customer not found: 404 Not Found
    /// // Domain error: 422 Unprocessable Entity
    /// </code>
    /// </example>
    /// <example>
    /// Async DELETE endpoint returning Unit:
    /// <code>
    /// app.MapDelete("/users/{id}", async (Guid id, IUserRepository repository) =>
    ///     await UserId.TryCreate(id)
    ///         .BindAsync(repository.DeleteAsync)
    ///         .ToHttpResultAsync());
    /// 
    /// // Success: 204 No Content (automatic for Unit)
    /// // Not found: 404 Not Found
    /// </code>
    /// </example>
    /// <example>
    /// Complex async workflow with validation and side effects:
    /// <code>
    /// app.MapPost("/payments", async (
    ///     ProcessPaymentRequest request,
    ///     IPaymentService paymentService,
    ///     INotificationService notificationService) =>
    ///     await Amount.TryCreate(request.Amount)
    ///         .Combine(CardNumber.TryCreate(request.CardNumber))
    ///         .BindAsync((amount, card) => 
    ///             paymentService.ProcessPaymentAsync(amount, card))
    ///         .TapAsync(async payment => 
    ///             await notificationService.SendReceiptAsync(payment))
    ///         .MapAsync(payment => new PaymentDto(payment))
    ///         .ToHttpResultAsync());
    /// 
    /// // Returns appropriate status codes for validation errors,
    /// // payment failures, or successful processing
    /// </code>
    /// </example>
    public static async Task<Microsoft.AspNetCore.Http.IResult> ToHttpResultAsync<TValue>(this Task<Result<TValue>> resultTask)
    {
        var result = await resultTask;
        return result.ToHttpResult();
    }

    /// <summary>
    /// Converts a ValueTask-wrapped <see cref="Result{TValue}"/> to an <see cref="Microsoft.AspNetCore.Http.IResult"/> with appropriate HTTP status code.
    /// </summary>
    /// <typeparam name="TValue">The type of the value contained in the result.</typeparam>
    /// <param name="resultTask">The ValueTask containing the result object to convert.</param>
    /// <returns>
    /// A ValueTask that represents the asynchronous operation, containing:
    /// <list type="bullet">
    /// <item>200 OK with value if result is successful (except for Unit)</item>
    /// <item>204 No Content if result is successful and TValue is Unit</item>
    /// <item>Appropriate error status code based on error type if result is failure</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This overload is optimized for scenarios where the async operation frequently completes synchronously
    /// (e.g., cached results, in-memory operations, ValueTask-returning service methods).
    /// ValueTask can reduce allocations in these cases.
    /// </para>
    /// <para>
    /// Use this when your service methods return ValueTask for performance optimization.
    /// </para>
    /// </remarks>
    /// <example>
    /// Using with cached data that might complete synchronously:
    /// <code>
    /// app.MapGet("/users/{id}", async (Guid id, IUserCache cache) =>
    ///     await UserId.TryCreate(id)
    ///         .BindAsync(cache.GetUserAsync) // Returns ValueTask
    ///         .MapAsync(user => new UserDto(user))
    ///         .ToHttpResultAsync());
    /// 
    /// // Optimized for frequent cache hits that complete synchronously
    /// </code>
    /// </example>
    /// <example>
    /// High-performance endpoint with ValueTask throughout:
    /// <code>
    /// app.MapGet("/metrics/{id}", async (string id, IMetricsService service) =>
    ///     await MetricId.TryCreate(id)
    ///         .BindAsync(service.GetMetricAsync) // ValueTask
    ///         .MapAsync(metric => new MetricDto(metric))
    ///         .ToHttpResultAsync());
    /// 
    /// // Reduced allocations for high-throughput scenarios
    /// </code>
    /// </example>
    public static async ValueTask<Microsoft.AspNetCore.Http.IResult> ToHttpResultAsync<TValue>(this ValueTask<Result<TValue>> resultTask)
    {
        var result = await resultTask;
        return result.ToHttpResult();
    }
}
