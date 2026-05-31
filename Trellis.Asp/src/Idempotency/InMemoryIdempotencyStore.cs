namespace Trellis.Asp.Idempotency;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// In-memory <see cref="IIdempotencyStore"/> implementation backed by a
/// <see cref="ConcurrentDictionary{TKey,TValue}"/>. Intended for unit / integration tests and
/// for single-instance development hosts; not safe across multiple instances or process
/// restarts. Production hosts should register the EF-backed store (see <c>Trellis.EntityFrameworkCore</c>).
/// </summary>
/// <remarks>
/// <para>
/// All mutations use compare-and-swap (CAS) loops on the underlying dictionary so concurrent
/// reservers contend cleanly: exactly one observes <see cref="IdempotencyReservationOutcome.Reserved"/>;
/// the rest observe <see cref="IdempotencyReservationOutcome.AlreadyInFlight"/>.
/// </para>
/// <para>
/// Time is provided by <see cref="TimeProvider"/> so tests can advance the clock deterministically
/// to exercise TTL and reservation-timeout transitions.
/// </para>
/// </remarks>
public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private sealed class Entry
    {
        public required string Fingerprint { get; init; }

        // Non-null while a reservation is outstanding; cleared to null when CompleteAsync stores
        // a Snapshot or when the entry transitions to the completed state.
        public string? ReservationId { get; init; }

        public DateTimeOffset ReservedAt { get; init; }

        public IdempotencyResponseSnapshot? Snapshot { get; init; }

        public DateTimeOffset CompletedAt { get; init; }
    }

    private readonly ConcurrentDictionary<(string Scope, string Key), Entry> _store = new();
    private readonly IdempotencyOptions _options;
    private readonly TimeProvider _time;

    /// <summary>
    /// Creates a new <see cref="InMemoryIdempotencyStore"/>.
    /// </summary>
    /// <param name="options">Idempotency options (used for <see cref="IdempotencyOptions.Ttl"/> and <see cref="IdempotencyOptions.ReservationTimeout"/>).</param>
    /// <param name="timeProvider">Optional time provider for deterministic testing; defaults to <see cref="TimeProvider.System"/>.</param>
    public InMemoryIdempotencyStore(IdempotencyOptions options, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
        _time = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc/>
    public ValueTask<IdempotencyReservationOutcome> TryReserveAsync(
        string scope,
        string key,
        string fingerprint,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(fingerprint);

        var compositeKey = (scope, key);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var now = _time.GetUtcNow();

            if (_store.TryGetValue(compositeKey, out var existing))
            {
                if (existing.Snapshot is not null)
                {
                    // Completed entry: honour TTL.
                    var age = now - existing.CompletedAt;
                    if (age >= _options.Ttl)
                    {
                        // Try to atomically remove the expired entry so the next loop iteration
                        // observes an empty slot and adds a fresh reservation. If removal fails
                        // because another caller already mutated the entry, retry.
                        var kvp = new KeyValuePair<(string, string), Entry>(compositeKey, existing);
                        ((ICollection<KeyValuePair<(string, string), Entry>>)_store).Remove(kvp);
                        continue;
                    }

                    if (string.Equals(existing.Fingerprint, fingerprint, StringComparison.Ordinal))
                    {
                        return ValueTask.FromResult<IdempotencyReservationOutcome>(
                            new IdempotencyReservationOutcome.Replay(existing.Snapshot));
                    }

                    return ValueTask.FromResult<IdempotencyReservationOutcome>(
                        new IdempotencyReservationOutcome.BodyHashMismatch(existing.Fingerprint));
                }

                // Reserved entry: same-key, different-body must hard-fail per IETF semantics.
                if (!string.Equals(existing.Fingerprint, fingerprint, StringComparison.Ordinal))
                {
                    return ValueTask.FromResult<IdempotencyReservationOutcome>(
                        new IdempotencyReservationOutcome.BodyHashMismatch(existing.Fingerprint));
                }

                // Reserved entry: honour reservation timeout.
                var elapsed = now - existing.ReservedAt;
                if (elapsed >= _options.ReservationTimeout)
                {
                    // Take over by replacing the stale reservation atomically. Fingerprint
                    // already matches so the takeover is a clean retry of the original request.
                    var takeover = new Entry
                    {
                        Fingerprint = fingerprint,
                        ReservationId = Guid.NewGuid().ToString("N"),
                        ReservedAt = now,
                    };
                    if (_store.TryUpdate(compositeKey, takeover, existing))
                    {
                        return ValueTask.FromResult<IdempotencyReservationOutcome>(
                            new IdempotencyReservationOutcome.Reserved(takeover.ReservationId!));
                    }

                    continue;
                }

                var retryAfter = _options.ReservationTimeout - elapsed;
                return ValueTask.FromResult<IdempotencyReservationOutcome>(
                    new IdempotencyReservationOutcome.AlreadyInFlight(retryAfter));
            }

            var fresh = new Entry
            {
                Fingerprint = fingerprint,
                ReservationId = Guid.NewGuid().ToString("N"),
                ReservedAt = now,
            };
            if (_store.TryAdd(compositeKey, fresh))
            {
                return ValueTask.FromResult<IdempotencyReservationOutcome>(
                    new IdempotencyReservationOutcome.Reserved(fresh.ReservationId!));
            }
            // Another caller added between TryGetValue and TryAdd; retry.
        }
    }

    /// <inheritdoc/>
    public ValueTask CompleteAsync(
        string scope,
        string key,
        string reservationId,
        IdempotencyResponseSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(reservationId);
        ArgumentNullException.ThrowIfNull(snapshot);

        var compositeKey = (scope, key);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_store.TryGetValue(compositeKey, out var existing))
                return ValueTask.CompletedTask;

            if (existing.Snapshot is not null ||
                !string.Equals(existing.ReservationId, reservationId, StringComparison.Ordinal))
            {
                // Either already completed (another finisher won), or the reservation has been
                // taken over by a different request. Late callers are silently ignored so they
                // cannot corrupt the active state.
                return ValueTask.CompletedTask;
            }

            var completed = new Entry
            {
                Fingerprint = existing.Fingerprint,
                ReservationId = null,
                ReservedAt = existing.ReservedAt,
                Snapshot = snapshot,
                CompletedAt = _time.GetUtcNow(),
            };

            if (_store.TryUpdate(compositeKey, completed, existing))
                return ValueTask.CompletedTask;
            // Another caller mutated the entry between TryGetValue and TryUpdate; retry.
        }
    }

    /// <inheritdoc/>
    public ValueTask AbandonAsync(
        string scope,
        string key,
        string reservationId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(reservationId);

        var compositeKey = (scope, key);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_store.TryGetValue(compositeKey, out var existing))
                return ValueTask.CompletedTask;

            if (existing.Snapshot is not null ||
                !string.Equals(existing.ReservationId, reservationId, StringComparison.Ordinal))
            {
                // Completed entries and stale-reservation abandons are silently ignored.
                return ValueTask.CompletedTask;
            }

            var kvp = new KeyValuePair<(string, string), Entry>(compositeKey, existing);
            if (((ICollection<KeyValuePair<(string, string), Entry>>)_store).Remove(kvp))
                return ValueTask.CompletedTask;
            // Another caller mutated the entry between TryGetValue and Remove; retry.
        }
    }
}
