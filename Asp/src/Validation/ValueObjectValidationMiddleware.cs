namespace FunctionalDdd;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Middleware that creates a validation error collection scope for each request.
/// This enables <see cref="ValidatingJsonConverter{T}"/> to collect validation errors
/// across the entire request deserialization process.
/// </summary>
/// <remarks>
/// <para>
/// This middleware should be registered early in the pipeline, before any middleware
/// that might deserialize JSON request bodies.
/// </para>
/// <para>
/// For each request:
/// <list type="bullet">
/// <item>Creates a new validation error collection scope</item>
/// <item>Allows the request to proceed through the pipeline</item>
/// <item>Cleans up the scope when the request completes</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Registering the middleware in Program.cs:
/// <code>
/// app.UseValueObjectValidation();
/// // ... other middleware
/// app.MapControllers();
/// </code>
/// </example>
public sealed class ValueObjectValidationMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Creates a new instance of <see cref="ValueObjectValidationMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public ValueObjectValidationMiddleware(RequestDelegate next) =>
        _next = next;

    /// <summary>
    /// Invokes the middleware, wrapping the request in a validation scope.
    /// </summary>
    /// <param name="context">The HTTP context for the request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        using (ValidationErrorsContext.BeginScope())
        {
            await _next(context).ConfigureAwait(false);
        }
    }
}
