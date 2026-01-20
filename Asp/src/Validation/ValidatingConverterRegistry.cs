namespace FunctionalDdd;

using System.Collections.Concurrent;
using System.Text.Json.Serialization;

/// <summary>
/// Delegate for creating a property-name-aware wrapper converter.
/// </summary>
/// <param name="innerConverter">The inner converter to wrap.</param>
/// <param name="propertyName">The property name to use for validation errors.</param>
/// <returns>A wrapper converter that sets the property name in context.</returns>
public delegate JsonConverter WrapperConverterFactory(JsonConverter innerConverter, string propertyName);

/// <summary>
/// Delegate for calling TryCreate on a value object type.
/// Returns a result with the value extracted to avoid reflection.
/// </summary>
/// <param name="value">The string value to parse.</param>
/// <param name="fieldName">The field name for validation errors.</param>
/// <returns>A tuple containing success state, the value (if successful), and the error (if failed).</returns>
public delegate (bool IsSuccess, object? Value, Error? Error) TryCreateFactory(string? value, string? fieldName);

/// <summary>
/// A registry for pre-instantiated JSON converters and TryCreate delegates for ITryCreatable types.
/// Used by <see cref="ValidatingJsonConverterFactory"/> and <see cref="ValueObjectModelBinder"/> 
/// to provide AOT-compatible lookup.
/// </summary>
/// <remarks>
/// <para>
/// The Asp.Generator source generator populates this registry at compile time by generating
/// a module initializer that calls <see cref="Register{T}"/> for each discovered ITryCreatable type.
/// </para>
/// <para>
/// When the generator is not used, the registry remains empty and
/// <see cref="ValidatingJsonConverterFactory"/> falls back to reflection-based converter creation.
/// </para>
/// </remarks>
public static class ValidatingConverterRegistry
{
    private static readonly ConcurrentDictionary<Type, JsonConverter> s_converters = new();
    private static readonly ConcurrentDictionary<Type, WrapperConverterFactory> s_wrapperFactories = new();
    private static readonly ConcurrentDictionary<Type, TryCreateFactory> s_tryCreateDelegates = new();

    /// <summary>
    /// Registers a converter and TryCreate delegate for the specified type.
    /// </summary>
    /// <typeparam name="T">The ITryCreatable type to register.</typeparam>
    public static void Register<T>() where T : class, ITryCreatable<T>
    {
        s_converters.TryAdd(typeof(T), new ValidatingJsonConverter<T>());
        s_wrapperFactories.TryAdd(typeof(T), static (inner, propName) =>
            new PropertyNameAwareConverter<T>((ValidatingJsonConverter<T>)inner, propName));
        s_tryCreateDelegates.TryAdd(typeof(T), static (value, fieldName) =>
        {
            var result = T.TryCreate(value, fieldName);
            return (result.IsSuccess, result.IsSuccess ? result.Value : null, result.IsFailure ? result.Error : null);
        });
    }

    /// <summary>
    /// Registers a struct converter and TryCreate delegate for the specified value type.
    /// </summary>
    /// <typeparam name="T">The ITryCreatable value type to register.</typeparam>
    public static void RegisterStruct<T>() where T : struct, ITryCreatable<T>
    {
        s_converters.TryAdd(typeof(T), new ValidatingStructJsonConverter<T>());
        s_wrapperFactories.TryAdd(typeof(T), static (inner, propName) =>
            new PropertyNameAwareConverter<T?>((ValidatingStructJsonConverter<T>)inner, propName));
        s_tryCreateDelegates.TryAdd(typeof(T), static (value, fieldName) =>
        {
            var result = T.TryCreate(value, fieldName);
            return (result.IsSuccess, result.IsSuccess ? (object?)result.Value : null, result.IsFailure ? result.Error : null);
        });
    }

    /// <summary>
    /// Registers a pre-instantiated converter for the specified type.
    /// </summary>
    /// <param name="type">The type to register the converter for.</param>
    /// <param name="converter">The converter instance.</param>
    public static void Register(Type type, JsonConverter converter) =>
        s_converters.TryAdd(type, converter);

    /// <summary>
    /// Gets whether a converter is registered for the specified type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if a converter is registered; otherwise false.</returns>
    public static bool HasConverter(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);
        var typeToCheck = underlyingType ?? type;
        return s_converters.ContainsKey(typeToCheck);
    }

    /// <summary>
    /// Gets the registered converter for the specified type.
    /// </summary>
    /// <param name="type">The type to get the converter for.</param>
    /// <returns>The converter if registered; otherwise null.</returns>
    public static JsonConverter? GetConverter(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);
        var typeToCheck = underlyingType ?? type;
        return s_converters.TryGetValue(typeToCheck, out var converter) ? converter : null;
    }

    /// <summary>
    /// Gets the registered wrapper factory for the specified type.
    /// </summary>
    /// <param name="type">The type to get the wrapper factory for.</param>
    /// <returns>The wrapper factory if registered; otherwise null.</returns>
    public static WrapperConverterFactory? GetWrapperFactory(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);
        var typeToCheck = underlyingType ?? type;
        return s_wrapperFactories.TryGetValue(typeToCheck, out var factory) ? factory : null;
    }

    /// <summary>
    /// Gets the registered TryCreate delegate for the specified type.
    /// </summary>
    /// <param name="type">The type to get the TryCreate delegate for.</param>
    /// <returns>The delegate if registered; otherwise null.</returns>
    public static TryCreateFactory? GetTryCreateFactory(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);
        var typeToCheck = underlyingType ?? type;
        return s_tryCreateDelegates.TryGetValue(typeToCheck, out var del) ? del : null;
    }

    /// <summary>
    /// Checks if a type has a registered TryCreate delegate.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if a delegate is registered; otherwise false.</returns>
    public static bool HasTryCreateFactory(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);
        var typeToCheck = underlyingType ?? type;
        return s_tryCreateDelegates.ContainsKey(typeToCheck);
    }

    /// <summary>
    /// Clears all registered converters and delegates. Primarily for testing.
    /// </summary>
    internal static void Clear()
    {
        s_converters.Clear();
        s_wrapperFactories.Clear();
        s_tryCreateDelegates.Clear();
    }
}
