namespace RailwayOrientedProgramming.Tests.Results.Extensions;
using Xunit;

public class CombineTests
{
    [Fact]
    public void Combine_two_results_where_both_are_success()
    {
        // Arrange
        var rHelloWorld = Result.Success("Hello")
            .Combine(Result.Success("World"))
            .OnOk((hello, world) => Result.Success($"{hello} {world}"));

        // Act

        // Assert
        rHelloWorld.IsSuccess.Should().BeTrue();
        var helloWorld = rHelloWorld.Value;
        helloWorld.Should().Be("Hello World");
    }

    [Fact]
    public void Combine_two_results_where_one_is_success()
    {
        // Arrange
        var rHelloWorld = Result.Success("Hello")
            .Combine(Result.Failure<string>(Error.Validation("Bad World", "key")))
            .OnOk((hello, world) => Result.Success($"{hello} {world}"));

        // Act

        // Assert
        rHelloWorld.IsFailure.Should().BeTrue();
        rHelloWorld.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)rHelloWorld.Error;
        validation.Errors.Should().ContainSingle();
        validation.Errors[0].Should().Be(new ValidationError.ModelError("Bad World", "key"));
    }

    [Fact]
    public void Combine_three_results_where_all_success()
    {
        // Arrange
        var rHelloWorld = Result.Success("Hello")
            .Combine(Result.Success("First"))
            .Combine(Result.Success("Last"))
            .OnOk((hello, first, last) => Result.Success($"{hello} {first} {last}"));

        // Act

        // Assert
        rHelloWorld.IsSuccess.Should().BeTrue();
        var helloWorld = rHelloWorld.Value;
        helloWorld.Should().Be("Hello First Last");
    }

    [Fact]
    public void Combine_three_results_where_one_is_success()
    {
        // Arrange
        var rHelloWorld = Result.Success("Hello")
            .Combine(Result.Failure<string>(Error.Validation("Bad First", "First")))
            .Combine(Result.Failure<string>(Error.Validation("Bad Last", "Last")))
            .OnOk((hello, first, last) => Result.Success($"{hello} {first} {last}"));

        // Act

        // Assert
        rHelloWorld.IsFailure.Should().BeTrue();
        rHelloWorld.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)rHelloWorld.Error;
        validation.Errors.Should().HaveCount(2);
        validation.Errors[0].Should().Be(new ValidationError.ModelError("Bad First", "First"));
        validation.Errors[1].Should().Be(new ValidationError.ModelError("Bad Last", "Last"));
    }

    [Fact]
    public void Combine_nine_results_where_all_success()
    {
        // Arrange
        var rHelloWorld = Result.Success("1")
            .Combine(Result.Success("2"))
            .Combine(Result.Success("3"))
            .Combine(Result.Success("4"))
            .Combine(Result.Success("5"))
            .Combine(Result.Success("6"))
            .Combine(Result.Success("7"))
            .Combine(Result.Success("8"))
            .Combine(Result.Success("9"))
            .OnOk((one, two, three, four, five, six, seven, eight, nine) => Result.Success($"{one}{two}{three}{four}{five}{six}{seven}{eight}{nine}"));


        // Act

        // Assert
        rHelloWorld.IsSuccess.Should().BeTrue();
        var helloWorld = rHelloWorld.Value;
        helloWorld.Should().Be("123456789");
    }

    [Fact]
    public void Combine_nine_results_with_one_failure()
    {
        // Arrange
        var rHelloWorld = Result.Success("1")
            .Combine(Result.Success("2"))
            .Combine(Result.Success("3"))
            .Combine(Result.Success("4"))
            .Combine(Result.Success("5"))
            .Combine(Result.Success("6"))
            .Combine(Result.Success("7"))
            .Combine(Result.Success("8"))
            .Combine(Result.Failure<string>(Error.Validation("Bad 9")))
            .OnOk((one, two, three, four, five, six, seven, eight, nine) => Result.Success($"{one}{two}{three}{four}{five}{six}{seven}{eight}{nine}"));

        // Act

        // Assert
        rHelloWorld.IsFailure.Should().BeTrue();
        rHelloWorld.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)rHelloWorld.Error;
        validation.Errors.Should().ContainSingle();
        validation.Errors[0].Should().Be(new ValidationError.ModelError("Bad 9", string.Empty));
    }

    [Fact]
    public void Combine_nine_results_with_two_failure()
    {
        // Arrange
        var rHelloWorld = Result.Success("1")
            .Combine(Result.Success("2"))
            .Combine(Result.Failure<string>(Error.Validation("Bad 3")))
            .Combine(Result.Success("4"))
            .Combine(Result.Success("5"))
            .Combine(Result.Success("6"))
            .Combine(Result.Success("7"))
            .Combine(Result.Success("8"))
            .Combine(Result.Failure<string>(Error.Validation("Bad 9")))
            .OnOk((one, two, three, four, five, six, seven, eight, nine) => Result.Success($"{one}{two}{three}{four}{five}{six}{seven}{eight}{nine}"));

        // Act

        // Assert
        rHelloWorld.IsFailure.Should().BeTrue();
        rHelloWorld.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)rHelloWorld.Error;
        validation.Errors.Should().HaveCount(2);
        validation.Errors[0].Should().Be(new ValidationError.ModelError("Bad 3", string.Empty));
        validation.Errors[1].Should().Be(new ValidationError.ModelError("Bad 9", string.Empty));
    }

    [Fact]
    public void Combine_validation_and_unexpected_error_will_return_aggregated_error()
    {
        // Arrange
        var called = false;

        // Act
        var rHelloWorld = Result.Success("Hello")
            .Combine(Result.Failure<string>(Error.Validation("Bad First", "First")))
            .Combine(Result.Failure<string>(Error.Unexpected("Server error")))
            .OnOk((hello, first, last) =>
            {
                return Result.Success($"{hello} {first} {last}");
            });

        // Assert
        called.Should().BeFalse();
        rHelloWorld.IsFailure.Should().BeTrue();
        rHelloWorld.Error.Should().BeOfType<AggregateError>();
        var ag = (AggregateError)rHelloWorld.Error;
        ag.Errors.Should().HaveCount(2);
        ag.Errors[0].Should().Be(Error.Validation("Bad First", "First"));
        ag.Errors[1].Should().Be(Error.Unexpected("Server error"));

    }

    [Fact]
    public void Combine_non_validation_error_will_return_aggregated_error()
    {
        // Arrange
        var called = false;

        // Act
        var rHelloWorld = Result.Success("Hello")
            .Combine(Result.Failure<string>(Error.Forbidden("You can't touch this.")))
            .Combine(Result.Failure<string>(Error.Unexpected("Server error")))
            .OnOk((hello, first, last) =>
            {
                called = true;
                return Result.Success($"{hello} {first} {last}");
            });

        // Assert
        called.Should().BeFalse();
        rHelloWorld.IsFailure.Should().BeTrue();
        rHelloWorld.Error.Should().BeOfType<AggregateError>();
        var ag = (AggregateError)rHelloWorld.Error;
        ag.Errors.Should().HaveCount(2);
        ag.Errors[0].Should().Be(Error.Forbidden("You can't touch this."));
        ag.Errors[1].Should().Be(Error.Unexpected("Server error"));

    }

}
