namespace RailwayOrientedProgramming.Tests;

using FluentAssertions;
using Xunit;

public class MatchTupleTests
{
    [Fact]
    public void Match_WithSuccessTuple2_DestructuresValues()
    {
        // Arrange
        var result = Result.Success((42, "hello"));

        // Act
        var output = result.Match(
            onSuccess: (num, str) => $"{num}-{str}",
            onFailure: err => $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("42-hello");
    }

    [Fact]
    public void Match_WithFailureTuple2_CallsOnFailure()
    {
        // Arrange
        var result = Result.Failure<(int, string)>(Error.NotFound("Not found"));

        // Act
        var output = result.Match(
            onSuccess: (num, str) => $"{num}-{str}",
            onFailure: err => $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("Error: Not found");
    }

    [Fact]
    public void Match_WithSuccessTuple3_DestructuresValues()
    {
        // Arrange
        var result = Result.Success((1, "two", 3.0));

        // Act
        var output = result.Match(
            onSuccess: (i, s, d) => $"{i}-{s}-{d}",
            onFailure: err => $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("1-two-3");
    }

    [Fact]
    public void Match_WithSuccessTuple4_DestructuresValues()
    {
        // Arrange
        var result = Result.Success((1, 2, 3, 4));

        // Act
        var output = result.Match(
            onSuccess: (a, b, c, d) => a + b + c + d,
            onFailure: err => 0
        );

        // Assert
        output.Should().Be(10);
    }

    [Fact]
    public void Match_WithSuccessTuple5_DestructuresValues()
    {
        // Arrange
        var result = Result.Success((1, 2, 3, 4, 5));

        // Act
        var output = result.Match(
            onSuccess: (a, b, c, d, e) => a + b + c + d + e,
            onFailure: err => 0
        );

        // Assert
        output.Should().Be(15);
    }

    [Fact]
    public void Switch_WithSuccessTuple2_DestructuresValues()
    {
        // Arrange
        var result = Result.Success((42, "hello"));
        var output = "";

        // Act
        result.Switch(
            onSuccess: (num, str) => output = $"{num}-{str}",
            onFailure: err => output = $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("42-hello");
    }

    [Fact]
    public void Switch_WithFailureTuple2_CallsOnFailure()
    {
        // Arrange
        var result = Result.Failure<(int, string)>(Error.NotFound("Not found"));
        var output = "";

        // Act
        result.Switch(
            onSuccess: (num, str) => output = $"{num}-{str}",
            onFailure: err => output = $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("Error: Not found");
    }

    [Fact]
    public void Switch_WithSuccessTuple3_DestructuresValues()
    {
        // Arrange
        var result = Result.Success((1, "two", 3.0));
        var output = "";

        // Act
        result.Switch(
            onSuccess: (i, s, d) => output = $"{i}-{s}-{d}",
            onFailure: err => output = $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("1-two-3");
    }

    [Fact]
    public void Switch_WithSuccessTuple4_DestructuresValues()
    {
        // Arrange
        var result = Result.Success((1, 2, 3, 4));
        var sum = 0;

        // Act
        result.Switch(
            onSuccess: (a, b, c, d) => sum = a + b + c + d,
            onFailure: err => sum = 0
        );

        // Assert
        sum.Should().Be(10);
    }

    [Fact]
    public void Switch_WithSuccessTuple5_DestructuresValues()
    {
        // Arrange
        var result = Result.Success((1, 2, 3, 4, 5));
        var sum = 0;

        // Act
        result.Switch(
            onSuccess: (a, b, c, d, e) => sum = a + b + c + d + e,
            onFailure: err => sum = 0
        );

        // Assert
        sum.Should().Be(15);
    }

    [Fact]
    public async Task MatchAsync_WithSuccessTuple2_DestructuresValues()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success((42, "hello")));

        // Act
        var output = await resultTask.MatchAsync(
            onSuccess: (num, str) => $"{num}-{str}",
            onFailure: err => $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("42-hello");
    }

    [Fact]
    public async Task MatchAsync_WithAsyncHandlers_DestructuresValues()
    {
        // Arrange
        var result = Result.Success((42, "hello"));

        // Act
        var output = await result.MatchAsync(
            onSuccess: async (num, str) =>
            {
                await Task.Delay(10);
                return $"{num}-{str}";
            },
            onFailure: async err =>
            {
                await Task.Delay(10);
                return $"Error: {err.Detail}";
            }
        );

        // Assert
        output.Should().Be("42-hello");
    }
}
