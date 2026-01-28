namespace FunctionalDdd.Specifications.Tests;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Tests for SpecificationEvaluator with EF Core.
/// </summary>
public class EvaluatorTests : TestBase
{
    #region Criteria Application Tests

    [Fact]
    public async Task Apply_WithCriteria_FiltersCorrectly()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1, status: OrderStatus.Active),
            CreateOrder(2, status: OrderStatus.Pending),
            CreateOrder(3, status: OrderStatus.Active)
        );

        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active);

        // Act
        var result = await SpecificationEvaluator
            .Apply(spec, Context.Orders)
            .ToListAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(o => o.Status == OrderStatus.Active);
    }

    [Fact]
    public async Task Apply_WithoutCriteria_ReturnsAll()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1),
            CreateOrder(2),
            CreateOrder(3)
        );

        var spec = Spec.All<Order>();

        // Act
        var result = await SpecificationEvaluator
            .Apply(spec, Context.Orders)
            .ToListAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    #endregion

    #region Ordering Tests

    [Fact]
    public async Task Apply_WithOrderBy_OrdersAscending()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1, total: 300m),
            CreateOrder(2, total: 100m),
            CreateOrder(3, total: 200m)
        );

        var spec = Spec.All<Order>().OrderBy(o => o.Total);

        // Act
        var result = await SpecificationEvaluator
            .Apply(spec, Context.Orders)
            .ToListAsync();

        // Assert
        result.Select(o => o.Total).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Apply_WithOrderByDescending_OrdersDescending()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1, total: 100m),
            CreateOrder(2, total: 300m),
            CreateOrder(3, total: 200m)
        );

        var spec = Spec.All<Order>().OrderByDescending(o => o.Total);

        // Act
        var result = await SpecificationEvaluator
            .Apply(spec, Context.Orders)
            .ToListAsync();

        // Assert
        result.Select(o => o.Total).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task Apply_WithMultipleOrderBy_AppliesThenBy()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1, status: OrderStatus.Active, total: 200m),
            CreateOrder(2, status: OrderStatus.Active, total: 100m),
            CreateOrder(3, status: OrderStatus.Pending, total: 300m)
        );

        var spec = Spec.All<Order>()
            .OrderBy(o => o.Status)
            .OrderBy(o => o.Total);

        // Act
        var result = await SpecificationEvaluator
            .Apply(spec, Context.Orders)
            .ToListAsync();

        // Assert
        // Pending (0) comes before Active (1), then order by Total within each status
        // Pending: 300m
        // Active: 100m, 200m
        result[0].Total.Should().Be(300m); // Pending
        result[1].Total.Should().Be(100m); // Active, lower total
        result[2].Total.Should().Be(200m); // Active, higher total
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task Apply_WithPagination_SkipsAndTakes()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1, total: 100m),
            CreateOrder(2, total: 200m),
            CreateOrder(3, total: 300m),
            CreateOrder(4, total: 400m),
            CreateOrder(5, total: 500m)
        );

        var spec = Spec.All<Order>()
            .OrderBy(o => o.Total)
            .Paginate(pageNumber: 2, pageSize: 2);

        // Act
        var result = await SpecificationEvaluator
            .Apply(spec, Context.Orders)
            .ToListAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Total.Should().Be(300m);
        result[1].Total.Should().Be(400m);
    }

    [Fact]
    public async Task Apply_WithFirstPage_DoesNotSkip()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1, total: 100m),
            CreateOrder(2, total: 200m),
            CreateOrder(3, total: 300m)
        );

        var spec = Spec.All<Order>()
            .OrderBy(o => o.Total)
            .Paginate(pageNumber: 1, pageSize: 2);

        // Act
        var result = await SpecificationEvaluator
            .Apply(spec, Context.Orders)
            .ToListAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Total.Should().Be(100m);
        result[1].Total.Should().Be(200m);
    }

    #endregion

    #region Combined Tests

    [Fact]
    public async Task Apply_SubclassSpecification_AppliesAllOptions()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1, status: OrderStatus.Active, createdAt: DateTime.UtcNow.AddDays(-1)),
            CreateOrder(2, status: OrderStatus.Active, createdAt: DateTime.UtcNow),
            CreateOrder(3, status: OrderStatus.Pending, createdAt: DateTime.UtcNow)
        );

        var spec = new ActiveOrdersSpec();

        // Act
        var result = await SpecificationEvaluator
            .Apply(spec, Context.Orders)
            .ToListAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(o => o.Status == OrderStatus.Active);
        // Should be ordered by CreatedAt descending
        result[0].CreatedAt.Should().BeAfter(result[1].CreatedAt);
    }

    [Fact]
    public async Task Apply_ComposedSpecification_AppliesAllCriteria()
    {
        // Arrange
        SeedOrders(
            CreateOrder(1, status: OrderStatus.Active, total: 600m),
            CreateOrder(2, status: OrderStatus.Active, total: 100m),
            CreateOrder(3, status: OrderStatus.Pending, total: 800m)
        );

        var activeSpec = new ActiveOrdersSpec();
        var highValueSpec = new HighValueOrdersSpec(500m);
        var combinedSpec = activeSpec.And(highValueSpec);

        // Act
        var result = await SpecificationEvaluator
            .Apply(combinedSpec, Context.Orders)
            .ToListAsync();

        // Assert
        result.Should().ContainSingle();
        result[0].Id.Should().Be(1);
    }

    #endregion
}
