namespace FunctionalDdd.PrimitiveValueObjectGenerator;

/// <summary>
/// Represents metadata about a partial class that requires source generation for value object functionality.
/// Used by the source generator to create factory methods, validation, and parsing logic.
/// </summary>
/// <remarks>
/// <para>
/// This class captures the essential information needed to generate the complementary partial class
/// that provides the public API for value objects inheriting from <see cref="RequiredGuid"/>, <see cref="RequiredString"/>,
/// <see cref="RequiredInt"/>, <see cref="RequiredDecimal"/>, or <see cref="RequiredEnum"/>.
/// </para>
/// <para>
/// The generator uses this information to create:
/// <list type="bullet">
/// <item>Static factory methods (NewUnique for GUIDs, TryCreate for all types)</item>
/// <item>Validation logic ensuring non-empty values</item>
/// <item>IParsable implementation for parsing support</item>
/// <item>JSON serialization attributes</item>
/// <item>Private constructors that call the base class (except for RequiredEnum)</item>
/// </list>
/// </para>
/// </remarks>
internal class RequiredPartialClassInfo
{
    /// <summary>
    /// Gets the namespace of the partial class.
    /// </summary>
    /// <value>
    /// The fully-qualified namespace (e.g., "MyApp.Domain.ValueObjects").
    /// </value>
    public readonly string NameSpace;

    /// <summary>
    /// Gets the name of the partial class.
    /// </summary>
    /// <value>
    /// The simple class name without namespace (e.g., "CustomerId", "EmailAddress").
    /// </value>
    public readonly string ClassName;

    /// <summary>
    /// Gets the base class that the partial class inherits from.
    /// </summary>
    /// <value>
    /// One of "RequiredGuid", "RequiredString", "RequiredInt", "RequiredDecimal", or "RequiredEnum",
    /// determining which factory methods are generated.
    /// </value>
    /// <remarks>
    /// <list type="bullet">
    /// <item><c>RequiredGuid</c>: Generates NewUniqueV4(), NewUniqueV7(), TryCreate(Guid?), TryParse(string?)</item>
    /// <item><c>RequiredString</c>: Generates TryCreate(string?)</item>
    /// <item><c>RequiredInt</c>: Generates TryCreate(int?), TryParse(string?)</item>
    /// <item><c>RequiredDecimal</c>: Generates TryCreate(decimal?), TryParse(string?)</item>
    /// <item><c>RequiredEnum</c>: Generates TryCreate(string?) delegating to TryFromName()</item>
    /// </list>
    /// </remarks>
    public readonly string ClassBase;

    /// <summary>
    /// Gets the accessibility level of the partial class.
    /// </summary>
    /// <value>
    /// The access modifier (e.g., "public", "internal", "private").
    /// </value>
    /// <remarks>
    /// The generated partial class will match this accessibility to ensure consistency.
    /// </remarks>
    public readonly string Accessibility;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequiredPartialClassInfo"/> class.
    /// </summary>
    /// <param name="nameSpace">The namespace of the partial class.</param>
    /// <param name="className">The name of the partial class.</param>
    /// <param name="classBase">The base class (RequiredGuid or RequiredString).</param>
    /// <param name="accessibility">The accessibility level (public, internal, etc.).</param>
    public RequiredPartialClassInfo(string nameSpace, string className, string classBase, string accessibility)
    {
        NameSpace = nameSpace;
        ClassName = className;
        ClassBase = classBase;
        Accessibility = accessibility;
    }
}