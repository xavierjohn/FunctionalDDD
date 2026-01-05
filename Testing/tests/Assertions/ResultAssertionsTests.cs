namespace FunctionalDdd.Testing.Tests.Assertions;

public class ResultAssertionsTests
{
    [Fact]
    public void BeSuccess_Should_Pass_When_Result_Is_Success()
    {
        // Arrange
        var result = Result.Success(42);

        // Act & Assert
        result.Should().BeSuccess();
    }

    [Fact]
    public void BeSuccess_Should_Fail_When_Result_Is_Failure()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound("Not found"));

        // Act & Assert
        var act = () => result.Should().BeSuccess();
        
        act.Should().Throw<Exception>()
            .WithMessage("*to be success*failed with error*");
    }

    [Fact]
    public void BeSuccess_Should_Return_Value_In_Which()
    {
        // Arrange
        var result = Result.Success(42);

        // Act & Assert
        result.Should()
            .BeSuccess()
            .Which.Should().Be(42);
    }

    [Fact]
    public void BeFailure_Should_Pass_When_Result_Is_Failure()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound("Not found"));

        // Act & Assert
        result.Should().BeFailure();
    }

    [Fact]
    public void BeFailure_Should_Fail_When_Result_Is_Success()
    {
        // Arrange
        var result = Result.Success(42);

        // Act & Assert
        var act = () => result.Should().BeFailure();
        
        act.Should().Throw<Exception>()
            .WithMessage("*to be failure*succeeded with value*");
    }

    [Fact]
    public void BeFailure_Should_Return_Error_In_Which()
    {
        // Arrange
        var error = Error.NotFound("Not found");
        var result = Result.Failure<int>(error);

        // Act & Assert
        result.Should()
            .BeFailure()
            .Which.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public void BeFailureOfType_Should_Pass_When_Error_Matches_Type()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound("Not found"));

        // Act & Assert
        result.Should().BeFailureOfType<NotFoundError>();
    }

    [Fact]
    public void BeFailureOfType_Should_Fail_When_Error_Type_Different()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Validation("Invalid"));

        // Act & Assert
        var act = () => result.Should().BeFailureOfType<NotFoundError>();
        
        act.Should().Throw<Exception>()
            .WithMessage("*to be of type*NotFoundError*found*ValidationError*");
    }

    [Fact]
    public void BeFailureOfType_Should_Return_Typed_Error_In_Which()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound("Not found"));

        // Act & Assert
        result.Should()
            .BeFailureOfType<NotFoundError>()
            .Which.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public void HaveValue_Should_Pass_When_Value_Matches()
    {
        // Arrange
        var result = Result.Success(42);

        // Act & Assert
        result.Should().HaveValue(42);
    }

    [Fact]
    public void HaveValue_Should_Fail_When_Value_Different()
    {
        // Arrange
        var result = Result.Success(42);

        // Act & Assert
        var act = () => result.Should().HaveValue(99);
        
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void HaveValueMatching_Should_Pass_When_Predicate_Satisfied()
    {
        // Arrange
        var result = Result.Success(42);

        // Act & Assert
        result.Should().HaveValueMatching(x => x > 40);
    }

    [Fact]
    public void HaveValueMatching_Should_Fail_When_Predicate_Not_Satisfied()
    {
        // Arrange
        var result = Result.Success(42);

        // Act & Assert
        var act = () => result.Should().HaveValueMatching(x => x > 50);
        
        act.Should().Throw<Exception>()
            .WithMessage("*value to match predicate*");
    }
}
