namespace FunctionalDdd;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

/// <summary>
/// An endpoint filter that checks for validation errors collected during JSON deserialization
/// and returns a 400 Bad Request response with validation problem details.
/// </summary>
/// <remarks>
/// <para>
/// This filter works with <see cref="ValidationErrorsContext"/> to provide
/// automatic validation of value objects in request DTOs for Minimal APIs.
/// </para>
/// <para>
/// Use <see cref="EndpointFilterExtensions.WithValueObjectValidation"/> to apply this filter.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// app.MapPost("/register", (CreateUserRequest request) =>
///     User.TryCreate(request.FirstName, request.LastName, request.Email)
///         .ToHttpResult())
///     .WithValueObjectValidation();
/// </code>
/// </example>
public sealed class ValueObjectValidationEndpointFilter : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Check if there are validation errors from deserialization
        var validationError = ValidationErrorsContext.GetValidationError();
        if (validationError is not null)
        {
            // Return validation problem details
            var errors = new Dictionary<string, string[]>();
            foreach (var fieldError in validationError.FieldErrors)
            {
                errors[fieldError.FieldName] = [.. fieldError.Details];
            }

            return Results.ValidationProblem(errors, title: "One or more validation errors occurred.");
        }

        return await next(context).ConfigureAwait(false);
    }
}

/// <summary>
/// Extension methods for applying value object validation to Minimal API endpoints.
/// </summary>
public static class EndpointFilterExtensions
{
    /// <summary>
    /// Adds value object validation to the endpoint.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This filter checks for validation errors collected by <see cref="ValidatingJsonConverter{T}"/>
    /// during JSON deserialization. If errors exist, it returns a 400 Bad Request with validation
    /// problem details.
    /// </para>
    /// <para>
    /// Note: This filter requires <see cref="ValueObjectValidationMiddleware"/> to be registered
    /// in the application pipeline using <see cref="ValueObjectValidationExtensions.UseValueObjectValidation"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Program.cs
    /// app.UseValueObjectValidation();
    /// 
    /// // Route registration
    /// app.MapPost("/users/register", (CreateUserRequest request) =>
    ///     User.TryCreate(request.FirstName, request.LastName, request.Email)
    ///         .ToHttpResult())
    ///     .WithValueObjectValidation();
    /// </code>
    /// </example>
    public static RouteHandlerBuilder WithValueObjectValidation(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter<ValueObjectValidationEndpointFilter>();
}
