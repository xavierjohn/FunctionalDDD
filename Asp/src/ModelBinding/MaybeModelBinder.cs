namespace FunctionalDdd.Asp.ModelBinding;

using FunctionalDdd;
using Microsoft.AspNetCore.Mvc.ModelBinding;

/// <summary>
/// Model binder for <see cref="Maybe{TValue}"/> parameters where <typeparamref name="TValue"/>
/// implements <see cref="IScalarValue{TSelf, TPrimitive}"/>.
/// Binds optional route, query, form, and header parameters to <see cref="Maybe{TValue}"/>.
/// </summary>
/// <typeparam name="TValue">The scalar value type.</typeparam>
/// <typeparam name="TPrimitive">The underlying primitive type.</typeparam>
/// <remarks>
/// <para>
/// Unlike <see cref="ScalarValueModelBinder{TValue, TPrimitive}"/> which leaves absent parameters
/// unbound (relying on ASP.NET Core's default handling), this binder explicitly produces:
/// </para>
/// <list type="bullet">
/// <item>Parameter absent → <c>Maybe.None&lt;TValue&gt;()</c> — success, value not provided</item>
/// <item>Parameter present, valid → <c>Maybe.From(validatedValue)</c> — success with value</item>
/// <item>Parameter present, invalid → validation error in ModelState</item>
/// </list>
/// </remarks>
public class MaybeModelBinder<TValue, TPrimitive> : IModelBinder
    where TValue : IScalarValue<TValue, TPrimitive>
    where TPrimitive : IComparable
{
    /// <summary>
    /// Attempts to bind a <see cref="Maybe{TValue}"/> from the value provider.
    /// </summary>
    /// <param name="bindingContext">The binding context.</param>
    /// <returns>A completed task.</returns>
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        // Parameter not provided → Maybe.None (not an error)
        if (valueProviderResult == ValueProviderResult.None)
        {
            bindingContext.Result = ModelBindingResult.Success(Maybe.None<TValue>());
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

        var rawValue = valueProviderResult.FirstValue;

        // Empty string for optional parameter → Maybe.None
        if (string.IsNullOrEmpty(rawValue))
        {
            bindingContext.Result = ModelBindingResult.Success(Maybe.None<TValue>());
            return Task.CompletedTask;
        }

        var parseResult = PrimitiveConverter.ConvertToPrimitive<TPrimitive>(rawValue);

        if (parseResult.IsFailure)
        {
            bindingContext.ModelState.AddModelError(modelName, parseResult.Error.Detail);
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        var primitiveValue = parseResult.Value;

        // Call TryCreate directly — no reflection needed due to static abstract interface
        var result = TValue.TryCreate(primitiveValue, modelName);

        if (result.IsSuccess)
        {
            bindingContext.Result = ModelBindingResult.Success(Maybe.From(result.Value));
        }
        else
        {
            bindingContext.ModelState.AddResultErrors(modelName, result.Error);
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }
}
