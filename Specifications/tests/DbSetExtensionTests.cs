namespace FunctionalDdd.Specifications.Tests;

using FluentAssertions;

/// <summary>
/// Tests for DbSet extension methods with Result integration.
/// </summary>
public class DbSetExtensionTests : TestBase
{
    #region ToListAsync Tests

    [Fact]
    public async Task ToListAsync_WithMatchingEntities_ReturnsAll()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1, status: OrderStatus.Active),
            CreateOrder(2, status: OrderStatus.Active),
            CreateOrder(3, status: OrderStatus.Pending)
        );

        var spec = new ActiveOrdersSpec();

        // Act
        var result = await Context.Orders.ToListAsync(spec);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ToListAsync_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1, status: OrderStatus.Pending),
            CreateOrder(2, status: OrderStatus.Cancelled)
        );

        var spec = new ActiveOrdersSpec();

        // Act
        var result = await Context.Orders.ToListAsync(spec);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region FirstOrNotFoundAsync Tests

    [Fact]
    public async Task FirstOrNotFoundAsync_WithMatch_ReturnsSuccess()
    {
        // Arrange
        SeedOrders(CreateOrder(1, status: OrderStatus.Active));

        var spec = new ActiveOrdersSpec();

        // Act
        var result = await Context.Orders.FirstOrNotFoundAsync(spec);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(1);
    }

    [Fact]
    public async Task FirstOrNotFoundAsync_WithNoMatch_ReturnsNotFound()
    {
        // Arrange
        SeedOrders(CreateOrder(1, status: OrderStatus.Pending));

        var spec = new ActiveOrdersSpec();

        // Act
        var result = await Context.Orders.FirstOrNotFoundAsync(spec);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task FirstOrNotFoundAsync_WithCustomEntityName_UsesNameInError()
    {
        // Arrange
        SeedOrders(CreateOrder(1, status: OrderStatus.Pending));

        var spec = new ActiveOrdersSpec();

        // Act
        var result = await Context.Orders.FirstOrNotFoundAsync(spec, "ActiveOrder");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Detail.Should().Contain("ActiveOrder");
    }

    [Fact]
    public async Task FirstOrNotFoundAsync_WithMultipleMatches_ReturnsFirst()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1, status: OrderStatus.Active, createdAt: DateTime.UtcNow),
            CreateOrder(2, status: OrderStatus.Active, createdAt: DateTime.UtcNow.AddDays(-1))
        );

        var spec = new ActiveOrdersSpec(); // Orders by CreatedAt descending

        // Act
        var result = await Context.Orders.FirstOrNotFoundAsync(spec);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(1); // Most recent
    }

    #endregion

    #region SingleOrNotFoundAsync Tests

    [Fact]
    public async Task SingleOrNotFoundAsync_WithSingleMatch_ReturnsSuccess()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1, status: OrderStatus.Active),
            CreateOrder(2, status: OrderStatus.Pending)
        );

        var spec = new ActiveOrdersSpec();

        // Act
        var result = await Context.Orders.SingleOrNotFoundAsync(spec);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(1);
    }

    [Fact]
    public async Task SingleOrNotFoundAsync_WithNoMatch_ReturnsNotFound()
    {
        // Arrange
        SeedOrders(CreateOrder(1, status: OrderStatus.Pending));

        var spec = new ActiveOrdersSpec();

        // Act
        var result = await Context.Orders.SingleOrNotFoundAsync(spec);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task SingleOrNotFoundAsync_WithMultipleMatches_ReturnsConflict()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1, status: OrderStatus.Active),
            CreateOrder(2, status: OrderStatus.Active)
        );

        var spec = new ActiveOrdersSpec();

        // Act
        var result = await Context.Orders.SingleOrNotFoundAsync(spec);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
    }

    [Fact]
    public async Task SingleOrNotFoundAsync_WithCustomEntityName_UsesNameInError()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1, status: OrderStatus.Active),
            CreateOrder(2, status: OrderStatus.Active)
        );

        var spec = new ActiveOrdersSpec();

        // Act
        var result = await Context.Orders.SingleOrNotFoundAsync(spec, "ActiveOrder");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Detail.Should().Contain("ActiveOrder");
    }

    #endregion

    #region AnyAsync Tests

    [Fact]
    public async Task AnyAsync_WithMatch_ReturnsTrue()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1, status: OrderStatus.Active),
            CreateOrder(2, status: OrderStatus.Pending)
        );

        var spec = new ActiveOrdersSpec();

        // Act
        var result = await Context.Orders.AnyAsync(spec);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AnyAsync_WithNoMatch_ReturnsFalse()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1, status: OrderStatus.Pending),
            CreateOrder(2, status: OrderStatus.Cancelled)
        );

        var spec = new ActiveOrdersSpec();

        // Act
        var result = await Context.Orders.AnyAsync(spec);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CountAsync Tests

    [Fact]
    public async Task CountAsync_WithMatches_ReturnsCount()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1, status: OrderStatus.Active),
            CreateOrder(2, status: OrderStatus.Active),
            CreateOrder(3, status: OrderStatus.Pending)
        );

        var spec = new ActiveOrdersSpec();

        // Act
        var result = await Context.Orders.CountAsync(spec);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task CountAsync_WithNoMatches_ReturnsZero()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1, status: OrderStatus.Pending),
            CreateOrder(2, status: OrderStatus.Cancelled)
        );

        var spec = new ActiveOrdersSpec();

        // Act
        var result = await Context.Orders.CountAsync(spec);

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region Integration with Result Pattern

    [Fact]
    public async Task ResultPattern_ChainWithBind_WorksCorrectly()
    {
        // Arrange
        SeedOrders(CreateOrder(1, status: OrderStatus.Active, total: 500m));

        var spec = Spec.For<Order>(o => o.Id == 1);

        // Act
        var result = await Context.Orders
            .FirstOrNotFoundAsync(spec)
            .BindAsync(order =>
                order.Status == OrderStatus.Active
                    ? Result.Success(order)
                    : Result.Failure<Order>(Error.Validation("Order is not active")));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Total.Should().Be(500m);
    }

    [Fact]
    public async Task ResultPattern_ChainWithEnsure_WorksCorrectly()
    {
        // Arrange
        SeedOrders(CreateOrder(1, status: OrderStatus.Active, total: 500m));

        var spec = Spec.For<Order>(o => o.Id == 1);

        // Act
        var result = await Context.Orders
            .FirstOrNotFoundAsync(spec)
            .EnsureAsync(
                order => order.Total >= 100m,
                Error.Validation("Order total is too low"));

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ResultPattern_FailedLookup_StopsChain()
    {
        // Arrange
        SeedOrders(CreateOrder(1, status: OrderStatus.Pending));

        var spec = Spec.For<Order>(o => o.Id == 999);
        var ensureInvoked = false;

        // Act
        var result = await Context.Orders
            .FirstOrNotFoundAsync(spec)
            .EnsureAsync(
                order =>
                {
                    ensureInvoked = true;
                    return true;
                },
                Error.Validation("Should not reach here"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
        ensureInvoked.Should().BeFalse();
    }

    #endregion
}
