namespace EcommerceExample.Workflows;

using EcommerceExample.Aggregates;
using EcommerceExample.Entities;
using EcommerceExample.Services;
using EcommerceExample.ValueObjects;
using FunctionalDdd;

/// <summary>
/// Demonstrates a complete order processing workflow using Railway Oriented Programming.
/// This showcases how complex business workflows can be composed using Result, Bind, Ensure, Tap, and Compensate.
/// </summary>
public class OrderWorkflow
{
    private readonly PaymentService _paymentService;
    private readonly InventoryService _inventoryService;
    private readonly NotificationService _notificationService;

    public OrderWorkflow(
        PaymentService paymentService,
        InventoryService inventoryService,
        NotificationService notificationService)
    {
        _paymentService = paymentService;
        _inventoryService = inventoryService;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Complete order processing workflow with error handling and compensations.
    /// Demonstrates: Bind, Ensure, Tap, BindAsync, TapAsync, CompensateAsync, Finally
    /// </summary>
    public async Task<Result<Order>> ProcessOrderAsync(
        CustomerId customerId,
        List<OrderLineRequest> items,
        PaymentInfo paymentInfo,
        CancellationToken cancellationToken = default)
    {
        Order? currentOrder = null;
        var ct = cancellationToken;
        
        return await Order.TryCreate(customerId)
            .TapAsync(Task (order) =>
            {
                currentOrder = order;
                return _notificationService.SendOrderCreatedEmailAsync(customerId, order.Id, ct);
            })
            .BindAsync(order => AddItemsToOrderAsync(order, items, ct))
            .BindAsync(async order =>
            {
                var reserveResult = await ReserveInventoryAsync(order, ct)
                    .CompensateAsync(
                        predicate: error => error is ValidationError,
                        funcAsync: () => SuggestAlternativeProductsAsync(order, ct));

                if (reserveResult.IsFailure)
                {
                    order.Cancel("Inventory unavailable");
                    return reserveResult.Error;
                }
                
                return Result.Success(order);
            })
            .BindAsync(async order =>
            {
                var submitResult = order.Submit();
                if (submitResult.IsFailure && currentOrder != null)
                {
                    await ReleaseInventoryAsync(currentOrder, ct);
                }
                
                return submitResult;
            })
            .BindAsync(async order =>
            {
                var paymentResult = await ProcessPaymentWithCompensationAsync(order, paymentInfo, ct);
                
                if (paymentResult.IsFailure)
                {
                    await ReleaseInventoryAsync(order, ct);
                    order.Cancel("Payment failed");
                    await _notificationService.SendPaymentFailedEmailAsync(customerId, order.Id, ct);
                    return paymentResult.Error;
                }
                
                return Result.Success(order);
            })
            .BindAsync(order => Task.FromResult(order.Confirm()))
            .TapAsync(Task (order) => _notificationService.SendOrderConfirmedEmailAsync(customerId, order.Id, ct));
    }

    /// <summary>
    /// Demonstrates parallel validation of multiple order lines using TraverseAsync.
    /// Note: For validating collections, TraverseAsync is more appropriate than ParallelAsync.
    /// ParallelAsync is best for 2-9 independent operations that need to run concurrently.
    /// </summary>
    private async Task<Result<Order>> AddItemsToOrderAsync(
        Order order,
        List<OrderLineRequest> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
            return Error.Validation("Order must contain at least one item");

        // Validate all items - errors are automatically aggregated
        var validationResult = await items.TraverseAsync(
            (item, ct) => Task.FromResult(_inventoryService.CheckAvailability(item.ProductId, item.Quantity)),
            cancellationToken
        );

        // If any validation failed, return aggregated errors
        if (validationResult.IsFailure)
            return validationResult.Error;

        // Add all items to order
        var currentOrder = Result.Success(order);
        foreach (var item in items)
        {
            currentOrder = currentOrder.Bind(o =>
                o.AddLine(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity)
            );

            if (currentOrder.IsFailure)
                return currentOrder;
        }

        return currentOrder;
    }

    /// <summary>
    /// Reserves inventory for all order lines.
    /// </summary>
    private async Task<Result<Unit>> ReserveInventoryAsync(Order order, CancellationToken cancellationToken)
    {
        foreach (var line in order.Lines)
        {
            var reserveResult = await _inventoryService.ReserveStockAsync(
                line.Id,
                line.Quantity,
                cancellationToken
            );

            if (reserveResult.IsFailure)
            {
                // Release any previously reserved items
                await ReleaseInventoryAsync(order, cancellationToken);
                return reserveResult;
            }
        }

        return Result.Success();
    }

    /// <summary>
    /// Releases all reserved inventory for an order.
    /// </summary>
    private async Task ReleaseInventoryAsync(Order order, CancellationToken cancellationToken)
    {
        foreach (var line in order.Lines)
        {
            await _inventoryService.ReleaseStockAsync(line.Id, line.Quantity, cancellationToken);
        }
    }

    /// <summary>
    /// Processes payment with automatic retry on transient failures.
    /// Demonstrates: Compensate with predicate for conditional error recovery.
    /// </summary>
    private async Task<Result<string>> ProcessPaymentWithCompensationAsync(
        Order order,
        PaymentInfo paymentInfo,
        CancellationToken cancellationToken)
    {
        return await Task.FromResult(order.ProcessPayment("PENDING"))
            .BindAsync(_ => _paymentService.ProcessPaymentAsync(order, paymentInfo.CardNumber, paymentInfo.CVV, cancellationToken))
            .CompensateAsync(
                predicate: error => error is UnexpectedError, // Retry on unexpected errors (e.g., timeouts)
                funcAsync: async () =>
                {
                    Console.WriteLine("Payment gateway timeout, retrying...");
                    await Task.Delay(1000, cancellationToken);
                    return await _paymentService.ProcessPaymentAsync(order, paymentInfo.CardNumber, paymentInfo.CVV, cancellationToken);
                })
            .TapErrorAsync(async error =>
            {
                await Task.FromResult(order.MarkPaymentFailed());
                Console.WriteLine($"Payment failed: {error.Detail}");
            });
    }

    /// <summary>
    /// Suggests alternative products when requested items are out of stock.
    /// Demonstrates: Compensate for providing alternative paths.
    /// </summary>
    private async Task<Result<Unit>> SuggestAlternativeProductsAsync(Order order, CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);
        Console.WriteLine($"Inventory unavailable for order {order.Id}. Suggesting alternatives...");

        // In a real system, this would query for similar products
        // For now, just return the original error
        return Error.Validation("Products out of stock. Please check alternative products.");
    }
}

/// <summary>
/// Request object for adding items to an order.
/// </summary>
public record OrderLineRequest(
    ProductId ProductId,
    string ProductName,
    Money UnitPrice,
    int Quantity
);

/// <summary>
/// Payment information for processing orders.
/// </summary>
public record PaymentInfo(
    string CardNumber,
    string CVV,
    string CardHolderName
);
