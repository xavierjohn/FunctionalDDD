using Trellis;

namespace EfCoreExample.ValueObjects;

/// <summary>
/// Strongly-typed Product name that cannot be null, empty, or whitespace.
/// </summary>
public partial class ProductName : RequiredString<ProductName>
{
}