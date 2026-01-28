namespace SpecificationExample.Domain;

using FunctionalDdd;
using FunctionalDdd.PrimitiveValueObjects;

/// <summary>
/// Product aggregate root using primitive value objects.
/// Demonstrates Ardalis.Specification integration with FunctionalDDD value objects.
/// </summary>
public class Product
{
    public ProductId Id { get; private set; } = null!;
    public ProductName Name { get; private set; } = null!;
    public Sku Sku { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public CategoryName Category { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public int StockQuantity { get; private set; }

    // For EF Core
    private Product() { }

    public static Result<Product> TryCreate(
        string name,
        string sku,
        decimal price,
        string category,
        int stockQuantity = 0) =>
        ProductName.TryCreate(name, "name")
            .Combine(Sku.TryCreate(sku, "sku"))
            .Combine(Money.TryCreate(price, "USD", "price"))
            .Combine(CategoryName.TryCreate(category, "category"))
            .Map((productName, productSku, productPrice, productCategory) => new Product
            {
                Id = ProductId.NewUnique(),
                Name = productName,
                Sku = productSku,
                Price = productPrice,
                Category = productCategory,
                IsActive = true,
                StockQuantity = stockQuantity
            });

    public static Product Create(string name, string sku, decimal price, string category, int stockQuantity = 0)
    {
        var result = TryCreate(name, sku, price, category, stockQuantity);
        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to create Product: {result.Error.Detail}");
        return result.Value;
    }

    public void Deactivate() => IsActive = false;

    public void UpdateStock(int quantity) => StockQuantity = quantity;
}
