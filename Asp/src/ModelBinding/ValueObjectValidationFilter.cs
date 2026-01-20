namespace FunctionalDdd;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

/// <summary>
/// MVC action filter that converts validation errors collected during JSON deserialization
/// into a validation problem response.
/// </summary>
/// <remarks>
/// <para>
/// This filter runs after model binding but before the action executes.
/// If any validation errors were collected during JSON deserialization 
/// (via <see cref="ValidatingJsonConverter{T}"/>), it returns a 400 Bad Request
/// with the validation problem details.
/// </para>
/// <para>
/// This filter is automatically registered when using <see cref="ValueObjectModelBindingExtensions.AddValueObjectModelBinding"/>.
/// </para>
/// </remarks>
internal sealed class ValueObjectValidationFilter : IActionFilter
{
    /// <inheritdoc />
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Check if there are any validation errors collected during JSON deserialization
        var validationError = ValidationErrorsContext.GetValidationError();
        if (validationError is not null)
        {
            // Add each field error to ModelState
            foreach (var fieldError in validationError.FieldErrors)
                foreach (var errorMessage in fieldError.Details)
                    context.ModelState.TryAddModelError(fieldError.FieldName, errorMessage);

            context.Result = new BadRequestObjectResult(
                new ValidationProblemDetails(context.ModelState));
        }
    }

    /// <inheritdoc />
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }
}
