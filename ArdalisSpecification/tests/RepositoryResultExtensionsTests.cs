namespace FunctionalDdd.ArdalisSpecification.Tests;

using Ardalis.Specification;
using FluentAssertions;
using Moq;

/// <summary>
/// Tests for RepositoryResultExtensions - Result-returning extensions for IRepositoryBase.
/// </summary>
public class RepositoryResultExtensionsTests
{
    #region Test Infrastructure

    private readonly Mock<IRepositoryBase<TestEntity>> _repositoryMock = new();

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
        var result = await _repositoryMock.Object.FirstOrNotFoundAsync(spec, entityName: "Order", ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Detail.Should().Contain("Order not found");
    }

    [Fact]
    public async Task FirstOrNotFoundAsync_WithCancellationToken_PassesTokenToRepository()
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
        _repositoryMock.Verify(r => r.FirstOrDefaultAsync(spec, TestContext.Current.CancellationToken), Times.Once);
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
        result.Error.Detail.Should().Contain("TestEntity not found");
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

    [Fact]
    public async Task SingleOrNotFoundAsync_WithCustomEntityName_UsesCustomNameInError()
    {
        // Arrange
        var spec = new Mock<ISingleResultSpecification<TestEntity>>().Object;

        _repositoryMock
            .Setup(r => r.SingleOrDefaultAsync(spec, TestContext.Current.CancellationToken))
            .ReturnsAsync((TestEntity?)null);

        // Act
        var result = await _repositoryMock.Object.SingleOrNotFoundAsync(spec, entityName: "Customer", ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Detail.Should().Contain("Customer not found");
    }

    [Fact]
    public async Task SingleOrNotFoundAsync_MultipleWithCustomName_UsesCustomNameInConflictError()
    {
        // Arrange
        var spec = new Mock<ISingleResultSpecification<TestEntity>>().Object;

        _repositoryMock
            .Setup(r => r.SingleOrDefaultAsync(spec, TestContext.Current.CancellationToken))
            .ThrowsAsync(new InvalidOperationException("Sequence contains more than one element"));

        // Act
        var result = await _repositoryMock.Object.SingleOrNotFoundAsync(spec, entityName: "User", ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
        result.Error.Detail.Should().Contain("Multiple User entities found");
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
            new() { Id = 2, Name = "Entity2" },
            new() { Id = 3, Name = "Entity3" }
        };
        var spec = new Mock<ISpecification<TestEntity>>().Object;

        _repositoryMock
            .Setup(r => r.ListAsync(spec, TestContext.Current.CancellationToken))
            .ReturnsAsync(entities);

        // Act
        var result = await _repositoryMock.Object.ToListAsync(spec, TestContext.Current.CancellationToken);

        // Assert
        result.Should().HaveCount(3);
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

    #region ROP Chain Integration Tests

    [Fact]
    public async Task FirstOrNotFoundAsync_SuccessResult_CanBeChainedWithBindAsync()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var spec = new Mock<ISpecification<TestEntity>>().Object;

        _repositoryMock
            .Setup(r => r.FirstOrDefaultAsync(spec, TestContext.Current.CancellationToken))
            .ReturnsAsync(entity);

        // Act
        var result = await _repositoryMock.Object
            .FirstOrNotFoundAsync(spec, ct: TestContext.Current.CancellationToken)
            .BindAsync(e => Result.Success(e.Name.ToUpperInvariant()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("TEST");
    }

    [Fact]
    public async Task FirstOrNotFoundAsync_FailureResult_ShortCircuitsBindChain()
    {
        // Arrange
        var spec = new Mock<ISpecification<TestEntity>>().Object;
        var bindInvoked = false;

        _repositoryMock
            .Setup(r => r.FirstOrDefaultAsync(spec, TestContext.Current.CancellationToken))
            .ReturnsAsync((TestEntity?)null);

        // Act
        var result = await _repositoryMock.Object
            .FirstOrNotFoundAsync(spec, ct: TestContext.Current.CancellationToken)
            .BindAsync(e =>
            {
                bindInvoked = true;
                return Result.Success(e.Name);
            });

        // Assert
        result.IsFailure.Should().BeTrue();
        bindInvoked.Should().BeFalse("Bind should not execute on failure");
    }

    [Fact]
    public async Task FirstOrNotFoundAsync_SuccessResult_CanBeChainedWithMapAsync()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var spec = new Mock<ISpecification<TestEntity>>().Object;

        _repositoryMock
            .Setup(r => r.FirstOrDefaultAsync(spec, TestContext.Current.CancellationToken))
            .ReturnsAsync(entity);

        // Act
        var result = await _repositoryMock.Object
            .FirstOrNotFoundAsync(spec, ct: TestContext.Current.CancellationToken)
            .MapAsync(e => e.Id * 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task FirstOrNotFoundAsync_SuccessResult_CanBeChainedWithTapAsync()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var spec = new Mock<ISpecification<TestEntity>>().Object;
        string? capturedName = null;

        _repositoryMock
            .Setup(r => r.FirstOrDefaultAsync(spec, TestContext.Current.CancellationToken))
            .ReturnsAsync(entity);

        // Act
        var result = await _repositoryMock.Object
            .FirstOrNotFoundAsync(spec, ct: TestContext.Current.CancellationToken)
            .TapAsync(e => capturedName = e.Name);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedName.Should().Be("Test");
    }

    #endregion
}