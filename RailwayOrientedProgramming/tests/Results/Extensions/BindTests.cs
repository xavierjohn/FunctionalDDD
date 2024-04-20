namespace RailwayOrientedProgramming.Tests.Results.Extensions;

public partial class BindTests : TestBase
{

    [Fact]
    public void Bind_WithSuccessResult_ShouldReturnNewResult()
    {
        // Arrange
        var result = Result.Success<int>(42);

        // Act
        var newResult = result.Bind(value => Result.Success<string>($"Value: {value}"));

        // Assert
        Assert.True(newResult.IsSuccess);
        Assert.Equal("Value: 42", newResult.Value);
    }

    [Fact]
    public void Bind_WithFailureResult_ShouldReturnFailedResult()
    {
        // Arrange
        var result = Result.Failure<int>(Error1);

        // Act
        var newResult = result.Bind(value => Result.Success<string>($"Value: {value}"));

        // Assert
        Assert.True(newResult.IsFailure);
        Assert.Equal(Error1, newResult.Error);
    }

    [Fact]
    public async Task BindAsync_Left_Task_ShouldReturnResult()
    {
        // Arrange
        var result = Result.Success("Hello").AsTask();

        // Act
        var actual = await result.BindAsync(str => Result.Success($"{str}"));

        // Assert
        actual.IsSuccess.Should().BeTrue();
        actual.Value.Should().Be("Hello");
    }

    [Fact]
    public async Task BindAsync_Left_Task_ShouldReturnFailedResult()
    {
        // Arrange
        var result = Result.Failure<string>(Error1).AsTask();

        // Act
        var actual = await result.BindAsync(str => Result.Success($"{str}"));

        // Assert
        actual.IsFailure.Should().BeTrue();
        actual.Error.Should().Be(Error1);
    }

    [Fact]
    public async Task BindAsync_Right_Task_ShouldReturnResult()
    {
        // Arrange
        var result = Result.Success("Hello");

        // Act
        var actual = await result.BindAsync(str => Result.Success($"{str}").AsTask());

        // Assert
        actual.IsSuccess.Should().BeTrue();
        actual.Value.Should().Be("Hello");
    }

    [Fact]
    public async Task BindAsync_Right_Task_ShouldReturnFailedResult()
    {
        // Arrange
        var result = Result.Failure<string>(Error1);

        // Act
        var actual = await result.BindAsync(str => Result.Success($"{str}").AsTask());

        // Assert
        actual.IsFailure.Should().BeTrue();
        actual.Error.Should().Be(Error1);
    }

    [Fact]
    public async Task BindAsync_Both_Task_ShouldReturnResult()
    {
        // Arrange
        var result = Result.Success("Hello").AsTask();

        // Act
        var actual = await result.BindAsync(str => Result.Success($"{str}").AsTask());

        // Assert
        actual.IsSuccess.Should().BeTrue();
        actual.Value.Should().Be("Hello");
    }

    [Fact]
    public async Task BindAsync_Both_Task_ShouldReturnFailedResult()
    {
        // Arrange
        var result = Result.Failure<string>(Error1).AsTask();

        // Act
        var actual = await result.BindAsync(str => Result.Success($"{str}").AsTask());

        // Assert
        actual.IsFailure.Should().BeTrue();
        actual.Error.Should().Be(Error1);
    }

    // Bind ValueTask
    [Fact]
    public async Task BindAsync_Left_ValueTask_ShouldReturnResult()
    {
        // Arrange
        var result = Result.Success("Hello").AsValueTask();

        // Act
        var actual = await result.BindAsync(str => Result.Success($"{str}"));

        // Assert
        actual.IsSuccess.Should().BeTrue();
        actual.Value.Should().Be("Hello");
    }

    [Fact]
    public async Task BindAsync_Left_ValueTask_ShouldReturnFailedResult()
    {
        // Arrange
        var result = Result.Failure<string>(Error1).AsValueTask();

        // Act
        var actual = await result.BindAsync(str => Result.Success($"{str}"));

        // Assert
        actual.IsFailure.Should().BeTrue();
        actual.Error.Should().Be(Error1);
    }

    [Fact]
    public async Task BindAsync_Right_ValueTask_ShouldReturnResult()
    {
        // Arrange
        var result = Result.Success("Hello");

        // Act
        var actual = await result.BindAsync(str => Result.Success($"{str}").AsValueTask());

        // Assert
        actual.IsSuccess.Should().BeTrue();
        actual.Value.Should().Be("Hello");
    }

    [Fact]
    public async Task BindAsync_Right_ValueTask_ShouldReturnFailedResult()
    {
        // Arrange
        var result = Result.Failure<string>(Error1);

        // Act
        var actual = await result.BindAsync(str => Result.Success($"{str}").AsValueTask());

        // Assert
        actual.IsFailure.Should().BeTrue();
        actual.Error.Should().Be(Error1);
    }

    [Fact]
    public async Task BindAsync_Both_ValueTask_ShouldReturnResult()
    {
        // Arrange
        var result = Result.Success("Hello").AsValueTask();

        // Act
        var actual = await result.BindAsync(str => Result.Success($"{str}").AsValueTask());

        // Assert
        actual.IsSuccess.Should().BeTrue();
        actual.Value.Should().Be("Hello");
    }

    [Fact]
    public async Task BindAsync_Both_ValueTask_ShouldReturnFailedResult()
    {
        // Arrange
        var result = Result.Failure<string>(Error1).AsValueTask();

        // Act
        var actual = await result.BindAsync(str => Result.Success($"{str}").AsValueTask());

        // Assert
        actual.IsFailure.Should().BeTrue();
        actual.Error.Should().Be(Error1);
    }

}
