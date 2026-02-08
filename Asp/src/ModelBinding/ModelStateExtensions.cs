namespace FunctionalDdd.Asp.ModelBinding;

using Microsoft.AspNetCore.Mvc.ModelBinding;

/// <summary>
/// Extension methods for adding <see cref="Error"/> details to <see cref="ModelStateDictionary"/>.
/// Shared by <see cref="ScalarValueModelBinder{TValue, TPrimitive}"/>
/// and <see cref="MaybeModelBinder{TValue, TPrimitive}"/>.
/// </summary>
internal static class ModelStateExtensions
{
    /// <summary>
    /// Adds errors from a <see cref="Error"/> to the model state dictionary.
    /// For <see cref="ValidationError"/>, each field error is added under its field name.
    /// For other error types, the error detail is added under the model name.
    /// </summary>
    /// <param name="modelState">The model state dictionary.</param>
    /// <param name="modelName">The model name to use for non-validation errors.</param>
    /// <param name="error">The error to add.</param>
    public static void AddResultErrors(this ModelStateDictionary modelState, string modelName, Error error)
    {
        if (error is ValidationError validationError)
        {
            foreach (var (fieldName, details) in validationError.ToDictionary())
                foreach (var detail in details)
                    modelState.AddModelError(fieldName, detail);
        }
        else
        {
            modelState.AddModelError(modelName, error.Detail);
        }
    }
}
