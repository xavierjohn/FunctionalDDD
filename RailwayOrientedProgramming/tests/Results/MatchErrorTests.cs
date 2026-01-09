namespace RailwayOrientedProgramming.Tests;

using FluentAssertions;
using FunctionalDdd.Testing;
using Xunit;

public class MatchErrorTests
{
    [Fact]
    public void MatchError_WithSuccessResult_CallsOnSuccess()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var output = result.MatchError(
            onSuccess: value => $"Success: {value}",
            onError: err => $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("Success: 42");
    }

    [Fact]
    public void MatchError_WithValidationError_CallsOnValidation()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Validation("Invalid email", "email"));

        // Act
        var output = result.MatchError(
            onSuccess: value => $"Success: {value}",
            onValidation: err => $"Validation: {err.Detail}",
            onError: err => $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("Validation: Invalid email");
        result.Should().BeFailureOfType<ValidationError>();
    }

    [Fact]
    public void MatchError_WithNotFoundError_CallsOnNotFound()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound("User not found"));

        // Act
        var output = result.MatchError(
            onSuccess: value => $"Success: {value}",
            onNotFound: err => $"NotFound: {err.Detail}",
            onError: err => $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("NotFound: User not found");
        result.Should().BeFailureOfType<NotFoundError>()
            .Which.Should().HaveDetail("User not found");
    }

    [Fact]
    public void MatchError_WithConflictError_CallsOnConflict()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Conflict("Email already exists"));

        // Act
        var output = result.MatchError(
            onSuccess: value => $"Success: {value}",
            onConflict: err => $"Conflict: {err.Detail}",
            onError: err => $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("Conflict: Email already exists");
        result.Should().BeFailureOfType<ConflictError>();
    }

    [Fact]
    public void MatchError_WithBadRequestError_CallsOnBadRequest()
    {
        // Arrange
        var result = Result.Failure<int>(Error.BadRequest("Invalid request"));

        // Act
        var output = result.MatchError(
            onSuccess: value => $"Success: {value}",
            onBadRequest: err => $"BadRequest: {err.Detail}",
            onError: err => $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("BadRequest: Invalid request");
    }

    [Fact]
    public void MatchError_WithUnauthorizedError_CallsOnUnauthorized()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Unauthorized("Not authenticated"));

        // Act
        var output = result.MatchError(
            onSuccess: value => $"Success: {value}",
            onUnauthorized: err => $"Unauthorized: {err.Detail}",
            onError: err => $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("Unauthorized: Not authenticated");
    }

    [Fact]
    public void MatchError_WithForbiddenError_CallsOnForbidden()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Forbidden("Access denied"));

        // Act
        var output = result.MatchError(
            onSuccess: value => $"Success: {value}",
            onForbidden: err => $"Forbidden: {err.Detail}",
            onError: err => $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("Forbidden: Access denied");
    }

    [Fact]
    public void MatchError_WithDomainError_CallsOnDomain()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Domain("Business rule violated"));

        // Act
        var output = result.MatchError(
            onSuccess: value => $"Success: {value}",
            onDomain: err => $"Domain: {err.Detail}",
            onError: err => $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("Domain: Business rule violated");
    }

    [Fact]
    public void MatchError_WithRateLimitError_CallsOnRateLimit()
    {
        // Arrange
        var result = Result.Failure<int>(Error.RateLimit("Too many requests"));

        // Act
        var output = result.MatchError(
            onSuccess: value => $"Success: {value}",
            onRateLimit: err => $"RateLimit: {err.Detail}",
            onError: err => $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("RateLimit: Too many requests");
    }

    [Fact]
    public void MatchError_WithServiceUnavailableError_CallsOnServiceUnavailable()
    {
        // Arrange
        var result = Result.Failure<int>(Error.ServiceUnavailable("Service down"));

        // Act
        var output = result.MatchError(
            onSuccess: value => $"Success: {value}",
            onServiceUnavailable: err => $"ServiceUnavailable: {err.Detail}",
            onError: err => $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("ServiceUnavailable: Service down");
    }

    [Fact]
    public void MatchError_WithUnexpectedError_CallsOnUnexpected()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Unexpected("Something went wrong"));

        // Act
        var output = result.MatchError(
            onSuccess: value => $"Success: {value}",
            onUnexpected: err => $"Unexpected: {err.Detail}",
            onError: err => $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("Unexpected: Something went wrong");
    }

    [Fact]
    public void MatchError_WithNoSpecificHandler_CallsOnError()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound("Not found"));

        // Act
        var output = result.MatchError(
            onSuccess: value => $"Success: {value}",
            onError: err => $"Default: {err.Detail}"
        );

        // Assert
        output.Should().Be("Default: Not found");
    }

    [Fact]
    public void MatchError_WithNoHandlerForErrorType_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound("Not found"));

        // Act
        var act = () => result.MatchError(
            onSuccess: value => $"Success: {value}"
            // No error handlers provided
        );

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No handler provided for error type NotFoundError*");
    }

    [Fact]
    public void SwitchError_WithSuccessResult_CallsOnSuccess()
    {
        // Arrange
        var result = Result.Success(42);
        var output = "";

        // Act
        result.SwitchError(
            onSuccess: value => output = $"Success: {value}",
            onError: err => output = $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("Success: 42");
    }

    [Fact]
    public void SwitchError_WithValidationError_CallsOnValidation()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Validation("Invalid email", "email"));
        var output = "";

        // Act
        result.SwitchError(
            onSuccess: value => output = $"Success: {value}",
            onValidation: err => output = $"Validation: {err.Detail}",
            onError: err => output = $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("Validation: Invalid email");
    }

    [Fact]
    public async Task MatchErrorAsync_WithSuccessResult_CallsOnSuccess()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(42));

        // Act
        var output = await resultTask.MatchErrorAsync(
            onSuccess: value => $"Success: {value}",
            onError: err => $"Error: {err.Detail}"
        );

        // Assert
        output.Should().Be("Success: 42");
    }

    [Fact]
    public async Task MatchErrorAsync_WithTaskResultError_UsesDefaultHandler()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Failure<int>(Error.NotFound("Missing")));

        // Act
        var output = await resultTask.MatchErrorAsync(
            onSuccess: value => $"Success: {value}",
            onError: err => $"Default: {err.Detail}"
        );

        // Assert
        output.Should().Be("Default: Missing");
    }

    [Fact]
    public async Task MatchErrorAsync_WithTaskResultAndNoHandlers_Throws()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Failure<int>(Error.Forbidden("denied")));

        // Act
        var act = async () => await resultTask.MatchErrorAsync(
            onSuccess: value => $"Success: {value}"
        );

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No handler provided for error type ForbiddenError*");
    }

    [Fact]
    public async Task MatchErrorAsync_WithAsyncHandlers_WorksCorrectly()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var output = await result.MatchErrorAsync(
            onSuccess: async (value, ct) =>
            {
                await Task.Delay(10, ct);
                return $"Success: {value}";
            },
            onError: async (err, ct) =>
            {
                await Task.Delay(10, ct);
                return $"Error: {err.Detail}";
            }
        );

        // Assert
        output.Should().Be("Success: 42");
    }

    [Fact]
    public async Task MatchErrorAsync_WithValidationError_CallsOnValidation()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Validation("Invalid", "field"));

        // Act
        var output = await result.MatchErrorAsync(
            onSuccess: async (value, ct) =>
            {
                await Task.Delay(10, ct);
                return $"Success: {value}";
            },
            onValidation: async (err, ct) =>
            {
                await Task.Delay(10, ct);
                return $"Validation: {err.Detail}";
            },
            onError: async (err, ct) =>
            {
                await Task.Delay(10, ct);
                return $"Error: {err.Detail}";
            }
        );

        // Assert
        output.Should().Be("Validation: Invalid");
    }

    [Fact]
    public async Task MatchErrorAsync_WithAsyncHandlers_DefaultsToOnError()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Domain("rule broken"));

        // Act
        var output = await result.MatchErrorAsync(
            onSuccess: async (value, ct) =>
            {
                await Task.Delay(1, ct);
                return $"Success: {value}";
            },
            onError: async (err, ct) =>
            {
                await Task.Delay(1, ct);
                return $"Default: {err.Detail}";
            }
        );

        // Assert
        output.Should().Be("Default: rule broken");
    }

    [Fact]
    public async Task SwitchErrorAsync_WithSuccessResult_CallsOnSuccess()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(42));
        var output = "";

        // Act
        await resultTask.SwitchErrorAsync(
            onSuccess: async (value, ct) =>
            {
                await Task.Delay(10, ct);
                output = $"Success: {value}";
            },
            onError: async (err, ct) =>
            {
                await Task.Delay(10, ct);
                output = $"Error: {err.Detail}";
            }
        );

        // Assert
        output.Should().Be("Success: 42");
    }

    [Fact]
    public async Task SwitchErrorAsync_WithNotFoundError_CallsOnNotFound()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Failure<int>(Error.NotFound("Not found")));
        var output = "";

        // Act
        await resultTask.SwitchErrorAsync(
            onSuccess: async (value, ct) =>
            {
                await Task.Delay(10, ct);
                output = $"Success: {value}";
            },
            onNotFound: async (err, ct) =>
            {
                await Task.Delay(10, ct);
                output = $"NotFound: {err.Detail}";
            },
            onError: async (err, ct) =>
            {
                await Task.Delay(10, ct);
                output = $"Error: {err.Detail}";
            }
        );

        // Assert
        output.Should().Be("NotFound: Not found");
    }

    [Fact]
    public async Task SwitchErrorAsync_WithNoSpecificHandler_UsesOnError()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Failure<int>(Error.Unexpected("boom")));
        var output = "";

        // Act
        await resultTask.SwitchErrorAsync(
            onSuccess: async (value, ct) =>
            {
                await Task.Delay(1, ct);
                output = $"Success: {value}";
            },
            onError: async (err, ct) =>
            {
                await Task.Delay(1, ct);
                output = $"Default: {err.Detail}";
            }
        );

        // Assert
        output.Should().Be("Default: boom");
    }

    [Fact]
    public async Task SwitchErrorAsync_WithNoHandlers_Throws()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Failure<int>(Error.BadRequest("invalid")));

        // Act
        var act = async () => await resultTask.SwitchErrorAsync(
            onSuccess: async (value, ct) => await Task.Delay(1, ct)
        );

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No handler provided for error type BadRequestError*");
    }
}
