using EcommerceExample.Aggregates;
using EcommerceExample.Services;
using EcommerceExample.ValueObjects;
using EcommerceExample.Workflows;
using FunctionalDdd;

namespace EcommerceExample;

/// <summary>
/// Demonstrates a complete e-commerce order processing system using Railway Oriented Programming
/// and Domain-Driven Design patterns.
/// 
/// This example showcases:
/// - Value Objects (Money, OrderId, ProductId, CustomerId)
/// - Entities (OrderLine)
/// - Aggregates with Domain Events (Order)
/// - Domain Services (PaymentService, InventoryService, NotificationService)
/// - Complex Workflows with error handling
/// - recovery and retry logic
/// - Domain Events and Change Tracking (UncommittedEvents, AcceptChanges)
/// - Various Error Types (Validation, Domain, Conflict, NotFound, Unexpected)
/// </summary>
public static class EcommerceExamples
{
    public static async Task RunExamplesAsync()
    {
        Console.WriteLine("=== E-Commerce Order Processing Examples ===");
        Console.WriteLine("=== Demonstrating FunctionalDDD Library ===\n");

        await Example1_SimpleOrderCreation();
        await Example2_CompleteOrderWorkflow();
        await Example3_PaymentFailureWithrecovery();
        await Example4_InsufficientInventory();
        await Example5_DomainEventsAndChangeTracking();
    }

    /// <summary>
    /// Example 1: Simple order creation with validation and domain events.
    /// Demonstrates: Aggregate creation, domain events, ROP chaining
    /// </summary>
    private static async Task Example1_SimpleOrderCreation()
    {
        Console.WriteLine("Example 1: Simple Order Creation with Domain Events");
        Console.WriteLine("----------------------------------------------------");

        var customerId = CustomerId.NewUnique();
        var productId = ProductId.NewUnique();

        var result = Order.TryCreate(customerId)
            .Tap(order => Console.WriteLine($"Order created with {order.UncommittedEvents().Count} uncommitted event(s)"))
            .Bind(order => Money.TryCreate(29.99m, "USD")
                .Bind(price => order.AddLine(productId, "Wireless Mouse", price, 2)))
            .Tap(order => Console.WriteLine($"After adding line: {order.UncommittedEvents().Count} uncommitted event(s)"))
            .Bind(order => order.Submit())
            .Tap(order =>
            {
                Console.WriteLine($"After submit: {order.UncommittedEvents().Count} uncommitted event(s)");
                Console.WriteLine($"IsChanged: {order.IsChanged}");

                // Simulate saving to repository
                order.AcceptChanges();
                Console.WriteLine($"After AcceptChanges: {order.UncommittedEvents().Count} uncommitted event(s)");
            })
            .Match(
                onSuccess: ok => $"✅ Order created successfully with {ok.Lines.Count} items. Total: {ok.Total}",
                onFailure: err => $"❌ Order creation failed: {err.Detail}"
            );

        Console.WriteLine(result);
        Console.WriteLine();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Example 2: Complete order workflow with payment, inventory, and event publishing.
    /// Demonstrates: Workflow orchestration, event publishing pattern
    /// </summary>
    private static async Task Example2_CompleteOrderWorkflow()
    {
        Console.WriteLine("Example 2: Complete Order Workflow with Event Publishing");
        Console.WriteLine("---------------------------------------------------------");

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
            new(product1, "Laptop", Money.Create(999.99m, "USD"), 1),
            new(product2, "Mouse", Money.Create(29.99m, "USD"), 2)
        };

        var paymentInfo = new PaymentInfo("4111111111111111", "123", "John Doe");

        var result = await workflow.ProcessOrderAsync(customerId, items, paymentInfo)
            .MatchAsync(
                onSuccess: ok => $"✅ Order {ok.Id} processed successfully! Status: {ok.Status}, Total: {ok.Total}",
                onFailure: err => $"❌ Order processing failed: {err.Detail}"
            );

        Console.WriteLine(result);
        Console.WriteLine();
    }

    /// <summary>
    /// Example 3: Payment failure with recovery (retry logic).
    /// Demonstrates: RecoverOnFailureAsync, TapErrorAsync, error recovery
    /// </summary>
    private static async Task Example3_PaymentFailureWithrecovery()
    {
        Console.WriteLine("Example 3: Payment Failure with recovery");
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
            new(product1, "Gaming Console", Money.Create(499.99m, "USD"), 1)
        };

        // Card ending in 0000 will be declined
        var paymentInfo = new PaymentInfo("4111111111110000", "123", "Jane Doe");

        var result = await workflow.ProcessOrderAsync(customerId, items, paymentInfo)
            .MatchAsync(
                onSuccess: ok => $"✅ Order processed: {ok.Status}",
                onFailure: err => $"⚠️ Expected failure - Payment declined: {err.Detail}"
            );

        Console.WriteLine(result);
        Console.WriteLine();
    }

    /// <summary>
    /// Example 4: Insufficient inventory with recovery.
    /// Demonstrates: Error.Domain for business rule violations
    /// </summary>
    private static async Task Example4_InsufficientInventory()
    {
        Console.WriteLine("Example 4: Insufficient Inventory (Domain Error)");
        Console.WriteLine("-------------------------------------------------");

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
            new(product1, "Popular Item", Money.Create(99.99m, "USD"), 1000)
        };

        var paymentInfo = new PaymentInfo("4111111111111111", "123", "Bob Smith");

        var result = await workflow.ProcessOrderAsync(customerId, items, paymentInfo)
            .MatchAsync(
                onSuccess: ok => $"✅ Order processed: {ok.Status}",
                onFailure: err => $"⚠️ Expected failure - Insufficient inventory: {err.Detail}"
            );

        Console.WriteLine(result);
        Console.WriteLine();
    }

    /// <summary>
    /// Example 5: Deep dive into domain events and change tracking.
    /// Demonstrates: UncommittedEvents, AcceptChanges, IsChanged, event inspection
    /// </summary>
    private static async Task Example5_DomainEventsAndChangeTracking()
    {
        Console.WriteLine("Example 5: Domain Events and Change Tracking");
        Console.WriteLine("---------------------------------------------");

        var customerId = CustomerId.NewUnique();
        var productId1 = ProductId.NewUnique();
        var productId2 = ProductId.NewUnique();

        // Create order - this raises OrderCreatedEvent
        var orderResult = Order.TryCreate(customerId);

        if (orderResult.IsFailure)
        {
            Console.WriteLine($"❌ Order creation failed: {orderResult.Error.Detail}");
            return;
        }

        var order = orderResult.Value;

        Console.WriteLine("After order creation:");
        Console.WriteLine($"  IsChanged: {order.IsChanged}");
        Console.WriteLine($"  Uncommitted events: {order.UncommittedEvents().Count}");
        PrintEvents(order);

        // Add lines - raises OrderLineAddedEvent for each
        var price1 = Money.Create(99.99m, "USD");
        var price2 = Money.Create(149.99m, "USD");

        order.AddLine(productId1, "Keyboard", price1, 1);
        order.AddLine(productId2, "Monitor", price2, 2);

        Console.WriteLine("\nAfter adding 2 lines:");
        Console.WriteLine($"  IsChanged: {order.IsChanged}");
        Console.WriteLine($"  Uncommitted events: {order.UncommittedEvents().Count}");
        Console.WriteLine($"  Order total: {order.Total}");
        PrintEvents(order);

        // Submit order - raises OrderSubmittedEvent
        order.Submit();

        Console.WriteLine("\nAfter submit:");
        Console.WriteLine($"  IsChanged: {order.IsChanged}");
        Console.WriteLine($"  Uncommitted events: {order.UncommittedEvents().Count}");
        Console.WriteLine($"  Order status: {order.Status}");
        PrintEvents(order);

        // Simulate saving to repository - accept changes
        Console.WriteLine("\nSimulating repository save (AcceptChanges)...");
        order.AcceptChanges();

        Console.WriteLine("\nAfter AcceptChanges:");
        Console.WriteLine($"  IsChanged: {order.IsChanged}");
        Console.WriteLine($"  Uncommitted events: {order.UncommittedEvents().Count}");

        // Process payment and confirm - new events after save
        order.ProcessPayment("TXN-12345");
        order.Confirm();

        Console.WriteLine("\nAfter payment and confirmation:");
        Console.WriteLine($"  IsChanged: {order.IsChanged}");
        Console.WriteLine($"  Uncommitted events: {order.UncommittedEvents().Count}");
        Console.WriteLine($"  Order status: {order.Status}");
        PrintEvents(order);

        // Demonstrate error types
        Console.WriteLine("\n--- Demonstrating Error Types ---");

        // Try to add line to confirmed order (Conflict error)
        var addResult = order.AddLine(productId1, "Another Item", price1, 1);
        if (addResult.IsFailure)
        {
            Console.WriteLine($"Error Type: {addResult.Error.GetType().Name}");
            Console.WriteLine($"Code: {addResult.Error.Code}");
            Console.WriteLine($"Detail: {addResult.Error.Detail}");
        }

        // Try to cancel confirmed order (Domain error)
        var cancelResult = order.Cancel("Changed my mind");
        if (cancelResult.IsFailure)
        {
            Console.WriteLine($"\nError Type: {cancelResult.Error.GetType().Name}");
            Console.WriteLine($"Code: {cancelResult.Error.Code}");
            Console.WriteLine($"Detail: {cancelResult.Error.Detail}");
        }

        Console.WriteLine();
        await Task.CompletedTask;
    }

    private static void PrintEvents(Order order)
    {
        var events = order.UncommittedEvents();
        foreach (var evt in events)
        {
            Console.WriteLine($"    - {evt.GetType().Name} at {evt.OccurredAt:HH:mm:ss.fff}");
        }
    }
}
