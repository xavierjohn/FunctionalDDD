namespace FunctionalDdd;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

/// <summary>
/// Model binder for value objects implementing <see cref="ITryCreatable{T}"/>.
/// Enables automatic validation of value objects during MVC model binding.
/// </summary>
/// <remarks>
/// <para>
/// This model binder works with binding sources that provide string values:
/// <list type="bullet">
/// <item><c>[FromQuery]</c> - Query string parameters</item>
/// <item><c>[FromRoute]</c> - Route values</item>
/// <item><c>[FromForm]</c> - Form data</item>
/// <item><c>[FromHeader]</c> - HTTP headers</item>
/// </list>
/// </para>
/// <para>
/// For <c>[FromBody]</c> JSON, use <see cref="ValidatingJsonConverterFactory"/> instead
/// (configured automatically by <see cref="ValueObjectModelBindingExtensions.AddValueObjectModelBinding"/>).
/// </para>
/// <para>
/// When binding fails, errors are added to <see cref="ModelStateDictionary"/>
/// and returned as standard ASP.NET Core validation problem details.
/// </para>
/// <para>
/// This model binder uses source-generated delegates from <see cref="ValidatingConverterRegistry"/>
/// when available, falling back to reflection only if the generator is not used.
/// </para>
/// </remarks>
public class ValueObjectModelBinder : IModelBinder
{
    // Reflection fallback cache (only used when generator is not available)
    private static readonly ConcurrentDictionary<Type, MethodInfo?> s_tryCreateMethodCache = new();
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> s_valuePropertyCache = new();

    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;

        if (string.IsNullOrEmpty(value))
            return Task.CompletedTask;

        var modelType = bindingContext.ModelType;

        // Try source-generated delegate first (fast path, AOT-compatible)
        var tryCreateFactory = ValidatingConverterRegistry.GetTryCreateFactory(modelType);
        if (tryCreateFactory is not null)
            return BindWithFactory(bindingContext, tryCreateFactory, value, modelName);

        // Fallback to reflection (for non-generated scenarios)
        return BindWithReflection(bindingContext, modelType, value, modelName);
    }

    private static Task BindWithFactory(
        ModelBindingContext bindingContext,
        TryCreateFactory factory,
        string value,
        string modelName)
    {
        var (isSuccess, valueObj, error) = factory(value, modelName);

        if (isSuccess)
        {
            bindingContext.Result = ModelBindingResult.Success(valueObj);
        }
        else
        {
            var errorMessage = error?.Detail ?? "Invalid value.";
            bindingContext.ModelState.TryAddModelError(modelName, errorMessage);
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }

    [UnconditionalSuppressMessage("AOT", "IL2070:RequiresDynamicCode",
        Justification = "Reflection fallback for non-generated scenarios. Use Asp.Generator for AOT compatibility.")]
    private static Task BindWithReflection(
        ModelBindingContext bindingContext,
        Type modelType,
        string value,
        string modelName)
    {
        var tryCreateMethod = GetOrCacheTryCreateMethod(modelType);

        if (tryCreateMethod is null)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        var result = tryCreateMethod.Invoke(null, [value, modelName]);

        if (result is null)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        if (result is IResult resultInterface)
        {
            if (resultInterface.IsSuccess)
            {
                var valueObj = GetValueFromResultReflection(result);
                bindingContext.Result = ModelBindingResult.Success(valueObj);
            }
            else
            {
                var errorMessage = resultInterface.Error?.Detail ?? "Invalid value.";
                bindingContext.ModelState.TryAddModelError(modelName, errorMessage);
                bindingContext.Result = ModelBindingResult.Failed();
            }
        }
        else
        {
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }

#pragma warning disable IL2070 // Reflection fallback for non-generated scenarios
    private static MethodInfo? GetOrCacheTryCreateMethod(Type modelType) =>
        s_tryCreateMethodCache.GetOrAdd(modelType, static type =>
            type.GetMethod(
                "TryCreate",
                BindingFlags.Public | BindingFlags.Static,
                null,
                [typeof(string), typeof(string)],
                null));

    private static object? GetValueFromResultReflection(object result)
    {
        var resultType = result.GetType();
        var valueProperty = s_valuePropertyCache.GetOrAdd(resultType, static type =>
            type.GetProperty("Value"));
        return valueProperty?.GetValue(result);
    }
#pragma warning restore IL2070
}

/// <summary>
/// Provider that creates <see cref="ValueObjectModelBinder"/> for types implementing 
/// <see cref="ITryCreatable{T}"/>.
/// </summary>
public class ValueObjectModelBinderProvider : IModelBinderProvider
{
    // Cache for reflection fallback
    private static readonly ConcurrentDictionary<Type, bool> s_typeCache = new();

    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var modelType = context.Metadata.ModelType;

        // Check generated registry first (fast path)
        if (ValidatingConverterRegistry.HasTryCreateFactory(modelType))
            return new ValueObjectModelBinder();

        // Fallback: check if type implements ITryCreatable<T>
        if (ImplementsITryCreatable(modelType))
            return new ValueObjectModelBinder();

        return null;
    }

    [UnconditionalSuppressMessage("AOT", "IL2070:RequiresDynamicCode",
        Justification = "Reflection fallback for non-generated scenarios.")]
    private static bool ImplementsITryCreatable(Type type) =>
        s_typeCache.GetOrAdd(type, static t =>
        {
            var iTryCreatableType = typeof(ITryCreatable<>);
            return t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == iTryCreatableType &&
                i.GetGenericArguments()[0] == t);
        });
}
