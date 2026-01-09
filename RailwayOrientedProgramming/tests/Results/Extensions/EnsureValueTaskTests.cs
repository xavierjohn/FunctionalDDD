namespace RailwayOrientedProgramming.Tests.Results.Extensions;

using FluentAssertions;
using FunctionalDdd;
using System.Threading.Tasks;
using Xunit;

public class EnsureValueTaskTests
{
    [Fact]
    public async Task Ensure_ValueTaskRight_short_circuits_when_source_is_failure()
    {
        var source = Result.Failure<string>(Error.Validation("original"));
        var invoked = false;

        var actual = await source.EnsureAsync(
            () =>
            {
                invoked = true;
                return new ValueTask<bool>(true);
            },
            Error.Validation("should not run"));

        invoked.Should().BeFalse();
        actual.Should().Be(source);
    }

    [Fact]
    public async Task Ensure_ValueTaskRight_handles_boolean_predicate()
    {
        var source = Result.Success("payload");
        var error = Error.Validation("predicate failed");

        var success = await source.EnsureAsync(() => new ValueTask<bool>(true), error);
        success.Should().Be(source);

        var failure = await source.EnsureAsync(() => new ValueTask<bool>(false), error);
        failure.IsFailure.Should().BeTrue();
        failure.Error.Should().Be(error);
    }

    [Fact]
    public async Task Ensure_ValueTaskRight_uses_value_based_predicates()
    {
        var source = Result.Success("abc");
        var error = Error.Validation("length invalid");

        var passing = await source.EnsureAsync(v => new ValueTask<bool>(v.Length == 3), error);
        passing.IsSuccess.Should().BeTrue();

        var failing = await source.EnsureAsync(v => new ValueTask<bool>(v.Length > 5), error);
        failing.Error.Should().Be(error);
    }

    [Fact]
    public async Task Ensure_ValueTaskRight_invokes_error_factories()
    {
        var source = Result.Success("xyz");

        var errorFromSyncFactory = await source.EnsureAsync(
            v => new ValueTask<bool>(false),
            v => Error.Validation($"bad:{v}"));
        errorFromSyncFactory.Error.Should().Be(Error.Validation("bad:xyz"));

        var errorFromAsyncFactory = await source.EnsureAsync(
            v => new ValueTask<bool>(false),
            v => new ValueTask<Error>(Error.Validation($"async:{v}")));
        errorFromAsyncFactory.Error.Should().Be(Error.Validation("async:xyz"));
    }

    [Fact]
    public async Task Ensure_ValueTaskRight_supports_result_predicates()
    {
        var source = Result.Success("data");
        var expected = Error.Validation("predicate failure");

        var predicateFailure = await source.EnsureAsync(
            () => new ValueTask<Result<string>>(Result.Failure<string>(expected)));
        predicateFailure.Error.Should().Be(expected);

        var valuePredicateFailure = await source.EnsureAsync(
            value => new ValueTask<Result<string>>(Result.Failure<string>(Error.Validation($"bad:{value}"))));
        valuePredicateFailure.Error.Should().Be(Error.Validation("bad:data"));
    }

    [Fact]
    public async Task Ensure_ValueTaskLeft_async_predicates_produce_errors()
    {
        var source = new ValueTask<Result<int>>(Result.Success(10));
        var error = Error.Validation("too small");

        var success = await source.EnsureAsync(v => new ValueTask<bool>(v == 10), error);
        success.IsSuccess.Should().BeTrue();

        var failure = await source.EnsureAsync(v => new ValueTask<bool>(v > 10), error);
        failure.Error.Should().Be(error);
    }

    [Fact]
    public async Task Ensure_ValueTaskLeft_invokes_error_factories()
    {
        var source = new ValueTask<Result<int>>(Result.Success(5));

        var syncFactoryFailure = await source.EnsureAsync(
            _ => new ValueTask<bool>(false),
            v => Error.Validation($"sync:{v}"));
        syncFactoryFailure.Error.Should().Be(Error.Validation("sync:5"));

        var asyncFactoryFailure = await source.EnsureAsync(
            _ => new ValueTask<bool>(false),
            v => new ValueTask<Error>(Error.Validation($"async:{v}")));
        asyncFactoryFailure.Error.Should().Be(Error.Validation("async:5"));
    }

    [Fact]
    public async Task Ensure_ValueTaskLeft_supports_valueTask_result_predicates()
    {
        var source = new ValueTask<Result<int>>(Result.Success(42));
        var expected = Error.Validation("predicate failed");

        var predicateFailure = await source.EnsureAsync(
            () => new ValueTask<Result<int>>(Result.Failure<int>(expected)));
        predicateFailure.Error.Should().Be(expected);

        var valuePredicateFailure = await source.EnsureAsync(
            v => new ValueTask<Result<int>>(Result.Failure<int>(Error.Validation($"bad:{v}"))));
        valuePredicateFailure.Error.Should().Be(Error.Validation("bad:42"));
    }

    [Fact]
    public async Task Ensure_ValueTaskLeft_short_circuits_when_initial_result_is_failure()
    {
        var original = Error.Validation("initial failure");
        var source = new ValueTask<Result<int>>(Result.Failure<int>(original));
        var invoked = false;

        var actual = await source.EnsureAsync(
            v =>
            {
                invoked = true;
                return new ValueTask<bool>(v > 0);
            },
            Error.Validation("should not execute"));

        invoked.Should().BeFalse();
        actual.Error.Should().Be(original);
    }

    [Fact]
    public async Task Ensure_ValueTaskLeft_supports_sync_predicates_with_async_error_factory()
    {
        var source = new ValueTask<Result<int>>(Result.Success(1));
        var actual = await source.EnsureAsync(
            _ => false,
            _ => new ValueTask<Error>(Error.Validation("from async")));

        actual.Error.Should().Be(Error.Validation("from async"));
    }
}
