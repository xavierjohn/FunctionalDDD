namespace SpecificationExample.Domain;

/// <summary>
/// Product aggregate root demonstrating Ardalis.Specification integration.
/// </summary>
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public int StockQuantity { get; private set; }

    // For EF Core
    private Product() { }

    public static Product Create(string name, string sku, decimal price, string category, int stockQuantity = 0) =>
        new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Sku = sku,
            Price = price,
            Category = category,
            IsActive = true,
            StockQuantity = stockQuantity
        };

    public void Deactivate() => IsActive = false;

    public void UpdateStock(int quantity) => StockQuantity = quantity;
}
