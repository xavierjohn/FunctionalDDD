using EcommerceExample.Aggregates;
using EcommerceExample.Services;
using EcommerceExample.ValueObjects;
using EcommerceExample.Workflows;
using FunctionalDdd;

namespace EcommerceExample;

/// <summary>
/// Demonstrates a complete e-commerce order processing system using Railway Oriented Programming.
/// 
/// This example showcases:
/// - Value Objects (Money, OrderId, ProductId, CustomerId)
/// - Entities (OrderLine)
/// - Aggregates (Order)
/// - Domain Services (PaymentService, InventoryService, NotificationService)
/// - Complex Workflows with error handling
/// - Compensation and retry logic
/// - Async operations
/// </summary>
public class EcommerceExamples
{
    public static async Task RunExamplesAsync()
    {
        Console.WriteLine("=== E-Commerce Order Processing Examples ===\n");

        await Example1_SimpleOrderCreation();
        await Example2_CompleteOrderWorkflow();
        await Example3_PaymentFailureWithCompensation();
        await Example4_InsufficientInventory();
    }

    /// <summary>
    /// Example 1: Simple order creation with validation.
    /// </summary>
    private static async Task Example1_SimpleOrderCreation()
    {
        Console.WriteLine("Example 1: Simple Order Creation");
        Console.WriteLine("----------------------------------");

        var customerId = CustomerId.NewUnique();
        var productId = ProductId.NewUnique();

        var result = Order.TryCreate(customerId)
            .Bind(order => Money.TryCreate(29.99m)
                .Bind(price => order.AddLine(productId, "Wireless Mouse", price, 2)))
            .Bind(order => order.Submit())
            .Match(
                onSuccess: ok => $"? Order created successfully with {ok.Lines.Count} items. Total: {ok.Total}",
                onFailure: err => $"? Order creation failed: {err.Detail}"
            );

        Console.WriteLine(result);
        Console.WriteLine();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Example 2: Complete order workflow with payment and inventory management.
    /// </summary>
    private static async Task Example2_CompleteOrderWorkflow()
    {
        Console.WriteLine("Example 2: Complete Order Workflow");
        Console.WriteLine("-----------------------------------");

        var paymentService = new PaymentService();
        var inventoryService = new InventoryService();
        var notificationService = new NotificationService();
        var workflow = new OrderWorkflow(paymentService, inventoryService, notificationService);

        var customerId = CustomerId.NewUnique();
        var stock = inventoryService.GetAllStock();
        var product1 = stock.Keys.First();
        var product2 = stock.Keys.Skip(1).First();

        var items = new List<OrderLineRequest>
        {
            new(product1, "Laptop", Money.TryCreate(999.99m).Value, 1),
            new(product2, "Mouse", Money.TryCreate(29.99m).Value, 2)
        };

        var paymentInfo = new PaymentInfo("4111111111111111", "123", "John Doe");

        var result = await workflow.ProcessOrderAsync(customerId, items, paymentInfo)
            .MatchAsync(
                onSuccess: ok => $"? Order {ok.Id} processed successfully! Status: {ok.Status}, Total: {ok.Total}",
                onFailure: err => $"? Order processing failed: {err.Detail}"
            );

        Console.WriteLine(result);
        Console.WriteLine();
    }

    /// <summary>
    /// Example 3: Payment failure with compensation (retry logic).
    /// </summary>
    private static async Task Example3_PaymentFailureWithCompensation()
    {
        Console.WriteLine("Example 3: Payment Failure with Compensation");
        Console.WriteLine("---------------------------------------------");

        var paymentService = new PaymentService();
        var inventoryService = new InventoryService();
        var notificationService = new NotificationService();
        var workflow = new OrderWorkflow(paymentService, inventoryService, notificationService);

        var customerId = CustomerId.NewUnique();
        var stock = inventoryService.GetAllStock();
        var product1 = stock.Keys.First();

        var items = new List<OrderLineRequest>
        {
            new(product1, "Gaming Console", Money.TryCreate(499.99m).Value, 1)
        };

        // Card ending in 0000 will be declined
        var paymentInfo = new PaymentInfo("4111111111110000", "123", "Jane Doe");

        var result = await workflow.ProcessOrderAsync(customerId, items, paymentInfo)
            .MatchAsync(
                onSuccess: ok => $"? Order processed: {ok.Status}",
                onFailure: err => $"? Expected failure - Payment declined: {err.Detail}"
            );

        Console.WriteLine(result);
        Console.WriteLine();
    }

    /// <summary>
    /// Example 4: Insufficient inventory with compensation.
    /// </summary>
    private static async Task Example4_InsufficientInventory()
    {
        Console.WriteLine("Example 4: Insufficient Inventory");
        Console.WriteLine("----------------------------------");

        var paymentService = new PaymentService();
        var inventoryService = new InventoryService();
        var notificationService = new NotificationService();
        var workflow = new OrderWorkflow(paymentService, inventoryService, notificationService);

        var customerId = CustomerId.NewUnique();
        var stock = inventoryService.GetAllStock();
        var product1 = stock.Keys.First();

        // Try to order more than available
        var items = new List<OrderLineRequest>
        {
            new(product1, "Popular Item", Money.TryCreate(99.99m).Value, 1000)
        };

        var paymentInfo = new PaymentInfo("4111111111111111", "123", "Bob Smith");

        var result = await workflow.ProcessOrderAsync(customerId, items, paymentInfo)
            .MatchAsync(
                onSuccess: ok => $"? Order processed: {ok.Status}",
                onFailure: err => $"? Expected failure - Insufficient inventory: {err.Detail}"
            );

        Console.WriteLine(result);
        Console.WriteLine();
    }
}
