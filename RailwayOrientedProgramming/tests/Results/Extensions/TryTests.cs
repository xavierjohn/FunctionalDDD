using FluentAssertions;
using Xunit;
using FunctionalDdd;

namespace RailwayOrientedProgramming.Tests.Results.Extensions;

public class TryTests
{
    [Fact]
    public void Try_wraps_exception_into_failure()
    {
        var r = Result.Try<int>(() => throw new InvalidOperationException("Boom"));

        r.IsFailure.Should().BeTrue();
        r.Error.Detail.Should().Be("Boom");
    }

    [Fact]
    public void Try_returns_success_on_normal_execution()
    {
        var r = Result.Try(() => 123);

        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be(123);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
    public async Task TryAsync_wraps_exception()
    {
        var r = await Result.TryAsync<int>(async () =>
        {
            await Task.Delay(5);
            throw new Exception("AsyncBoom");
        });

        r.IsFailure.Should().BeTrue();
        r.Error.Detail.Should().Be("AsyncBoom");
    }

    [Fact]
    public async Task TryAsync_success()
    {
        var r = await Result.TryAsync(async () =>
        {
            await Task.Delay(5);
            return 7;
        });

        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be(7);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
    public void FromException_converts_exception()
    {
        var ex = new ApplicationException("Custom");
        var r = Result.FromException<int>(ex);

        r.IsFailure.Should().BeTrue();
        r.Error.Detail.Should().Be("Custom");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
    public void Custom_exception_mapper()
    {
        var r = Result.Try<int>(() => throw new Exception("HideMe"), ex => Error.BadRequest("Mapped"));

        r.IsFailure.Should().BeTrue();
        r.Error.Should().Be(Error.BadRequest("Mapped"));
    }
}