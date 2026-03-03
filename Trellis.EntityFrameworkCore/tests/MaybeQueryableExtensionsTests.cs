namespace Trellis.EntityFrameworkCore.Tests;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Trellis.EntityFrameworkCore.Tests.Helpers;
using Trellis.Primitives;
using Trellis.Testing;

/// <summary>
/// Tests for <see cref="MaybeQueryableExtensions"/> — WhereNone, WhereHasValue, WhereEquals.
/// Validates that expression tree rewriting correctly translates to SQL NULL/value checks.
/// </summary>
public class MaybeQueryableExtensionsTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly SqliteConnection _connection;

    public MaybeQueryableExtensionsTests() =>
        (_context, _connection) = TestDbContext.CreateInMemory();

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region WhereNone — reference type inner (PhoneNumber)

    [Fact]
    public async Task WhereNone_ReturnsEntitiesWithNullBackingField()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var withPhone = CreateCustomer("Alice", "+1-555-0100");
        var withoutPhone = CreateCustomer("Bob");

        _context.Customers.AddRange(withPhone, withoutPhone);
        await _context.SaveChangesAsync(ct);
        _context.ChangeTracker.Clear();

        // Act
        var results = await _context.Customers
            .WhereNone(c => c.Phone)
            .ToListAsync(ct);

        // Assert
        results.Should().ContainSingle()
            .Which.Name.Should().Be(withoutPhone.Name);
    }

    [Fact]
    public async Task WhereNone_AllHaveValue_ReturnsEmpty()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        _context.Customers.Add(CreateCustomer("Alice", "+1-555-0100"));
        await _context.SaveChangesAsync(ct);
        _context.ChangeTracker.Clear();

        // Act
        var results = await _context.Customers
            .WhereNone(c => c.Phone)
            .ToListAsync(ct);

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region WhereHasValue — reference type inner (PhoneNumber)

    [Fact]
    public async Task WhereHasValue_ReturnsEntitiesWithNonNullBackingField()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var withPhone = CreateCustomer("Alice", "+1-555-0100");
        var withoutPhone = CreateCustomer("Bob");

        _context.Customers.AddRange(withPhone, withoutPhone);
        await _context.SaveChangesAsync(ct);
        _context.ChangeTracker.Clear();

        // Act
        var results = await _context.Customers
            .WhereHasValue(c => c.Phone)
            .ToListAsync(ct);

        // Assert
        results.Should().ContainSingle()
            .Which.Name.Should().Be(withPhone.Name);
    }

    [Fact]
    public async Task WhereHasValue_NoneHaveValue_ReturnsEmpty()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        _context.Customers.Add(CreateCustomer("Bob"));
        await _context.SaveChangesAsync(ct);
        _context.ChangeTracker.Clear();

        // Act
        var results = await _context.Customers
            .WhereHasValue(c => c.Phone)
            .ToListAsync(ct);

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region WhereEquals — reference type inner (PhoneNumber)

    [Fact]
    public async Task WhereEquals_MatchingValue_ReturnsMatchingEntities()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var phone = PhoneNumber.Create("+1-555-0100");
        var alice = CreateCustomer("Alice", "+1-555-0100");
        var bob = CreateCustomer("Bob", "+1-555-0200");
        var charlie = CreateCustomer("Charlie");

        _context.Customers.AddRange(alice, bob, charlie);
        await _context.SaveChangesAsync(ct);
        _context.ChangeTracker.Clear();

        // Act
        var results = await _context.Customers
            .WhereEquals(c => c.Phone, phone)
            .ToListAsync(ct);

        // Assert
        results.Should().ContainSingle()
            .Which.Name.Should().Be(alice.Name);
    }

    [Fact]
    public async Task WhereEquals_NoMatch_ReturnsEmpty()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var phone = PhoneNumber.Create("+1-555-9999");
        _context.Customers.Add(CreateCustomer("Alice", "+1-555-0100"));
        await _context.SaveChangesAsync(ct);
        _context.ChangeTracker.Clear();

        // Act
        var results = await _context.Customers
            .WhereEquals(c => c.Phone, phone)
            .ToListAsync(ct);

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region WhereNone / WhereHasValue — enum inner (TestOrderStatus)

    [Fact]
    public async Task WhereNone_EnumInner_ReturnsEntitiesWithNullBackingField()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var customer = CreateCustomer("Alice");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(ct);

        var withStatus = CreateOrder(customer.Id, TestOrderStatus.Confirmed);
        var withoutStatus = CreateOrder(customer.Id);

        _context.Orders.AddRange(withStatus, withoutStatus);
        await _context.SaveChangesAsync(ct);
        _context.ChangeTracker.Clear();

        // Act
        var results = await _context.Orders
            .WhereNone(o => o.OptionalStatus)
            .ToListAsync(ct);

        // Assert
        results.Should().ContainSingle()
            .Which.Id.Should().Be(withoutStatus.Id);
    }

    [Fact]
    public async Task WhereHasValue_EnumInner_ReturnsEntitiesWithValue()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var customer = CreateCustomer("Alice");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(ct);

        var withStatus = CreateOrder(customer.Id, TestOrderStatus.Confirmed);
        var withoutStatus = CreateOrder(customer.Id);

        _context.Orders.AddRange(withStatus, withoutStatus);
        await _context.SaveChangesAsync(ct);
        _context.ChangeTracker.Clear();

        // Act
        var results = await _context.Orders
            .WhereHasValue(o => o.OptionalStatus)
            .ToListAsync(ct);

        // Assert
        results.Should().ContainSingle()
            .Which.Id.Should().Be(withStatus.Id);
    }

    #endregion

    #region WhereNone / WhereHasValue / WhereEquals — value-type inner (DateTime)

    [Fact]
    public async Task WhereNone_ValueTypeInner_ReturnsEntitiesWithNullBackingField()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var customer = CreateCustomer("Alice");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(ct);

        var submitted = CreateOrder(customer.Id);
        submitted.SubmittedAt = Maybe.From(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        var notSubmitted = CreateOrder(customer.Id);

        _context.Orders.AddRange(submitted, notSubmitted);
        await _context.SaveChangesAsync(ct);
        _context.ChangeTracker.Clear();

        // Act
        var results = await _context.Orders
            .WhereNone(o => o.SubmittedAt)
            .ToListAsync(ct);

        // Assert
        results.Should().ContainSingle()
            .Which.Id.Should().Be(notSubmitted.Id);
    }

    [Fact]
    public async Task WhereHasValue_ValueTypeInner_ReturnsEntitiesWithValue()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var customer = CreateCustomer("Bob");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(ct);

        var submitted = CreateOrder(customer.Id);
        submitted.SubmittedAt = Maybe.From(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        var notSubmitted = CreateOrder(customer.Id);

        _context.Orders.AddRange(submitted, notSubmitted);
        await _context.SaveChangesAsync(ct);
        _context.ChangeTracker.Clear();

        // Act
        var results = await _context.Orders
            .WhereHasValue(o => o.SubmittedAt)
            .ToListAsync(ct);

        // Assert
        results.Should().ContainSingle()
            .Which.Id.Should().Be(submitted.Id);
    }

    [Fact]
    public async Task WhereEquals_ValueTypeInner_ReturnsMatchingEntities()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var customer = CreateCustomer("Charlie");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(ct);

        var targetDate = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
        var order1 = CreateOrder(customer.Id);
        order1.SubmittedAt = Maybe.From(targetDate);
        var order2 = CreateOrder(customer.Id);
        order2.SubmittedAt = Maybe.From(new DateTime(2026, 6, 15, 8, 0, 0, DateTimeKind.Utc));
        var order3 = CreateOrder(customer.Id); // no SubmittedAt

        _context.Orders.AddRange(order1, order2, order3);
        await _context.SaveChangesAsync(ct);
        _context.ChangeTracker.Clear();

        // Act
        var results = await _context.Orders
            .WhereEquals(o => o.SubmittedAt, targetDate)
            .ToListAsync(ct);

        // Assert
        results.Should().ContainSingle()
            .Which.Id.Should().Be(order1.Id);
    }

    #endregion

    #region Helpers

    private static TestCustomer CreateCustomer(string name, string? phone = null)
    {
        var customer = new TestCustomer
        {
            Id = TestCustomerId.NewUniqueV4(),
            Name = TestCustomerName.Create(name),
            Email = EmailAddress.Create($"{name.ToLowerInvariant()}@example.com"),
            CreatedAt = DateTime.UtcNow
        };

        if (phone is not null)
            customer.Phone = Maybe.From(PhoneNumber.Create(phone));

        return customer;
    }

    private static TestOrder CreateOrder(TestCustomerId customerId, TestOrderStatus? optionalStatus = null)
    {
        var order = new TestOrder
        {
            Id = TestOrderId.NewUniqueV4(),
            CustomerId = customerId,
            Amount = 100m,
            Status = TestOrderStatus.Draft
        };

        if (optionalStatus is not null)
            order.OptionalStatus = Maybe.From(optionalStatus);

        return order;
    }

    #endregion
}