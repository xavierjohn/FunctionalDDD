// =============================================================================
// EF Core Example with FunctionalDDD Primitive Value Objects
// =============================================================================
// This example demonstrates seamless integration of strongly-typed value objects
// with Entity Framework Core using an in-memory database.
//
// Featured Value Objects:
// - RequiredUlid<T>  : OrderId, CustomerId (time-ordered, sortable identifiers)
// - RequiredGuid<T>  : ProductId (traditional GUID-based identifiers)
// - RequiredString<T>: ProductName, CustomerName (non-empty string validation)
// - EmailAddress     : Built-in RFC 5322 validated email addresses
//
// Key Benefits:
// 1. Type Safety     - Cannot mix OrderId with CustomerId
// 2. Validation      - All values validated at creation time
// 3. Clean Entities  - No primitive obsession, expressive domain model
// 4. EF Core Ready   - Simple value converters for database persistence
// =============================================================================

using EfCoreExample.Data;
using EfCoreExample.Entities;
using FunctionalDdd;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║  EF Core Example with FunctionalDDD Primitive Value Objects      ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// Configure EF Core with in-memory database
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase("EfCoreExample")
    .Options;

await using var context = new AppDbContext(options);

// =============================================================================
// 1. CREATE PRODUCTS (Using RequiredGuid and RequiredString)
// =============================================================================
Console.WriteLine("📦 Creating Products...");
Console.WriteLine("────────────────────────────────────────────────────────────────────");

var products = new List<Product>();
var productResults = new[]
{
    Product.TryCreate("MacBook Pro 16\"", 2499.99m, 50),
    Product.TryCreate("iPhone 15 Pro", 1199.99m, 100),
    Product.TryCreate("AirPods Pro", 249.99m, 200)
};

foreach (var result in productResults)
{
    result
        .Tap(product =>
        {
            products.Add(product);
            Console.WriteLine($"  ✓ Created: {product.Name} (ID: {product.Id})");
            Console.WriteLine($"             Price: ${product.Price:N2}, Stock: {product.StockQuantity}");
        })
        .TapOnFailure(error => Console.WriteLine($"  ✗ Failed: {error.Detail}"));
}

context.Products.AddRange(products);
await context.SaveChangesAsync();
Console.WriteLine();

// =============================================================================
// 2. CREATE CUSTOMER (Using RequiredUlid, RequiredString, EmailAddress)
// =============================================================================
Console.WriteLine("👤 Creating Customer...");
Console.WriteLine("────────────────────────────────────────────────────────────────────");

var customerResult = Customer.TryCreate("John Doe", "john.doe@example.com");
Customer customer = null!;

customerResult
    .Tap(c =>
    {
        customer = c;
        context.Customers.Add(c);
        Console.WriteLine($"  ✓ Created: {c.Name}");
        Console.WriteLine($"             ID: {c.Id}");
        Console.WriteLine($"             Email: {c.Email}");
        Console.WriteLine($"             Note: ULID naturally encodes creation time!");
    })
    .TapOnFailure(error => Console.WriteLine($"  ✗ Failed: {error.Detail}"));

await context.SaveChangesAsync();
Console.WriteLine();

// =============================================================================
// 3. DEMONSTRATE VALIDATION (Railway Oriented Programming)
// =============================================================================
Console.WriteLine("🔒 Demonstrating Validation...");
Console.WriteLine("────────────────────────────────────────────────────────────────────");

// Invalid email
Customer.TryCreate("Jane Doe", "not-an-email")
    .Tap(_ => Console.WriteLine("  ✓ Customer created"))
    .TapOnFailure(error => Console.WriteLine($"  ✗ Validation failed: {error.Detail}"));

// Empty name
Customer.TryCreate("", "jane@example.com")
    .Tap(_ => Console.WriteLine("  ✓ Customer created"))
    .TapOnFailure(error => Console.WriteLine($"  ✗ Validation failed: {error.Detail}"));

// Invalid product price
Product.TryCreate("Invalid Product", -10m, 5)
    .Tap(_ => Console.WriteLine("  ✓ Product created"))
    .TapOnFailure(error => Console.WriteLine($"  ✗ Validation failed: {error.Detail}"));

Console.WriteLine();

// =============================================================================
// 4. CREATE ORDER (Using RequiredUlid - time-ordered!)
// =============================================================================
Console.WriteLine("🛒 Creating Order...");
Console.WriteLine("────────────────────────────────────────────────────────────────────");

Order.TryCreate(customer.Id)
    .Bind(order => order.AddLine(products[0], 1)) // MacBook
    .Bind(order => order.AddLine(products[1], 2)) // 2x iPhone
    .Bind(order => order.AddLine(products[2], 3)) // 3x AirPods
    .Bind(order => order.Confirm())
    .Tap(order =>
    {
        context.Orders.Add(order);
        Console.WriteLine($"  ✓ Order Created and Confirmed!");
        Console.WriteLine($"             Order ID: {order.Id}");
        Console.WriteLine($"             Customer: {customer.Name} ({order.CustomerId})");
        Console.WriteLine($"             Status: {order.Status}");
        Console.WriteLine($"             Items: {order.Lines.Count}");
        Console.WriteLine($"             Total: ${order.Total:N2}");
        Console.WriteLine();
        Console.WriteLine("    Order Lines:");
        foreach (var line in order.Lines)
            Console.WriteLine($"      - {line.ProductName} x{line.Quantity} @ ${line.UnitPrice:N2} = ${line.LineTotal:N2}");
    })
    .TapOnFailure(error => Console.WriteLine($"  ✗ Failed: {error.Detail}"));

await context.SaveChangesAsync();
Console.WriteLine();

// =============================================================================
// 5. QUERY DATA (Demonstrating EF Core Integration)
// =============================================================================
Console.WriteLine("🔍 Querying Data from Database...");
Console.WriteLine("────────────────────────────────────────────────────────────────────");

// Query customer by ID (using strongly-typed CustomerId)
var foundCustomer = await context.Customers.FindAsync(customer.Id);
Console.WriteLine($"  Found Customer: {foundCustomer?.Name} ({foundCustomer?.Email})");

// Query all orders with their lines
var orders = await context.Orders
    .Include(o => o.Lines)
    .ToListAsync();

Console.WriteLine($"  Total Orders: {orders.Count}");
foreach (var order in orders)
{
    Console.WriteLine($"    Order {order.Id}:");
    Console.WriteLine($"      Status: {order.Status}, Total: ${order.Total:N2}");
    Console.WriteLine($"      Created: {order.CreatedAt:yyyy-MM-dd HH:mm:ss}");
}

Console.WriteLine();

// =============================================================================
// 6. DEMONSTRATE ULID ORDERING
// =============================================================================
Console.WriteLine("📊 Demonstrating ULID Ordering (Time-based Sortability)...");
Console.WriteLine("────────────────────────────────────────────────────────────────────");

// Create a few more orders to show ULID ordering
for (int i = 0; i < 3; i++)
{
    await Task.Delay(10); // Small delay to ensure different timestamps
    Order.TryCreate(customer.Id)
        .Bind(o => o.AddLine(products[0], 1))
        .Tap(o => context.Orders.Add(o));
}

await context.SaveChangesAsync();

// Query orders sorted by ID (ULIDs sort chronologically!)
var sortedOrders = await context.Orders
    .OrderBy(o => o.Id) // ULID provides natural chronological ordering
    .Select(o => new { o.Id, o.CreatedAt, o.Status })
    .ToListAsync();

Console.WriteLine("  Orders sorted by ULID (natural chronological order):");
foreach (var o in sortedOrders)
{
    Console.WriteLine($"    {o.Id} - Created: {o.CreatedAt:HH:mm:ss.fff} - {o.Status}");
}

Console.WriteLine();

// =============================================================================
// 7. TYPE SAFETY DEMONSTRATION
// =============================================================================
Console.WriteLine("🛡️  Type Safety Demonstration...");
Console.WriteLine("────────────────────────────────────────────────────────────────────");
Console.WriteLine("  The following would NOT compile:");
Console.WriteLine("    // OrderId orderId = CustomerId.NewUnique();  // Error!");
Console.WriteLine("    // ProductId productId = OrderId.NewUnique(); // Error!");
Console.WriteLine("    // context.Customers.Find(orderId);           // Error!");
Console.WriteLine();
Console.WriteLine("  This is the power of strongly-typed value objects:");
Console.WriteLine("    - Cannot accidentally mix OrderId with CustomerId");
Console.WriteLine("    - Cannot pass ProductId where CustomerId is expected");
Console.WriteLine("    - Compile-time safety prevents runtime bugs");
Console.WriteLine();

// =============================================================================
// SUMMARY
// =============================================================================
Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║  Summary                                                          ║");
Console.WriteLine("╠══════════════════════════════════════════════════════════════════╣");
Console.WriteLine("║  ✓ RequiredUlid<T>   - Time-ordered, sortable identifiers        ║");
Console.WriteLine("║  ✓ RequiredGuid<T>   - Traditional GUID identifiers              ║");
Console.WriteLine("║  ✓ RequiredString<T> - Non-empty string validation               ║");
Console.WriteLine("║  ✓ EmailAddress      - RFC 5322 email validation                 ║");
Console.WriteLine("║  ✓ EF Core           - Seamless persistence with converters      ║");
Console.WriteLine("║  ✓ ROP               - Railway Oriented Programming for errors   ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");

return 0;
