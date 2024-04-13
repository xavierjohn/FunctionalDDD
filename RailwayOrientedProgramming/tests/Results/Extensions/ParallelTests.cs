namespace RailwayOrientedProgramming.Tests.Results.Extensions;

public class ParallelTests
{
    [Fact]
    public async Task Run_two_parallel_tasks_bind()
    {
        // Arrange
        // Act
        var r = await Task.FromResult(Result.Success("Hi"))
            .ParallelAsync(Task.FromResult(Result.Success("Bye")))
            .AwaitAsync()
            .BindAsync((a, b) => Result.Success(a + b));

        // Assert
        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be("HiBye");
    }

    [Fact]
    public async Task Run_two_parallel_tasks_tap()
    {
        // Arrange
        string result= string.Empty;

        // Act
        var r = await Task.FromResult(Result.Success("Hi"))
            .ParallelAsync(Task.FromResult(Result.Success("Bye")))
            .AwaitAsync()
            .TapAsync((a, b) => result = a + b);

        // Assert
        r.IsSuccess.Should().BeTrue();
        result.Should().Be("HiBye");
    }

    [Fact]
    public async Task Run_five_parallel_tasks_bind()
    {
        // Arrange
        // Act
        var r = await Task.FromResult(Result.Success("1"))
            .ParallelAsync(Task.FromResult(Result.Success("2")))
            .ParallelAsync(Task.FromResult(Result.Success("3")))
            .ParallelAsync(Task.FromResult(Result.Success("4")))
            .ParallelAsync(Task.FromResult(Result.Success("5")))
            .AwaitAsync()
            .BindAsync((a, b, c, d, e) => Result.Success(a + b + c + d + e));

        // Assert
        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be("12345");
    }

    [Fact]
    public async Task Run_five_parallel_tasks_tap()
    {
        // Arrange
        string result = string.Empty;

        // Act
        var r = await Task.FromResult(Result.Success("1"))
            .ParallelAsync(Task.FromResult(Result.Success("2")))
            .ParallelAsync(Task.FromResult(Result.Success("3")))
            .ParallelAsync(Task.FromResult(Result.Success("4")))
            .ParallelAsync(Task.FromResult(Result.Success("5")))
            .AwaitAsync()
            .TapAsync((a, b, c, d, e) => result = a + b + c + d + e);

        // Assert
        r.IsSuccess.Should().BeTrue();
        result.Should().Be("12345");
    }

    [Fact]
    public async Task Run_five_parallel_tasks_with_two_failures()
    {
        // Arrange
        var calledFunction = false;

        // Act
        var r = await Task.FromResult(Result.Success("1"))
            .ParallelAsync(Task.FromResult(Result.Success("2")))
            .ParallelAsync(Task.FromResult(Result.Failure<string>(Error.Unexpected("Internal Server error."))))
            .ParallelAsync(Task.FromResult(Result.Success("4")))
            .ParallelAsync(Task.FromResult(Result.Failure<string>(Error.Unexpected("Network unreachable."))))
            .AwaitAsync()
            .BindAsync((a, b, c, d, e) =>
             {
                 calledFunction = true;
                 return Result.Success(a + b + c + d + e);
             });

        // Assert
        r.IsFailure.Should().BeTrue();
        r.Error.Should().BeOfType<AggregateError>();
        var aggregate = (AggregateError)r.Error;
        aggregate.Errors.Should().HaveCount(2);
        calledFunction.Should().BeFalse();
        aggregate.Errors.Should().BeEquivalentTo(new List<Error>() {
            Error.Unexpected("Internal Server error."),
            Error.Unexpected("Network unreachable.")
        }, opt => opt.WithStrictOrdering());
    }
}
