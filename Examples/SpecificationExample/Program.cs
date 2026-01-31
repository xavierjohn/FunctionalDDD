using FunctionalDdd;
using FunctionalDdd.PrimitiveValueObjects;
using Microsoft.EntityFrameworkCore;
using SpecificationExample.Domain;
using SpecificationExample.Infrastructure;
using SpecificationExample.Specifications;

Console.WriteLine("╔═══════════════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║  FunctionalDDD + Ardalis.Specification + Value Objects Integration        ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// Setup in-memory database
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase("SpecificationExample")
    .Options;

await using var context = new AppDbContext(options);
var repository = new EfRepository<Product>(context);

// Seed sample data
await SeedData(repository);

Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
Console.WriteLine("  1. Value Object Type Safety - Specifications Accept Value Objects");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");

// Create value objects for type-safe queries
var laptopSku = Sku.Create("LAPTOP-001");
Console.WriteLine($"  Created Sku value object: {laptopSku}");

// Query using the value object - type-safe!
var laptopResult = await repository.SingleOrNotFoundAsync(
    new ProductBySkuSpec(laptopSku),  // ← Accepts Sku, not string!
    entityName: "Laptop");

Console.WriteLine($"  Query: ProductBySkuSpec(Sku.Create('LAPTOP-001'))");
Console.WriteLine($"  Result: {(laptopResult.IsSuccess ? $"Found '{laptopResult.Value.Name}' - {laptopResult.Value.Price}" : laptopResult.Error.Detail)}");
Console.WriteLine();

// Compiler prevents passing wrong types!
// var wrongResult = await repository.SingleOrNotFoundAsync(
//     new ProductBySkuSpec("LAPTOP-001")); // ❌ Compile error! Expects Sku, not string

Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
Console.WriteLine("  2. Validate-Then-Query Pattern with Result<T>");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");

// User input simulation
string userInput = "PHONE-001";

// Validate input → Create value object → Query → Transform
var skuResult = Sku.TryCreate(userInput, "sku");
if (skuResult.IsSuccess)
{
    var queryPipeline = await repository
        .SingleOrNotFoundAsync(new ProductBySkuSpec(skuResult.Value))
        .EnsureAsync(p => p.IsActive, Error.Validation("Product is not active"))
        .MapAsync(p => new ProductDto(p.Id.Value, p.Name.Value, p.Price.Amount, p.StockQuantity));

    Console.WriteLine($"  Input: '{userInput}'");
    Console.WriteLine($"  Pipeline: Sku.TryCreate → SingleOrNotFoundAsync → Ensure(IsActive) → Map");
    Console.WriteLine($"  Result: {(queryPipeline.IsSuccess ? $"Success: {queryPipeline.Value}" : $"Error: {queryPipeline.Error.Detail}")}");
}

Console.WriteLine();

// Invalid input - fails at validation
string invalidInput = "";
var invalidSkuResult = Sku.TryCreate(invalidInput, "sku");
Console.WriteLine($"  Input: '' (empty)");
Console.WriteLine($"  Result: {(invalidSkuResult.IsSuccess ? "Success" : $"Error: {invalidSkuResult.Error.Detail}")}");
Console.WriteLine();

Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
Console.WriteLine("  3. Category Queries with Value Objects");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");

var electronicsCategory = CategoryName.Create("Electronics");
var electronicsList = await repository.ToListAsync(new ActiveProductsByCategorySpec(electronicsCategory));

Console.WriteLine($"  Query: ActiveProductsByCategorySpec(CategoryName.Create('Electronics'))");
Console.WriteLine($"  Found {electronicsList.Count} products:");
foreach (var product in electronicsList)
    Console.WriteLine($"    - {product.Name} ({product.Sku}) - {product.Price}");

Console.WriteLine();

Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
Console.WriteLine("  4. Price Range Queries with Money Value Object");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");

var minPrice = Money.Create(100m, "USD");
var maxPrice = Money.Create(500m, "USD");
var affordableProducts = await repository.ToListAsync(new ProductsByPriceRangeSpec(minPrice, maxPrice));

Console.WriteLine($"  Query: ProductsByPriceRangeSpec(Money.Create(100, 'USD'), Money.Create(500, 'USD'))");
Console.WriteLine($"  Found {affordableProducts.Count} products:");
foreach (var product in affordableProducts)
    Console.WriteLine($"    - {product.Name}: {product.Price}");

Console.WriteLine();

Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
Console.WriteLine("  5. ProductId - Type-Safe Entity References");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");

// Get a product ID from the first query
if (laptopResult.IsSuccess)
{
    var productId = laptopResult.Value.Id;  // ProductId, not Guid!
    Console.WriteLine($"  ProductId: {productId}");

    // Query by ID - type-safe!
    var byIdResult = await repository.SingleOrNotFoundAsync(
        new ProductByIdSpec(productId),  // ← Accepts ProductId, not Guid!
        entityName: "Product");

    Console.WriteLine($"  Query: ProductByIdSpec(productId)");
    Console.WriteLine($"  Result: {(byIdResult.IsSuccess ? $"Found '{byIdResult.Value.Name}'" : byIdResult.Error.Detail)}");
}

Console.WriteLine();

Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
Console.WriteLine("  6. Low Stock Query - Mixing Value Objects and Primitives");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");

var lowStockProducts = await repository.ToListAsync(new LowStockProductsSpec(15));

Console.WriteLine($"  Query: LowStockProductsSpec(threshold: 15)");
Console.WriteLine($"  Found {lowStockProducts.Count} low-stock products:");
foreach (var product in lowStockProducts)
    Console.WriteLine($"    - {product.Name}: {product.StockQuantity} units @ {product.Price}");

Console.WriteLine();

Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
Console.WriteLine("  7. ROP Chaining with Repository Extensions");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");

// Complex pipeline: Query → Business rules → Transform
var orderSku = Sku.Create("LAPTOP-001");
var createOrderPipeline = await repository
    .SingleOrNotFoundAsync(new ProductBySkuSpec(orderSku))
    .EnsureAsync(p => p.IsActive, Error.Validation("Product is not active"))
    .EnsureAsync(p => p.StockQuantity >= 2, Error.Validation("Insufficient stock"))
    .MapAsync(p => new OrderLineDto(p.Id.Value, p.Name.Value, 2, p.Price.Amount * 2));

Console.WriteLine($"  Pipeline: SingleOrNotFoundAsync → Ensure(Active) → Ensure(Stock >= 2) → Map");
Console.WriteLine($"  Result: {(createOrderPipeline.IsSuccess ? $"Success: {createOrderPipeline.Value}" : $"Error: {createOrderPipeline.Error.Detail}")}");
Console.WriteLine();

Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
Console.WriteLine("  8. Error Handling Examples");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");

// Non-existent product
var notFoundSku = Sku.Create("NONEXISTENT");
var notFoundResult = await repository
    .SingleOrNotFoundAsync(new ProductBySkuSpec(notFoundSku))
    .MatchAsync(
        onSuccess: p => $"Found: {p.Name}",
        onFailure: e => $"Failed: [{e.Code}] {e.Detail}");

Console.WriteLine($"  Missing product → {notFoundResult}");
Console.WriteLine();

Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
Console.WriteLine("  Benefits of Value Objects with Ardalis.Specification:");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
Console.WriteLine("  ✓ Type safety - compiler prevents passing wrong types");
Console.WriteLine("  ✓ Validation - invalid data caught before query execution");
Console.WriteLine("  ✓ Self-documenting - ProductBySkuSpec(Sku) vs ProductBySkuSpec(string)");
Console.WriteLine("  ✓ Refactoring - rename ProductId and compiler finds all usages");
Console.WriteLine("  ✓ Domain logic - Money knows about currency, Sku has format rules");
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
Console.WriteLine("  Example Complete!");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");

// Helper method to seed data
static async Task SeedData(EfRepository<Product> repository)
{
    var products = new[]
    {
        Product.Create("Gaming Laptop Pro", "LAPTOP-001", 1299.99m, "Electronics", stockQuantity: 25),
        Product.Create("Wireless Mouse", "MOUSE-001", 49.99m, "Electronics", stockQuantity: 150),
        Product.Create("Mechanical Keyboard", "KEYBOARD-001", 129.99m, "Electronics", stockQuantity: 8),
        Product.Create("USB-C Hub", "HUB-001", 79.99m, "Electronics", stockQuantity: 45),
        Product.Create("Smartphone X", "PHONE-001", 899.99m, "Electronics", stockQuantity: 30),
        Product.Create("Smartphone Y", "PHONE-002", 699.99m, "Electronics", stockQuantity: 12),
        Product.Create("Ergonomic Chair", "CHAIR-001", 399.99m, "Furniture", stockQuantity: 5),
        Product.Create("Standing Desk", "DESK-001", 549.99m, "Furniture", stockQuantity: 3),
    };

    foreach (var product in products)
        await repository.AddAsync(product);

    await repository.SaveChangesAsync();
}

// DTOs for demonstration
internal record ProductDto(Guid Id, string Name, decimal Price, int Stock);
internal record OrderLineDto(Guid ProductId, string ProductName, int Quantity, decimal Total);