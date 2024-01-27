namespace RailwayOrientedProgramming.Tests.Results;

public class ResultTests
{
    [Fact]
    public void Success_argument_is_not_null_Success_result_expected()
    {
        var result = Result.Success("Hello");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello");
    }

    [Fact]
    public void Success_argument_via_callback()
    {
        var result = Result.Success(() => "Hello");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello");
    }

    [Fact]
    public void Success_argument_is_null_Success_result_expected()
    {
        var result = Result.Success(default(string));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Failure_returns_failed_result()
    {
        var result = Result.Failure<string>(Error.Validation("Bad first name"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Validation("Bad first name"));
    }

    [Fact]
    public void Failure_returns_failed_result_via_callback()
    {
        var result = Result.Failure<string>(() => Error.Validation("Bad first name"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Validation("Bad first name"));
    }


    [Fact]
    public void CreateFailure_value_is_null_Success_result_expected()
    {
        var result = Result.FailureIf(false, "Hello", Error.Validation("Bad first name"));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CreateFailure_predicate_is_false_Success_result_expected()
    {
        var result = Result.FailureIf(() => false, string.Empty, Error.Unexpected(string.Empty));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CreateFailure_predicate_is_true_Failure_result_expected()
    {
        var result = Result.FailureIf(() => true, "Hello", Error.Unexpected("You can't touch this."));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Unexpected("You can't touch this."));
    }

    [Fact]
    public async Task CreateFailure_async_predicate_is_false_Success_result_expected()
    {
        var result = await Result.FailureIfAsync(() => Task.FromResult(false), "Hello", Error.Unexpected(string.Empty));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello");
    }

    [Fact]
    public async Task CreateFailure_async_predicate_is_true_Failure_result_expected()
    {
        var result = await Result.FailureIfAsync(() => Task.FromResult(true), "Hello", Error.Unexpected("You can't touch this."));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Unexpected("You can't touch this."));
    }

    [Fact]
    public void CreateFailure_generic_argument_is_false_Success_result_expected()
    {
        byte val = 7;
        var result = Result.FailureIf(false, val, Error.Unexpected(string.Empty));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(val);
    }

    [Fact]
    public void CreateFailure_generic_argument_is_true_Failure_result_expected()
    {
        var val = .56;
        var result = Result.FailureIf(true, val, Error.Unexpected("simple result error"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Unexpected("simple result error"));
    }

    [Fact]
    public void Can_work_with_nullable_structs()
    {
        var result = Result.Success((DateTime?)null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(null);
    }

    [Fact]
    public void Can_implicitly_convert_to_Result()
    {
        // Arrange
        var hello = "Hello";

        // Act
        Result<string> result = hello;

        // Assert
        result.Value.Should().Be(hello);
    }

    [Fact]
    public void Success_Unit_Result()
    {
        // Arrange
        var result = Result.Success();

        // Act
        result.IsSuccess.Should().BeTrue();

        // Assert
        result.Value.Should().Be(default(Unit));
    }

    [Fact]
    public void Failed_Unit_Result()
    {
        // Arrange
        var result = Result.Failure(Error.Forbidden("Testing"));

        // Act
        result.IsFailure.Should().BeTrue();

        // Assert
        result.Error.Should().Be(Error.Forbidden("Testing"));
    }
}
