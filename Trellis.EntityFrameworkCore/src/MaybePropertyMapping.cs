namespace Trellis.EntityFrameworkCore;

/// <summary>
/// Describes how a <see cref="Maybe{T}"/> property resolved to an EF Core field-only mapping.
/// </summary>
public sealed class MaybePropertyMapping
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MaybePropertyMapping"/> class.
    /// </summary>
    /// <param name="entityTypeName">The EF Core entity type name.</param>
    /// <param name="entityClrType">The entity CLR type.</param>
    /// <param name="propertyName">The original <see cref="Maybe{T}"/> CLR property name.</param>
    /// <param name="backingFieldName">The source-generated private backing field name.</param>
    /// <param name="innerType">The inner <see cref="Maybe{T}"/> value type.</param>
    /// <param name="storeType">The field-only EF Core property CLR type.</param>
    /// <param name="isMapped">Whether the backing field is present in the EF model.</param>
    /// <param name="isNullable">Whether the mapped backing field is nullable in the EF model.</param>
    /// <param name="columnName">The relational column name, if available.</param>
    /// <param name="providerClrType">The provider CLR type after value conversion, if any.</param>
    public MaybePropertyMapping(
        string entityTypeName,
        Type entityClrType,
        string propertyName,
        string backingFieldName,
        Type innerType,
        Type storeType,
        bool isMapped,
        bool isNullable,
        string? columnName,
        Type? providerClrType)
    {
        EntityTypeName = entityTypeName;
        EntityClrType = entityClrType;
        PropertyName = propertyName;
        BackingFieldName = backingFieldName;
        InnerType = innerType;
        StoreType = storeType;
        IsMapped = isMapped;
        IsNullable = isNullable;
        ColumnName = columnName;
        ProviderClrType = providerClrType;
    }

    /// <summary>
    /// Gets the EF Core entity type name.
    /// </summary>
    public string EntityTypeName { get; }

    /// <summary>
    /// Gets the entity CLR type.
    /// </summary>
    public Type EntityClrType { get; }

    /// <summary>
    /// Gets the original <see cref="Maybe{T}"/> CLR property name.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the source-generated private backing field name.
    /// </summary>
    public string BackingFieldName { get; }

    /// <summary>
    /// Gets the inner <see cref="Maybe{T}"/> value type.
    /// </summary>
    public Type InnerType { get; }

    /// <summary>
    /// Gets the CLR type of the field-only EF Core property.
    /// </summary>
    public Type StoreType { get; }

    /// <summary>
    /// Gets a value indicating whether the backing field is mapped in the EF model.
    /// </summary>
    public bool IsMapped { get; }

    /// <summary>
    /// Gets a value indicating whether the mapped field is nullable.
    /// </summary>
    public bool IsNullable { get; }

    /// <summary>
    /// Gets the relational column name, if available.
    /// </summary>
    public string? ColumnName { get; }

    /// <summary>
    /// Gets the provider CLR type after value conversion, if one is configured.
    /// </summary>
    public Type? ProviderClrType { get; }
}