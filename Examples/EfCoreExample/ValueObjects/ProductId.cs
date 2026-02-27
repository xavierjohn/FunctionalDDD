using Trellis;

namespace EfCoreExample.ValueObjects;

/// <summary>
/// Strongly-typed Product identifier using GUID.
/// </summary>
public partial class ProductId : RequiredGuid<ProductId>
{
}