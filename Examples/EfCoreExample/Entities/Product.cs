namespace EfCoreExample.Entities;

using EfCoreExample.ValueObjects;
using FunctionalDdd;

/// <summary>
/// Product entity with strongly-typed ID (GUID) and validated name.
/// Demonstrates using RequiredGuid alongside RequiredUlid in the same domain.
/// </summary>
public class Product : Entity<ProductId>
{
    public ProductName Name { get; private set; } = null!;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }

    // EF Core requires parameterless constructor
    private Product() : base(ProductId.NewUniqueV4()) { }

    private Product(ProductId id, ProductName name, decimal price, int stockQuantity) : base(id)
    {
        Name = name;
        Price = price;
        StockQuantity = stockQuantity;
    }

    /// <summary>
    /// Creates a new product with validation.
    /// </summary>
    public static Result<Product> TryCreate(string? name, decimal price, int stockQuantity) =>
        ProductName.TryCreate(name, nameof(name))
            .Ensure(_ => price > 0, Error.Validation("Price must be greater than zero", nameof(price)))
            .Ensure(_ => stockQuantity >= 0, Error.Validation("Stock cannot be negative", nameof(stockQuantity)))
            .Map(productName => new Product(
                ProductId.NewUniqueV4(),
                productName,
                price,
                stockQuantity));

    /// <summary>
    /// Reduces stock when order is placed.
    /// </summary>
    public Result<Product> ReduceStock(int quantity) =>
        this.ToResult()
            .Ensure(_ => quantity > 0, Error.Validation("Quantity must be positive", nameof(quantity)))
            .Ensure(_ => StockQuantity >= quantity, Error.Validation($"Insufficient stock. Available: {StockQuantity}", nameof(quantity)))
            .Tap(_ => StockQuantity -= quantity);
}