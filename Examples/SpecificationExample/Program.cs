// =============================================================================
// Specification Pattern Example with FunctionalDDD
// =============================================================================
// This example demonstrates the Specification Pattern for building type-safe,
// composable, and reusable query specifications with EF Core integration.
//
// Featured Capabilities:
// - Subclass specifications (reusable business rules)
// - Inline specifications (ad-hoc queries with Spec.For<T>)
// - Composition (And, Or, Not)
// - Fluent extensions (Include, OrderBy, Paginate, AsNoTracking)
// - Result pattern integration (FirstOrNotFoundAsync, SingleOrNotFoundAsync)
// - Railway Oriented Programming with specifications
//
// Key Benefits:
// 1. Encapsulation - Query logic lives with the domain, not scattered in services
// 2. Testability   - Specifications can be tested without a database
// 3. Reusability   - Compose complex queries from simple building blocks
// 4. Type Safety   - Works seamlessly with strongly-typed value objects
// =============================================================================

using FunctionalDdd;
using FunctionalDdd.Testing;
using Microsoft.EntityFrameworkCore;
using SpecificationExample.Data;
using SpecificationExample.Entities;
using SpecificationExample.Services;
using SpecificationExample.Specifications;
using SpecificationExample.ValueObjects;

Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║  Specification Pattern Example with FunctionalDDD                ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// Configure EF Core with in-memory database
var options = new DbContextOptionsBuilder<OrderDbContext>()
    .UseInMemoryDatabase("SpecificationExample")
    .Options;

await using var context = new OrderDbContext(options);
var orderService = new OrderService(context);

// =============================================================================
// 1. SEED SAMPLE DATA
// =============================================================================
Console.WriteLine("📦 Seeding Sample Orders...");
Console.WriteLine("────────────────────────────────────────────────────────────────────");

var customerId1 = CustomerId.NewUnique();
var customerId2 = CustomerId.NewUnique();
var customerName1 = CustomerName.Create("Alice Johnson");
var customerName2 = CustomerName.Create("Bob Smith");

var orders = new List<Order>
{
    // Customer 1 Orders
    Order.Create(customerId1, customerName1, 150.00m, OrderStatus.Pending, PaymentStatus.Unpaid),
    Order.Create(customerId1, customerName1, 750.00m, OrderStatus.Confirmed, PaymentStatus.Paid, isPriority: true),
    Order.Create(customerId1, customerName1, 1200.00m, OrderStatus.Processing, PaymentStatus.Paid),
    Order.Create(customerId1, customerName1, 89.99m, OrderStatus.Shipped, PaymentStatus.Paid),
    Order.Create(customerId1, customerName1, 450.00m, OrderStatus.Cancelled, PaymentStatus.Refunded),

    // Customer 2 Orders
    Order.Create(customerId2, customerName2, 2500.00m, OrderStatus.Confirmed, PaymentStatus.Paid, isPriority: true),
    Order.Create(customerId2, customerName2, 199.99m, OrderStatus.Processing, PaymentStatus.Paid),
    Order.Create(customerId2, customerName2, 999.00m, OrderStatus.Pending, PaymentStatus.Unpaid),
    Order.Create(customerId2, customerName2, 50.00m, OrderStatus.Delivered, PaymentStatus.Paid),
};

// Add some order lines
orders[1].Lines.Add(new OrderLine { Id = 1, OrderId = orders[1].Id, ProductName = "MacBook Pro", Quantity = 1, UnitPrice = 750.00m });
orders[2].Lines.Add(new OrderLine { Id = 2, OrderId = orders[2].Id, ProductName = "iPhone 15", Quantity = 1, UnitPrice = 1200.00m });
orders[5].Lines.Add(new OrderLine { Id = 3, OrderId = orders[5].Id, ProductName = "Mac Studio", Quantity = 1, UnitPrice = 2500.00m });

context.Orders.AddRange(orders);
await context.SaveChangesAsync();

Console.WriteLine($"  ✓ Created {orders.Count} orders for 2 customers");
Console.WriteLine();

// =============================================================================
// 2. SIMPLE SPECIFICATION (Subclass)
// =============================================================================
Console.WriteLine("🔍 Using Simple Specifications...");
Console.WriteLine("────────────────────────────────────────────────────────────────────");

// Get orders ready for processing
var readyForProcessing = await context.Orders.ToListAsync(new OrdersReadyForProcessingSpec());
Console.WriteLine($"  Orders ready for processing: {readyForProcessing.Count}");
foreach (var order in readyForProcessing)
{
    Console.WriteLine($"    • {order.CustomerName}: ${order.Total:N2} (Priority: {order.IsPriority})");
}

Console.WriteLine();

// Get high-value orders
var highValue = await context.Orders.ToListAsync(new HighValueOrdersSpec(500m));
Console.WriteLine($"  High-value orders (>$500): {highValue.Count}");
foreach (var order in highValue)
{
    Console.WriteLine($"    • {order.CustomerName}: ${order.Total:N2} ({order.Status})");
}

Console.WriteLine();

// =============================================================================
// 3. INLINE SPECIFICATION (Spec.For)
// =============================================================================
Console.WriteLine("🎯 Using Inline Specifications...");
Console.WriteLine("────────────────────────────────────────────────────────────────────");

// Ad-hoc query for shipped orders
var shippedSpec = Spec.For<Order>(o => o.Status == OrderStatus.Shipped);
var shippedOrders = await context.Orders.ToListAsync(shippedSpec);
Console.WriteLine($"  Shipped orders: {shippedOrders.Count}");

// Match all orders
var allSpec = Spec.All<Order>();
var allOrders = await context.Orders.ToListAsync(allSpec);
Console.WriteLine($"  Total orders: {allOrders.Count}");
Console.WriteLine();

// =============================================================================
// 4. COMPOSITION (And, Or, Not)
// =============================================================================
Console.WriteLine("🔗 Composing Specifications...");
Console.WriteLine("────────────────────────────────────────────────────────────────────");

// High-value AND priority orders
var highValuePriority = new HighValueOrdersSpec(500m).And(new PriorityOrdersSpec());
var hvpOrders = await context.Orders.ToListAsync(highValuePriority);
Console.WriteLine($"  High-value priority orders: {hvpOrders.Count}");
foreach (var order in hvpOrders)
{
    Console.WriteLine($"    • {order.CustomerName}: ${order.Total:N2}");
}

// Confirmed OR Processing orders
var activeSpec = new OrdersByStatusSpec(OrderStatus.Confirmed)
    .Or(new OrdersByStatusSpec(OrderStatus.Processing));
var activeOrders = await context.Orders.ToListAsync(activeSpec);
Console.WriteLine($"  Active orders (confirmed or processing): {activeOrders.Count}");

// NOT cancelled orders
var notCancelled = new CancelledOrdersSpec().Not();
var notCancelledOrders = await context.Orders.ToListAsync(notCancelled);
Console.WriteLine($"  Non-cancelled orders: {notCancelledOrders.Count}");
Console.WriteLine();

// =============================================================================
// 5. FLUENT EXTENSIONS
// =============================================================================
Console.WriteLine("⚡ Using Fluent Extensions...");
Console.WriteLine("────────────────────────────────────────────────────────────────────");

// Build a complex query fluently
var complexSpec = Spec.For<Order>(o => o.Status != OrderStatus.Cancelled)
    .Include(o => o.Lines)
    .OrderByDescending(o => o.Total)
    .Paginate(pageNumber: 1, pageSize: 3)
    .AsNoTracking();

var topOrders = await context.Orders.ToListAsync(complexSpec);
Console.WriteLine($"  Top 3 non-cancelled orders by value:");
foreach (var order in topOrders)
{
    Console.WriteLine($"    • {order.CustomerName}: ${order.Total:N2} ({order.Status}, Lines: {order.Lines.Count})");
}

Console.WriteLine();

// =============================================================================
// 6. RESULT INTEGRATION
// =============================================================================
Console.WriteLine("🎯 Result Pattern Integration...");
Console.WriteLine("────────────────────────────────────────────────────────────────────");

// FirstOrNotFoundAsync - existing order
var existingOrderId = orders[1].Id;
var findResult = await orderService.GetOrderByIdAsync(existingOrderId);
findResult
    .Tap(o => Console.WriteLine($"  ✓ Found order: {o.CustomerName}, ${o.Total:N2}"))
    .TapOnFailure(e => Console.WriteLine($"  ✗ Error: {e.Detail}"));

// FirstOrNotFoundAsync - non-existing order
var fakeOrderId = OrderId.NewUnique();
var notFoundResult = await orderService.GetOrderByIdAsync(fakeOrderId);
notFoundResult
    .Tap(o => Console.WriteLine($"  ✓ Found order: {o.CustomerName}"))
    .TapOnFailure(e => Console.WriteLine($"  ✗ Not found: {e.Detail}"));

// SingleOrNotFoundAsync - conflict detection
var conflictResult = await orderService.GetSingleActiveOrderAsync(customerId1);
conflictResult
    .Tap(o => Console.WriteLine($"  ✓ Single active order: {o.CustomerName}"))
    .TapOnFailure(e => Console.WriteLine($"  ✗ Conflict/Error: {e.Detail}"));

Console.WriteLine();

// =============================================================================
// 7. ROP CHAIN WITH SPECIFICATIONS
// =============================================================================
Console.WriteLine("🚂 Railway Oriented Programming Chain...");
Console.WriteLine("────────────────────────────────────────────────────────────────────");

// Process an order using ROP chain
var confirmedPaidOrderId = orders[1].Id; // Confirmed & Paid order
var processResult = await orderService.ProcessOrderAsync(confirmedPaidOrderId);
processResult
    .Tap(o => Console.WriteLine($"  ✓ Processed order: {o.CustomerName} -> Status: {o.Status}"))
    .TapOnFailure(e => Console.WriteLine($"  ✗ Failed: {e.Detail}"));

// Try to process an already processing order (should fail)
var alreadyProcessingId = orders[2].Id; // Already Processing
var failResult = await orderService.ProcessOrderAsync(alreadyProcessingId);
failResult
    .Tap(o => Console.WriteLine($"  ✓ Processed: {o.CustomerName}"))
    .TapOnFailure(e => Console.WriteLine($"  ✗ Failed: {e.Detail}"));

Console.WriteLine();

// =============================================================================
// 8. TESTING SPECIFICATIONS (Without Database)
// =============================================================================
Console.WriteLine("🧪 Testing Specifications In-Memory...");
Console.WriteLine("────────────────────────────────────────────────────────────────────");

// Create test orders
var testOrders = new[]
{
    Order.Create(customerId1, customerName1, 100m, OrderStatus.Pending),
    Order.Create(customerId1, customerName1, 600m, OrderStatus.Confirmed, PaymentStatus.Paid),
    Order.Create(customerId1, customerName1, 800m, OrderStatus.Processing, PaymentStatus.Paid, isPriority: true),
};

// Test specification without database using IsSatisfiedBy
var highValueSpec = new HighValueOrdersSpec(500m);

Console.WriteLine("  Testing HighValueOrdersSpec (>$500):");
foreach (var order in testOrders)
{
    var satisfied = highValueSpec.IsSatisfiedBy(order);
    var icon = satisfied ? "✓" : "✗";
    Console.WriteLine($"    {icon} ${order.Total:N2} - {(satisfied ? "Matches" : "Does not match")}");
}

// Test composition
var priorityHighValue = new PriorityOrdersSpec().And(highValueSpec);
Console.WriteLine($"  Priority AND High-value orders: {priorityHighValue.Count(testOrders)}");

// Filter in-memory
var filtered = highValueSpec.Filter(testOrders).ToList();
Console.WriteLine($"  Filtered high-value orders: {filtered.Count}");

Console.WriteLine();

// =============================================================================
// 9. SERVICE LAYER USAGE
// =============================================================================
Console.WriteLine("🏢 Service Layer Integration...");
Console.WriteLine("────────────────────────────────────────────────────────────────────");

// Count by status
var pendingCount = await orderService.CountOrdersByStatusAsync(OrderStatus.Pending);
var confirmedCount = await orderService.CountOrdersByStatusAsync(OrderStatus.Confirmed);
var processingCount = await orderService.CountOrdersByStatusAsync(OrderStatus.Processing);

Console.WriteLine($"  Order counts by status:");
Console.WriteLine($"    • Pending: {pendingCount}");
Console.WriteLine($"    • Confirmed: {confirmedCount}");
Console.WriteLine($"    • Processing: {processingCount}");

// Check if customer has pending orders
var hasPending = await orderService.CustomerHasPendingOrdersAsync(customerId1);
Console.WriteLine($"  Customer 1 has pending orders: {hasPending}");

// Get paginated orders
var page1 = await orderService.GetOrdersReadyForProcessingAsync(page: 1, pageSize: 2);
Console.WriteLine($"  Orders ready for processing (page 1, size 2): {page1.Count}");

Console.WriteLine();

// =============================================================================
// SUMMARY
// =============================================================================
Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║  Summary: Specification Pattern Benefits                         ║");
Console.WriteLine("╠══════════════════════════════════════════════════════════════════╣");
Console.WriteLine("║  ✓ Type-safe queries with strongly-typed IDs                    ║");
Console.WriteLine("║  ✓ Composable with And/Or/Not operators                         ║");
Console.WriteLine("║  ✓ Reusable business rules as specification classes             ║");
Console.WriteLine("║  ✓ Fluent API for includes, ordering, pagination                ║");
Console.WriteLine("║  ✓ Result pattern integration for error handling                ║");
Console.WriteLine("║  ✓ Testable without database using IsSatisfiedBy                ║");
Console.WriteLine("║  ✓ Clean separation of query logic from services                ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
