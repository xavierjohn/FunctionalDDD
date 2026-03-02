namespace Trellis.EntityFrameworkCore.Tests.Helpers;

using Trellis.Primitives;

/// <summary>
/// Test entity representing a customer with various Trellis value object properties.
/// Demonstrates the backing-field pattern for <see cref="Maybe{T}"/> with EF Core.
/// </summary>
public class TestCustomer
{
    public TestCustomerId Id { get; set; } = null!;
    public TestCustomerName Name { get; set; } = null!;
    public EmailAddress Email { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    private PhoneNumber? _phone;
    public Maybe<PhoneNumber> Phone
    {
        get => _phone is not null ? Maybe.From(_phone) : Maybe.None<PhoneNumber>();
        set => _phone = value.HasValue ? value.Value : null;
    }
}

/// <summary>
/// Test entity representing an order with foreign key to customer.
/// Demonstrates the backing-field pattern for <see cref="Maybe{T}"/> with value types and enums.
/// </summary>
public class TestOrder
{
    public TestOrderId Id { get; set; } = null!;
    public TestCustomerId CustomerId { get; set; } = null!;
    public decimal Amount { get; set; }
    public TestOrderStatus Status { get; set; } = null!;
    public TestTicketNumber? TicketNumber { get; set; }
    public TestUnitPrice? UnitPrice { get; set; }

    private TestOrderStatus? _optionalStatus;
    public Maybe<TestOrderStatus> OptionalStatus
    {
        get => _optionalStatus is not null ? Maybe.From(_optionalStatus) : Maybe.None<TestOrderStatus>();
        set => _optionalStatus = value.HasValue ? value.Value : null;
    }

    private DateTime? _submittedAt;
    public Maybe<DateTime> SubmittedAt
    {
        get => _submittedAt.HasValue ? Maybe.From(_submittedAt.Value) : Maybe.None<DateTime>();
        set => _submittedAt = value.HasValue ? value.Value : null;
    }
}