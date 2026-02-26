namespace SpecificationExample.Domain;

using Trellis;
using Trellis.PrimitiveValueObjects;

/// <summary>
/// Strongly-typed Product ID.
/// </summary>
public partial class ProductId : RequiredGuid<ProductId>;

/// <summary>
/// Strongly-typed SKU (Stock Keeping Unit).
/// </summary>
public partial class Sku : RequiredString<Sku>;

/// <summary>
/// Strongly-typed Category name.
/// </summary>
public partial class CategoryName : RequiredString<CategoryName>;

/// <summary>
/// Strongly-typed Product name.
/// </summary>
public partial class ProductName : RequiredString<ProductName>;