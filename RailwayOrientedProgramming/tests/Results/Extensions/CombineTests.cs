namespace RailwayOrientedProgramming.Tests.Results.Extensions;

public class CombineTests
{
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
        var helloWorld = rHelloWorld.Ok;
        helloWorld.Should().Be("Hello World");
    }

    [Fact]
    public void Combine_two_results_where_one_is_success()
    {
        // Arrange
        var rHelloWorld = Result.Success("Hello")
            .Combine(Result.Failure<string>(Error.Validation("Bad World")))
            .Bind((hello, world) => Result.Success($"{hello} {world}"));

        // Act

        // Assert
        rHelloWorld.IsFailure.Should().BeTrue();
        rHelloWorld.Errors.Count.Should().Be(1);
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
        var helloWorld = rHelloWorld.Ok;
        helloWorld.Should().Be("Hello First Last");
    }

    [Fact]
    public void Combine_three_results_where_one_is_success()
    {
        // Arrange
        var rHelloWorld = Result.Success("Hello")
            .Combine(Result.Failure<string>(Error.Validation("Bad First")))
            .Combine(Result.Failure<string>(Error.Validation("Bad Last")))
            .Bind((hello, first, last) => Result.Success($"{hello} {first} {last}"));

        // Act

        // Assert
        rHelloWorld.IsFailure.Should().BeTrue();
        rHelloWorld.Errors.Count.Should().Be(2);
        rHelloWorld.Errors[0].Should().Be(Error.Validation("Bad First"));
        rHelloWorld.Errors[1].Should().Be(Error.Validation("Bad Last"));
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
        var helloWorld = rHelloWorld.Ok;
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
        rHelloWorld.Errors.Count.Should().Be(1);
        rHelloWorld.Errors[0].Should().Be(Error.Validation("Bad 9"));
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
        rHelloWorld.Errors.Count.Should().Be(2);
        rHelloWorld.Errors[0].Should().Be(Error.Validation("Bad 3"));
        rHelloWorld.Errors[1].Should().Be(Error.Validation("Bad 9"));
    }

}
