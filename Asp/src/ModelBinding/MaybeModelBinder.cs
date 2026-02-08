namespace FunctionalDdd.Asp.ModelBinding;

using System.Globalization;
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

        var parseResult = ConvertToPrimitive(rawValue);

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
                    : Error.Validation($"'{value}' is not a valid long.");

            if (underlyingType == typeof(decimal))
                return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
                    ? (TPrimitive)(object)d
                    : Error.Validation($"'{value}' is not a valid decimal.");

            if (underlyingType == typeof(double))
                return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dbl)
                    ? (TPrimitive)(object)dbl
                    : Error.Validation($"'{value}' is not a valid number.");

            if (underlyingType == typeof(float))
                return float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var flt)
                    ? (TPrimitive)(object)flt
                    : Error.Validation($"'{value}' is not a valid number.");

            if (underlyingType == typeof(bool))
                return bool.TryParse(value, out var b)
                    ? (TPrimitive)(object)b
                    : Error.Validation($"'{value}' is not a valid boolean.");

            if (underlyingType == typeof(DateTime))
                return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt)
                    ? (TPrimitive)(object)dt
                    : Error.Validation($"'{value}' is not a valid date/time.");

            if (underlyingType == typeof(DateTimeOffset))
                return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto)
                    ? (TPrimitive)(object)dto
                    : Error.Validation($"'{value}' is not a valid date/time.");

            if (underlyingType == typeof(DateOnly))
                return DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnly)
                    ? (TPrimitive)(object)dateOnly
                    : Error.Validation($"'{value}' is not a valid date.");

            if (underlyingType == typeof(TimeOnly))
                return TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var timeOnly)
                    ? (TPrimitive)(object)timeOnly
                    : Error.Validation($"'{value}' is not a valid time.");

            // Fallback: try Convert.ChangeType
            var converted = Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
            return (TPrimitive)converted;
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
        {
            return Error.Validation($"'{value}' is not a valid {underlyingType.Name}.");
        }
    }

    private static void AddErrorsToModelState(ModelStateDictionary modelState, string modelName, Error error)
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
