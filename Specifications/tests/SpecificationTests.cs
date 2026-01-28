namespace FunctionalDdd.Specifications.Tests;

using FluentAssertions;
using FunctionalDdd.Testing;

/// <summary>
/// Tests for core specification functionality.
/// </summary>
public class SpecificationTests : TestBase
{
    #region Spec Factory Tests

    [Fact]
    public void Spec_For_CreatesCriteriaSpecification()
    {
        // Arrange
        var order = CreateOrder(1, status: OrderStatus.Active);

        // Act
        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active);

        // Assert
        spec.Criteria.Should().NotBeNull();
        spec.IsSatisfiedBy(order).Should().BeTrue();
    }

    [Fact]
    public void Spec_All_CreatesSpecificationWithNoCriteria()
    {
        // Arrange
        var order = CreateOrder(1);

        // Act
        var spec = Spec.All<Order>();

        // Assert
        spec.Criteria.Should().BeNull();
        spec.IsSatisfiedBy(order).Should().BeTrue();
    }

    [Fact]
    public void Spec_Where_CreatesCriteriaSpecification()
    {
        // Arrange
        var order = CreateOrder(1, total: 150m);

        // Act
        var spec = Spec<Order>.Where(o => o.Total > 100m);

        // Assert
        spec.IsSatisfiedBy(order).Should().BeTrue();
    }

    #endregion

    #region Subclass Specification Tests

    [Fact]
    public void Subclass_Specification_FiltersCorrectly()
    {
        // Arrange
        var activeOrder = CreateOrder(1, status: OrderStatus.Active);
        var pendingOrder = CreateOrder(2, status: OrderStatus.Pending);

        // Act
        var spec = new ActiveOrdersSpec();

        // Assert
        spec.IsSatisfiedBy(activeOrder).Should().BeTrue();
        spec.IsSatisfiedBy(pendingOrder).Should().BeFalse();
    }

    [Fact]
    public void Subclass_Specification_SetsIncludes()
    {
        // Act
        var spec = new ActiveOrdersSpec();

        // Assert
        spec.Includes.Should().HaveCount(1);
    }

    [Fact]
    public void Subclass_Specification_SetsOrdering()
    {
        // Act
        var spec = new ActiveOrdersSpec();

        // Assert
        spec.OrderByDescending.Should().HaveCount(1);
    }

    [Fact]
    public void Subclass_Specification_SetsNoTracking()
    {
        // Act
        var spec = new ActiveOrdersSpec();

        // Assert
        spec.AsNoTracking.Should().BeTrue();
    }

    [Fact]
    public void Parameterized_Specification_UsesParameter()
    {
        // Arrange
        var order1 = CreateOrder(1, customerName: "Alice");
        var order2 = CreateOrder(2, customerName: "Bob");

        // Act
        var spec = new OrdersByCustomerSpec("Alice");

        // Assert
        spec.IsSatisfiedBy(order1).Should().BeTrue();
        spec.IsSatisfiedBy(order2).Should().BeFalse();
    }

    #endregion

    #region IsSatisfiedBy Tests

    [Fact]
    public void IsSatisfiedBy_WithNullCriteria_ReturnsTrue()
    {
        // Arrange
        var order = CreateOrder(1);
        var spec = Spec.All<Order>();

        // Act
        var result = spec.IsSatisfiedBy(order);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithMatchingCriteria_ReturnsTrue()
    {
        // Arrange
        var order = CreateOrder(1, total: 500m);
        var spec = new HighValueOrdersSpec(400m);

        // Act
        var result = spec.IsSatisfiedBy(order);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithNonMatchingCriteria_ReturnsFalse()
    {
        // Arrange
        var order = CreateOrder(1, total: 200m);
        var spec = new HighValueOrdersSpec(400m);

        // Act
        var result = spec.IsSatisfiedBy(order);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedByAll_AllMatch_ReturnsTrue()
    {
        // Arrange
        var orders = new[]
        {
            CreateOrder(1, status: OrderStatus.Active),
            CreateOrder(2, status: OrderStatus.Active),
            CreateOrder(3, status: OrderStatus.Active)
        };
        var spec = new ActiveOrdersSpec();

        // Act
        var result = spec.IsSatisfiedByAll(orders);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedByAll_NotAllMatch_ReturnsFalse()
    {
        // Arrange
        var orders = new[]
        {
            CreateOrder(1, status: OrderStatus.Active),
            CreateOrder(2, status: OrderStatus.Pending),
            CreateOrder(3, status: OrderStatus.Active)
        };
        var spec = new ActiveOrdersSpec();

        // Act
        var result = spec.IsSatisfiedByAll(orders);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedByAny_SomeMatch_ReturnsTrue()
    {
        // Arrange
        var orders = new[]
        {
            CreateOrder(1, status: OrderStatus.Pending),
            CreateOrder(2, status: OrderStatus.Active),
            CreateOrder(3, status: OrderStatus.Pending)
        };
        var spec = new ActiveOrdersSpec();

        // Act
        var result = spec.IsSatisfiedByAny(orders);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedByAny_NoneMatch_ReturnsFalse()
    {
        // Arrange
        var orders = new[]
        {
            CreateOrder(1, status: OrderStatus.Pending),
            CreateOrder(2, status: OrderStatus.Cancelled)
        };
        var spec = new ActiveOrdersSpec();

        // Act
        var result = spec.IsSatisfiedByAny(orders);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Filter_ReturnsMatchingEntities()
    {
        // Arrange
        var orders = new[]
        {
            CreateOrder(1, status: OrderStatus.Active),
            CreateOrder(2, status: OrderStatus.Pending),
            CreateOrder(3, status: OrderStatus.Active)
        };
        var spec = new ActiveOrdersSpec();

        // Act
        var result = spec.Filter(orders).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(o => o.Status == OrderStatus.Active);
    }

    [Fact]
    public void Count_ReturnsMatchingCount()
    {
        // Arrange
        var orders = new[]
        {
            CreateOrder(1, status: OrderStatus.Active),
            CreateOrder(2, status: OrderStatus.Pending),
            CreateOrder(3, status: OrderStatus.Active),
            CreateOrder(4, status: OrderStatus.Active)
        };
        var spec = new ActiveOrdersSpec();

        // Act
        var result = spec.Count(orders);

        // Assert
        result.Should().Be(3);
    }

    #endregion
}
