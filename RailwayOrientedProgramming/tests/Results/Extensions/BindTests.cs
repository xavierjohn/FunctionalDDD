namespace RailwayOrientedProgramming.Tests.Results.Extensions.Bind;

using FunctionalDdd.Testing;

public partial class BindTests : TestBase
{

    [Fact]
    public void Bind_WithSuccessResult_ShouldReturnNewResult()
    {
        // Arrange
        var result = Result.Success<int>(42);
        var functionCalled = false;

        // Act
        var newResult = result.Bind(value =>
        {
            functionCalled = true;
            return Result.Success<string>($"Value: {value}");
        });

        // Assert
        functionCalled.Should().BeTrue();
        newResult.Should().BeSuccess()
            .Which.Should().Be("Value: 42");
    }

    [Fact]
    public void Bind_WithFailureResult_ShouldReturnFailedResult()
    {
        // Arrange
        var result = Result.Failure<int>(Error1);
        var functionCalled = false;

        // Act
        var newResult = result.Bind(value =>
        {
            functionCalled = true;
            return Result.Success<string>($"Value: {value}");
        });

        // Assert
        functionCalled.Should().BeFalse();
        newResult.Should().BeFailure();
        newResult.Error.Should().Be(Error1);
    }

    [Fact]
    public async Task BindAsync_Left_Task_ShouldReturnResult()
    {
        // Arrange
        var result = Result.Success("Hello").AsTask();
        var functionCalled = false;

        // Act
        var actual = await result.BindAsync(str =>
        {
            functionCalled = true;
            return Result.Success($"{str}");
        });

        // Assert
        functionCalled.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be("Hello");
    }

    [Fact]
    public async Task BindAsync_Left_Task_ShouldReturnFailedResult()
    {
        // Arrange
        var result = Result.Failure<string>(Error1).AsTask();
        var functionCalled = false;

        // Act
        var actual = await result.BindAsync(str =>
        {
            functionCalled = true;
            return Result.Success($"{str}");
        });

        // Assert
        functionCalled.Should().BeFalse();
        actual.Should().BeFailure();
        actual.Error.Should().Be(Error1);
    }

    [Fact]
    public async Task BindAsync_Right_Task_ShouldReturnResult()
    {
        // Arrange
        var result = Result.Success("Hello");
        var functionCalled = false;

        // Act
        var actual = await result.BindAsync(str =>
        {
            functionCalled = true;
            return Result.Success($"{str}").AsTask();
        });

        // Assert
        functionCalled.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be("Hello");
    }

    [Fact]
    public async Task BindAsync_Right_Task_ShouldReturnFailedResult()
    {
        // Arrange
        var result = Result.Failure<string>(Error1);
        var functionCalled = false;

        // Act
        var actual = await result.BindAsync(str =>
        {
            functionCalled = true;
            return Result.Success($"{str}").AsTask();
        });

        // Assert
        functionCalled.Should().BeFalse();
        actual.Should().BeFailure();
        actual.Error.Should().Be(Error1);
    }

    [Fact]
    public async Task BindAsync_Both_Task_ShouldReturnResult()
    {
        // Arrange
        var result = Result.Success("Hello").AsTask();
        var functionCalled = false;

        // Act
        var actual = await result.BindAsync(str =>
        {
            functionCalled = true;
            return Result.Success($"{str}").AsTask();
        });

        // Assert
        functionCalled.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be("Hello");
    }

    [Fact]
    public async Task BindAsync_Both_Task_ShouldReturnFailedResult()
    {
        // Arrange
        var result = Result.Failure<string>(Error1).AsTask();
        var functionCalled = false;

        // Act
        var actual = await result.BindAsync(str =>
        {
            functionCalled = true;
            return Result.Success($"{str}").AsTask();
        });

        // Assert
        functionCalled.Should().BeFalse();
        actual.Should().BeFailure();
        actual.Error.Should().Be(Error1);
    }

    // Bind ValueTask
    [Fact]
    public async Task BindAsync_Left_ValueTask_ShouldReturnResult()
    {
        // Arrange
        var result = Result.Success("Hello").AsValueTask();
        var functionCalled = false;

        // Act
        var actual = await result.BindAsync(str =>
        {
            functionCalled = true;
            return Result.Success($"{str}");
        });

        // Assert
        functionCalled.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be("Hello");
    }

    [Fact]
    public async Task BindAsync_Left_ValueTask_ShouldReturnFailedResult()
    {
        // Arrange
        var result = Result.Failure<string>(Error1).AsValueTask();
        var functionCalled = false;

        // Act
        var actual = await result.BindAsync(str =>
        {
            functionCalled = true;
            return Result.Success($"{str}");
        });

        // Assert
        functionCalled.Should().BeFalse();
        actual.Should().BeFailure();
        actual.Error.Should().Be(Error1);
    }

    [Fact]
    public async Task BindAsync_Right_ValueTask_ShouldReturnResult()
    {
        // Arrange
        var result = Result.Success("Hello");
        var functionCalled = false;

        // Act
        var actual = await result.BindAsync(str =>
        {
            functionCalled = true;
            return Result.Success($"{str}").AsValueTask();
        });

        // Assert
        functionCalled.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be("Hello");
    }

    [Fact]
    public async Task BindAsync_Right_ValueTask_ShouldReturnFailedResult()
    {
        // Arrange
        var result = Result.Failure<string>(Error1);
        var functionCalled = false;

        // Act
        var actual = await result.BindAsync(str =>
        {
            functionCalled = true;
            return Result.Success($"{str}").AsValueTask();
        });

        // Assert
        functionCalled.Should().BeFalse();
        actual.Should().BeFailure();
        actual.Error.Should().Be(Error1);
    }

    [Fact]
    public async Task BindAsync_Both_ValueTask_ShouldReturnResult()
    {
        // Arrange
        var result = Result.Success("Hello").AsValueTask();
        var functionCalled = false;

        // Act
        var actual = await result.BindAsync(str =>
        {
            functionCalled = true;
            return Result.Success($"{str}").AsValueTask();
        });

        // Assert
        functionCalled.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be("Hello");
    }

    [Fact]
    public async Task BindAsync_Both_ValueTask_ShouldReturnFailedResult()
    {
        // Arrange
        var result = Result.Failure<string>(Error1).AsValueTask();
        var functionCalled = false;

        // Act
        var actual = await result.BindAsync(str =>
        {
            functionCalled = true;
            return Result.Success($"{str}").AsValueTask();
        });

        // Assert
        functionCalled.Should().BeFalse();
        actual.Should().BeFailure();
        actual.Error.Should().Be(Error1);
    }

}