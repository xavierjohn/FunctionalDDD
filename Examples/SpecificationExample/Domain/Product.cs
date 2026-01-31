namespace SpecificationExample.Domain;

using FunctionalDdd;
using FunctionalDdd.PrimitiveValueObjects;

/// <summary>
/// Product aggregate root using primitive value objects.
/// Demonstrates Ardalis.Specification integration with FunctionalDDD value objects.
/// </summary>
public class Product : Aggregate<ProductId>
{
    public ProductName Name { get; private set; } = null!;
    public Sku Sku { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public CategoryName Category { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public int StockQuantity { get; private set; }

    // EF Core requires parameterless constructor for entity reconstitution
    private Product() : base(null!) { }

    // Private constructor - all creation goes through factory methods
    private Product(
        ProductId id,
        ProductName name,
        Sku sku,
        Money price,
        CategoryName category,
        int stockQuantity,
        bool isActive = true) : base(id)
    {
        Name = name;
        Sku = sku;
        Price = price;
        Category = category;
        IsActive = isActive;
        StockQuantity = stockQuantity;
    }

    /// <summary>
    /// Creates a NEW product with a freshly generated ID.
    /// Use this for creating new products in the domain.
    /// </summary>
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
            .Map((productName, productSku, productPrice, productCategory) =>
                new Product(
                    ProductId.NewUnique(),  // ✅ New ID for new products
                    productName,
                    productSku,
                    productPrice,
                    productCategory,
                    stockQuantity));

    /// <summary>
    /// Reconstitutes an EXISTING product with a known ID.
    /// Use this when loading from database, testing, or working with existing products.
    /// </summary>
    public static Result<Product> TryCreateExisting(
        ProductId id,
        string name,
        string sku,
        decimal price,
        string category,
        int stockQuantity = 0,
        bool isActive = true) =>
        id.ToResult(Error.Validation("Product ID is required", nameof(id)))
            .Combine(ProductName.TryCreate(name, "name"))
            .Combine(Sku.TryCreate(sku, "sku"))
            .Combine(Money.TryCreate(price, "USD", "price"))
            .Combine(CategoryName.TryCreate(category, "category"))
            .Map((productId, productName, productSku, productPrice, productCategory) =>
                new Product(
                    productId,  // ✅ Existing ID preserved
                    productName,
                    productSku,
                    productPrice,
                    productCategory,
                    stockQuantity,
                    isActive));

    /// <summary>
    /// Creates a NEW product (throws on validation failure).
    /// Use in tests or when validation failure is exceptional.
    /// </summary>
    public static Product Create(string name, string sku, decimal price, string category, int stockQuantity = 0)
    {
        var result = TryCreate(name, sku, price, category, stockQuantity);
        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to create Product: {result.Error.Detail}");
        return result.Value;
    }

    /// <summary>
    /// Reconstitutes an EXISTING product (throws on validation failure).
    /// Use in tests when you need a product with a specific ID.
    /// </summary>
    public static Product CreateExisting(
        ProductId id,
        string name,
        string sku,
        decimal price,
        string category,
        int stockQuantity = 0,
        bool isActive = true)
    {
        var result = TryCreateExisting(id, name, sku, price, category, stockQuantity, isActive);
        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to create existing Product: {result.Error.Detail}");
        return result.Value;
    }

    public void Deactivate() => IsActive = false;

    public void UpdateStock(int quantity) => StockQuantity = quantity;

    /// <summary>
    /// Updates product details with validation.
    /// </summary>
    public Result<Product> UpdateDetails(string name, decimal price, string category) =>
        ProductName.TryCreate(name, "name")
            .Combine(Money.TryCreate(price, "USD", "price"))
            .Combine(CategoryName.TryCreate(category, "category"))
            .Tap((productName, productPrice, productCategory) =>
            {
                Name = productName;
                Price = productPrice;
                Category = productCategory;
            })
            .Map(_ => this);
}