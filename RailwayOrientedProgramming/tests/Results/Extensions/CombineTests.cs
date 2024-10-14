﻿namespace RailwayOrientedProgramming.Tests.Results.Extensions;

public class CombineTests
{
    [Fact]
    public void Combine_one_result_and_Unit_where_both_are_success()
    {
        // Arrange
        var rHello = Result.Success("Hello")
            .Combine(Result.Success())
            .Bind(hello => Result.Success($"{hello}"));

        // Act

        // Assert
        rHello.IsSuccess.Should().BeTrue();
        var helloWorld = rHello.Value;
        helloWorld.Should().Be("Hello");
    }

    [Fact]
    public void Combine_one_result_and_Unit_where_one_is_success()
    {
        // Arrange
        var rHelloWorld = Result.Success("Hello")
            .Combine(Result.Failure(Error.Validation("Bad World", "key")))
            .Bind(hello => Result.Success($"{hello}"));

        // Act

        // Assert
        rHelloWorld.IsFailure.Should().BeTrue();
        rHelloWorld.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)rHelloWorld.Error;
        validation.Errors.Should().ContainSingle();
        validation.Errors[0].Should().BeEquivalentTo(new ValidationError.FieldDetails("key", ["Bad World"]));
    }

    [Fact]
    public void Combine_two_results_where_both_are_success()
    {
        // Arrange
        var rHelloWorld = Result.Success("Hello")
            .Combine(Result.Success("World"))
            .Bind((hello, world) => Result.Success($"{hello} {world}"));

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
            .Bind((hello, world) => Result.Success($"{hello} {world}"));

        // Act

        // Assert
        rHelloWorld.IsFailure.Should().BeTrue();
        rHelloWorld.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)rHelloWorld.Error;
        validation.Errors.Should().ContainSingle();
        validation.Errors[0].Should().BeEquivalentTo(new ValidationError.FieldDetails("key", ["Bad World"]));
    }

    [Fact]
    public void Combine_two_results_where_2nd_is_success()
    {
        // Arrange
        var rHelloWorld = Result.Failure<string>(Error.Validation("Bad World", "key"))
            .Combine(Result.Success("World"))
            .Bind((hello, world) => Result.Success($"{hello} {world}"));

        // Act

        // Assert
        rHelloWorld.IsFailure.Should().BeTrue();
        rHelloWorld.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)rHelloWorld.Error;
        validation.Errors.Should().ContainSingle();
        validation.Errors[0].Should().BeEquivalentTo(new ValidationError.FieldDetails("key", ["Bad World"]));
    }

    [Fact]
    public void Combine_two_result_and_Unit_where_both_are_success()
    {
        // Arrange
        var rHelloWorld = Result.Success("Hello")
            .Combine(Result.Success("World"))
            .Combine(Result.Success())
            .Bind((hello, world) => Result.Success($"{hello} {world}"));

        // Act

        // Assert
        rHelloWorld.IsSuccess.Should().BeTrue();
        var helloWorld = rHelloWorld.Value;
        helloWorld.Should().Be("Hello World");
    }

    [Fact]
    public void Combine_two_result_and_Unit_where_one_is_success()
    {
        // Arrange
        var rHelloWorld = Result.Success("Hello")
            .Combine(Result.Success("World"))
            .Combine(Result.Failure(Error.Validation("Bad World", "key")))
            .Bind((hello, world) => Result.Success($"{hello} {world}"));

        // Act

        // Assert
        rHelloWorld.IsFailure.Should().BeTrue();
        rHelloWorld.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)rHelloWorld.Error;
        validation.Errors.Should().ContainSingle();
        validation.Errors[0].Should().BeEquivalentTo(new ValidationError.FieldDetails("key", ["Bad World"]));
    }

    [Fact]
    public void Combine_three_results_where_all_success()
    {
        // Arrange
        var rHelloWorld = Result.Success("Hello")
            .Combine(Result.Success("First"))
            .Combine(Result.Success("Last"))
            .Bind((hello, first, last) => Result.Success($"{hello} {first} {last}"));

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
            .Bind((hello, first, last) => Result.Success($"{hello} {first} {last}"));

        // Act

        // Assert
        rHelloWorld.IsFailure.Should().BeTrue();
        rHelloWorld.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)rHelloWorld.Error;
        validation.Errors.Should().HaveCount(2);
        validation.Errors[0].Should().BeEquivalentTo(new ValidationError.FieldDetails("First", ["Bad First"]));
        validation.Errors[1].Should().BeEquivalentTo(new ValidationError.FieldDetails("Last", ["Bad Last"]));
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
            .Bind((one, two, three, four, five, six, seven, eight, nine) => Result.Success($"{one}{two}{three}{four}{five}{six}{seven}{eight}{nine}"));

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
            .Bind((one, two, three, four, five, six, seven, eight, nine) => Result.Success($"{one}{two}{three}{four}{five}{six}{seven}{eight}{nine}"));

        // Act

        // Assert
        rHelloWorld.IsFailure.Should().BeTrue();
        rHelloWorld.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)rHelloWorld.Error;
        validation.Errors.Should().ContainSingle();
        validation.Errors[0].Should().BeEquivalentTo(new ValidationError.FieldDetails(string.Empty, ["Bad 9"]));
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
            .Bind((one, two, three, four, five, six, seven, eight, nine) => Result.Success($"{one}{two}{three}{four}{five}{six}{seven}{eight}{nine}"));

        // Act

        // Assert
        rHelloWorld.IsFailure.Should().BeTrue();
        rHelloWorld.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)rHelloWorld.Error;
        validation.Errors.Should().HaveCount(2);
        validation.Errors[0].Should().BeEquivalentTo(new ValidationError.FieldDetails(string.Empty, ["Bad 3"]));
        validation.Errors[1].Should().BeEquivalentTo(new ValidationError.FieldDetails(string.Empty, ["Bad 9"]));
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
            .Bind((hello, first, last) =>
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
            .Bind((hello, first, last) =>
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

    [Fact]
    public async Task Combine_async_task_results_where_both_are_success()
    {
        // Arrange
        var rHelloWorld = await Task.FromResult(Result.Success("Hello"))
            .CombineAsync(Result.Success("World"))
            .BindAsync((hello, world) => Result.Success($"{hello} {world}"));

        // Act

        // Assert
        rHelloWorld.IsSuccess.Should().BeTrue();
        var helloWorld = rHelloWorld.Value;
        helloWorld.Should().Be("Hello World");
    }
}
