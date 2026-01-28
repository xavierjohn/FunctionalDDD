namespace FunctionalDdd.Specifications.Tests;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Base test class providing common test infrastructure.
/// </summary>
public abstract class TestBase : IDisposable
{
    protected TestDbContext Context { get; }

    protected TestBase()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new TestDbContext(options);
    }

    protected void SeedOrders(params Order[] orders)
    {
        Context.Orders.AddRange(orders);
        Context.SaveChanges();
    }

    protected static Order CreateOrder(
        int id,
        string customerName = "Test Customer",
        decimal total = 100m,
        OrderStatus status = OrderStatus.Active,
        DateTime? createdAt = null) =>
        new Order
        {
            Id = id,
            CustomerName = customerName,
            Total = total,
            Status = status,
            CreatedAt = createdAt ?? DateTime.UtcNow
        };

    protected static Order CreateOrderWithLines(
        int id,
        string customerName = "Test Customer",
        decimal total = 100m,
        OrderStatus status = OrderStatus.Active,
        params (string product, int qty, decimal price)[] lines)
    {
        var order = CreateOrder(id, customerName, total, status);
        var lineId = 1;
        foreach (var (product, qty, price) in lines)
        {
            order.Lines.Add(new OrderLine
            {
                Id = lineId++,
                OrderId = id,
                ProductName = product,
                Quantity = qty,
                Price = price
            });
        }

        return order;
    }

    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}
