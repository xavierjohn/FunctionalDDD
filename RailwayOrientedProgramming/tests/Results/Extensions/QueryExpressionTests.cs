using FluentAssertions;
using Xunit;
using FunctionalDdd;

namespace RailwayOrientedProgramming.Tests.Results.Extensions.Linq;

public class QueryExpressionTests : TestBase
{
    [Fact]
    public void Query_expression_success_chain()
    {
        var query =
            from a in Result.Success(2)
            from b in Result.Success(3)
            from c in Result.Success(5)
            select a + b + c;

        query.IsSuccess.Should().BeTrue();
        query.Value.Should().Be(10);
    }

    [Fact]
    public void Query_expression_short_circuits_on_first_failure()
    {
        var query =
            from a in Result.Failure<int>(Error1)
            from b in Result.Success(3)
            select a + b;

        query.IsFailure.Should().BeTrue();
        query.Error.Should().Be(Error1);
    }

    [Fact]
    public void Query_expression_where_clause_filters()
    {
        var query =
            from a in Result.Success(4)
            where a % 2 == 1   // false
            select a;

        query.IsFailure.Should().BeTrue();
        query.Error.Detail.Should().Be("Result filtered out by predicate.");
    }

    [Fact]
    public void Query_expression_where_clause_passes()
    {
        var query =
            from a in Result.Success(9)
            where a > 3
            select a * 2;

        query.IsSuccess.Should().BeTrue();
        query.Value.Should().Be(18);
    }
}