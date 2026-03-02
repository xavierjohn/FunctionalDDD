namespace Trellis.EntityFrameworkCore.Tests;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Trellis.EntityFrameworkCore.Tests.Helpers;
using Trellis.Primitives;
using Trellis.Testing;

/// <summary>
/// Tests for <see cref="MaybePropertyExtensions.MaybeProperty{TEntity,TInner}"/>.
/// Validates that Maybe{T} properties backed by private nullable fields
/// round-trip correctly through EF Core with SQLite in-memory.
/// </summary>
public class MaybePropertyTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly SqliteConnection _connection;

    public MaybePropertyTests() =>
        (_context, _connection) = TestDbContext.CreateInMemory();

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Maybe<T> with scalar value object (reference type inner)

    [Fact]
    public async Task MaybeScalar_WithValue_RoundTripPreservesValue()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var phone = PhoneNumber.Create("+1-555-0100");
        var customer = new TestCustomer
        {
            Id = TestCustomerId.NewUniqueV4(),
            Name = TestCustomerName.Create("Alice"),
            Email = EmailAddress.Create("alice@example.com"),
            CreatedAt = DateTime.UtcNow,
            Phone = Maybe.From(phone)
        };

        // Act
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(ct);
        _context.ChangeTracker.Clear();

        var loaded = await _context.Customers.FindAsync([customer.Id], ct);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.Phone.HasValue.Should().BeTrue();
        loaded.Phone.Value.Value.Should().Be(phone.Value);
    }

    [Fact]
    public async Task MaybeScalar_WithNone_RoundTripPreservesNone()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var customer = new TestCustomer
        {
            Id = TestCustomerId.NewUniqueV4(),
            Name = TestCustomerName.Create("Bob"),
            Email = EmailAddress.Create("bob@example.com"),
            CreatedAt = DateTime.UtcNow
            // Phone is default (Maybe.None<PhoneNumber>())
        };

        // Act
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(ct);
        _context.ChangeTracker.Clear();

        var loaded = await _context.Customers.FindAsync([customer.Id], ct);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.Phone.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public async Task MaybeScalar_SetThenClear_RoundTripsCorrectly()
    {
        // Arrange — start with a phone number
        var ct = TestContext.Current.CancellationToken;
        var customer = new TestCustomer
        {
            Id = TestCustomerId.NewUniqueV4(),
            Name = TestCustomerName.Create("Charlie"),
            Email = EmailAddress.Create("charlie@example.com"),
            CreatedAt = DateTime.UtcNow,
            Phone = Maybe.From(PhoneNumber.Create("+1-555-0200"))
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(ct);

        // Act — clear to None
        _context.ChangeTracker.Clear();
        var tracked = await _context.Customers.FindAsync([customer.Id], ct);
        tracked!.Phone = Maybe.None<PhoneNumber>();
        await _context.SaveChangesAsync(ct);

        _context.ChangeTracker.Clear();
        var loaded = await _context.Customers.FindAsync([customer.Id], ct);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.Phone.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Maybe<T> with RequiredEnum (enum inner type)

    [Fact]
    public async Task MaybeEnum_WithValue_RoundTripPreservesValue()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var customerId = TestCustomerId.NewUniqueV4();
        _context.Customers.Add(new TestCustomer
        {
            Id = customerId,
            Name = TestCustomerName.Create("EnumCustomer"),
            Email = EmailAddress.Create("enum@example.com"),
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(ct);

        var order = new TestOrder
        {
            Id = TestOrderId.NewUniqueV4(),
            CustomerId = customerId,
            Amount = 42m,
            Status = TestOrderStatus.Draft,
            OptionalStatus = Maybe.From(TestOrderStatus.Shipped)
        };

        // Act
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(ct);
        _context.ChangeTracker.Clear();

        var loaded = await _context.Orders.FindAsync([order.Id], ct);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.OptionalStatus.HasValue.Should().BeTrue();
        loaded.OptionalStatus.Value.Should().Be(TestOrderStatus.Shipped);
    }

    [Fact]
    public async Task MaybeEnum_WithNone_RoundTripPreservesNone()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var customerId = TestCustomerId.NewUniqueV4();
        _context.Customers.Add(new TestCustomer
        {
            Id = customerId,
            Name = TestCustomerName.Create("EnumNoneCustomer"),
            Email = EmailAddress.Create("enumnone@example.com"),
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(ct);

        var order = new TestOrder
        {
            Id = TestOrderId.NewUniqueV4(),
            CustomerId = customerId,
            Amount = 50m,
            Status = TestOrderStatus.Confirmed
            // OptionalStatus is default (Maybe.None<TestOrderStatus>())
        };

        // Act
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(ct);
        _context.ChangeTracker.Clear();

        var loaded = await _context.Orders.FindAsync([order.Id], ct);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.OptionalStatus.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Column metadata

    [Fact]
    public void MaybeProperty_BackingField_IsNullableInModel()
    {
        var customerType = _context.Model.FindEntityType(typeof(TestCustomer))!;
        var phoneProp = customerType.FindProperty("_phone")!;

        phoneProp.IsNullable.Should().BeTrue("Maybe<T> backing field should be nullable");
    }

    [Fact]
    public void MaybeProperty_MaybeProperty_IsIgnored()
    {
        var customerType = _context.Model.FindEntityType(typeof(TestCustomer))!;
        var phoneProp = customerType.FindProperty("Phone");

        phoneProp.Should().BeNull("The Maybe<T> CLR property should be ignored by EF Core");
    }

    #endregion
}
