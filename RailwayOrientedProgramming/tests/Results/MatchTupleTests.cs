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

    [Fact]
    public async Task MatchAsync_WithCancellationToken_DestructuresValues()
    {
        // Arrange
        var result = Result.Success((42, "hello"));
        var cts = new CancellationTokenSource();

        // Act
        var output = await result.MatchAsync(
            onSuccess: async (num, str, ct) =>
            {
                await Task.Delay(10, ct);
                return $"{num}-{str}";
            },
            onFailure: async (err, ct) =>
            {
                await Task.Delay(10, ct);
                return $"Error: {err.Detail}";
            },
            cancellationToken: cts.Token
        );

        // Assert
        output.Should().Be("42-hello");
    }

    [Fact]
    public async Task MatchAsync_WithTuple3AndCancellationToken_DestructuresValues()
    {
        // Arrange
        var result = Result.Success((1, "two", 3.0));
        var cts = new CancellationTokenSource();

        // Act
        var output = await result.MatchAsync(
            onSuccess: async (i, s, d, ct) =>
            {
                await Task.Delay(10, ct);
                return $"{i}-{s}-{d}";
            },
            onFailure: async (err, ct) =>
            {
                await Task.Delay(10, ct);
                return $"Error: {err.Detail}";
            },
            cancellationToken: cts.Token
        );

        // Assert
        output.Should().Be("1-two-3");
    }

    [Fact]
    public async Task MatchAsync_WithTuple4AndCancellationToken_DestructuresValues()
    {
        // Arrange
        var result = Result.Success((1, 2, 3, 4));
        var cts = new CancellationTokenSource();

        // Act
        var output = await result.MatchAsync(
            onSuccess: async (a, b, c, d, ct) =>
            {
                await Task.Delay(10, ct);
                return a + b + c + d;
            },
            onFailure: async (err, ct) =>
            {
                await Task.Delay(10, ct);
                return 0;
            },
            cancellationToken: cts.Token
        );

        // Assert
        output.Should().Be(10);
    }

    [Fact]
    public async Task MatchAsync_WithTuple5AndCancellationToken_DestructuresValues()
    {
        // Arrange
        var result = Result.Success((1, 2, 3, 4, 5));
        var cts = new CancellationTokenSource();

        // Act
        var output = await result.MatchAsync(
            onSuccess: async (a, b, c, d, e, ct) =>
            {
                await Task.Delay(10, ct);
                return a + b + c + d + e;
            },
            onFailure: async (err, ct) =>
            {
                await Task.Delay(10, ct);
                return 0;
            },
            cancellationToken: cts.Token
        );

        // Assert
        output.Should().Be(15);
    }

    [Fact]
    public async Task SwitchAsync_WithTuple2AndCancellationToken_DestructuresValues()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success((42, "hello")));
        var output = "";
        var cts = new CancellationTokenSource();

        // Act
        await resultTask.SwitchAsync(
            onSuccess: async (num, str, ct) =>
            {
                await Task.Delay(10, ct);
                output = $"{num}-{str}";
            },
            onFailure: async (err, ct) =>
            {
                await Task.Delay(10, ct);
                output = $"Error: {err.Detail}";
            },
            cancellationToken: cts.Token
        );

        // Assert
        output.Should().Be("42-hello");
    }

    [Fact]
    public async Task SwitchAsync_WithTuple3_DestructuresValues()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success((1, "two", 3.0)));
        var output = "";
        var cts = new CancellationTokenSource();

        // Act
        await resultTask.SwitchAsync(
            onSuccess: async (i, s, d, ct) =>
            {
                await Task.Delay(10, ct);
                output = $"{i}-{s}-{d}";
            },
            onFailure: async (err, ct) =>
            {
                await Task.Delay(10, ct);
                output = $"Error: {err.Detail}";
            },
            cancellationToken: cts.Token
        );

        // Assert
        output.Should().Be("1-two-3");
    }

    [Fact]
    public async Task SwitchAsync_WithTuple4_DestructuresValues()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success((1, 2, 3, 4)));
        var sum = 0;
        var cts = new CancellationTokenSource();

        // Act
        await resultTask.SwitchAsync(
            onSuccess: async (a, b, c, d, ct) =>
            {
                await Task.Delay(10, ct);
                sum = a + b + c + d;
            },
            onFailure: async (err, ct) =>
            {
                await Task.Delay(10, ct);
                sum = 0;
            },
            cancellationToken: cts.Token
        );

        // Assert
        sum.Should().Be(10);
    }

    [Fact]
    public async Task SwitchAsync_WithTuple5_DestructuresValues()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success((1, 2, 3, 4, 5)));
        var sum = 0;
        var cts = new CancellationTokenSource();

        // Act
        await resultTask.SwitchAsync(
            onSuccess: async (a, b, c, d, e, ct) =>
            {
                await Task.Delay(10, ct);
                sum = a + b + c + d + e;
            },
            onFailure: async (err, ct) =>
            {
                await Task.Delay(10, ct);
                sum = 0;
            },
            cancellationToken: cts.Token
        );

        // Assert
        sum.Should().Be(15);
    }

    [Fact]
    public async Task MatchAsync_WithFailureTuple_CallsOnFailure()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Failure<(int, string)>(Error.NotFound("Not found")));
        var cts = new CancellationTokenSource();

        // Act
        var output = await resultTask.MatchAsync(
            onSuccess: async (num, str, ct) =>
            {
                await Task.Delay(10, ct);
                return $"{num}-{str}";
            },
            onFailure: async (err, ct) =>
            {
                await Task.Delay(10, ct);
                return $"Error: {err.Detail}";
            },
            cancellationToken: cts.Token
        );

        // Assert
        output.Should().Be("Error: Not found");
    }

    [Fact]
    public async Task SwitchAsync_WithFailureTuple_CallsOnFailure()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Failure<(int, string)>(Error.NotFound("Not found")));
        var output = "";
        var cts = new CancellationTokenSource();

        // Act
        await resultTask.SwitchAsync(
            onSuccess: async (num, str, ct) =>
            {
                await Task.Delay(10, ct);
                output = $"{num}-{str}";
            },
            onFailure: async (err, ct) =>
            {
                await Task.Delay(10, ct);
                output = $"Error: {err.Detail}";
            },
            cancellationToken: cts.Token
        );

        // Assert
        output.Should().Be("Error: Not found");
    }
}
