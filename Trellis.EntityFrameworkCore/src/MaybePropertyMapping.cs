namespace Trellis.EntityFrameworkCore;

/// <summary>
/// Describes how a <see cref="Maybe{T}"/> property resolved to an EF Core field-only mapping.
/// </summary>
/// <param name="EntityTypeName">The EF Core entity type name.</param>
/// <param name="EntityClrType">The entity CLR type.</param>
/// <param name="PropertyName">The original <see cref="Maybe{T}"/> CLR property name.</param>
/// <param name="BackingFieldName">The source-generated private backing field name.</param>
/// <param name="InnerType">The inner <see cref="Maybe{T}"/> value type.</param>
/// <param name="StoreType">The field-only EF Core property CLR type.</param>
/// <param name="IsMapped">Whether the backing field is present in the EF model.</param>
/// <param name="IsNullable">Whether the mapped backing field is nullable in the EF model.</param>
/// <param name="ColumnName">The relational column name, if available.</param>
/// <param name="ProviderClrType">The provider CLR type after value conversion, if any.</param>
public sealed record MaybePropertyMapping(
    string EntityTypeName,
    Type EntityClrType,
    string PropertyName,
    string BackingFieldName,
    Type InnerType,
    Type StoreType,
    bool IsMapped,
    bool IsNullable,
    string? ColumnName,
    Type? ProviderClrType);