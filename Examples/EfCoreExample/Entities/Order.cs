namespace EfCoreExample.Entities;

using EfCoreExample.ValueObjects;
using FunctionalDdd;

/// <summary>
/// Order status enumeration.
/// </summary>
public enum OrderStatus
{
    Draft,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}

/// <summary>
/// Order aggregate with ULID-based ID for time-ordered, sortable identifiers.
/// ULIDs are perfect for orders as they naturally sort by creation time.
/// </summary>
public class Order : Aggregate<OrderId>
{
    private readonly List<OrderLine> _lines = [];

    public CustomerId CustomerId { get; private set; } = null!;
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public OrderStatus Status { get; private set; }
    public decimal Total => _lines.Sum(l => l.LineTotal);
    public DateTime CreatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }

    // EF Core requires parameterless constructor
    private Order() : base(OrderId.NewUnique()) { }

    private Order(CustomerId customerId) : base(OrderId.NewUnique())
    {
        CustomerId = customerId;
        Status = OrderStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new order for a customer.
    /// </summary>
    public static Result<Order> TryCreate(CustomerId customerId) =>
        customerId.ToResult()
            .Ensure(id => id != null, Error.Validation("Customer ID is required", nameof(customerId)))
            .Map(_ => new Order(customerId));

    /// <summary>
    /// Adds a product to the order.
    /// </summary>
    public Result<Order> AddLine(Product product, int quantity) =>
        this.ToResult()
            .Ensure(_ => Status == OrderStatus.Draft, Error.Validation("Cannot modify confirmed order"))
            .Ensure(_ => quantity > 0, Error.Validation("Quantity must be positive", nameof(quantity)))
            .Tap(_ => _lines.Add(new OrderLine(Id, product, quantity)));

    /// <summary>
    /// Confirms the order.
    /// </summary>
    public Result<Order> Confirm() =>
        this.ToResult()
            .Ensure(_ => Status == OrderStatus.Draft, Error.Validation("Order is not in draft status"))
            .Ensure(_ => _lines.Count > 0, Error.Validation("Order must have at least one item"))
            .Tap(_ =>
            {
                Status = OrderStatus.Confirmed;
                ConfirmedAt = DateTime.UtcNow;
            });

    // For EF Core to populate the lines collection
    internal void SetLines(List<OrderLine> lines) => _lines.AddRange(lines);
}
