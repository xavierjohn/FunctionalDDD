using FunctionalDdd;
using Microsoft.EntityFrameworkCore;
using SpecificationExample.Domain;
using SpecificationExample.Infrastructure;
using SpecificationExample.Specifications;

Console.WriteLine("╔═══════════════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║     FunctionalDDD + Ardalis.Specification Integration Example             ║");
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
Console.WriteLine("  1. SingleOrNotFoundAsync - Query by Unique SKU");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");

// Find existing product by SKU
var laptopResult = await repository.SingleOrNotFoundAsync(
    new ProductBySkuSpec("LAPTOP-001"),
    entityName: "Laptop");

Console.WriteLine($"  Query: ProductBySkuSpec('LAPTOP-001')");
Console.WriteLine($"  Result: {(laptopResult.IsSuccess ? $"Found '{laptopResult.Value.Name}' - ${laptopResult.Value.Price}" : laptopResult.Error.Detail)}");
Console.WriteLine();

// Find non-existing product by SKU
var notFoundResult = await repository.SingleOrNotFoundAsync(
    new ProductBySkuSpec("NONEXISTENT-SKU"),
    entityName: "Product");

Console.WriteLine($"  Query: ProductBySkuSpec('NONEXISTENT-SKU')");
Console.WriteLine($"  Result: {(notFoundResult.IsSuccess ? "Found" : $"Error: {notFoundResult.Error.Detail}")}");
Console.WriteLine();

Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
Console.WriteLine("  2. FirstOrNotFoundAsync - Query First Match");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");

// Get first electronics product
var firstElectronicsResult = await repository.FirstOrNotFoundAsync(
    new ActiveProductsByCategorySpec("Electronics"),
    entityName: "Electronics product");

Console.WriteLine($"  Query: ActiveProductsByCategorySpec('Electronics')");
Console.WriteLine($"  Result: {(firstElectronicsResult.IsSuccess ? $"Found '{firstElectronicsResult.Value.Name}'" : firstElectronicsResult.Error.Detail)}");
Console.WriteLine();

Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
Console.WriteLine("  3. ToListAsync - Query Multiple Products");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");

// Get all electronics products
var electronicsList = await repository.ToListAsync(new ActiveProductsByCategorySpec("Electronics"));

Console.WriteLine($"  Query: ActiveProductsByCategorySpec('Electronics')");
Console.WriteLine($"  Found {electronicsList.Count} products:");
foreach (var product in electronicsList)
    Console.WriteLine($"    - {product.Name} ({product.Sku}) - ${product.Price}");
Console.WriteLine();

// Get low stock products
var lowStockProducts = await repository.ToListAsync(new LowStockProductsSpec(15));

Console.WriteLine($"  Query: LowStockProductsSpec(threshold: 15)");
Console.WriteLine($"  Found {lowStockProducts.Count} low-stock products:");
foreach (var product in lowStockProducts)
    Console.WriteLine($"    - {product.Name}: {product.StockQuantity} units");
Console.WriteLine();

Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
Console.WriteLine("  4. AnyAsync - Check Existence");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");

var hasElectronics = await repository.AnyAsync(new ActiveProductsByCategorySpec("Electronics"));
var hasGaming = await repository.AnyAsync(new ActiveProductsByCategorySpec("Gaming"));

Console.WriteLine($"  AnyAsync(ActiveProductsByCategorySpec('Electronics')): {hasElectronics}");
Console.WriteLine($"  AnyAsync(ActiveProductsByCategorySpec('Gaming')): {hasGaming}");
Console.WriteLine();

Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
Console.WriteLine("  5. ROP Chaining - Fluent Query Pipelines");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");

// Chain operations: Find product -> Validate -> Transform
var pipelineResult = await repository
    .SingleOrNotFoundAsync(new ProductBySkuSpec("LAPTOP-001"))
    .EnsureAsync(p => p.IsActive, Error.Validation("Product is not active"))
    .EnsureAsync(p => p.StockQuantity > 0, Error.Validation("Product is out of stock"))
    .MapAsync(p => new ProductDto(p.Id, p.Name, p.Price, p.StockQuantity));

Console.WriteLine($"  Pipeline: SingleOrNotFoundAsync -> Ensure(IsActive) -> Ensure(InStock) -> Map");
Console.WriteLine($"  Result: {(pipelineResult.IsSuccess ? $"Success: {pipelineResult.Value}" : $"Error: {pipelineResult.Error.Detail}")}");
Console.WriteLine();

// Pipeline that fails validation
var outOfStockResult = await repository
    .SingleOrNotFoundAsync(new ProductBySkuSpec("PHONE-002"))
    .EnsureAsync(p => p.StockQuantity > 20, Error.Validation("Insufficient stock for bulk order"))
    .MapAsync(p => new ProductDto(p.Id, p.Name, p.Price, p.StockQuantity));

Console.WriteLine($"  Pipeline: SingleOrNotFoundAsync('PHONE-002') -> Ensure(Stock > 20) -> Map");
Console.WriteLine($"  Result: {(outOfStockResult.IsSuccess ? $"Success" : $"Error: {outOfStockResult.Error.Detail}")}");
Console.WriteLine();

Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
Console.WriteLine("  6. Complex Queries with Price Range");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");

var affordableProducts = await repository.ToListAsync(new ProductsByPriceRangeSpec(100m, 500m));

Console.WriteLine($"  Query: ProductsByPriceRangeSpec($100 - $500)");
Console.WriteLine($"  Found {affordableProducts.Count} products:");
foreach (var product in affordableProducts)
    Console.WriteLine($"    - {product.Name}: ${product.Price}");
Console.WriteLine();

Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
Console.WriteLine("  7. Error Handling Examples");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");

// Demonstrate Match for error handling
var matchResult = await repository
    .SingleOrNotFoundAsync(new ProductBySkuSpec("MISSING-SKU"))
    .MatchAsync(
        onSuccess: p => $"Found product: {p.Name}",
        onFailure: e => $"Query failed: {e.Detail}");

Console.WriteLine($"  Match pattern on failure:");
Console.WriteLine($"    {matchResult}");
Console.WriteLine();

// Demonstrate TapOnFailure for logging
await repository
    .SingleOrNotFoundAsync(new ProductBySkuSpec("ALSO-MISSING"))
    .TapOnFailureAsync(error => Console.WriteLine($"  TapOnFailure: Logged error - {error.Detail}"));

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

// DTO for demonstration
internal record ProductDto(Guid Id, string Name, decimal Price, int Stock);
