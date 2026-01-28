namespace SpecificationExample.Entities;

using SpecificationExample.ValueObjects;

/// <summary>
/// Order status enumeration.
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}

/// <summary>
/// Payment status enumeration.
/// </summary>
public enum PaymentStatus
{
    Unpaid = 0,
    Paid = 1,
    Refunded = 2
}

/// <summary>
/// Order entity with strongly-typed identifiers.
/// </summary>
public class Order
{
    public OrderId Id { get; set; } = null!;
    public CustomerId CustomerId { get; set; } = null!;
    public CustomerName CustomerName { get; set; } = null!;
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public bool IsPriority { get; set; }
    public List<OrderLine> Lines { get; set; } = [];

    public static Order Create(
        CustomerId customerId,
        CustomerName customerName,
        decimal total,
        OrderStatus status = OrderStatus.Pending,
        PaymentStatus paymentStatus = PaymentStatus.Unpaid,
        bool isPriority = false) =>
        new()
        {
            Id = OrderId.NewUnique(),
            CustomerId = customerId,
            CustomerName = customerName,
            Total = total,
            Status = status,
            PaymentStatus = paymentStatus,
            CreatedAt = DateTime.UtcNow,
            IsPriority = isPriority
        };
}

/// <summary>
/// Order line item.
/// </summary>
public class OrderLine
{
    public int Id { get; set; }
    public OrderId OrderId { get; set; } = null!;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
    public Order Order { get; set; } = null!;
}
