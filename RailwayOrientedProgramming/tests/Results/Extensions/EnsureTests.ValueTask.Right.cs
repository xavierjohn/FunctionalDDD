namespace RailwayOrientedProgramming.Tests.Results.Extensions;

using FunctionalDdd;

public class Ensure_ValueTask_Right_Tests
{
    #region EnsureAsync with Func<ValueTask<bool>> predicate and static Error

    [Fact]
    public async Task EnsureAsync_Right_Bool_NoParam_StaticError_SuccessResult_PredicateTrue_ReturnsSuccess()
    {
        // Arrange
        var initialResult = Result.Success("Initial value");

        // Act
        var result = await initialResult.EnsureAsync(
            () => ValueTask.FromResult(true),
            Error.Validation("Should not fail"));

        // Assert
        result.IsSuccess.Should().BeTrue("predicate passed");
        result.Value.Should().Be("Initial value");
    }

    [Fact]
    public async Task EnsureAsync_Right_Bool_NoParam_StaticError_SuccessResult_PredicateFalse_ReturnsFailure()
    {
        // Arrange
        var initialResult = Result.Success("Initial value");

        // Act
        var result = await initialResult.EnsureAsync(
            () => ValueTask.FromResult(false),
            Error.Validation("Predicate check failed"));

        // Assert
        result.IsFailure.Should().BeTrue("predicate failed");
        result.Error.Should().Be(Error.Validation("Predicate check failed"));
    }

    [Fact]
    public async Task EnsureAsync_Right_Bool_NoParam_StaticError_FailureResult_PredicateNotInvoked_ReturnsOriginalFailure()
    {
        // Arrange
        var initialError = Error.NotFound("Initial error");
        var initialResult = Result.Failure<string>(initialError);
        var predicateInvoked = false;

        // Act
        var result = await initialResult.EnsureAsync(
            () =>
            {
                predicateInvoked = true;
                return ValueTask.FromResult(true);
            },
            Error.Validation("Should not see this error"));

        // Assert
        result.IsFailure.Should().BeTrue("initial result is failure");
        result.Error.Should().Be(initialError);
        predicateInvoked.Should().BeFalse("predicate should not be invoked for failed results");
    }

    [Fact]
    public async Task EnsureAsync_Right_Bool_NoParam_StaticError_WithComplexAsyncPredicate_WorksCorrectly()
    {
        // Arrange
        var initialResult = Result.Success(42);

        // Act
        var result = await initialResult.EnsureAsync(
            () => new ValueTask<bool>(true), // Explicitly use ValueTask
            Error.Validation("System check failed"));

        // Assert
        result.IsSuccess.Should().BeTrue("predicate passed");
        result.Value.Should().Be(42);
    }

    #endregion

    #region EnsureAsync with Func<TOk, ValueTask<bool>> predicate and static Error

    [Fact]
    public async Task EnsureAsync_Right_Bool_WithParam_StaticError_SuccessResult_PredicateTrue_ReturnsSuccess()
    {
        // Arrange
        var initialResult = Result.Success("test");

        // Act
        var result = await initialResult.EnsureAsync(
            value => ValueTask.FromResult(value.Length > 0),
            Error.Validation("Value should not be empty"));

        // Assert
        result.IsSuccess.Should().BeTrue("predicate passed");
        result.Value.Should().Be("test");
    }

    [Fact]
    public async Task EnsureAsync_Right_Bool_WithParam_StaticError_SuccessResult_PredicateFalse_ReturnsFailure()
    {
        // Arrange
        var initialResult = Result.Success("abc");

        // Act
        var result = await initialResult.EnsureAsync(
            value => ValueTask.FromResult(value.Length > 10),
            Error.Validation("String is too short"));

        // Assert
        result.IsFailure.Should().BeTrue("predicate failed");
        result.Error.Should().Be(Error.Validation("String is too short"));
    }

    [Fact]
    public async Task EnsureAsync_Right_Bool_WithParam_StaticError_FailureResult_PredicateNotInvoked_ReturnsOriginalFailure()
    {
        // Arrange
        var initialError = Error.Conflict("Initial conflict");
        var initialResult = Result.Failure<string>(initialError);
        var predicateInvoked = false;

        // Act
        var result = await initialResult.EnsureAsync(
            value =>
            {
                predicateInvoked = true;
                return ValueTask.FromResult(value.Length > 0);
            },
            Error.Validation("Should not see this error"));

        // Assert
        result.IsFailure.Should().BeTrue("initial result is failure");
        result.Error.Should().Be(initialError);
        predicateInvoked.Should().BeFalse("predicate should not be invoked for failed results");
    }

    [Fact]
    public async Task EnsureAsync_Right_Bool_WithParam_StaticError_PredicateUsesValue_WorksCorrectly()
    {
        // Arrange
        var initialResult = Result.Success(25);

        // Act
        var result = await initialResult.EnsureAsync(
            age => new ValueTask<bool>(age >= 18), // Explicitly use ValueTask
            Error.Validation("Must be 18 or older"));

        // Assert
        result.IsSuccess.Should().BeTrue("age is valid");
        result.Value.Should().Be(25);
    }

    #endregion

    #region EnsureAsync with Func<TOk, ValueTask<bool>> predicate and sync Error factory

    [Fact]
    public async Task EnsureAsync_Right_Bool_WithParam_ErrorFactory_SuccessResult_PredicateTrue_ReturnsSuccess()
    {
        // Arrange
        var initialResult = Result.Success(100);

        // Act
        var result = await initialResult.EnsureAsync(
            value => ValueTask.FromResult(value >= 0),
            value => Error.Validation($"Value {value} must be non-negative"));

        // Assert
        result.IsSuccess.Should().BeTrue("predicate passed");
        result.Value.Should().Be(100);
    }

    [Fact]
    public async Task EnsureAsync_Right_Bool_WithParam_ErrorFactory_SuccessResult_PredicateFalse_ReturnsContextualError()
    {
        // Arrange
        var initialResult = Result.Success(-5);

        // Act
        var result = await initialResult.EnsureAsync(
            value => ValueTask.FromResult(value >= 0),
            value => Error.Validation($"Value {value} must be non-negative"));

        // Assert
        result.IsFailure.Should().BeTrue("predicate failed");
        result.Error.Should().Be(Error.Validation("Value -5 must be non-negative"));
    }

    [Fact]
    public async Task EnsureAsync_Right_Bool_WithParam_ErrorFactory_FailureResult_ErrorFactoryNotInvoked()
    {
        // Arrange
        var initialError = Error.Unauthorized("Initial error");
        var initialResult = Result.Failure<int>(initialError);
        var errorFactoryInvoked = false;

        // Act
        var result = await initialResult.EnsureAsync(
            value => ValueTask.FromResult(value > 0),
            value =>
            {
                errorFactoryInvoked = true;
                return Error.Validation("Should not see this");
            });

        // Assert
        result.IsFailure.Should().BeTrue("initial result is failure");
        result.Error.Should().Be(initialError);
        errorFactoryInvoked.Should().BeFalse("error factory should not be invoked for failed results");
    }

    [Fact]
    public async Task EnsureAsync_Right_Bool_WithParam_ErrorFactory_ErrorFactoryReceivesValue()
    {
        // Arrange
        var initialResult = Result.Success("test@example");
        var capturedValue = string.Empty;

        // Act
        var result = await initialResult.EnsureAsync(
            value => ValueTask.FromResult(value.Contains('@') && value.Contains('.')),
            value =>
            {
                capturedValue = value;
                return Error.Validation($"Email '{value}' is invalid");
            });

        // Assert
        result.IsFailure.Should().BeTrue("email validation failed");
        capturedValue.Should().Be("test@example");
        result.Error.Detail.Should().Contain("test@example");
    }

    #endregion

    #region EnsureAsync with Func<TOk, ValueTask<bool>> predicate and async Error factory

    [Fact]
    public async Task EnsureAsync_Right_Bool_WithParam_AsyncErrorFactory_SuccessResult_PredicateTrue_ReturnsSuccess()
    {
        // Arrange
        var initialResult = Result.Success("valid-username");

        // Act
        var result = await initialResult.EnsureAsync(
            value => ValueTask.FromResult(value.Length >= 3),
            async value =>
            {
                await Task.Delay(1); // Simulate async error creation
                return Error.Validation($"Username '{value}' is too short");
            });

        // Assert
        result.IsSuccess.Should().BeTrue("predicate passed");
        result.Value.Should().Be("valid-username");
    }

    [Fact]
    public async Task EnsureAsync_Right_Bool_WithParam_AsyncErrorFactory_SuccessResult_PredicateFalse_ReturnsAsyncError()
    {
        // Arrange
        var initialResult = Result.Success("ab");

        // Act
        var result = await initialResult.EnsureAsync(
            value => ValueTask.FromResult(value.Length >= 3),
            async value =>
            {
                await Task.Delay(1); // Simulate async error creation
                return Error.Validation($"Username '{value}' must be at least 3 characters");
            });

        // Assert
        result.IsFailure.Should().BeTrue("predicate failed");
        result.Error.Detail.Should().Contain("ab");
        result.Error.Detail.Should().Contain("must be at least 3 characters");
    }

    [Fact]
    public async Task EnsureAsync_Right_Bool_WithParam_AsyncErrorFactory_FailureResult_AsyncErrorFactoryNotInvoked()
    {
        // Arrange
        var initialError = Error.Forbidden("Initial error");
        var initialResult = Result.Failure<string>(initialError);
        var asyncErrorFactoryInvoked = false;

        // Act
        var result = await initialResult.EnsureAsync(
            value => ValueTask.FromResult(value.Length > 0),
            async value =>
            {
                asyncErrorFactoryInvoked = true;
                await Task.Delay(1);
                return Error.Validation("Should not see this");
            });

        // Assert
        result.IsFailure.Should().BeTrue("initial result is failure");
        result.Error.Should().Be(initialError);
        asyncErrorFactoryInvoked.Should().BeFalse("async error factory should not be invoked for failed results");
    }

    [Fact]
    public async Task EnsureAsync_Right_Bool_WithParam_AsyncErrorFactory_ComplexAsyncOperation_WorksCorrectly()
    {
        // Arrange
        var initialResult = Result.Success(DateTime.UtcNow.AddDays(-1));

        // Act
        var result = await initialResult.EnsureAsync(
            value => new ValueTask<bool>(value <= DateTime.UtcNow), // Explicitly use ValueTask
            async value =>
            {
                await Task.Delay(1); // Simulate async error lookup
                return Error.Validation($"Date {value:yyyy-MM-dd} cannot be in the future");
            });

        // Assert
        result.IsSuccess.Should().BeTrue("date is in the past");
    }

    #endregion

    #region EnsureAsync with Func<ValueTask<Result<TOk>>> predicate (no parameter)

    [Fact]
    public async Task EnsureAsync_Right_Result_NoParam_SuccessResult_PredicateSuccess_ReturnsOriginalSuccess()
    {
        // Arrange
        var initialResult = Result.Success("Initial message");

        // Act
        var result = await initialResult.EnsureAsync(
            () => ValueTask.FromResult(Result.Success<string>("Validation passed")));

        // Assert
        result.IsSuccess.Should().BeTrue("initial result and predicate succeeded");
        result.Value.Should().Be("Initial message");
    }

    [Fact]
    public async Task EnsureAsync_Right_Result_NoParam_SuccessResult_PredicateFailure_ReturnsPredicateError()
    {
        // Arrange
        var initialResult = Result.Success("Initial value");
        var predicateError = Error.Validation("System validation failed");

        // Act
        var result = await initialResult.EnsureAsync(
            () => ValueTask.FromResult(Result.Failure<string>(predicateError)));

        // Assert
        result.IsFailure.Should().BeTrue("predicate returned failure");
        result.Error.Should().Be(predicateError);
    }

    [Fact]
    public async Task EnsureAsync_Right_Result_NoParam_FailureResult_PredicateNotInvoked_ReturnsOriginalFailure()
    {
        // Arrange
        var initialError = Error.RateLimit("Initial error");
        var initialResult = Result.Failure<string>(initialError);
        var predicateInvoked = false;

        // Act
        var result = await initialResult.EnsureAsync(
            () =>
            {
                predicateInvoked = true;
                return ValueTask.FromResult(Result.Success<string>("Should not reach"));
            });

        // Assert
        result.IsFailure.Should().BeTrue("initial result is failure");
        result.Error.Should().Be(initialError);
        predicateInvoked.Should().BeFalse("predicate should not be invoked for failed results");
    }

    [Fact]
    public async Task EnsureAsync_Right_Result_NoParam_ComplexAsyncPredicate_WorksCorrectly()
    {
        // Arrange
        var initialResult = Result.Success(42);

        // Act
        var result = await initialResult.EnsureAsync(
            () => new ValueTask<Result<int>>(
                Random.Shared.Next(0, 100) >= 0
                    ? Result.Success(Random.Shared.Next(0, 100))
                    : Result.Failure<int>(Error.ServiceUnavailable("System unavailable"))));

        // Assert
        result.IsSuccess.Should().BeTrue("system check passed");
        result.Value.Should().Be(42, "original value is preserved");
    }

    #endregion

    #region EnsureAsync with Func<TOk, ValueTask<Result<TOk>>> predicate (with parameter)

    [Fact]
    public async Task EnsureAsync_Right_Result_WithParam_SuccessResult_PredicateSuccess_ReturnsOriginalSuccess()
    {
        // Arrange
        var initialResult = Result.Success("Initial message");

        // Act
        var result = await initialResult.EnsureAsync(
            value => ValueTask.FromResult(Result.Success<string>($"Validated: {value}")));

        // Assert
        result.IsSuccess.Should().BeTrue("initial result and predicate succeeded");
        result.Value.Should().Be("Initial message");
    }

    [Fact]
    public async Task EnsureAsync_Right_Result_WithParam_SuccessResult_PredicateFailure_ReturnsPredicateError()
    {
        // Arrange
        var initialResult = Result.Success(15);
        var predicateError = Error.Validation("Age must be 18 or older");

        // Act
        var result = await initialResult.EnsureAsync(
            age => ValueTask.FromResult(
                age >= 18
                    ? Result.Success(age)
                    : Result.Failure<int>(predicateError)));

        // Assert
        result.IsFailure.Should().BeTrue("age validation failed");
        result.Error.Should().Be(predicateError);
    }

    [Fact]
    public async Task EnsureAsync_Right_Result_WithParam_FailureResult_PredicateNotInvoked_ReturnsOriginalFailure()
    {
        // Arrange
        var initialError = Error.Domain("Initial domain error");
        var initialResult = Result.Failure<int>(initialError);
        var predicateInvoked = false;

        // Act
        var result = await initialResult.EnsureAsync(
            value =>
            {
                predicateInvoked = true;
                return ValueTask.FromResult(Result.Success<int>(value));
            });

        // Assert
        result.IsFailure.Should().BeTrue("initial result is failure");
        result.Error.Should().Be(initialError);
        predicateInvoked.Should().BeFalse("predicate should not be invoked for failed results");
    }

    [Fact]
    public async Task EnsureAsync_Right_Result_WithParam_PredicateUsesValue_WorksCorrectly()
    {
        // Arrange
        var initialResult = Result.Success("user@example.com");

        // Act
        var result = await initialResult.EnsureAsync(
            email => new ValueTask<Result<string>>(
                email.Contains('@') && email.Contains('.')
                    ? Result.Success(email)
                    : Result.Failure<string>(Error.Validation($"Email '{email}' is invalid"))));

        // Assert
        result.IsSuccess.Should().BeTrue("email is valid");
        result.Value.Should().Be("user@example.com");
    }

    [Fact]
    public async Task EnsureAsync_Right_Result_WithParam_PredicateFailsWithContextualError_ReturnsContextualError()
    {
        // Arrange
        var initialResult = Result.Success("invalid-email");

        // Act
        var result = await initialResult.EnsureAsync(
            email => new ValueTask<Result<string>>(
                email.Contains('@') && email.Contains('.')
                    ? Result.Success(email)
                    : Result.Failure<string>(Error.Validation($"Email '{email}' must contain @ and ."))));

        // Assert
        result.IsFailure.Should().BeTrue("email validation failed");
        result.Error.Detail.Should().Contain("invalid-email");
        result.Error.Detail.Should().Contain("must contain @ and .");
    }

    [Fact]
    public async Task EnsureAsync_Right_Result_WithParam_ChainedEnsures_WorkCorrectly()
    {
        // Arrange
        var initialResult = Result.Success("valid-password-123");

        // Act
        var result = await initialResult
            .EnsureAsync(
                pwd => ValueTask.FromResult(
                    pwd.Length >= 8
                        ? Result.Success(pwd)
                        : Result.Failure<string>(Error.Validation("Password must be at least 8 characters"))))
            .EnsureAsync(
                pwd => ValueTask.FromResult(pwd.Any(char.IsDigit)),
                Error.Validation("Password must contain at least one digit"));

        // Assert
        result.IsSuccess.Should().BeTrue("password meets all requirements");
        result.Value.Should().Be("valid-password-123");
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Fact]
    public async Task EnsureAsync_Right_WithNullableType_HandlesNullCorrectly()
    {
        // Arrange
        int? nullValue = null;
        var initialResult = Result.Success(nullValue);

        // Act
        var result = await initialResult.EnsureAsync(
            value => ValueTask.FromResult(!value.HasValue),
            Error.Validation("Value should be null for this test"));

        // Assert
        result.IsSuccess.Should().BeTrue("null value passes the null check");
    }

    [Fact]
    public async Task EnsureAsync_Right_WithComplexType_WorksCorrectly()
    {
        // Arrange
        var person = new Person("John", 25);
        var initialResult = Result.Success(person);

        // Act
        var result = await initialResult.EnsureAsync(
            p => new ValueTask<bool>(p.Age >= 18), // Explicitly use ValueTask
            p => Error.Validation($"{p.Name} must be 18 or older"));

        // Assert
        result.IsSuccess.Should().BeTrue("person is adult");
        result.Value.Should().Be(person);
    }

    [Fact]
    public async Task EnsureAsync_Right_MultipleChainedEnsures_AllExecuteInOrder()
    {
        // Arrange
        var executionOrder = new List<int>();
        var initialResult = Result.Success(100);

        // Act
        var result = await initialResult
            .EnsureAsync(
                value =>
                {
                    executionOrder.Add(1);
                    return new ValueTask<bool>(value > 0); // Explicitly use ValueTask
                },
                Error.Validation("First check failed"))
            .EnsureAsync(
                value =>
                {
                    executionOrder.Add(2);
                    return new ValueTask<bool>(value < 1000); // Explicitly use ValueTask
                },
                Error.Validation("Second check failed"))
            .EnsureAsync(
                value =>
                {
                    executionOrder.Add(3);
                    return new ValueTask<bool>(value % 2 == 0); // Explicitly use ValueTask
                },
                Error.Validation("Third check failed"));

        // Assert
        result.IsSuccess.Should().BeTrue("all checks passed");
        executionOrder.Should().Equal([1, 2, 3], "ensures execute in order");
    }

    [Fact]
    public async Task EnsureAsync_Right_MultipleChainedEnsures_StopsAtFirstFailure()
    {
        // Arrange
        var executionOrder = new List<int>();
        var initialResult = Result.Success(100);

        // Act
        var result = await initialResult
            .EnsureAsync(
                value =>
                {
                    executionOrder.Add(1);
                    return new ValueTask<bool>(value > 0); // Explicitly use ValueTask
                },
                Error.Validation("First check failed"))
            .EnsureAsync(
                value =>
                {
                    executionOrder.Add(2);
                    return new ValueTask<bool>(value < 50); // This will fail - Explicitly use ValueTask
                },
                Error.Validation("Second check failed"))
            .EnsureAsync(
                value =>
                {
                    executionOrder.Add(3);
                    return new ValueTask<bool>(value % 2 == 0); // Explicitly use ValueTask
                },
                Error.Validation("Third check failed"));

        // Assert
        result.IsFailure.Should().BeTrue("second check failed");
        result.Error.Should().Be(Error.Validation("Second check failed"));
        executionOrder.Should().Equal([1, 2], "execution stops after first failure");
    }

    [Fact]
    public async Task EnsureAsync_Right_WithAsyncException_ThrowsException()
    {
        // Arrange
        var initialResult = Result.Success("test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await initialResult.EnsureAsync(
                value =>
                {
                    throw new InvalidOperationException("Simulated async failure");
#pragma warning disable CS0162 // Unreachable code detected
                    return new ValueTask<bool>(true);
#pragma warning restore CS0162 // Unreachable code detected
                },
                Error.Validation("Should not reach")));
    }

    [Fact]
    public async Task EnsureAsync_Right_MixingResultAndBooleanPredicates_WorksCorrectly()
    {
        // Arrange
        var initialResult = Result.Success("valid-input");

        // Act
        var result = await initialResult
            .EnsureAsync(
                value => new ValueTask<bool>(value.Length >= 5), // Explicitly use ValueTask
                Error.Validation("Too short"))
            .EnsureAsync(
                value => new ValueTask<Result<string>>(
                    value.Contains("valid")
                        ? Result.Success(value)
                        : Result.Failure<string>(Error.Validation("Must contain 'valid'")))); // Explicitly use ValueTask

        // Assert
        result.IsSuccess.Should().BeTrue("all validations passed");
        result.Value.Should().Be("valid-input");
    }

    #endregion

    private record Person(string Name, int Age);
}
