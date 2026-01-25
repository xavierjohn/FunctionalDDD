namespace FunctionalDdd;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

/// <summary>
/// An action filter that checks for validation errors collected during JSON deserialization
/// and returns a 400 Bad Request response with validation problem details.
/// </summary>
/// <remarks>
/// <para>
/// This filter works in conjunction with ValidatingJsonConverterFactory to provide
/// automatic validation of value objects in request DTOs. The converter collects validation errors
/// during deserialization, and this filter checks for errors before the action executes.
/// </para>
/// <para>
/// If validation errors were collected during deserialization:
/// <list type="bullet">
/// <item>The action is short-circuited (not executed)</item>
/// <item>A 400 Bad Request response with validation problem details is returned</item>
/// <item>The response format matches ASP.NET Core's standard validation error format</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// The filter is typically registered globally in Program.cs:
/// <code>
/// builder.Services.AddControllers(options =>
/// {
///     options.Filters.Add&lt;ValueObjectValidationFilter&gt;();
/// });
/// </code>
/// </example>
public sealed class ValueObjectValidationFilter : IActionFilter, IOrderedFilter
{
    /// <summary>
    /// Gets the order value for filter execution. This filter runs early to catch validation errors
    /// before other filters or the action execute.
    /// </summary>
    public int Order => -2000; // Run early, before most other filters

    /// <inheritdoc />
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var validationError = ValidationErrorsContext.GetValidationError();
        if (validationError is null)
            return;

        // Clear existing ModelState errors for fields we have validation errors for
        // This overrides the "field is required" errors from ASP.NET Core's null validation
        foreach (var fieldError in validationError.FieldErrors)
        {
            // Try common field name patterns (direct name or dto.fieldName)
            context.ModelState.Remove(fieldError.FieldName);
            foreach (var key in context.ModelState.Keys.ToList())
            {
                if (key.EndsWith("." + fieldError.FieldName, StringComparison.OrdinalIgnoreCase) ||
                    key.Equals(fieldError.FieldName, StringComparison.OrdinalIgnoreCase))
                {
                    context.ModelState.Remove(key);
                }
            }
        }

        // Add our validation errors to ModelState
        foreach (var (fieldName, details) in validationError.ToDictionary())
        {
            foreach (var detail in details)
                context.ModelState.AddModelError(fieldName, detail);
        }

        context.Result = new BadRequestObjectResult(
            new ValidationProblemDetails(context.ModelState)
            {
                Title = "One or more validation errors occurred.",
                Status = 400
            });
    }

    /// <inheritdoc />
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }
}