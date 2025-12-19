namespace RailwayOrientedProgramming.Tests.Results;

using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FunctionalDdd;
using Xunit;

public class CancellationTokenTests
{
    [Fact]
    public async Task TryAsync_WithCancellationToken_ShouldPassTokenToFunction()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var tokenPassed = false;

        // Act
        var result = await Result.TryAsync(async ct =>
        {
            tokenPassed = ct == cts.Token;
            await Task.Delay(1, ct);
            return 42;
        }, cancellationToken: cts.Token);

        // Assert
        tokenPassed.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TryAsync_WithCancellation_ShouldReturnFailureResult()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await Result.TryAsync(async ct =>
        {
            await Task.Delay(1000, ct); // This will throw
            return 42;
        }, cancellationToken: cts.Token);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_WithCancellationToken_ShouldPassTokenToFunction()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var tokenPassed = false;

        Task<Result<int>> BindFunction(int value, CancellationToken ct)
        {
            tokenPassed = ct == cts.Token;
            return Task.FromResult(Result.Success(value * 2));
        }

        // Act
        var result = await Result.Success(10)
            .BindAsync(BindFunction, cts.Token);

        // Assert
        tokenPassed.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(20);
    }

    [Fact]
    public async Task MapAsync_WithCancellationToken_ShouldPassTokenToFunction()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var tokenPassed = false;

        Task<int> MapFunction(int value, CancellationToken ct)
        {
            tokenPassed = ct == cts.Token;
            return Task.FromResult(value * 2);
        }

        // Act
        var result = await Result.Success(10)
            .MapAsync(MapFunction, cts.Token);

        // Assert
        tokenPassed.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(20);
    }

    [Fact]
    public async Task TapAsync_WithCancellationToken_ShouldPassTokenToFunction()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var tokenPassed = false;

        Task TapFunction(int value, CancellationToken ct)
        {
            tokenPassed = ct == cts.Token;
            return Task.CompletedTask;
        }

        // Act
        var result = await Result.Success(10)
            .TapAsync(TapFunction, cts.Token);

        // Assert
        tokenPassed.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task EnsureAsync_WithCancellationToken_ShouldPassTokenToFunction()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var tokenPassed = false;

        // Act
        var result = await Task.FromResult(Result.Success(10))
            .EnsureAsync(async (value, ct) =>
            {
                tokenPassed = ct == cts.Token;
                await Task.Delay(1, ct);
                return value > 5;
            }, Error.Validation("Value too small"), cts.Token);

        // Assert
        tokenPassed.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task CompensateAsync_WithCancellationToken_ShouldPassTokenToFunction()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var tokenPassed = false;

        // Act
        var result = await Result.Failure<int>(Error.NotFound("Not found"))
            .CompensateAsync(async ct =>
            {
                tokenPassed = ct == cts.Token;
                await Task.Delay(1, ct);
                return Result.Success(42);
            }, cts.Token);

        // Assert
        tokenPassed.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task MatchAsync_WithCancellationToken_ShouldPassTokenToFunction()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var tokenPassed = false;

        // Act
        var output = await Result.Success(10)
            .MatchAsync(
                async (int value, CancellationToken ct) =>
                {
                    tokenPassed = ct == cts.Token;
                    await Task.Delay(1, ct);
                    return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                },
                async (Error error, CancellationToken ct) =>
                {
                    await Task.Delay(1, ct);
                    return error.Detail;
                },
                cts.Token);

        // Assert
        tokenPassed.Should().BeTrue();
        output.Should().Be("10");
    }

    [Fact]
    public async Task MatchAsync_WithCancellationToken_AndResultCheck_ShouldPassTokenToFunction()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var tokenPassed = false;

        // Act
        var output = await Task.FromResult(Result.Success(10))
            .MatchAsync(
                async (value, ct) =>
                {
                    tokenPassed = ct == cts.Token;
                    await Task.Delay(1, ct);
                    return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                },
                async (error, ct) =>
                {
                    await Task.Delay(1, ct);
                    return "error";
                },
                cts.Token);

        // Assert
        tokenPassed.Should().BeTrue();
        output.Should().Be("10");
    }

    [Fact]
    public async Task SuccessIfAsync_WithCancellationToken_ShouldPassTokenToPredicate()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var tokenPassed = false;

        // Act
        var result = await Result.SuccessIfAsync(
            async ct =>
            {
                tokenPassed = ct == cts.Token;
                await Task.Delay(1, ct);
                return true;
            },
            42,
            Error.Unexpected("Failed"),
            cts.Token);

        // Assert
        tokenPassed.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task FailureIfAsync_WithCancellationToken_ShouldPassTokenToPredicate()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var tokenPassed = false;

        // Act
        var result = await Result.FailureIfAsync(
            async ct =>
            {
                tokenPassed = ct == cts.Token;
                await Task.Delay(1, ct);
                return false;
            },
            42,
            Error.Unexpected("Failed"),
            cts.Token);

        // Assert
        tokenPassed.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task ChainedOperations_WithCancellationToken_ShouldPropagateToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var bindTokenPassed = false;
        var mapTokenPassed = false;
        var tapTokenPassed = false;

        Task<Result<int>> BindFunction(int value, CancellationToken ct)
        {
            bindTokenPassed = ct == cts.Token;
            return Task.FromResult(Result.Success(value * 2));
        }

        Task<int> MapFunction(int value, CancellationToken ct)
        {
            mapTokenPassed = ct == cts.Token;
            return Task.FromResult(value + 5);
        }

        Task TapFunction(int value, CancellationToken ct)
        {
            tapTokenPassed = ct == cts.Token;
            return Task.CompletedTask;
        }

        // Act
        var result = await Result.Success(10)
            .BindAsync(BindFunction, cts.Token)
            .MapAsync(MapFunction, cts.Token)
            .TapAsync(TapFunction, cts.Token);

        // Assert
        bindTokenPassed.Should().BeTrue();
        mapTokenPassed.Should().BeTrue();
        tapTokenPassed.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(25);
    }

    [Fact]
    public async Task TapErrorAsync_WithCancellationToken_ShouldPassTokenToFunction()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var tokenPassed = false;

        Task TapErrorFunction(Error error, CancellationToken ct)
        {
            tokenPassed = ct == cts.Token;
            return Task.CompletedTask;
        }

        // Act
        var result = await Result.Failure<int>(Error.NotFound("Not found"))
            .TapErrorAsync(TapErrorFunction, cts.Token);

        // Assert
        tokenPassed.Should().BeTrue();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TapErrorAsync_OnTaskResult_WithCancellationToken_ShouldPassToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var tokenPassed = false;

        Task TapErrorFunction(Error error, CancellationToken ct)
        {
            tokenPassed = ct == cts.Token;
            return Task.CompletedTask;
        }

        // Act
        var result = await Task.FromResult(Result.Failure<int>(Error.NotFound("Not found")))
            .TapErrorAsync(TapErrorFunction, cts.Token);

        // Assert
        tokenPassed.Should().BeTrue();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TapErrorAsync_OnTaskResult_WithCancellationToken_NoError_ShouldPassToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var tokenPassed = false;

        Task TapErrorFunction(CancellationToken ct)
        {
            tokenPassed = ct == cts.Token;
            return Task.CompletedTask;
        }

        // Act
        var result = await Task.FromResult(Result.Failure<int>(Error.NotFound("Not found")))
            .TapErrorAsync(TapErrorFunction, cts.Token);

        // Assert
        tokenPassed.Should().BeTrue();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TapErrorAsync_OnValueTaskResult_WithCancellationToken_ShouldPassToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var tokenPassed = false;

        ValueTask TapErrorFunction(Error error, CancellationToken ct)
        {
            tokenPassed = ct == cts.Token;
            return ValueTask.CompletedTask;
        }

        // Act
        var result = await ValueTask.FromResult(Result.Failure<int>(Error.NotFound("Not found")))
            .TapErrorAsync(TapErrorFunction, cts.Token);

        // Assert
        tokenPassed.Should().BeTrue();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TapErrorAsync_OnValueTaskResult_WithCancellationToken_NoError_ShouldPassToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var tokenPassed = false;

        ValueTask TapErrorFunction(CancellationToken ct)
        {
            tokenPassed = ct == cts.Token;
            return ValueTask.CompletedTask;
        }

        // Act
        var result = await ValueTask.FromResult(Result.Failure<int>(Error.NotFound("Not found")))
            .TapErrorAsync(TapErrorFunction, cts.Token);

        // Assert
        tokenPassed.Should().BeTrue();
        result.IsFailure.Should().BeTrue();
    }
}
