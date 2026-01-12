namespace FunctionalDdd.Asp.ModelBinding;

using Microsoft.AspNetCore.Mvc.ModelBinding;

/// <summary>
/// Model binder for value objects implementing <see cref="ITryCreatable{T}"/>.
/// Converts validation failures to ModelState errors for proper API responses.
/// </summary>
/// <typeparam name="T">The value object type implementing ITryCreatable.</typeparam>
/// <remarks>
/// <para>
/// This model binder is automatically used for any parameter or DTO property that implements ITryCreatable.
/// Validation errors are added to ModelState and returned as 400 Bad Request with Problem Details format.
/// </para>
/// <para>
/// The binder:
/// <list type="bullet">
/// <item>Retrieves the string value from the value provider (route, query, form, body)</item>
/// <item>Calls the value object's static TryCreate method</item>
/// <item>On success: Sets the binding result to the value object</item>
/// <item>On failure: Adds validation errors to ModelState for Problem Details response</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Usage in controller (automatic):
/// <code>
/// public record CreateUserRequest(
///     FirstName FirstName,    // Automatically bound and validated
///     LastName LastName,      // Automatically bound and validated
///     EmailAddress Email      // Automatically bound and validated
/// );
/// 
/// [HttpPost]
/// public ActionResult&lt;User&gt; Create([FromBody] CreateUserRequest request) =>
///     User.TryCreate(request.FirstName, request.LastName, request.Email)
///         .ToActionResult(this);
/// 
/// // Invalid request automatically returns 400 Bad Request:
/// // {
/// //   "errors": {
/// //     "FirstName": ["First Name cannot be empty."],
/// //     "Email": ["Email address is not valid."]
/// //   }
/// // }
/// </code>
/// </example>
public class ValueObjectModelBinder<T> : IModelBinder where T : ITryCreatable<T>
{
    /// <summary>
    /// Binds the model by calling TryCreate and handling the result.
    /// </summary>
    /// <param name="bindingContext">The model binding context.</param>
    /// <returns>A completed task.</returns>
    /// <remarks>
    /// <para>
    /// The binding process:
    /// <list type="number">
    /// <item>Get the value from the value provider (route, query, form, body)</item>
    /// <item>Call T.TryCreate(value) using the static abstract method</item>
    /// <item>If successful: Set ModelBindingResult.Success with the value object</item>
    /// <item>If failed: Add errors to ModelState (field-level for ValidationError, generic for others)</item>
    /// </list>
    /// </para>
    /// <para>
    /// For <see cref="ValidationError"/> with multiple field errors, each field is added to ModelState separately.
    /// This enables Problem Details responses with field-specific error messages.
    /// </para>
    /// </remarks>
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
        var value = valueProviderResult.FirstValue;

        var result = T.TryCreate(value);

        if (result.IsSuccess)
        {
            bindingContext.Result = ModelBindingResult.Success(result.Value);
        }
        else
        {
            AddErrorsToModelState(bindingContext, modelName, result.Error);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds validation errors to ModelState for Problem Details formatting.
    /// </summary>
    /// <param name="bindingContext">The model binding context.</param>
    /// <param name="modelName">The name of the model/property being bound.</param>
    /// <param name="error">The domain error to convert to ModelState errors.</param>
    /// <remarks>
    /// <para>
    /// For <see cref="ValidationError"/>: Each field error is added with its specific field name.
    /// This enables responses like:
    /// <code>
    /// {
    ///   "errors": {
    ///     "firstName": ["First Name cannot be empty."],
    ///     "email": ["Email address is not valid.", "Email is required."]
    ///   }
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// For other error types: The error detail is added to ModelState with the model name as the key.
    /// </para>
    /// </remarks>
    private static void AddErrorsToModelState(
        ModelBindingContext bindingContext,
        string modelName,
        Error error)
    {
        if (error is ValidationError validationError)
        {
            // Add field-level errors
            foreach (var fieldError in validationError.FieldErrors)
            {
                var fieldName = string.IsNullOrEmpty(fieldError.FieldName)
                    ? modelName
                    : fieldError.FieldName;

                foreach (var detail in fieldError.Details)
                {
                    bindingContext.ModelState.AddModelError(fieldName, detail);
                }
            }
        }
        else
        {
            // Add generic error
            bindingContext.ModelState.AddModelError(modelName, error.Detail);
        }
    }
}
