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
        r.Value.Should().Be("HiBye");
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
        r.Value.Should().Be("12345");
    }
}
