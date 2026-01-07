using FunctionalDdd.Testing;

namespace RailwayOrientedProgramming.Tests.Results.Extensions;

public class MapErrorTests : TestBase
{
    [Fact]
    public void MapError_transforms_failure_error()
    {
        var original = Result.Failure<int>(Error1);

        var mapped = original.MapError(e => Error.Conflict($"Wrapped: {e.Detail}"));

        mapped.Should().BeFailure();
        mapped.Error.Should().Be(Error.Conflict($"Wrapped: {Error1.Detail}"));
    }

    [Fact]
    public void MapError_does_not_touch_success()
    {
        var success = Result.Success(42);

        var mapped = success.MapError(e => Error.Unexpected("ShouldNotHappen"));

        mapped.Should().BeSuccess()
            .Which.Should().Be(42);
    }

    #region MapErrorAsync - Task<Result<T>> with Func<Error, Error>

    [Fact]
    public async Task MapErrorAsync_TaskResult_WithFunc_TransformsError()
    {
        var original = Result.Failure<int>(Error1).AsTask();

        var mapped = await original.MapErrorAsync(e => Error.Conflict($"Wrapped: {e.Detail}"));

        mapped.Should().BeFailure();
        mapped.Error.Should().BeOfType<ConflictError>();
    }

    [Fact]
    public async Task MapErrorAsync_TaskResult_WithFunc_DoesNotTouchSuccess()
    {
        var original = Result.Success(42).AsTask();

        var mapped = await original.MapErrorAsync(e => Error.Unexpected("ShouldNotHappen"));

        mapped.Should().BeSuccess()
            .Which.Should().Be(42);
    }

    #endregion

    #region MapErrorAsync - Result<T> with Func<Error, Task<Error>>

    [Fact]
    public async Task MapErrorAsync_Result_WithAsyncFunc_TransformsError()
    {
        var original = Result.Failure<int>(Error1);

        Func<Error, Task<Error>> mapper = async e =>
        {
            await Task.Delay(1);
            return Error.Conflict($"Wrapped: {e.Detail}");
        };

        var mapped = await original.MapErrorAsync(mapper);

        mapped.Should().BeFailure();
        mapped.Error.Should().BeOfType<ConflictError>();
    }

    [Fact]
    public async Task MapErrorAsync_Result_WithAsyncFunc_DoesNotTouchSuccess()
    {
        var original = Result.Success(42);

        Func<Error, Task<Error>> mapper = async e =>
        {
            await Task.Delay(1);
            return Error.Unexpected("ShouldNotHappen");
        };

        var mapped = await original.MapErrorAsync(mapper);

        mapped.Should().BeSuccess()
            .Which.Should().Be(42);
    }

    #endregion

    #region MapErrorAsync - Result<T> with CancellationToken

    [Fact]
    public async Task MapErrorAsync_Result_WithCancellation_TransformsError()
    {
        var original = Result.Failure<int>(Error1);
        using var cts = new CancellationTokenSource();

        Func<Error, CancellationToken, Task<Error>> mapper = async (e, ct) =>
        {
            await Task.Delay(1, ct);
            return Error.Conflict($"Wrapped: {e.Detail}");
        };

        var mapped = await original.MapErrorAsync(mapper, cts.Token);

        mapped.Should().BeFailure();
        mapped.Error.Should().BeOfType<ConflictError>();
    }

    [Fact]
    public async Task MapErrorAsync_Result_WithCancellation_DoesNotTouchSuccess()
    {
        var original = Result.Success(42);
        using var cts = new CancellationTokenSource();

        Func<Error, CancellationToken, Task<Error>> mapper = async (e, ct) =>
        {
            await Task.Delay(1, ct);
            return Error.Unexpected("ShouldNotHappen");
        };

        var mapped = await original.MapErrorAsync(mapper, cts.Token);

        mapped.Should().BeSuccess()
            .Which.Should().Be(42);
    }

    #endregion

    #region MapErrorAsync - Task<Result<T>> with Func<Error, Task<Error>>

    [Fact]
    public async Task MapErrorAsync_TaskResult_WithAsyncFunc_TransformsError()
    {
        var original = Result.Failure<int>(Error1).AsTask();

        var mapped = await original.MapErrorAsync(async e =>
        {
            await Task.Delay(1);
            return Error.Conflict($"Wrapped: {e.Detail}");
        });

        mapped.Should().BeFailure();
        mapped.Error.Should().BeOfType<ConflictError>();
    }

    [Fact]
    public async Task MapErrorAsync_TaskResult_WithCancellation_TransformsError()
    {
        var original = Result.Failure<int>(Error1).AsTask();
        using var cts = new CancellationTokenSource();

        var mapped = await original.MapErrorAsync(async (e, ct) =>
        {
            await Task.Delay(1, ct);
            return Error.Conflict($"Wrapped: {e.Detail}");
        }, cts.Token);

        mapped.Should().BeFailure();
        mapped.Error.Should().BeOfType<ConflictError>();
    }

    #endregion

    #region MapErrorAsync - ValueTask<Result<T>> with Func<Error, Error>

    [Fact]
    public async Task MapErrorAsync_ValueTaskResult_WithFunc_TransformsError()
    {
        var original = Result.Failure<int>(Error1).AsValueTask();

        var mapped = await original.MapErrorAsync(e => Error.Conflict($"Wrapped: {e.Detail}"));

        mapped.Should().BeFailure();
        mapped.Error.Should().BeOfType<ConflictError>();
    }

    [Fact]
    public async Task MapErrorAsync_ValueTaskResult_WithFunc_DoesNotTouchSuccess()
    {
        var original = Result.Success(42).AsValueTask();

        var mapped = await original.MapErrorAsync(e => Error.Unexpected("ShouldNotHappen"));

        mapped.Should().BeSuccess()
            .Which.Should().Be(42);
    }

    #endregion

    #region MapErrorAsync - Result<T> with Func<Error, ValueTask<Error>>

    [Fact]
    public async Task MapErrorAsync_Result_WithValueTaskFunc_TransformsError()
    {
        var original = Result.Failure<int>(Error1);

        var mapped = await original.MapErrorAsync(e =>
            ValueTask.FromResult<Error>(Error.Conflict($"Wrapped: {e.Detail}")));

        mapped.Should().BeFailure();
        mapped.Error.Should().BeOfType<ConflictError>();
    }

    [Fact]
    public async Task MapErrorAsync_Result_WithValueTaskFunc_DoesNotTouchSuccess()
    {
        var original = Result.Success(42);

        var mapped = await original.MapErrorAsync(e =>
            ValueTask.FromResult<Error>(Error.Unexpected("ShouldNotHappen")));

        mapped.Should().BeSuccess()
            .Which.Should().Be(42);
    }

    #endregion

    #region MapErrorAsync - Result<T> with ValueTask and CancellationToken

    [Fact]
    public async Task MapErrorAsync_Result_ValueTaskWithCancellation_TransformsError()
    {
        var original = Result.Failure<int>(Error1);
        using var cts = new CancellationTokenSource();

        Func<Error, CancellationToken, ValueTask<Error>> mapper = (e, ct) =>
            ValueTask.FromResult<Error>(Error.Conflict($"Wrapped: {e.Detail}"));

        var mapped = await original.MapErrorAsync(mapper, cts.Token);

        mapped.Should().BeFailure();
        mapped.Error.Should().BeOfType<ConflictError>();
    }

    [Fact]
    public async Task MapErrorAsync_Result_ValueTaskWithCancellation_DoesNotTouchSuccess()
    {
        var original = Result.Success(42);
        using var cts = new CancellationTokenSource();

        Func<Error, CancellationToken, ValueTask<Error>> mapper = (e, ct) =>
            ValueTask.FromResult<Error>(Error.Unexpected("ShouldNotHappen"));

        var mapped = await original.MapErrorAsync(mapper, cts.Token);

        mapped.Should().BeSuccess()
            .Which.Should().Be(42);
    }

    #endregion

    #region MapErrorAsync - ValueTask<Result<T>> with Func<Error, ValueTask<Error>>

    [Fact]
    public async Task MapErrorAsync_ValueTaskResult_WithValueTaskFunc_TransformsError()
    {
        var original = Result.Failure<int>(Error1).AsValueTask();

        var mapped = await original.MapErrorAsync(e =>
            ValueTask.FromResult<Error>(Error.Conflict($"Wrapped: {e.Detail}")));

        mapped.Should().BeFailure();
        mapped.Error.Should().BeOfType<ConflictError>();
    }

    [Fact]
    public async Task MapErrorAsync_ValueTaskResult_WithCancellation_TransformsError()
    {
        var original = Result.Failure<int>(Error1).AsValueTask();
        using var cts = new CancellationTokenSource();

        Func<Error, CancellationToken, ValueTask<Error>> mapper = (e, ct) =>
            ValueTask.FromResult<Error>(Error.Conflict($"Wrapped: {e.Detail}"));

        var mapped = await original.MapErrorAsync(mapper, cts.Token);

        mapped.Should().BeFailure();
        mapped.Error.Should().BeOfType<ConflictError>();
    }

    #endregion

    #region Real-World Scenarios

    [Fact]
    public void MapError_ConvertsDomainErrorToApiError()
    {
        // Arrange
        var domainResult = Result.Failure<string>(Error.Domain("Insufficient balance"));

        // Act
        var apiResult = domainResult.MapError(e => Error.BadRequest($"API Error: {e.Detail}"));

        // Assert
        apiResult.Should().BeFailure();
        apiResult.Error.Should().BeOfType<BadRequestError>();
        apiResult.Error.Detail.Should().Contain("Insufficient balance");
    }

    [Fact]
    public void MapError_AddsContextToError()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound("Entity not found"));

        // Act
        var enriched = result.MapError(e =>
            Error.NotFound($"User lookup failed: {e.Detail}", "user-123"));

        // Assert
        enriched.Should().BeFailure();
        enriched.Error.Instance.Should().Be("user-123");
    }

    [Fact]
    public async Task MapErrorAsync_ChainedErrorTransformations()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Unexpected("Internal error")).AsTask();

        // Act
        var transformed = await result
            .MapErrorAsync(e => Error.ServiceUnavailable($"Service error: {e.Detail}"));

        // Assert
        transformed.Should().BeFailure();
        transformed.Error.Should().BeOfType<ServiceUnavailableError>();
    }

    #endregion
}