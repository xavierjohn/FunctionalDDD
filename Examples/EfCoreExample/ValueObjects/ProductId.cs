namespace EfCoreExample.ValueObjects;

using Trellis.PrimitiveValueObjects;

/// <summary>
/// Strongly-typed Product identifier using GUID.
/// </summary>
public partial class ProductId : RequiredGuid<ProductId>
{
}