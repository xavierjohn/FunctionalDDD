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
            .ParallelWhenAll()
            .BindAsync((a, b) => Result.Success(a + b));

        // Assert
        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be("HiBye");

    }
}
