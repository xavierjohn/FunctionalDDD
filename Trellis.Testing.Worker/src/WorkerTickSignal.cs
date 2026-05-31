namespace Trellis.Testing.Worker;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Singleton implementation of <see cref="IWorkerTickSignal"/> registered by
/// <see cref="WorkerHarness{TWorker}"/>. Records every signal and releases any pending
/// test-side waiters when a matching name arrives.
/// </summary>
/// <remarks>
/// Parallel to <see cref="DomainEventCapture"/>: the signal list is guarded by an internal
/// lock and waiters follow the snapshot-then-register-then-recheck pattern to avoid races
/// between worker-side <see cref="SignalAsync(string, CancellationToken)"/> calls and
/// test-side <see cref="WaitForAsync(string?, CancellationToken)"/> calls.
/// </remarks>
internal sealed class WorkerTickSignal : IWorkerTickSignal
{
    private readonly object _gate = new();
    private readonly List<string> _signals = [];
    private readonly List<Waiter> _waiters = [];

    public IReadOnlyList<string> Snapshot()
    {
        lock (_gate)
            return [.. _signals];
    }

    public ValueTask SignalAsync(CancellationToken cancellationToken = default) =>
        SignalAsync(string.Empty, cancellationToken);

    public ValueTask SignalAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        cancellationToken.ThrowIfCancellationRequested();

        List<Waiter>? released = null;

        lock (_gate)
        {
            _signals.Add(name);

            for (var i = _waiters.Count - 1; i >= 0; i--)
            {
                if (_waiters[i].Matches(name))
                {
                    released ??= [];
                    released.Add(_waiters[i]);
                    _waiters.RemoveAt(i);
                }
            }
        }

        if (released is not null)
            foreach (var waiter in released)
                waiter.Complete();

        return ValueTask.CompletedTask;
    }

    public Task WaitForAsync(string? name, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TaskCompletionSource tcs;
        Waiter waiter;

        lock (_gate)
        {
            foreach (var captured in _signals)
            {
                if (name is null || string.Equals(captured, name, StringComparison.Ordinal))
                    return Task.CompletedTask;
            }

            tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            waiter = new Waiter(tcs, name);
            _waiters.Add(waiter);
        }

        return WaitWithCancellationAsync(tcs, waiter, cancellationToken);
    }

    private async Task WaitWithCancellationAsync(
        TaskCompletionSource tcs,
        Waiter waiter,
        CancellationToken cancellationToken)
    {
        await using var registration = cancellationToken.Register(static state =>
        {
            var (signal, w) = ((WorkerTickSignal, Waiter))state!;
            // Only cancel when this callback wins the removal race; if SignalAsync(...) already
            // pulled the waiter out of the list, its TrySetResult is in flight and must win.
            if (signal.TryRemoveWaiter(w))
                w.Cancel();
        }, (this, waiter));

        await tcs.Task.ConfigureAwait(false);
    }

    private bool TryRemoveWaiter(Waiter waiter)
    {
        lock (_gate)
            return _waiters.Remove(waiter);
    }

    private sealed class Waiter(TaskCompletionSource tcs, string? expectedName)
    {
        public bool Matches(string name) =>
            expectedName is null || string.Equals(name, expectedName, StringComparison.Ordinal);

        public void Complete() => tcs.TrySetResult();

        public void Cancel() => tcs.TrySetCanceled();
    }
}
