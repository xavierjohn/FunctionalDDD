using FluentAssertions;
using FunctionalDdd;
using Xunit;

namespace RailwayOrientedProgramming.Tests.Results.Extensions.Linq;

public class LinqTests : TestBase
{
    [Fact]
    public void Select_projects_success_value()
    {
        var r = Result.Success(5);

        var projected = r.Select(x => x * 2); // query Select extension

        projected.IsSuccess.Should().BeTrue();
        projected.Value.Should().Be(10);
    }

    [Fact]
    public void Select_propagates_failure()
    {
        var r = Result.Failure<int>(Error1);

        var projected = r.Select(x => x * 2);

        projected.IsFailure.Should().BeTrue();
        projected.Error.Should().Be(Error1);
    }

    [Fact]
    public void SelectMany_combines_two_success_results()
    {
        var a = Result.Success(2);
        var b = Result.Success(3);

        var combined =
            a.SelectMany(_ => b, (x, y) => x + y);

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be(5);
    }

    [Fact]
    public void SelectMany_propagates_first_failure()
    {
        var a = Result.Failure<int>(Error1);
        var b = Result.Success(3);

        var combined =
            a.SelectMany(_ => b, (x, y) => x + y);

        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be(Error1);
    }

    [Fact]
    public void Where_filters_out_value_and_returns_failure_when_predicate_false()
    {
        var r =
            Result.Success(5)
                  .Where(v => v > 10); // predicate false

        r.IsFailure.Should().BeTrue();
        r.Error.Should().Be(Error.Unexpected("Result filtered out by predicate."));
    }

    [Fact]
    public void Where_keeps_success_when_predicate_true()
    {
        Result<int> r =
            Result.Success(15)
                  .Where(v => v > 10);

        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be(15);
    }

    [Fact]
    public void SelectMany_combines_four_success_results()
    {
        var a = Result.Success(1);
        var b = Result.Success(2);
        var c = Result.Success(3);
        var d = Result.Success(4);

        // LINQ query syntax exercises chained SelectMany calls for 4 Results
        Result<int> combined =
            from av in a
            from bv in b
            from cv in c
            from dv in d
            select av + bv + cv + dv;

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be(1 + 2 + 3 + 4);
    }
}