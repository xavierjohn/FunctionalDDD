namespace FunctionalDDD.Tests.ResultTests;

public class ResultTests
{
    [Fact]
    public void Success_argument_is_not_null_Success_result_expected()
    {
        Result<string> result = Result.Success("Hello");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello");
    }

    [Fact]
    public void Success_argument_is_null_Success_result_expected()
    {
        Result<string?> result = Result.Success(default(string));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Fail_argument_is_not_default_Fail_result_expected()
    {
        Result<string> result = Result.Failure<string>(Error.Validation("Bad first name"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Validation("Bad first name"));
    }


    [Fact]
    public void CreateFailure_value_is_null_Success_result_expected()
    {
        Result<string> result = Result.FailureIf(false, "Hello", Error.Validation("Bad first name"));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CreateFailure_predicate_is_false_Success_result_expected()
    {
        Result<string> result = Result.FailureIf(() => false, string.Empty, Error.Unexpected(string.Empty));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CreateFailure_predicate_is_true_Failure_result_expected()
    {
        Result<string> result = Result.FailureIf(() => true, "Hello", Error.Unexpected("You can't touch this."));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Unexpected("You can't touch this."));
    }

    [Fact]
    public async Task CreateFailure_async_predicate_is_false_Success_result_expected()
    {
        Result<string> result = await Result.FailureIfAsync(() => Task.FromResult(false), "Hello", Error.Unexpected(string.Empty));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello");
    }

    [Fact]
    public async Task CreateFailure_async_predicate_is_true_Failure_result_expected()
    {
        Result<string> result = await Result.FailureIfAsync(() => Task.FromResult(true), "Hello", Error.Unexpected("You can't touch this."));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Unexpected("You can't touch this."));
    }

    [Fact]
    public void CreateFailure_generic_argument_is_false_Success_result_expected()
    {
        byte val = 7;
        Result<byte> result = Result.FailureIf(false, val, Error.Unexpected(string.Empty));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(val);
    }

    [Fact]
    public void CreateFailure_generic_argument_is_true_Failure_result_expected()
    {
        double val = .56;
        Result<double> result = Result.FailureIf(true, val, Error.Unexpected("simple result error"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Unexpected("simple result error"));
    }

    [Fact]
    public void Can_work_with_nullable_structs()
    {
        Result<DateTime?> result = Result.Success((DateTime?)null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(null);
    }

    [Fact]
    public void Can_work_with_maybe_of_struct()
    {
        Maybe<DateTime> maybe = Maybe<DateTime>.None;

        Result<Maybe<DateTime>> result = Result.Success(maybe);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Maybe<DateTime>.None);
    }

    [Fact]
    public void Can_work_with_maybe_of_ref_type()
    {
        Maybe<string> maybe = Maybe<string>.None;

        Result<Maybe<string>> result = Result.Success(maybe);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Maybe<string>.None);
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
}
