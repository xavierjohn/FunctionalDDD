using FluentAssertions;
using Xunit;
using FunctionalDdd;

namespace RailwayOrientedProgramming.Tests.Results.Extensions;

public class MapErrorTests : TestBase
{
    [Fact]
    public void MapError_transforms_failure_error()
    {
        var original = Result.Failure<int>(Error1);

        var mapped = original.MapError(e => Error.Conflict($"Wrapped: {e.Detail}"));

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(Error.Conflict($"Wrapped: {Error1.Detail}"));
    }

    [Fact]
    public void MapError_does_not_touch_success()
    {
        var success = Result.Success(42);

        var mapped = success.MapError(e => Error.Unexpected("ShouldNotHappen"));

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }
}