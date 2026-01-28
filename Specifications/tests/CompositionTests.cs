namespace FunctionalDdd.Specifications.Tests;

using FluentAssertions;
using FunctionalDdd.Testing;

/// <summary>
/// Tests for specification composition (And, Or, Not).
/// </summary>
public class CompositionTests : TestBase
{
    #region And Composition Tests

    [Fact]
    public void And_BothMatch_ReturnsTrue()
    {
        // Arrange
        var order = CreateOrder(1, status: OrderStatus.Active, total: 600m);
        var activeSpec = new ActiveOrdersSpec();
        var highValueSpec = new HighValueOrdersSpec(500m);

        // Act
        var combinedSpec = activeSpec.And(highValueSpec);

        // Assert
        combinedSpec.IsSatisfiedBy(order).Should().BeTrue();
    }

    [Fact]
    public void And_OnlyLeftMatches_ReturnsFalse()
    {
        // Arrange
        var order = CreateOrder(1, status: OrderStatus.Active, total: 100m);
        var activeSpec = new ActiveOrdersSpec();
        var highValueSpec = new HighValueOrdersSpec(500m);

        // Act
        var combinedSpec = activeSpec.And(highValueSpec);

        // Assert
        combinedSpec.IsSatisfiedBy(order).Should().BeFalse();
    }

    [Fact]
    public void And_OnlyRightMatches_ReturnsFalse()
    {
        // Arrange
        var order = CreateOrder(1, status: OrderStatus.Pending, total: 600m);
        var activeSpec = new ActiveOrdersSpec();
        var highValueSpec = new HighValueOrdersSpec(500m);

        // Act
        var combinedSpec = activeSpec.And(highValueSpec);

        // Assert
        combinedSpec.IsSatisfiedBy(order).Should().BeFalse();
    }

    [Fact]
    public void And_NeitherMatches_ReturnsFalse()
    {
        // Arrange
        var order = CreateOrder(1, status: OrderStatus.Pending, total: 100m);
        var activeSpec = new ActiveOrdersSpec();
        var highValueSpec = new HighValueOrdersSpec(500m);

        // Act
        var combinedSpec = activeSpec.And(highValueSpec);

        // Assert
        combinedSpec.IsSatisfiedBy(order).Should().BeFalse();
    }

    [Fact]
    public void And_CombinesIncludes()
    {
        // Arrange
        var activeSpec = new ActiveOrdersSpec();
        var customerSpec = new OrdersByCustomerSpec("Alice");

        // Act
        var combinedSpec = activeSpec.And(customerSpec);

        // Assert
        combinedSpec.Includes.Should().HaveCount(2);
    }

    [Fact]
    public void And_CombinesTrackingSettings()
    {
        // Arrange
        var activeSpec = new ActiveOrdersSpec(); // Has AsNoTracking
        var customerSpec = new OrdersByCustomerSpec("Alice");

        // Act
        var combinedSpec = activeSpec.And(customerSpec);

        // Assert
        combinedSpec.AsNoTracking.Should().BeTrue();
    }

    #endregion

    #region Or Composition Tests

    [Fact]
    public void Or_BothMatch_ReturnsTrue()
    {
        // Arrange
        var order = CreateOrder(1, status: OrderStatus.Active, total: 600m);
        var activeSpec = new ActiveOrdersSpec();
        var highValueSpec = new HighValueOrdersSpec(500m);

        // Act
        var combinedSpec = activeSpec.Or(highValueSpec);

        // Assert
        combinedSpec.IsSatisfiedBy(order).Should().BeTrue();
    }

    [Fact]
    public void Or_OnlyLeftMatches_ReturnsTrue()
    {
        // Arrange
        var order = CreateOrder(1, status: OrderStatus.Active, total: 100m);
        var activeSpec = new ActiveOrdersSpec();
        var highValueSpec = new HighValueOrdersSpec(500m);

        // Act
        var combinedSpec = activeSpec.Or(highValueSpec);

        // Assert
        combinedSpec.IsSatisfiedBy(order).Should().BeTrue();
    }

    [Fact]
    public void Or_OnlyRightMatches_ReturnsTrue()
    {
        // Arrange
        var order = CreateOrder(1, status: OrderStatus.Pending, total: 600m);
        var activeSpec = new ActiveOrdersSpec();
        var highValueSpec = new HighValueOrdersSpec(500m);

        // Act
        var combinedSpec = activeSpec.Or(highValueSpec);

        // Assert
        combinedSpec.IsSatisfiedBy(order).Should().BeTrue();
    }

    [Fact]
    public void Or_NeitherMatches_ReturnsFalse()
    {
        // Arrange
        var order = CreateOrder(1, status: OrderStatus.Pending, total: 100m);
        var activeSpec = new ActiveOrdersSpec();
        var highValueSpec = new HighValueOrdersSpec(500m);

        // Act
        var combinedSpec = activeSpec.Or(highValueSpec);

        // Assert
        combinedSpec.IsSatisfiedBy(order).Should().BeFalse();
    }

    #endregion

    #region Not Composition Tests

    [Fact]
    public void Not_MatchingEntity_ReturnsFalse()
    {
        // Arrange
        var order = CreateOrder(1, status: OrderStatus.Active);
        var activeSpec = new ActiveOrdersSpec();

        // Act
        var notSpec = activeSpec.Not();

        // Assert
        notSpec.IsSatisfiedBy(order).Should().BeFalse();
    }

    [Fact]
    public void Not_NonMatchingEntity_ReturnsTrue()
    {
        // Arrange
        var order = CreateOrder(1, status: OrderStatus.Pending);
        var activeSpec = new ActiveOrdersSpec();

        // Act
        var notSpec = activeSpec.Not();

        // Assert
        notSpec.IsSatisfiedBy(order).Should().BeTrue();
    }

    [Fact]
    public void Not_PreservesIncludes()
    {
        // Arrange
        var activeSpec = new ActiveOrdersSpec();

        // Act
        var notSpec = activeSpec.Not();

        // Assert
        notSpec.Includes.Should().HaveCount(1);
    }

    [Fact]
    public void Not_PreservesOrdering()
    {
        // Arrange
        var activeSpec = new ActiveOrdersSpec();

        // Act
        var notSpec = activeSpec.Not();

        // Assert
        notSpec.OrderByDescending.Should().HaveCount(1);
    }

    #endregion

    #region Complex Composition Tests

    [Fact]
    public void ComplexComposition_And_Or_Not()
    {
        // Arrange
        var orders = new[]
        {
            CreateOrder(1, status: OrderStatus.Active, total: 600m),   // Active AND high value
            CreateOrder(2, status: OrderStatus.Pending, total: 800m),  // High value only
            CreateOrder(3, status: OrderStatus.Active, total: 100m),   // Active only
            CreateOrder(4, status: OrderStatus.Cancelled, total: 50m)  // Neither
        };

        var activeSpec = new ActiveOrdersSpec();
        var highValueSpec = new HighValueOrdersSpec(500m);

        // (Active AND HighValue) OR (NOT Active)
        var combinedSpec = activeSpec.And(highValueSpec).Or(activeSpec.Not());

        // Act & Assert
        combinedSpec.IsSatisfiedBy(orders[0]).Should().BeTrue();  // Active AND high value
        combinedSpec.IsSatisfiedBy(orders[1]).Should().BeTrue();  // NOT Active
        combinedSpec.IsSatisfiedBy(orders[2]).Should().BeFalse(); // Active but not high value
        combinedSpec.IsSatisfiedBy(orders[3]).Should().BeTrue();  // NOT Active
    }

    [Fact]
    public void ChainedComposition_MultipleAnds()
    {
        // Arrange
        var order = CreateOrder(1, customerName: "Alice", status: OrderStatus.Active, total: 600m);

        var activeSpec = new ActiveOrdersSpec();
        var highValueSpec = new HighValueOrdersSpec(500m);
        var customerSpec = new OrdersByCustomerSpec("Alice");

        // Act
        var combinedSpec = activeSpec.And(highValueSpec).And(customerSpec);

        // Assert
        combinedSpec.IsSatisfiedBy(order).Should().BeTrue();
    }

    #endregion
}
