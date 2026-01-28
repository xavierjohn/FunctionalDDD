namespace SpecificationExample.Services;

using FunctionalDdd;
using Microsoft.EntityFrameworkCore;
using SpecificationExample.Data;
using SpecificationExample.Entities;
using SpecificationExample.Specifications;
using SpecificationExample.ValueObjects;

/// <summary>
/// Order service demonstrating specification pattern with Result integration.
/// </summary>
public class OrderService
{
    private readonly OrderDbContext _context;

    public OrderService(OrderDbContext context) => _context = context;

    /// <summary>
    /// Get an order by ID with full details.
    /// Returns NotFoundError if not found.
    /// </summary>
    public async Task<Result<Order>> GetOrderByIdAsync(OrderId orderId)
    {
        var spec = Spec.For<Order>(o => o.Id == orderId)
            .Include(o => o.Lines)
            .AsNoTracking();

        return await _context.Orders.FirstOrNotFoundAsync(spec, "Order");
    }

    /// <summary>
    /// Get all orders for a customer.
    /// </summary>
    public async Task<List<Order>> GetCustomerOrdersAsync(CustomerId customerId) =>
        await _context.Orders.ToListAsync(new OrdersByCustomerSpec(customerId));

    /// <summary>
    /// Get orders ready for processing with pagination.
    /// </summary>
    public async Task<List<Order>> GetOrdersReadyForProcessingAsync(int page, int pageSize)
    {
        var spec = new OrdersReadyForProcessingSpec()
            .Paginate(page, pageSize);

        return await _context.Orders.ToListAsync(spec);
    }

    /// <summary>
    /// Get high-value priority orders (composite specification).
    /// </summary>
    public async Task<List<Order>> GetHighValuePriorityOrdersAsync(decimal minTotal = 500m)
    {
        // Compose specifications: High value AND Priority
        var spec = new HighValueOrdersSpec(minTotal)
            .And(new PriorityOrdersSpec())
            .Include(o => o.Lines)
            .AsNoTracking();

        return await _context.Orders.ToListAsync(spec);
    }

    /// <summary>
    /// Get orders ready for shipping.
    /// </summary>
    public async Task<List<Order>> GetOrdersReadyForShippingAsync() =>
        await _context.Orders.ToListAsync(new OrdersReadyForShippingSpec());

    /// <summary>
    /// Process an order - validates status and updates.
    /// Demonstrates ROP integration with specifications.
    /// </summary>
    public async Task<Result<Order>> ProcessOrderAsync(OrderId orderId)
    {
        var spec = Spec.For<Order>(o => o.Id == orderId)
            .Include(o => o.Lines);

        return await _context.Orders
            .FirstOrNotFoundAsync(spec, "Order")
            .EnsureAsync(
                o => o.Status == OrderStatus.Confirmed,
                Error.Validation("Order must be confirmed before processing"))
            .EnsureAsync(
                o => o.PaymentStatus == PaymentStatus.Paid,
                Error.Validation("Order must be paid before processing"))
            .TapAsync(o =>
            {
                o.Status = OrderStatus.Processing;
                return _context.SaveChangesAsync();
            });
    }

    /// <summary>
    /// Ship an order - validates and updates status.
    /// </summary>
    public async Task<Result<Order>> ShipOrderAsync(OrderId orderId)
    {
        var spec = Spec.For<Order>(o => o.Id == orderId);

        return await _context.Orders
            .FirstOrNotFoundAsync(spec, "Order")
            .EnsureAsync(
                o => o.Status == OrderStatus.Processing,
                Error.Validation("Order must be processing before shipping"))
            .TapAsync(o =>
            {
                o.Status = OrderStatus.Shipped;
                o.ShippedAt = DateTime.UtcNow;
                return _context.SaveChangesAsync();
            });
    }

    /// <summary>
    /// Check if a customer has any pending orders.
    /// </summary>
    public async Task<bool> CustomerHasPendingOrdersAsync(CustomerId customerId)
    {
        var spec = new OrdersByCustomerSpec(customerId)
            .And(new OrdersByStatusSpec(OrderStatus.Pending));

        return await _context.Orders.AnyAsync(spec);
    }

    /// <summary>
    /// Count orders by status.
    /// </summary>
    public async Task<int> CountOrdersByStatusAsync(OrderStatus status) =>
        await _context.Orders.CountAsync(new OrdersByStatusSpec(status));

    /// <summary>
    /// Get a single order ensuring uniqueness.
    /// Returns ConflictError if multiple found.
    /// </summary>
    public async Task<Result<Order>> GetSingleActiveOrderAsync(CustomerId customerId)
    {
        var spec = new OrdersByCustomerSpec(customerId)
            .And(Spec.For<Order>(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed));

        return await _context.Orders.SingleOrNotFoundAsync(spec, "Active order");
    }
}
