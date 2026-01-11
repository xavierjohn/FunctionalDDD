namespace RailwayOrientedProgramming.Tests.Results.Extensions;

using FunctionalDdd;

public class Ensure_ValueTask_Tests
{
    #region EnsureAsync with ValueTask<bool> predicate and static Error

    [Fact]
    public async Task EnsureAsync_ValueTask_Bool_StaticError_SuccessResult_PredicateTrue_ReturnsSuccess()
    {
        // Arrange
        var initialResult = ValueTask.FromResult(Result.Success("Initial value"));

        // Act
        var result = await initialResult.EnsureAsync(
            value => ValueTask.FromResult(value.Length > 0),
            Error.Validation("Value should not be empty"));

        // Assert
        result.IsSuccess.Should().BeTrue("predicate passed");
        result.Value.Should().Be("Initial value");
    }

    [Fact]
    public async Task EnsureAsync_ValueTask_Bool_StaticError_SuccessResult_PredicateFalse_ReturnsFailure()
    {
        // Arrange
        var initialResult = ValueTask.FromResult(Result.Success("Initial value"));

        // Act
        var result = await initialResult.EnsureAsync(
            value => ValueTask.FromResult(value.Length > 100),
            Error.Validation("Value is too short"));

        // Assert
        result.IsFailure.Should().BeTrue("predicate failed");
        result.Error.Should().Be(Error.Validation("Value is too short"));
    }

    [Fact]
    public async Task EnsureAsync_ValueTask_Bool_StaticError_FailureResult_PredicateNotInvoked_ReturnsOriginalFailure()
    {
        // Arrange
        var initialError = Error.NotFound("Initial error");
        var initialResult = ValueTask.FromResult(Result.Failure<string>(initialError));
        var predicateInvoked = false;

        // Act
        var result = await initialResult.EnsureAsync(
            value =>
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
    public async Task EnsureAsync_ValueTask_Bool_StaticError_WithComplexPredicate_ReturnsExpectedResult()
    {
        // Arrange
        var initialResult = ValueTask.FromResult(Result.Success(42));

        // Act
        var result = await initialResult.EnsureAsync(
            async value =>
            {
                await Task.Delay(1); // Simulate async operation
                return value is > 0 and < 100;
            },
            Error.Validation("Value must be between 0 and 100"));

        // Assert
        result.IsSuccess.Should().BeTrue("value is within range");
        result.Value.Should().Be(42);
    }

    #endregion

    #region EnsureAsync with ValueTask<bool> predicate and Error factory (sync)

    [Fact]
    public async Task EnsureAsync_ValueTask_Bool_ErrorFactory_SuccessResult_PredicateTrue_ReturnsSuccess()
    {
        // Arrange
        var initialResult = ValueTask.FromResult(Result.Success(100));

        // Act
        var result = await initialResult.EnsureAsync(
            value => ValueTask.FromResult(value >= 0),
            value => Error.Validation($"Value {value} must be non-negative"));

        // Assert
        result.IsSuccess.Should().BeTrue("predicate passed");
        result.Value.Should().Be(100);
    }

    [Fact]
    public async Task EnsureAsync_ValueTask_Bool_ErrorFactory_SuccessResult_PredicateFalse_ReturnsFailureWithContextualError()
    {
        // Arrange
        var initialResult = ValueTask.FromResult(Result.Success(-5));

        // Act
        var result = await initialResult.EnsureAsync(
            value => ValueTask.FromResult(value >= 0),
            value => Error.Validation($"Value {value} must be non-negative"));

        // Assert
        result.IsFailure.Should().BeTrue("predicate failed");
        result.Error.Should().Be(Error.Validation("Value -5 must be non-negative"));
    }

    [Fact]
    public async Task EnsureAsync_ValueTask_Bool_ErrorFactory_FailureResult_ErrorFactoryNotInvoked_ReturnsOriginalFailure()
    {
        // Arrange
        var initialError = Error.Conflict("Initial error");
        var initialResult = ValueTask.FromResult(Result.Failure<int>(initialError));
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
    public async Task EnsureAsync_ValueTask_Bool_ErrorFactory_PredicateFalse_ErrorFactoryReceivesValue()
    {
        // Arrange
        var initialResult = ValueTask.FromResult(Result.Success("test@example"));
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
        result.IsFailure.Should().BeTrue("email is invalid");
        capturedValue.Should().Be("test@example");
        result.Error.Detail.Should().Contain("test@example");
    }

    #endregion

    #region EnsureAsync with ValueTask<bool> predicate and async Error factory

    [Fact]
    public async Task EnsureAsync_ValueTask_Bool_AsyncErrorFactory_SuccessResult_PredicateTrue_ReturnsSuccess()
    {
        // Arrange
        var initialResult = ValueTask.FromResult(Result.Success("valid-username"));

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
    public async Task EnsureAsync_ValueTask_Bool_AsyncErrorFactory_SuccessResult_PredicateFalse_ReturnsFailureWithAsyncError()
    {
        // Arrange
        var initialResult = ValueTask.FromResult(Result.Success("ab"));

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
    public async Task EnsureAsync_ValueTask_Bool_AsyncErrorFactory_FailureResult_AsyncErrorFactoryNotInvoked_ReturnsOriginalFailure()
    {
        // Arrange
        var initialError = Error.Unauthorized("Initial error");
        var initialResult = ValueTask.FromResult(Result.Failure<string>(initialError));
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
    public async Task EnsureAsync_ValueTask_Bool_AsyncErrorFactory_ComplexAsyncOperation_WorksCorrectly()
    {
        // Arrange
        var initialResult = ValueTask.FromResult(Result.Success(DateTime.UtcNow.AddDays(-1)));

        // Act
        var result = await initialResult.EnsureAsync(
            async value =>
            {
                await Task.Delay(1); // Simulate async validation
                return value <= DateTime.UtcNow;
            },
            async value =>
            {
                await Task.Delay(1); // Simulate async error lookup/translation
                return Error.Validation($"Date {value:yyyy-MM-dd} cannot be in the future");
            });

        // Assert
        result.IsSuccess.Should().BeTrue("date is in the past");
    }

    #endregion

    #region EnsureAsync with ValueTask<Result<TOk>> predicate (no parameter)

    [Fact]
    public async Task EnsureAsync_ValueTask_Result_NoParam_SuccessResult_PredicateSuccess_ReturnsOriginalSuccess()
    {
        // Arrange
        var initialResult = ValueTask.FromResult(Result.Success("Initial message"));

        // Act
        var result = await initialResult.EnsureAsync(
            () => ValueTask.FromResult(Result.Success<string>("Validation passed")));

        // Assert
        result.IsSuccess.Should().BeTrue("initial result and predicate succeeded");
        result.Value.Should().Be("Initial message");
    }

    [Fact]
    public async Task EnsureAsync_ValueTask_Result_NoParam_SuccessResult_PredicateFailure_ReturnsPredicateError()
    {
        // Arrange
        var initialResult = ValueTask.FromResult(Result.Success("Initial value"));
        var predicateError = Error.Validation("Validation failed");

        // Act
        var result = await initialResult.EnsureAsync(
            () => ValueTask.FromResult(Result.Failure<string>(predicateError)));

        // Assert
        result.IsFailure.Should().BeTrue("predicate returned failure");
        result.Error.Should().Be(predicateError);
    }

    [Fact]
    public async Task EnsureAsync_ValueTask_Result_NoParam_FailureResult_PredicateNotInvoked_ReturnsOriginalFailure()
    {
        // Arrange
        var initialError = Error.NotFound("Initial error");
        var initialResult = ValueTask.FromResult(Result.Failure<string>(initialError));
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
    public async Task EnsureAsync_ValueTask_Result_NoParam_ComplexAsyncPredicate_WorksCorrectly()
    {
        // Arrange
        var initialResult = ValueTask.FromResult(Result.Success(42));

        // Act
        var result = await initialResult.EnsureAsync(
            async () =>
            {
                await Task.Delay(1); // Simulate async validation
                var systemCheck = Random.Shared.Next(0, 100);
                return systemCheck >= 0
                    ? Result.Success(systemCheck)
                    : Result.Failure<int>(Error.ServiceUnavailable("System unavailable"));
            });

        // Assert
        result.IsSuccess.Should().BeTrue("system check passed");
        result.Value.Should().Be(42, "original value is preserved");
    }

    #endregion

    #region EnsureAsync with ValueTask<Result<TOk>> predicate (with parameter)

    [Fact]
    public async Task EnsureAsync_ValueTask_Result_WithParam_SuccessResult_PredicateSuccess_ReturnsOriginalSuccess()
    {
        // Arrange
        var initialResult = ValueTask.FromResult(Result.Success("Initial message"));

        // Act
        var result = await initialResult.EnsureAsync(
            value => ValueTask.FromResult(Result.Success<string>($"Validated: {value}")));

        // Assert
        result.IsSuccess.Should().BeTrue("initial result and predicate succeeded");
        result.Value.Should().Be("Initial message");
    }

    [Fact]
    public async Task EnsureAsync_ValueTask_Result_WithParam_SuccessResult_PredicateFailure_ReturnsPredicateError()
    {
        // Arrange
        var initialResult = ValueTask.FromResult(Result.Success(15));
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
    public async Task EnsureAsync_ValueTask_Result_WithParam_FailureResult_PredicateNotInvoked_ReturnsOriginalFailure()
    {
        // Arrange
        var initialError = Error.Conflict("Initial conflict");
        var initialResult = ValueTask.FromResult(Result.Failure<int>(initialError));
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
    public async Task EnsureAsync_ValueTask_Result_WithParam_PredicateUsesValue_WorksCorrectly()
    {
        // Arrange
        var initialResult = ValueTask.FromResult(Result.Success("user@example.com"));

        // Act
        var result = await initialResult.EnsureAsync(
            async email =>
            {
                await Task.Delay(1); // Simulate async email validation
                var isValid = email.Contains('@') && email.Contains('.');
                return isValid
                    ? Result.Success(email)
                    : Result.Failure<string>(Error.Validation($"Email '{email}' is invalid"));
            });

        // Assert
        result.IsSuccess.Should().BeTrue("email is valid");
        result.Value.Should().Be("user@example.com");
    }

    [Fact]
    public async Task EnsureAsync_ValueTask_Result_WithParam_PredicateFailsWithContextualError_ReturnsContextualError()
    {
        // Arrange
        var initialResult = ValueTask.FromResult(Result.Success("invalid-email"));

        // Act
        var result = await initialResult.EnsureAsync(
            async email =>
            {
                await Task.Delay(1);
                return email.Contains('@') && email.Contains('.')
                    ? Result.Success(email)
                    : Result.Failure<string>(Error.Validation($"Email '{email}' must contain @ and ."));
            });

        // Assert
        result.IsFailure.Should().BeTrue("email validation failed");
        result.Error.Detail.Should().Contain("invalid-email");
        result.Error.Detail.Should().Contain("must contain @ and .");
    }

    [Fact]
    public async Task EnsureAsync_ValueTask_Result_WithParam_ChainedEnsures_WorkCorrectly()
    {
        // Arrange
        var initialResult = ValueTask.FromResult(Result.Success("valid-password-123"));

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
    public async Task EnsureAsync_ValueTask_WithNullableType_HandlesNullCorrectly()
    {
        // Arrange
        int? nullValue = null;
        var initialResult = ValueTask.FromResult(Result.Success(nullValue));

        // Act
        var result = await initialResult.EnsureAsync(
            value => ValueTask.FromResult(!value.HasValue),
            Error.Validation("Value should be null for this test"));

        // Assert
        result.IsSuccess.Should().BeTrue("null value passes the null check");
    }

    [Fact]
    public async Task EnsureAsync_ValueTask_WithComplexType_WorksCorrectly()
    {
        // Arrange
        var person = new Person("John", 25);
        var initialResult = ValueTask.FromResult(Result.Success(person));

        // Act
        var result = await initialResult.EnsureAsync(
            async p =>
            {
                await Task.Delay(1);
                return p.Age >= 18;
            },
            p => Error.Validation($"{p.Name} must be 18 or older"));

        // Assert
        result.IsSuccess.Should().BeTrue("person is adult");
        result.Value.Should().Be(person);
    }

    [Fact]
    public async Task EnsureAsync_ValueTask_MultipleChainedEnsures_AllExecuteInOrder()
    {
        // Arrange
        var executionOrder = new List<int>();
        var initialResult = ValueTask.FromResult(Result.Success(100));

        // Act
        var result = await initialResult
            .EnsureAsync(
                async value =>
                {
                    await Task.Delay(1);
                    executionOrder.Add(1);
                    return value > 0;
                },
                Error.Validation("First check failed"))
            .EnsureAsync(
                async value =>
                {
                    await Task.Delay(1);
                    executionOrder.Add(2);
                    return value < 1000;
                },
                Error.Validation("Second check failed"))
            .EnsureAsync(
                async value =>
                {
                    await Task.Delay(1);
                    executionOrder.Add(3);
                    return value % 2 == 0;
                },
                Error.Validation("Third check failed"));

        // Assert
        result.IsSuccess.Should().BeTrue("all checks passed");
        executionOrder.Should().Equal([1, 2, 3], "ensures execute in order");
    }

    [Fact]
    public async Task EnsureAsync_ValueTask_MultipleChainedEnsures_StopsAtFirstFailure()
    {
        // Arrange
        var executionOrder = new List<int>();
        var initialResult = ValueTask.FromResult(Result.Success(100));

        // Act
        var result = await initialResult
            .EnsureAsync(
                async value =>
                {
                    await Task.Delay(1);
                    executionOrder.Add(1);
                    return value > 0;
                },
                Error.Validation("First check failed"))
            .EnsureAsync(
                async value =>
                {
                    await Task.Delay(1);
                    executionOrder.Add(2);
                    return value < 50; // This will fail
                },
                Error.Validation("Second check failed"))
            .EnsureAsync(
                async value =>
                {
                    await Task.Delay(1);
                    executionOrder.Add(3);
                    return value % 2 == 0;
                },
                Error.Validation("Third check failed"));

        // Assert
        result.IsFailure.Should().BeTrue("second check failed");
        result.Error.Should().Be(Error.Validation("Second check failed"));
        executionOrder.Should().Equal([1, 2], "execution stops after first failure");
    }

    #endregion

    private record Person(string Name, int Age);
}
