namespace Trellis.EntityFrameworkCore;

/// <summary>
/// Determines whether a CLR type is a Trellis value object
/// (<see cref="IScalarValue{TSelf, TPrimitive}"/> or <see cref="RequiredEnum{TSelf}"/>)
/// and extracts its database provider type.
/// </summary>
internal static class TrellisTypeScanner
{
    private static readonly Type s_scalarValueType = typeof(IScalarValue<,>);
    private static readonly Type s_requiredEnumType = typeof(RequiredEnum<>);

    /// <summary>
    /// Returns the provider type and whether the type is a <see cref="RequiredEnum{TSelf}"/>,
    /// or <see langword="null"/> if the type is not a Trellis value object.
    /// </summary>
    /// <remarks>
    /// <see cref="RequiredEnum{TSelf}"/> is checked first because it implements
    /// <see cref="IScalarValue{TSelf, TPrimitive}"/> but requires a different converter
    /// (<c>Name</c>/<c>TryFromName</c> instead of <c>Value</c>/<c>Create</c>).
    /// </remarks>
    internal static (Type ProviderType, bool IsEnum)? FindTrellisBase(Type type)
    {
        // Check RequiredEnum first — it implements IScalarValue<TSelf, string>
        // but needs a different converter (Name/TryFromName instead of Value/Create).
        var current = type.BaseType;
        while (current is not null)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == s_requiredEnumType)
                return (typeof(string), true);

            current = current.BaseType;
        }

        // Check for IScalarValue<TSelf, TPrimitive> interface.
        foreach (var iface in type.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == s_scalarValueType)
                return (iface.GetGenericArguments()[1], false);
        }

        return null;
    }
}
