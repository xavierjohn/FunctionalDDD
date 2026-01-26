namespace EfCoreExample.Entities;

using EfCoreExample.ValueObjects;
using FunctionalDdd;

/// <summary>
/// Order line entity representing a product in an order.
/// </summary>
public class OrderLine : Entity<int>
{
    public OrderId OrderId { get; private set; } = null!;
    public ProductId ProductId { get; private set; } = null!;
    public ProductName ProductName { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal LineTotal => UnitPrice * Quantity;

    // EF Core requires parameterless constructor
    private OrderLine() : base(0) { }

    internal OrderLine(OrderId orderId, Product product, int quantity) : base(0)
    {
        OrderId = orderId;
        ProductId = product.Id;
        ProductName = product.Name;
        UnitPrice = product.Price;
        Quantity = quantity;
    }
}