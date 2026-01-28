namespace FunctionalDdd.ArdalisSpecification.Tests;

using Ardalis.Specification;
using FluentAssertions;
using Moq;

/// <summary>
/// Tests for ReadRepositoryResultExtensions - Result-returning extensions for IReadRepositoryBase.
/// </summary>
public class ReadRepositoryResultExtensionsTests
{
    #region Test Infrastructure

    private readonly Mock<IReadRepositoryBase<TestEntity>> _repositoryMock = new();

    #endregion

    #region FirstOrNotFoundAsync Tests

    [Fact]
    public async Task FirstOrNotFoundAsync_EntityExists_ReturnsSuccess()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var spec = new Mock<ISpecification<TestEntity>>().Object;

        _repositoryMock
            .Setup(r => r.FirstOrDefaultAsync(spec, TestContext.Current.CancellationToken))
            .ReturnsAsync(entity);

        // Act
        var result = await _repositoryMock.Object.FirstOrNotFoundAsync(spec, ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(entity);
    }

    [Fact]
    public async Task FirstOrNotFoundAsync_EntityNotExists_ReturnsNotFoundError()
    {
        // Arrange
        var spec = new Mock<ISpecification<TestEntity>>().Object;

        _repositoryMock
            .Setup(r => r.FirstOrDefaultAsync(spec, TestContext.Current.CancellationToken))
            .ReturnsAsync((TestEntity?)null);

        // Act
        var result = await _repositoryMock.Object.FirstOrNotFoundAsync(spec, ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
        result.Error.Detail.Should().Contain("TestEntity not found");
    }

    [Fact]
    public async Task FirstOrNotFoundAsync_WithCustomEntityName_UsesCustomNameInError()
    {
        // Arrange
        var spec = new Mock<ISpecification<TestEntity>>().Object;

        _repositoryMock
            .Setup(r => r.FirstOrDefaultAsync(spec, TestContext.Current.CancellationToken))
            .ReturnsAsync((TestEntity?)null);

        // Act
        var result = await _repositoryMock.Object.FirstOrNotFoundAsync(spec, entityName: "Product", ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Detail.Should().Contain("Product not found");
    }

    #endregion

    #region SingleOrNotFoundAsync Tests

    [Fact]
    public async Task SingleOrNotFoundAsync_SingleEntityExists_ReturnsSuccess()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var spec = new Mock<ISingleResultSpecification<TestEntity>>().Object;

        _repositoryMock
            .Setup(r => r.SingleOrDefaultAsync(spec, TestContext.Current.CancellationToken))
            .ReturnsAsync(entity);

        // Act
        var result = await _repositoryMock.Object.SingleOrNotFoundAsync(spec, ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(entity);
    }

    [Fact]
    public async Task SingleOrNotFoundAsync_NoEntityExists_ReturnsNotFoundError()
    {
        // Arrange
        var spec = new Mock<ISingleResultSpecification<TestEntity>>().Object;

        _repositoryMock
            .Setup(r => r.SingleOrDefaultAsync(spec, TestContext.Current.CancellationToken))
            .ReturnsAsync((TestEntity?)null);

        // Act
        var result = await _repositoryMock.Object.SingleOrNotFoundAsync(spec, ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task SingleOrNotFoundAsync_MultipleEntitiesExist_ReturnsConflictError()
    {
        // Arrange
        var spec = new Mock<ISingleResultSpecification<TestEntity>>().Object;

        _repositoryMock
            .Setup(r => r.SingleOrDefaultAsync(spec, TestContext.Current.CancellationToken))
            .ThrowsAsync(new InvalidOperationException("Sequence contains more than one element"));

        // Act
        var result = await _repositoryMock.Object.SingleOrNotFoundAsync(spec, ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
        result.Error.Detail.Should().Contain("Multiple TestEntity entities found");
    }

    #endregion

    #region ToListAsync Tests

    [Fact]
    public async Task ToListAsync_EntitiesExist_ReturnsAllMatching()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new() { Id = 1, Name = "Entity1" },
            new() { Id = 2, Name = "Entity2" }
        };
        var spec = new Mock<ISpecification<TestEntity>>().Object;

        _repositoryMock
            .Setup(r => r.ListAsync(spec, TestContext.Current.CancellationToken))
            .ReturnsAsync(entities);

        // Act
        var result = await _repositoryMock.Object.ToListAsync(spec, TestContext.Current.CancellationToken);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(entities);
    }

    [Fact]
    public async Task ToListAsync_NoEntities_ReturnsEmptyList()
    {
        // Arrange
        var spec = new Mock<ISpecification<TestEntity>>().Object;

        _repositoryMock
            .Setup(r => r.ListAsync(spec, TestContext.Current.CancellationToken))
            .ReturnsAsync([]);

        // Act
        var result = await _repositoryMock.Object.ToListAsync(spec, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AnyAsync Tests

    [Fact]
    public async Task AnyAsync_EntitiesExist_ReturnsTrue()
    {
        // Arrange
        var spec = new Mock<ISpecification<TestEntity>>().Object;

        _repositoryMock
            .Setup(r => r.AnyAsync(spec, TestContext.Current.CancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _repositoryMock.Object.AnyAsync(spec, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AnyAsync_NoEntitiesExist_ReturnsFalse()
    {
        // Arrange
        var spec = new Mock<ISpecification<TestEntity>>().Object;

        _repositoryMock
            .Setup(r => r.AnyAsync(spec, TestContext.Current.CancellationToken))
            .ReturnsAsync(false);

        // Act
        var result = await _repositoryMock.Object.AnyAsync(spec, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
