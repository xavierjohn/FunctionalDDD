namespace EfCoreExample.ValueObjects;

using FunctionalDdd.PrimitiveValueObjects;

/// <summary>
/// Strongly-typed Product identifier using GUID.
/// </summary>
public partial class ProductId : RequiredGuid<ProductId>
{
}