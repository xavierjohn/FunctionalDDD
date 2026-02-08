namespace FunctionalDdd.Asp.ModelBinding;

using FunctionalDdd;
using Microsoft.AspNetCore.Mvc.ModelBinding;

/// <summary>
/// Model binder for scalar value types.
/// Validates scalar values during model binding by calling TryCreate.
/// </summary>
/// <typeparam name="TValue">The scalar value type.</typeparam>
/// <typeparam name="TPrimitive">The underlying primitive type.</typeparam>
/// <remarks>
/// <para>
/// This binder is automatically used for any type that implements 
/// <see cref="IScalarValue{TSelf, TPrimitive}"/>. It intercepts model binding
/// and calls the static <c>TryCreate</c> method to create validated scalar values.
/// </para>
/// <para>
/// Validation errors are added to <see cref="ModelStateDictionary"/>, which integrates
/// with ASP.NET Core's standard validation infrastructure. When used with
/// <c>[ApiController]</c>, invalid requests automatically return 400 Bad Request.
/// </para>
/// </remarks>
public class ScalarValueModelBinder<TValue, TPrimitive> : IModelBinder
    where TValue : IScalarValue<TValue, TPrimitive>
    where TPrimitive : IComparable
{
    /// <summary>
    /// Attempts to bind a model from the value provider.
    /// </summary>
    /// <param name="bindingContext">The binding context.</param>
    /// <returns>A completed task.</returns>
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

        var rawValue = valueProviderResult.FirstValue;
        var parseResult = PrimitiveConverter.ConvertToPrimitive<TPrimitive>(rawValue);

        if (parseResult.IsFailure)
        {
            bindingContext.ModelState.AddModelError(modelName, parseResult.Error.Detail);
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        var primitiveValue = parseResult.Value;

        // Call TryCreate directly - no reflection needed due to static abstract interface
        // Pass the model name so validation errors have the correct field name
        var result = TValue.TryCreate(primitiveValue, modelName);

        if (result.IsSuccess)
        {
            bindingContext.Result = ModelBindingResult.Success(result.Value);
        }
        else
        {
            bindingContext.ModelState.AddResultErrors(modelName, result.Error);
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }
}