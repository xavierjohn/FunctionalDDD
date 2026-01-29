namespace SpecificationExample.Specifications;

using Ardalis.Specification;
using FunctionalDdd.PrimitiveValueObjects;
using SpecificationExample.Domain;

/// <summary>
/// Specification to find a product by its unique SKU value object.
/// Uses SingleResultSpecification since SKU should be unique.
/// </summary>
public sealed class ProductBySkuSpec : SingleResultSpecification<Product>
{
    public ProductBySkuSpec(Sku sku) =>
        Query.Where(p => p.Sku == sku);
}

/// <summary>
/// Specification to find a product by its ID value object.
/// </summary>
public sealed class ProductByIdSpec : SingleResultSpecification<Product>
{
    public ProductByIdSpec(ProductId id) =>
        Query.Where(p => p.Id == id);
}

/// <summary>
/// Specification to find all active products in a category.
/// </summary>
public sealed class ActiveProductsByCategorySpec : Specification<Product>
{
    public ActiveProductsByCategorySpec(CategoryName category) =>
        Query
            .Where(p => p.IsActive)
            .Where(p => p.Category == category)
            .OrderBy(p => p.Name.Value);
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
    public ProductsByPriceRangeSpec(Money minPrice, Money maxPrice) =>
        Query
            .Where(p => p.IsActive)
            .Where(p => p.Price.Amount >= minPrice.Amount && p.Price.Amount <= maxPrice.Amount)
            .OrderBy(p => p.Price.Amount);
}
