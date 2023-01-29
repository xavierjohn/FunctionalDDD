namespace RailwayOrientedProgramming.Tests.Results.Extensions;
using Xunit;
public class ParallelTests
{
    [Fact]
    public async Task Run_two_parallel_tasks()
    {
        // Arrange
        // Act
        var r = await Task.FromResult(Result.Success("Hi"))
            .ParallelAsync(Task.FromResult(Result.Success("Bye")))
            .BindAsync((a, b) => Result.Success(a + b));

        // Assert
        r.IsSuccess.Should().BeTrue();
        r.Ok.Should().Be("HiBye");
    }

    [Fact]
    public async Task Run_five_parallel_tasks()
    {
        // Arrange
        // Act
        var r = await Task.FromResult(Result.Success("1"))
            .ParallelAsync(Task.FromResult(Result.Success("2")))
            .ParallelAsync(Task.FromResult(Result.Success("3")))
            .ParallelAsync(Task.FromResult(Result.Success("4")))
            .ParallelAsync(Task.FromResult(Result.Success("5")))
            .BindAsync((a, b, c, d, e) => Result.Success(a + b + c + d + e));

        // Assert
        r.IsSuccess.Should().BeTrue();
        r.Ok.Should().Be("12345");
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
            .ParallelAsync(Task.FromResult(Result.Failure<string>(Error.Transient("Network unreachable."))))
            .BindAsync((a, b, c, d, e) =>
             {
                 calledFunction = true;
                 return Result.Success(a + b + c + d + e);
             });

        // Assert
        r.IsFailure.Should().BeTrue();
        r.Errors.Count.Should().Be(2);
        calledFunction.Should().BeFalse();
        r.Errors.Should().BeEquivalentTo(new List<Error>() {
            Error.Unexpected("Internal Server error."),
            Error.Transient("Network unreachable.")
        }, opt => opt.WithStrictOrdering());
    }
}
