namespace FunctionalDdd.Specifications.Tests;

using FluentAssertions;

/// <summary>
/// Tests for specification extension methods (Include, OrderBy, Paginate, etc.).
/// </summary>
public class ExtensionTests : TestBase
{
    #region Include Extension Tests

    [Fact]
    public void Include_Expression_AddsToIncludes()
    {
        // Arrange
        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active);

        // Act
        var result = spec.Include(o => o.Lines);

        // Assert
        result.Includes.Should().HaveCount(1);
    }

    [Fact]
    public void Include_String_AddsToIncludeStrings()
    {
        // Arrange
        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active);

        // Act
        var result = spec.Include("Lines.Product");

        // Assert
        result.IncludeStrings.Should().Contain("Lines.Product");
    }

    [Fact]
    public void Include_Multiple_ChainsCorrectly()
    {
        // Arrange
        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active);

        // Act
        var result = spec
            .Include(o => o.Lines)
            .Include("Lines.Product");

        // Assert
        result.Includes.Should().HaveCount(1);
        result.IncludeStrings.Should().HaveCount(1);
    }

    [Fact]
    public void Include_PreservesCriteria()
    {
        // Arrange
        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active);

        // Act
        var result = spec.Include(o => o.Lines);

        // Assert
        result.Criteria.Should().NotBeNull();
    }

    #endregion

    #region OrderBy Extension Tests

    [Fact]
    public void OrderBy_AddsAscendingOrder()
    {
        // Arrange
        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active);

        // Act
        var result = spec.OrderBy(o => o.CreatedAt);

        // Assert
        result.OrderBy.Should().HaveCount(1);
        result.OrderByDescending.Should().BeEmpty();
    }

    [Fact]
    public void OrderByDescending_AddsDescendingOrder()
    {
        // Arrange
        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active);

        // Act
        var result = spec.OrderByDescending(o => o.Total);

        // Assert
        result.OrderByDescending.Should().HaveCount(1);
        result.OrderBy.Should().BeEmpty();
    }

    [Fact]
    public void OrderBy_Multiple_ChainsCorrectly()
    {
        // Arrange
        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active);

        // Act
        var result = spec
            .OrderBy(o => o.Status)
            .OrderBy(o => o.CreatedAt);

        // Assert
        result.OrderBy.Should().HaveCount(2);
    }

    [Fact]
    public void OrderBy_PreservesCriteria()
    {
        // Arrange
        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active);

        // Act
        var result = spec.OrderBy(o => o.CreatedAt);

        // Assert
        result.Criteria.Should().NotBeNull();
    }

    #endregion

    #region Paginate Extension Tests

    [Fact]
    public void Paginate_SetsSkipAndTake()
    {
        // Arrange
        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active);

        // Act
        var result = spec.Paginate(pageNumber: 2, pageSize: 10);

        // Assert
        result.Skip.Should().Be(10);
        result.Take.Should().Be(10);
    }

    [Fact]
    public void Paginate_FirstPage_SkipsZero()
    {
        // Arrange
        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active);

        // Act
        var result = spec.Paginate(pageNumber: 1, pageSize: 20);

        // Assert
        result.Skip.Should().Be(0);
        result.Take.Should().Be(20);
    }

    [Fact]
    public void Paginate_ThirdPage_CalculatesSkipCorrectly()
    {
        // Arrange
        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active);

        // Act
        var result = spec.Paginate(pageNumber: 3, pageSize: 25);

        // Assert
        result.Skip.Should().Be(50);
        result.Take.Should().Be(25);
    }

    [Fact]
    public void Paginate_PreservesCriteria()
    {
        // Arrange
        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active);

        // Act
        var result = spec.Paginate(pageNumber: 1, pageSize: 10);

        // Assert
        result.Criteria.Should().NotBeNull();
    }

    #endregion

    #region AsNoTracking Extension Tests

    [Fact]
    public void AsNoTracking_SetsFlag()
    {
        // Arrange
        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active);

        // Act
        var result = spec.AsNoTracking();

        // Assert
        result.AsNoTracking.Should().BeTrue();
    }

    [Fact]
    public void AsNoTracking_PreservesCriteria()
    {
        // Arrange
        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active);

        // Act
        var result = spec.AsNoTracking();

        // Assert
        result.Criteria.Should().NotBeNull();
    }

    #endregion

    #region AsSplitQuery Extension Tests

    [Fact]
    public void AsSplitQuery_SetsFlag()
    {
        // Arrange
        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active);

        // Act
        var result = spec.AsSplitQuery();

        // Assert
        result.AsSplitQuery.Should().BeTrue();
    }

    [Fact]
    public void AsSplitQuery_PreservesCriteria()
    {
        // Arrange
        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active);

        // Act
        var result = spec.AsSplitQuery();

        // Assert
        result.Criteria.Should().NotBeNull();
    }

    #endregion

    #region Combined Extension Tests

    [Fact]
    public void FluentChain_AllExtensions()
    {
        // Arrange & Act
        var spec = Spec.For<Order>(o => o.Status == OrderStatus.Active)
            .Include(o => o.Lines)
            .Include("Lines.Product")
            .OrderBy(o => o.CustomerName)
            .OrderByDescending(o => o.Total)
            .Paginate(pageNumber: 2, pageSize: 10)
            .AsNoTracking()
            .AsSplitQuery();

        // Assert
        spec.Criteria.Should().NotBeNull();
        spec.Includes.Should().HaveCount(1);
        spec.IncludeStrings.Should().HaveCount(1);
        spec.OrderBy.Should().HaveCount(1);
        spec.OrderByDescending.Should().HaveCount(1);
        spec.Skip.Should().Be(10);
        spec.Take.Should().Be(10);
        spec.AsNoTracking.Should().BeTrue();
        spec.AsSplitQuery.Should().BeTrue();
    }

    [Fact]
    public void Composition_WithExtensions()
    {
        // Arrange
        var activeSpec = new ActiveOrdersSpec();
        var highValueSpec = new HighValueOrdersSpec(500m);

        // Act
        var combinedSpec = activeSpec
            .And(highValueSpec)
            .OrderByDescending(o => o.Total)
            .Paginate(pageNumber: 1, pageSize: 10);

        // Assert
        combinedSpec.Skip.Should().Be(0);
        combinedSpec.Take.Should().Be(10);
        // Should have ordering from HighValueSpec plus additional
        combinedSpec.OrderByDescending.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    #endregion
}
