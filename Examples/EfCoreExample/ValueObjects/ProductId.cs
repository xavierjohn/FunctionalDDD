namespace EfCoreExample.ValueObjects;

using FunctionalDdd.PrimitiveValueObjects;

/// <summary>
/// Strongly-typed Product identifier using GUID for compatibility with legacy systems.
/// Demonstrates mixing ULID and GUID value objects in the same domain.
/// </summary>
public partial class ProductId : RequiredGuid<ProductId>
{
}