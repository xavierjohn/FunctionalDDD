namespace Trellis.EntityFrameworkCore.Generator;

/// <summary>
/// Metadata about a partial property of type <c>Maybe&lt;T&gt;</c> that requires source generation.
/// </summary>
internal sealed class MaybePropertyInfo
{
    /// <summary>The namespace of the containing type.</summary>
    public readonly string Namespace;

    /// <summary>The name of the containing type (e.g., "Customer").</summary>
    public readonly string TypeName;

    /// <summary>The accessibility of the containing type (e.g., "public").</summary>
    public readonly string TypeAccessibility;

    /// <summary>Whether the containing type is a record.</summary>
    public readonly bool IsRecord;

    /// <summary>The property name (e.g., "Phone").</summary>
    public readonly string PropertyName;

    /// <summary>The accessibility of the property (e.g., "public").</summary>
    public readonly string PropertyAccessibility;

    /// <summary>The accessibility of the setter, if different from the property (e.g., "private").</summary>
    public readonly string SetterAccessibility;

    /// <summary>The fully-qualified inner type name (e.g., "PhoneNumber" or "System.DateTime").</summary>
    public readonly string InnerTypeName;

    /// <summary>The minimal inner type name for display (e.g., "PhoneNumber" or "DateTime").</summary>
    public readonly string InnerTypeShortName;

    /// <summary>Whether the inner type is a value type (struct).</summary>
    public readonly bool InnerTypeIsValueType;

    /// <summary>The nesting chain of parent types if the type is nested (e.g., ["OuterClass"]).</summary>
    public readonly string[] NestingParents;

    public MaybePropertyInfo(
        string @namespace,
        string typeName,
        string typeAccessibility,
        bool isRecord,
        string propertyName,
        string propertyAccessibility,
        string setterAccessibility,
        string innerTypeName,
        string innerTypeShortName,
        bool innerTypeIsValueType,
        string[] nestingParents)
    {
        Namespace = @namespace;
        TypeName = typeName;
        TypeAccessibility = typeAccessibility;
        IsRecord = isRecord;
        PropertyName = propertyName;
        PropertyAccessibility = propertyAccessibility;
        SetterAccessibility = setterAccessibility;
        InnerTypeName = innerTypeName;
        InnerTypeShortName = innerTypeShortName;
        InnerTypeIsValueType = innerTypeIsValueType;
        NestingParents = nestingParents;
    }
}