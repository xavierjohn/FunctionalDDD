namespace FunctionalDdd.Asp.ModelBinding;

using FunctionalDdd;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

/// <summary>
/// Model binder for ScalarValueObject-derived types.
/// Validates value objects during model binding by calling TryCreate.
/// </summary>
/// <typeparam name="TValueObject">The value object type.</typeparam>
/// <typeparam name="TPrimitive">The underlying primitive type.</typeparam>
/// <remarks>
/// <para>
/// This binder is automatically used for any type that implements 
/// <see cref="IScalarValueObject{TSelf, TPrimitive}"/>. It intercepts model binding
/// and calls the static <c>TryCreate</c> method to create validated value objects.
/// </para>
/// <para>
/// Validation errors are added to <see cref="ModelStateDictionary"/>, which integrates
/// with ASP.NET Core's standard validation infrastructure. When used with
/// <c>[ApiController]</c>, invalid requests automatically return 400 Bad Request.
/// </para>
/// </remarks>
public class ScalarValueObjectModelBinder<TValueObject, TPrimitive> : IModelBinder
    where TValueObject : IScalarValueObject<TValueObject, TPrimitive>
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
        var primitiveValue = ConvertToPrimitive(rawValue);

        if (primitiveValue is null)
        {
            bindingContext.ModelState.AddModelError(
                modelName,
                $"The value '{rawValue}' is not valid for {typeof(TPrimitive).Name}.");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        // Call TryCreate directly - no reflection needed due to static abstract interface
        var result = TValueObject.TryCreate(primitiveValue);

        if (result.IsSuccess)
        {
            bindingContext.Result = ModelBindingResult.Success(result.Value);
        }
        else
        {
            AddErrorsToModelState(bindingContext.ModelState, modelName, result.Error);
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }

    private static TPrimitive? ConvertToPrimitive(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return default;

        var targetType = typeof(TPrimitive);
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            if (underlyingType == typeof(string))
                return (TPrimitive)(object)value;

            if (underlyingType == typeof(Guid))
                return Guid.TryParse(value, out var guid) ? (TPrimitive)(object)guid : default;

            if (underlyingType == typeof(int))
                return int.TryParse(value, out var i) ? (TPrimitive)(object)i : default;

            if (underlyingType == typeof(long))
                return long.TryParse(value, out var l) ? (TPrimitive)(object)l : default;

            if (underlyingType == typeof(decimal))
                return decimal.TryParse(value, out var d) ? (TPrimitive)(object)d : default;

            if (underlyingType == typeof(double))
                return double.TryParse(value, out var dbl) ? (TPrimitive)(object)dbl : default;

            if (underlyingType == typeof(bool))
                return bool.TryParse(value, out var b) ? (TPrimitive)(object)b : default;

            if (underlyingType == typeof(DateTime))
                return DateTime.TryParse(value, out var dt) ? (TPrimitive)(object)dt : default;

            if (underlyingType == typeof(DateOnly))
                return DateOnly.TryParse(value, out var d) ? (TPrimitive)(object)d : default;

            if (underlyingType == typeof(TimeOnly))
                return TimeOnly.TryParse(value, out var t) ? (TPrimitive)(object)t : default;

            if (underlyingType == typeof(DateTimeOffset))
                return DateTimeOffset.TryParse(value, out var dto) ? (TPrimitive)(object)dto : default;

            if (underlyingType == typeof(short))
                return short.TryParse(value, out var s) ? (TPrimitive)(object)s : default;

            if (underlyingType == typeof(byte))
                return byte.TryParse(value, out var by) ? (TPrimitive)(object)by : default;

            if (underlyingType == typeof(float))
                return float.TryParse(value, out var f) ? (TPrimitive)(object)f : default;

            // Use Convert for other types
            return (TPrimitive)Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
        }
        catch
        {
            return default;
        }
    }

    private static void AddErrorsToModelState(
        ModelStateDictionary modelState,
        string modelName,
        Error error)
    {
        if (error is ValidationError validationError)
        {
            foreach (var fieldError in validationError.FieldErrors)
            {
                foreach (var detail in fieldError.Details)
                    modelState.AddModelError(modelName, detail);
            }
        }
        else
        {
            modelState.AddModelError(modelName, error.Detail);
        }
    }
}
