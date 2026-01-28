namespace SpecificationExample.Specifications;

using Ardalis.Specification;
using SpecificationExample.Domain;

/// <summary>
/// Specification to find a product by its unique SKU.
/// Uses SingleResultSpecification since SKU should be unique.
/// </summary>
public sealed class ProductBySkuSpec : SingleResultSpecification<Product>
{
    public ProductBySkuSpec(string sku) =>
        Query.Where(p => p.Sku == sku);
}

/// <summary>
/// Specification to find a product by its ID.
/// </summary>
public sealed class ProductByIdSpec : SingleResultSpecification<Product>
{
    public ProductByIdSpec(Guid id) =>
        Query.Where(p => p.Id == id);
}

/// <summary>
/// Specification to find all active products in a category.
/// </summary>
public sealed class ActiveProductsByCategorySpec : Specification<Product>
{
    public ActiveProductsByCategorySpec(string category) =>
        Query
            .Where(p => p.IsActive)
            .Where(p => p.Category == category)
            .OrderBy(p => p.Name);
}

/// <summary>
/// Specification to find all products with low stock.
/// </summary>
public sealed class LowStockProductsSpec : Specification<Product>
{
    public LowStockProductsSpec(int threshold = 10) =>
        Query
            .Where(p => p.IsActive)
            .Where(p => p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity);
}

/// <summary>
/// Specification to find products within a price range.
/// </summary>
public sealed class ProductsByPriceRangeSpec : Specification<Product>
{
    public ProductsByPriceRangeSpec(decimal minPrice, decimal maxPrice) =>
        Query
            .Where(p => p.IsActive)
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .OrderBy(p => p.Price);
}
