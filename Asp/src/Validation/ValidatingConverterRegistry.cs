namespace FunctionalDdd;

using System.Collections.Concurrent;
using System.Text.Json.Serialization;

/// <summary>
/// A registry for pre-instantiated JSON converters for ITryCreatable types.
/// Used by <see cref="ValidatingJsonConverterFactory"/> to provide AOT-compatible converter lookup.
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

    /// <summary>
    /// Registers a converter for the specified type.
    /// </summary>
    /// <typeparam name="T">The ITryCreatable type to register.</typeparam>
    public static void Register<T>() where T : class, ITryCreatable<T> =>
        s_converters.TryAdd(typeof(T), new ValidatingJsonConverter<T>());

    /// <summary>
    /// Registers a struct converter for the specified value type.
    /// </summary>
    /// <typeparam name="T">The ITryCreatable value type to register.</typeparam>
    public static void RegisterStruct<T>() where T : struct, ITryCreatable<T> =>
        s_converters.TryAdd(typeof(T), new ValidatingStructJsonConverter<T>());

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
    /// Clears all registered converters. Primarily for testing.
    /// </summary>
    internal static void Clear() => s_converters.Clear();
}
