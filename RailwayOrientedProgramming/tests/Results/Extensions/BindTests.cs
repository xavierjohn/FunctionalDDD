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
    public void Bind_WithTwoParameters_ShouldReturnResult()
    {
        // Arrange
        var result = Result.Success(("Hello", 42));

        // Act
        var actual = result.Bind((str, num) => Result.Success($"{str} {num}"));

        // Assert
        Assert.True(actual.IsSuccess);
        Assert.Equal("Hello 42", actual.Value);
    }

    [Fact]
    public void Bind_WithTwoParameters_ShouldReturnFailedResult()
    {
        // Arrange
        var result = Result.Failure<(string, int)>(Error1);

        // Act
        var actual = result.Bind((str, num) => Result.Success($"{str} {num}"));

        // Assert
        Assert.True(actual.IsFailure);
        Assert.Equal(Error1, actual.Error);
    }

    [Fact]
    public async Task BindAsync_Right_Task_WithTwoParameters_ShouldReturnResult()
    {
        // Arrange
        var result = Result.Success(("Hello", 42));

        // Act
        var actual = await result.BindAsync((str, num) => Task.FromResult(Result.Success($"{str} {num}")));

        // Assert
        Assert.True(actual.IsSuccess);
        Assert.Equal("Hello 42", actual.Value);
    }

    [Fact]
    public async Task BindAsync_Right_Task_WithTwoParameters_ShouldReturnFailedResult()
    {
        // Arrange
        var result = Result.Failure<(string, int)>(Error1);

        // Act
        var actual = await result.BindAsync((str, num) => Task.FromResult(Result.Success($"{str} {num}")));

        // Assert
        Assert.True(actual.IsFailure);
        Assert.Equal(Error1, actual.Error);
    }

    [Fact]
    public async Task BindAsync_Right_ValueTask_WithTwoParameters_ShouldReturnResult()
    {
        // Arrange
        var result = Result.Success(("Hello", 42));

        // Act
        var actual = await result.BindAsync((str, num) => ValueTask.FromResult(Result.Success($"{str} {num}")));

        // Assert
        Assert.True(actual.IsSuccess);
        Assert.Equal("Hello 42", actual.Value);
    }

    [Fact]
    public async Task BindAsync_Right_ValueTask_WithTwoParameters_ShouldReturnFailedResult()
    {
        // Arrange
        var result = Result.Failure<(string, int)>(Error1);

        // Act
        var actual = await result.BindAsync((str, num) => ValueTask.FromResult(Result.Success($"{str} {num}")));

        // Assert
        Assert.True(actual.IsFailure);
        Assert.Equal(Error1, actual.Error);
    }
}
