namespace FunctionalDdd.Asp.Validation;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

/// <summary>
/// Helper class for detecting and working with <see cref="IScalarValue{TSelf, TPrimitive}"/> types.
/// Centralizes reflection logic to avoid duplication across converters, model binders, and configuration.
/// </summary>
internal static class ScalarValueTypeHelper
{
    /// <summary>
    /// Checks if the given type implements <see cref="IScalarValue{TSelf, TPrimitive}"/>
    /// where TSelf is the type itself (CRTP pattern).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a scalar value object, false otherwise.</returns>
    public static bool IsScalarValue([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return GetScalarValueInterface(type) is not null;
    }

    /// <summary>
    /// Gets the <see cref="IScalarValue{TSelf, TPrimitive}"/> interface implemented by the type,
    /// or null if the type doesn't implement it correctly.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>The interface type if found, null otherwise.</returns>
    /// <remarks>
    /// This method verifies the CRTP pattern by ensuring the first generic argument
    /// of the interface matches the type itself.
    /// </remarks>
    public static Type? GetScalarValueInterface([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(IScalarValue<,>) &&
                                i.GetGenericArguments()[0] == type);
    }

    /// <summary>
    /// Gets the primitive type (TPrimitive) from a scalar value object type.
    /// </summary>
    /// <param name="valueType">The value object type.</param>
    /// <returns>The primitive type, or null if the type is not a scalar value object.</returns>
    public static Type? GetPrimitiveType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type valueType)
    {
        ArgumentNullException.ThrowIfNull(valueType);
        var interfaceType = GetScalarValueInterface(valueType);
        return interfaceType?.GetGenericArguments()[1];
    }

    /// <summary>
    /// Creates an instance of a generic type parameterized with a value object type and its primitive type.
    /// </summary>
    /// <typeparam name="TResult">The expected result type (usually an interface like IModelBinder or JsonConverter).</typeparam>
    /// <param name="genericTypeDefinition">The open generic type (e.g., typeof(SomeClass&lt;,&gt;)).</param>
    /// <param name="valueObjectType">The value object type (first type argument).</param>
    /// <param name="primitiveType">The primitive type (second type argument).</param>
    /// <returns>An instance of the constructed generic type, or null if creation fails.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2055", Justification = "MakeGenericType is used with known converter/binder types")]
    [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "Types are preserved by ASP.NET Core serialization infrastructure")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Not compatible with Native AOT")]
    public static TResult? CreateGenericInstance<TResult>(
        Type genericTypeDefinition,
        Type valueObjectType,
        Type primitiveType)
        where TResult : class
    {
        ArgumentNullException.ThrowIfNull(genericTypeDefinition);
        ArgumentNullException.ThrowIfNull(valueObjectType);
        ArgumentNullException.ThrowIfNull(primitiveType);

        try
        {
            var constructedType = genericTypeDefinition.MakeGenericType(valueObjectType, primitiveType);
            return Activator.CreateInstance(constructedType) as TResult;
        }
        catch
        {
            // Return null if type construction fails (e.g., constraint violations)
            return null;
        }
    }

    /// <summary>
    /// Invokes <c>TryCreate(string?, string?)</c> on a scalar value type and returns
    /// the validation errors as a dictionary suitable for <c>Results.ValidationProblem</c>.
    /// </summary>
    /// <param name="scalarValueType">The scalar value type to validate against.</param>
    /// <param name="rawValue">The raw string value that failed binding.</param>
    /// <param name="parameterName">The parameter/field name for error messages.</param>
    /// <returns>
    /// A dictionary of field names to error messages if validation fails, or null if
    /// the <c>TryCreate</c> method is not found or the value is valid.
    /// </returns>
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "TryCreate is always present on IScalarValue types generated by the source generator.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Not compatible with Native AOT — reflection-based fallback only.")]
    public static IDictionary<string, string[]>? GetValidationErrors(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type scalarValueType,
        string? rawValue,
        string? parameterName)
    {
        // All generated scalar value types have TryCreate(string?, string?) overload
        var tryCreateMethod = scalarValueType.GetMethod(
            "TryCreate",
            BindingFlags.Public | BindingFlags.Static,
            null,
            [typeof(string), typeof(string)],
            null);

        if (tryCreateMethod is null)
            return null;

        object? result;
        try
        {
            result = tryCreateMethod.Invoke(null, [rawValue, parameterName]);
        }
        catch
        {
            return null;
        }

        // Result<T> implements IResult, so we can check IsFailure and extract Error
        if (result is not IResult { IsFailure: true } failure)
            return null;

        if (failure.Error is ValidationError validationError)
            return validationError.ToDictionary();

        // Fallback for non-validation errors
        var fieldName = parameterName ?? string.Empty;
        return new Dictionary<string, string[]>
        {
            [fieldName] = [failure.Error.Detail]
        };
    }
}