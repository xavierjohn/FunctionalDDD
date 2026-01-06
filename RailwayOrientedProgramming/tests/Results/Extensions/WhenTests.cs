namespace RailwayOrientedProgramming.Tests.Results.Extensions.WhenTests;

using FunctionalDdd.Testing;
using System.Diagnostics;

public class WhenTests : TestBase
{
    #region When with Predicate

    [Fact]
    public void When_WithPredicate_TrueCondition_ExecutesOperation()
    {
        // Arrange
        var result = Result.Success(42);
        var operationExecuted = false;

        // Act
        var actual = result.When(
            x => x > 40,
            x => { operationExecuted = true; return Result.Success(x * 2); });

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public void When_WithPredicate_FalseCondition_SkipsOperation()
    {
        // Arrange
        var result = Result.Success(42);
        var operationExecuted = false;

        // Act
        var actual = result.When(
            x => x < 40,
            x => { operationExecuted = true; return Result.Success(x * 2); });

        // Assert
        operationExecuted.Should().BeFalse();
        actual.Should().BeSuccess()
            .Which.Should().Be(42); // Original value unchanged
    }

    [Fact]
    public void When_WithPredicate_FailureResult_SkipsOperation()
    {
        // Arrange
        var result = Result.Failure<int>(Error1);
        var operationExecuted = false;

        // Act
        var actual = result.When(
            x => x > 40,
            x => { operationExecuted = true; return Result.Success(x * 2); });

        // Assert
        operationExecuted.Should().BeFalse();
        actual.Should().BeFailure()
            .Which.Should().HaveCode(Error1.Code);
    }

    [Fact]
    public void When_WithPredicate_OperationReturnsFailure_ReturnsFailure()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var actual = result.When(
            x => x > 40,
            x => Result.Failure<int>(Error2));

        // Assert
        actual.Should().BeFailure()
            .Which.Should().HaveCode(Error2.Code);
    }

    #endregion

    #region When with Boolean Condition

    [Fact]
    public void When_WithBoolean_TrueCondition_ExecutesOperation()
    {
        // Arrange
        var result = Result.Success(42);
        var operationExecuted = false;

        // Act
        var actual = result.When(
            true,
            x => { operationExecuted = true; return Result.Success(x * 2); });

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public void When_WithBoolean_FalseCondition_SkipsOperation()
    {
        // Arrange
        var result = Result.Success(42);
        var operationExecuted = false;

        // Act
        var actual = result.When(
            false,
            x => { operationExecuted = true; return Result.Success(x * 2); });

        // Assert
        operationExecuted.Should().BeFalse();
        actual.Should().BeSuccess()
            .Which.Should().Be(42);
    }

    [Fact]
    public void When_WithBoolean_FailureResult_SkipsOperation()
    {
        // Arrange
        var result = Result.Failure<int>(Error1);
        var operationExecuted = false;

        // Act
        var actual = result.When(
            true,
            x => { operationExecuted = true; return Result.Success(x * 2); });

        // Assert
        operationExecuted.Should().BeFalse();
        actual.Should().BeFailure();
    }

    [Fact]
    public void When_WithBoolean_OperationReturnsFailure_ReturnsFailure()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var actual = result.When(
            true,
            x => Result.Failure<int>(Error2));

        // Assert
        actual.Should().BeFailure()
            .Which.Should().HaveCode(Error2.Code);
    }

    #endregion

    #region Unless with Predicate

    [Fact]
    public void Unless_WithPredicate_FalseCondition_ExecutesOperation()
    {
        // Arrange
        var result = Result.Success(42);
        var operationExecuted = false;

        // Act
        var actual = result.Unless(
            x => x > 50,
            x => { operationExecuted = true; return Result.Success(x * 2); });

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public void Unless_WithPredicate_TrueCondition_SkipsOperation()
    {
        // Arrange
        var result = Result.Success(42);
        var operationExecuted = false;

        // Act
        var actual = result.Unless(
            x => x < 50,
            x => { operationExecuted = true; return Result.Success(x * 2); });

        // Assert
        operationExecuted.Should().BeFalse();
        actual.Should().BeSuccess()
            .Which.Should().Be(42);
    }

    [Fact]
    public void Unless_WithPredicate_FailureResult_SkipsOperation()
    {
        // Arrange
        var result = Result.Failure<int>(Error1);
        var operationExecuted = false;

        // Act
        var actual = result.Unless(
            x => x > 50,
            x => { operationExecuted = true; return Result.Success(x * 2); });

        // Assert
        operationExecuted.Should().BeFalse();
        actual.Should().BeFailure();
    }

    [Fact]
    public void Unless_WithPredicate_OperationReturnsFailure_ReturnsFailure()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var actual = result.Unless(
            x => x > 50,
            x => Result.Failure<int>(Error2));

        // Assert
        actual.Should().BeFailure()
            .Which.Should().HaveCode(Error2.Code);
    }

    #endregion

    #region Unless with Boolean Condition

    [Fact]
    public void Unless_WithBoolean_FalseCondition_ExecutesOperation()
    {
        // Arrange
        var result = Result.Success(42);
        var operationExecuted = false;

        // Act
        var actual = result.Unless(
            false,
            x => { operationExecuted = true; return Result.Success(x * 2); });

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public void Unless_WithBoolean_TrueCondition_SkipsOperation()
    {
        // Arrange
        var result = Result.Success(42);
        var operationExecuted = false;

        // Act
        var actual = result.Unless(
            true,
            x => { operationExecuted = true; return Result.Success(x * 2); });

        // Assert
        operationExecuted.Should().BeFalse();
        actual.Should().BeSuccess()
            .Which.Should().Be(42);
    }

    [Fact]
    public void Unless_WithBoolean_FailureResult_SkipsOperation()
    {
        // Arrange
        var result = Result.Failure<int>(Error1);
        var operationExecuted = false;

        // Act
        var actual = result.Unless(
            false,
            x => { operationExecuted = true; return Result.Success(x * 2); });

        // Assert
        operationExecuted.Should().BeFalse();
        actual.Should().BeFailure();
    }

    [Fact]
    public void Unless_WithBoolean_OperationReturnsFailure_ReturnsFailure()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var actual = result.Unless(
            false,
            x => Result.Failure<int>(Error2));

        // Assert
        actual.Should().BeFailure()
            .Which.Should().HaveCode(Error2.Code);
    }

    #endregion

    #region WhenAsync with Predicate

    [Fact]
    public async Task WhenAsync_WithPredicate_TrueCondition_ExecutesOperation()
    {
        // Arrange
        var result = Result.Success(42);
        var operationExecuted = false;

        // Act
        var actual = await result.WhenAsync(
            x => x > 40,
            x =>
            {
                operationExecuted = true;
                return Task.FromResult(Result.Success(x * 2));
            });

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public async Task WhenAsync_WithPredicate_FalseCondition_SkipsOperation()
    {
        // Arrange
        var result = Result.Success(42);
        var operationExecuted = false;

        // Act
        var actual = await result.WhenAsync(
            x => x < 40,
            x =>
            {
                operationExecuted = true;
                return Task.FromResult(Result.Success(x * 2));
            });

        // Assert
        operationExecuted.Should().BeFalse();
        actual.Should().BeSuccess()
            .Which.Should().Be(42);
    }

    [Fact]
    public async Task WhenAsync_WithPredicate_FailureResult_SkipsOperation()
    {
        // Arrange
        var result = Result.Failure<int>(Error1);
        var operationExecuted = false;

        // Act
        var actual = await result.WhenAsync(
            x => x > 40,
            x =>
            {
                operationExecuted = true;
                return Task.FromResult(Result.Success(x * 2));
            });

        // Assert
        operationExecuted.Should().BeFalse();
        actual.Should().BeFailure();
    }

    [Fact]
    public async Task WhenAsync_WithPredicate_WithCancellationToken_Success()
    {
        // Arrange
        var result = Result.Success(42);
        var operationExecuted = false;
        using var cts = new CancellationTokenSource();

        // Act
        var actual = await result.WhenAsync(
            x => x > 40,
            (x, ct) =>
            {
                operationExecuted = true;
                return Task.FromResult(Result.Success(x * 2));
            },
            cts.Token);

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    #endregion

    #region Real-World Scenarios

    [Fact]
    public void When_ApplyDiscountForPremiumUsers()
    {
        // Arrange
        var order = new Order { Amount = 100, IsPremium = true };
        var result = Result.Success(order);

        // Act
        var actual = result.When(
            o => o.IsPremium,
            o => Result.Success(new Order { Amount = o.Amount * 0.9m, IsPremium = o.IsPremium }));

        // Assert
        actual.Should().BeSuccess();
        actual.Value.Amount.Should().Be(90);
    }

    [Fact]
    public async Task WhenAsync_ValidateOnlyIfAmountExceedsThreshold()
    {
        // Arrange
        var transaction = new Transaction { Amount = 15000 };
        var result = Result.Success(transaction);
        var validationExecuted = false;

        // Act
        var actual = await result.WhenAsync(
            t => t.Amount > 10000,
            t =>
            {
                validationExecuted = true;
                return Task.FromResult(Result.Success(t));
            });

        // Assert
        validationExecuted.Should().BeTrue();
        actual.Should().BeSuccess();
    }

    [Fact]
    public void Unless_SkipProcessingForInactiveUsers()
    {
        // Arrange
        var user = new User { IsActive = false };
        var result = Result.Success(user);
        var processingExecuted = false;

        // Act
        var actual = result.Unless(
            u => u.IsActive,
            u => { processingExecuted = true; return Result.Failure<User>(Error.Validation("User is inactive")); });

        // Assert
        processingExecuted.Should().BeTrue();
        actual.Should().BeFailure();
    }

    private class Order
    {
        public decimal Amount { get; set; }
        public bool IsPremium { get; set; }
    }

    private class Transaction
    {
        public decimal Amount { get; set; }
    }

    private class User
    {
        public bool IsActive { get; set; }
    }

    #endregion
}
