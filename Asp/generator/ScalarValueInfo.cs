namespace FunctionalDdd.AspSourceGenerator;

/// <summary>
/// Represents metadata about a scalar value type discovered during source generation.
/// Used to generate AOT-compatible JSON converters and serializer context entries.
/// </summary>
/// <remarks>
/// <para>
/// This class captures essential information needed to generate:
/// <list type="bullet">
/// <item>Strongly-typed JSON converters without runtime reflection</item>
/// <item><c>[JsonSerializable]</c> attributes for AOT compilation</item>
/// <item>Registration code for JSON serialization infrastructure</item>
/// </list>
/// </para>
/// </remarks>
internal class ScalarValueInfo
{
    /// <summary>
    /// Gets the namespace of the scalar value type.
    /// </summary>
    /// <value>
    /// The fully-qualified namespace (e.g., "MyApp.Domain.Values").
    /// </value>
    public readonly string Namespace;

    /// <summary>
    /// Gets the name of the scalar value type.
    /// </summary>
    /// <value>
    /// The simple class name without namespace (e.g., "CustomerId", "EmailAddress").
    /// </value>
    public readonly string TypeName;

    /// <summary>
    /// Gets the primitive type that the scalar value wraps.
    /// </summary>
    /// <value>
    /// The primitive type name (e.g., "string", "Guid", "int").
    /// </value>
    public readonly string PrimitiveType;

    /// <summary>
    /// Gets the fully qualified name of the value object type.
    /// </summary>
    /// <value>
    /// The full type name including namespace (e.g., "MyApp.Domain.ValueObjects.CustomerId").
    /// </value>
    public string FullTypeName => string.IsNullOrEmpty(Namespace) ? TypeName : $"{Namespace}.{TypeName}";

    /// <summary>
    /// Initializes a new instance of the <see cref="ScalarValueInfo"/> class.
    /// </summary>
    /// <param name="namespace">The namespace of the value object type.</param>
    /// <param name="typeName">The name of the value object type.</param>
    /// <param name="primitiveType">The primitive type that the value object wraps.</param>
    public ScalarValueInfo(string @namespace, string typeName, string primitiveType)
    {
        Namespace = @namespace;
        TypeName = typeName;
        PrimitiveType = primitiveType;
    }

    /// <summary>
    /// Returns a string representation for debugging purposes.
    /// </summary>
    public override string ToString() => $"{FullTypeName} : IScalarValue<{TypeName}, {PrimitiveType}>";
}
