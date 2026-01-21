namespace FunctionalDdd.Asp.Validation;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Helper class for detecting and working with <see cref="IScalarValueObject{TSelf, TPrimitive}"/> types.
/// Centralizes reflection logic to avoid duplication across converters, model binders, and configuration.
/// </summary>
internal static class ScalarValueObjectTypeHelper
{
    /// <summary>
    /// Checks if the given type implements <see cref="IScalarValueObject{TSelf, TPrimitive}"/>
    /// where TSelf is the type itself (CRTP pattern).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a scalar value object, false otherwise.</returns>
    public static bool IsScalarValueObject([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type) =>
        GetScalarValueObjectInterface(type) is not null;

    /// <summary>
    /// Gets the <see cref="IScalarValueObject{TSelf, TPrimitive}"/> interface implemented by the type,
    /// or null if the type doesn't implement it correctly.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>The interface type if found, null otherwise.</returns>
    /// <remarks>
    /// This method verifies the CRTP pattern by ensuring the first generic argument
    /// of the interface matches the type itself.
    /// </remarks>
    public static Type? GetScalarValueObjectInterface([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type) =>
        type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(IScalarValueObject<,>) &&
                                i.GetGenericArguments()[0] == type);

    /// <summary>
    /// Gets the primitive type (TPrimitive) from a scalar value object type.
    /// </summary>
    /// <param name="valueObjectType">The value object type.</param>
    /// <returns>The primitive type, or null if the type is not a scalar value object.</returns>
    public static Type? GetPrimitiveType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type valueObjectType)
    {
        var interfaceType = GetScalarValueObjectInterface(valueObjectType);
        return interfaceType?.GetGenericArguments()[1];
    }
}
