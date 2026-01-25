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
        var parseResult = ConvertToPrimitive(rawValue);

        if (parseResult.IsFailure)
        {
            bindingContext.ModelState.AddModelError(modelName, parseResult.Error.Detail);
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        var primitiveValue = parseResult.Value;

        // Call TryCreate directly - no reflection needed due to static abstract interface
        // Pass the model name so validation errors have the correct field name
        var result = TValueObject.TryCreate(primitiveValue, modelName);

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

    private static Result<TPrimitive> ConvertToPrimitive(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return Error.Validation("Value is required.");

        var targetType = typeof(TPrimitive);
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        var typeName = underlyingType.Name;

        try
        {
            if (underlyingType == typeof(string))
                return (TPrimitive)(object)value;

            if (underlyingType == typeof(Guid))
                return Guid.TryParse(value, out var guid)
                    ? (TPrimitive)(object)guid
                    : Error.Validation($"'{value}' is not a valid GUID.");

            if (underlyingType == typeof(int))
                return int.TryParse(value, out var i)
                    ? (TPrimitive)(object)i
                    : Error.Validation($"'{value}' is not a valid integer.");

            if (underlyingType == typeof(long))
                return long.TryParse(value, out var l)
                    ? (TPrimitive)(object)l
                    : Error.Validation($"'{value}' is not a valid integer.");

            if (underlyingType == typeof(decimal))
                return decimal.TryParse(value, out var d)
                    ? (TPrimitive)(object)d
                    : Error.Validation($"'{value}' is not a valid decimal.");

            if (underlyingType == typeof(double))
                return double.TryParse(value, out var dbl)
                    ? (TPrimitive)(object)dbl
                    : Error.Validation($"'{value}' is not a valid number.");

            if (underlyingType == typeof(bool))
                return bool.TryParse(value, out var b)
                    ? (TPrimitive)(object)b
                    : Error.Validation($"'{value}' is not a valid boolean. Use 'true' or 'false'.");

            if (underlyingType == typeof(DateTime))
                return DateTime.TryParse(value, out var dt)
                    ? (TPrimitive)(object)dt
                    : Error.Validation($"'{value}' is not a valid date/time.");

            if (underlyingType == typeof(DateOnly))
                return DateOnly.TryParse(value, out var dateOnly)
                    ? (TPrimitive)(object)dateOnly
                    : Error.Validation($"'{value}' is not a valid date.");

            if (underlyingType == typeof(TimeOnly))
                return TimeOnly.TryParse(value, out var t)
                    ? (TPrimitive)(object)t
                    : Error.Validation($"'{value}' is not a valid time.");

            if (underlyingType == typeof(DateTimeOffset))
                return DateTimeOffset.TryParse(value, out var dto)
                    ? (TPrimitive)(object)dto
                    : Error.Validation($"'{value}' is not a valid date/time.");

            if (underlyingType == typeof(short))
                return short.TryParse(value, out var s)
                    ? (TPrimitive)(object)s
                    : Error.Validation($"'{value}' is not a valid integer.");

            if (underlyingType == typeof(byte))
                return byte.TryParse(value, out var by)
                    ? (TPrimitive)(object)by
                    : Error.Validation($"'{value}' is not a valid byte (0-255).");

            if (underlyingType == typeof(float))
                return float.TryParse(value, out var f)
                    ? (TPrimitive)(object)f
                    : Error.Validation($"'{value}' is not a valid number.");

            // Use Convert for other types
            return (TPrimitive)Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
        }
        catch
        {
            return Error.Validation($"'{value}' is not a valid {typeName}.");
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