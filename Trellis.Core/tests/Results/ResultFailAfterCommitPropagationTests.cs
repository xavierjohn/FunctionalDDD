namespace Trellis.Core.Tests;

using Trellis.Testing;

/// <summary>
/// Issue #533 follow-up: the per-instance <see cref="IPersistOnFailure.PersistOnFailure"/> flag
/// on <c>Result.FailAfterCommit&lt;T&gt;(...)</c> must survive every railway operator that
/// projects an upstream failure into a new <see cref="Result{TValue}"/>. The bug these tests
/// guard against: operators historically called <c>Result.Fail&lt;TOut&gt;(error)</c> when
/// projecting an upstream failure, silently dropping the persist-on-failure intent so the
/// downstream <c>TransactionalCommandBehavior</c> would never commit the staged state.
/// </summary>
/// <remarks>
/// <para>
/// Propagation rules (encoded once here and enforced by each operator):
/// </para>
/// <list type="bullet">
/// <item><description>Single-source operators (<c>Map</c>, <c>Bind</c>, <c>MapOnFailure</c>, <c>Check</c>, <c>CheckIf</c>, <c>BindZip</c> outer-fail, <c>Ensure</c> propagation, single-failure <c>Traverse</c>) carry the upstream's flag.</description></item>
/// <item><description>Multi-source aggregators (<c>Combine</c>, <c>TraverseAll</c>, <c>SequenceAll</c>) OR-accumulate the flag across every failing source — any persist-on-failure source promotes the aggregated outcome.</description></item>
/// <item><description>Fresh failures (predicate-fails-after-success in <c>Ensure</c>, <c>EnsureAll</c>; <c>EnsureNotNull</c>'s value-was-null branch; <c>NullableExtensions</c>) carry no upstream flag and remain plain <c>Result.Fail</c>.</description></item>
/// </list>
/// </remarks>
public class ResultFailAfterCommitPropagationTests
{
    private static readonly Error PersistError = new Error.Conflict(null, "persist.intent") { Detail = "stage me" };
    private static readonly Error PlainError = new Error.Conflict(null, "plain.intent") { Detail = "drop me" };

    private static bool PersistFlag<T>(Result<T> result) => ((IPersistOnFailure)result).PersistOnFailure;

    // ---------- Map ----------

    [Fact]
    public void Map_propagates_persist_flag_from_failed_source()
    {
        var source = Result.FailAfterCommit<int>(PersistError);

        var mapped = source.Map<int, string>(x => x.ToString(System.Globalization.CultureInfo.InvariantCulture));

        mapped.Should().BeFailure();
        PersistFlag(mapped).Should().BeTrue();
    }

    [Fact]
    public async Task Map_Task_propagates_persist_flag_from_failed_source()
    {
        var source = Task.FromResult(Result.FailAfterCommit<int>(PersistError));

        var mapped = await source.MapAsync(x => x.ToString(System.Globalization.CultureInfo.InvariantCulture));

        mapped.Should().BeFailure();
        PersistFlag(mapped).Should().BeTrue();
    }

    [Fact]
    public async Task Map_ValueTask_propagates_persist_flag_from_failed_source()
    {
        var source = new ValueTask<Result<int>>(Result.FailAfterCommit<int>(PersistError));

        var mapped = await source.MapAsync(x => x.ToString(System.Globalization.CultureInfo.InvariantCulture));

        mapped.Should().BeFailure();
        PersistFlag(mapped).Should().BeTrue();
    }

    // ---------- Bind ----------

    [Fact]
    public void Bind_propagates_persist_flag_from_failed_source()
    {
        var source = Result.FailAfterCommit<int>(PersistError);

        var bound = source.Bind<int, string>(x => Result.Ok(x.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        bound.Should().BeFailure();
        PersistFlag(bound).Should().BeTrue();
    }

    [Fact]
    public async Task Bind_Task_propagates_persist_flag_from_failed_source()
    {
        var source = Task.FromResult(Result.FailAfterCommit<int>(PersistError));

        var bound = await source.BindAsync(x => Result.Ok(x.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        bound.Should().BeFailure();
        PersistFlag(bound).Should().BeTrue();
    }

    [Fact]
    public async Task Bind_ValueTask_propagates_persist_flag_from_failed_source()
    {
        var source = new ValueTask<Result<int>>(Result.FailAfterCommit<int>(PersistError));

        var bound = await source.BindAsync(x => Result.Ok(x.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        bound.Should().BeFailure();
        PersistFlag(bound).Should().BeTrue();
    }

    [Fact]
    public async Task Bind_ValueTask_continuation_propagates_persist_flag_from_failed_source()
    {
        var source = Result.FailAfterCommit<int>(PersistError);

        var bound = await source.BindAsync(x => new ValueTask<Result<string>>(Result.Ok(x.ToString(System.Globalization.CultureInfo.InvariantCulture))));

        bound.Should().BeFailure();
        PersistFlag(bound).Should().BeTrue(
            "the source-failed short-circuit in the ValueTask continuation overload must project via ProjectFailure");
    }

    // ---------- MapOnFailure ----------

    [Fact]
    public void MapOnFailure_propagates_persist_flag_from_failed_source()
    {
        var source = Result.FailAfterCommit<int>(PersistError);

        var mapped = source.MapOnFailure(err => new Error.Conflict(null, "rewritten") { Detail = err.Detail });

        mapped.Should().BeFailure();
        PersistFlag(mapped).Should().BeTrue(
            "MapOnFailure rewrites the error but must preserve the per-instance persist-on-failure intent");
    }

    // ---------- Check ----------

    [Fact]
    public void Check_propagates_persist_flag_when_check_returns_persist_failure()
    {
        var source = Result.Ok(42);

        var checked_ = source.Check(_ => Result.FailAfterCommit(PersistError));

        checked_.Should().BeFailure();
        PersistFlag(checked_).Should().BeTrue(
            "Check's failure is the propagated one; the check's persist intent must carry through");
    }

    // ---------- CheckIf ----------

    [Fact]
    public void CheckIf_propagates_persist_flag_when_condition_true_and_check_returns_persist_failure()
    {
        var source = Result.Ok(42);

        var checked_ = source.CheckIf(condition: true, _ => Result.FailAfterCommit(PersistError));

        checked_.Should().BeFailure();
        PersistFlag(checked_).Should().BeTrue(
            "CheckIf with condition=true must project the check's persist intent into the typed failure");
    }

    [Fact]
    public void CheckIf_short_circuits_with_source_when_condition_false_preserving_source_persist_flag()
    {
        var source = Result.FailAfterCommit<int>(PersistError);

        var checked_ = source.CheckIf(condition: false, _ => Result.Fail(PlainError));

        checked_.Should().BeFailure();
        PersistFlag(checked_).Should().BeTrue(
            "CheckIf with condition=false returns the source unchanged; the source's persist intent must survive");
    }

    // ---------- BindZip ----------

    [Fact]
    public void BindZip_outer_failure_propagates_outer_persist_flag()
    {
        var source = Result.FailAfterCommit<int>(PersistError);

        var zipped = source.BindZip(x => Result.Ok(x.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        zipped.Should().BeFailure();
        PersistFlag(zipped).Should().BeTrue();
    }

    [Fact]
    public void BindZip_inner_failure_propagates_inner_persist_flag()
    {
        var source = Result.Ok(42);

        var zipped = source.BindZip(_ => Result.FailAfterCommit<string>(PersistError));

        zipped.Should().BeFailure();
        PersistFlag(zipped).Should().BeTrue(
            "the inner bind's persist intent must carry through; outer was a plain success");
    }

    // ---------- Combine (multi-source OR-accumulator) ----------

    [Fact]
    public void Combine_propagates_persist_flag_when_any_failing_source_has_it()
    {
        var persistFailure = Result.FailAfterCommit<int>(PersistError);
        var plainFailure = Result.Fail<string>(PlainError);

        var combined = persistFailure.Combine(plainFailure);

        combined.Should().BeFailure();
        PersistFlag(combined).Should().BeTrue(
            "OR-accumulation: any persist-on-failure source promotes the combined outcome");
    }

    [Fact]
    public void Combine_does_not_promote_persist_flag_when_no_failing_source_has_it()
    {
        var plainA = Result.Fail<int>(PersistError);
        var plainB = Result.Fail<string>(PlainError);

        var combined = plainA.Combine(plainB);

        combined.Should().BeFailure();
        PersistFlag(combined).Should().BeFalse(
            "neither source opted in; the combined outcome must not invent persist intent");
    }

    [Fact]
    public void Combine_three_way_propagates_persist_flag_from_middle_source()
    {
        var plainA = Result.Fail<int>(PlainError);
        var persistB = Result.FailAfterCommit<string>(PersistError);
        var plainC = Result.Fail<long>(PlainError);

        var combined = plainA.Combine(persistB).Combine(plainC);

        combined.Should().BeFailure();
        PersistFlag(combined).Should().BeTrue(
            "the persist flag must survive a chained Combine that introduces an extra source");
    }

    // ---------- Traverse (first-failure-wins) ----------

    [Fact]
    public void Traverse_typed_propagates_persist_flag_from_first_failure()
    {
        var inputs = new[] { 1, 2, 3 };

        var traversed = inputs.Traverse(x =>
            x == 2
                ? Result.FailAfterCommit<int>(PersistError)
                : Result.Ok(x));

        traversed.Should().BeFailure();
        PersistFlag(traversed).Should().BeTrue();
    }

    [Fact]
    public void Traverse_unit_propagates_persist_flag_from_first_failure()
    {
        var inputs = new[] { 1, 2, 3 };

        var traversed = inputs.Traverse(x =>
            x == 2
                ? Result.FailAfterCommit(PersistError)
                : Result.Ok());

        traversed.Should().BeFailure();
        PersistFlag(traversed).Should().BeTrue();
    }

    // ---------- TraverseAll (multi-source OR-accumulator) ----------

    [Fact]
    public void TraverseAll_typed_propagates_persist_flag_when_any_failure_has_it()
    {
        var inputs = new[] { 1, 2, 3 };

        var traversed = inputs.TraverseAll(x => x switch
        {
            1 => Result.Fail<int>(PlainError),
            2 => Result.FailAfterCommit<int>(PersistError),
            _ => Result.Ok(x),
        });

        traversed.Should().BeFailure();
        PersistFlag(traversed).Should().BeTrue();
    }

    [Fact]
    public void TraverseAll_unit_propagates_persist_flag_when_any_failure_has_it()
    {
        var inputs = new[] { 1, 2, 3 };

        var traversed = inputs.TraverseAll(x => x switch
        {
            1 => Result.Fail(PlainError),
            2 => Result.FailAfterCommit(PersistError),
            _ => Result.Ok(),
        });

        traversed.Should().BeFailure();
        PersistFlag(traversed).Should().BeTrue();
    }

    // ---------- SequenceAll (multi-source OR-accumulator) ----------

    [Fact]
    public void SequenceAll_typed_propagates_persist_flag_when_any_failure_has_it()
    {
        var results = new[]
        {
            Result.Fail<int>(PlainError),
            Result.FailAfterCommit<int>(PersistError),
            Result.Ok(3),
        };

        var sequenced = results.SequenceAll();

        sequenced.Should().BeFailure();
        PersistFlag(sequenced).Should().BeTrue();
    }

    [Fact]
    public void SequenceAll_unit_propagates_persist_flag_when_any_failure_has_it()
    {
        var results = new[]
        {
            Result.Fail(PlainError),
            Result.FailAfterCommit(PersistError),
            Result.Ok(),
        };

        var sequenced = results.SequenceAll();

        sequenced.Should().BeFailure();
        PersistFlag(sequenced).Should().BeTrue();
    }

    // ---------- Sequence (first-failure-wins) ----------

    [Fact]
    public void Sequence_typed_propagates_persist_flag_from_first_failure()
    {
        var results = new[]
        {
            Result.Ok(1),
            Result.FailAfterCommit<int>(PersistError),
            Result.Ok(3),
        };

        var sequenced = results.Sequence();

        sequenced.Should().BeFailure();
        PersistFlag(sequenced).Should().BeTrue();
    }

    [Fact]
    public void Sequence_unit_propagates_persist_flag_from_first_failure()
    {
        var results = new[]
        {
            Result.Ok(),
            Result.FailAfterCommit(PersistError),
            Result.Ok(),
        };

        var sequenced = results.Sequence();

        sequenced.Should().BeFailure();
        PersistFlag(sequenced).Should().BeTrue();
    }

    // ---------- WhenAllAsync (generated combinator delegating to Combine) ----------

    [Fact]
    public async Task WhenAllAsync_two_way_propagates_persist_flag_when_either_task_failure_has_it()
    {
        var persistTask = Task.FromResult(Result.FailAfterCommit<int>(PersistError));
        var plainTask = Task.FromResult(Result.Fail<string>(PlainError));

        var combined = await (persistTask, plainTask).WhenAllAsync();

        combined.Should().BeFailure();
        PersistFlag(combined).Should().BeTrue(
            "WhenAllAsync delegates to Combine; OR-accumulation must survive the await boundary");
    }

    // ---------- Ensure (propagation vs fresh failure) ----------

    [Fact]
    public void Ensure_propagates_persist_flag_when_upstream_already_failed()
    {
        var source = Result.FailAfterCommit<int>(PersistError);

        var ensured = source.Ensure(x => x > 0, PlainError);

        ensured.Should().BeFailure();
        ensured.Error.Should().BeSameAs(PersistError,
            "the upstream error must propagate; the predicate is never evaluated");
        PersistFlag(ensured).Should().BeTrue();
    }

    [Fact]
    public void Ensure_does_not_promote_persist_flag_on_fresh_predicate_failure()
    {
        var source = Result.Ok(0);

        var ensured = source.Ensure(x => x > 0, PlainError);

        ensured.Should().BeFailure();
        PersistFlag(ensured).Should().BeFalse(
            "fresh caller-supplied failure has no persist intent; the source was successful");
    }

    // ---------- EnsureAll (fresh failures only) ----------

    [Fact]
    public void EnsureAll_does_not_promote_persist_flag_on_fresh_predicate_failures()
    {
        var source = Result.Ok(0);

        var ensured = source.EnsureAll(
            (x => x > 0, PlainError),
            (x => x < -10, PersistError));

        ensured.Should().BeFailure();
        PersistFlag(ensured).Should().BeFalse(
            "EnsureAll's caller-supplied errors are fresh — even if a caller passed PersistError here, it was constructed as a plain Error, not a FailAfterCommit");
    }
}
