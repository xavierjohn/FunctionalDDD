namespace Trellis.EntityFrameworkCore.Tests.Helpers;

using Trellis.Primitives;

/// <summary>
/// Test entity representing a customer with various Trellis value object properties.
/// </summary>
public class TestCustomer
{
    public TestCustomerId Id { get; set; } = null!;
    public TestCustomerName Name { get; set; } = null!;
    public EmailAddress Email { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Test entity representing an order with foreign key to customer.
/// </summary>
public class TestOrder
{
    public TestOrderId Id { get; set; } = null!;
    public TestCustomerId CustomerId { get; set; } = null!;
    public decimal Amount { get; set; }
    public TestOrderStatus Status { get; set; } = null!;
    public TestTicketNumber? TicketNumber { get; set; }
    public TestUnitPrice? UnitPrice { get; set; }
}
