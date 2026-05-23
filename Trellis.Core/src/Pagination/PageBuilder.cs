namespace Trellis;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

/// <summary>
/// Storage-agnostic helper that turns an over-fetched list into a <see cref="Page{T}"/>
/// with the correct <c>Next</c> cursor. Works with any data source — EF Core, Dapper,
/// Cosmos, gRPC, in-memory — as long as the caller has already executed an
/// <c>OrderBy(...).Take(pageSize.Applied + 1)</c> query against a stable sort key.
/// </summary>
/// <remarks>
/// <para>
/// <b>Forward-only.</b> <see cref="Page{T}.Previous"/> is always <c>null</c>. Trellis
/// does not yet ship a reverse-seek API; emitting a previous cursor that the existing
/// next-URL builder would walk forward from would re-fetch the current page rather
/// than the page before it.
/// </para>
/// <para>
/// <b>Selector contract.</b> The selectors passed to <c>FromOverFetch</c> MUST match
/// the sort keys used in the upstream query. Mismatched selectors produce semantically
/// wrong cursors — the boundary item that the cursor points at will not be the one
/// the next query would seek past.
/// </para>
/// </remarks>
public static class PageBuilder
{
    /// <summary>
    /// Builds a <see cref="Page{T}"/> from a single-key over-fetched list. The caller
    /// should have asked the data source for <c>pageSize.Applied + 1</c> rows ordered
    /// by the same key returned by <paramref name="idSelector"/>.
    /// </summary>
    /// <typeparam name="T">The row type.</typeparam>
    /// <typeparam name="TKey">The Id type used for cursoring.</typeparam>
    /// <param name="overFetched">The over-fetched list (at most <c>pageSize.Applied + 1</c> items).</param>
    /// <param name="pageSize">The validated page-size pair.</param>
    /// <param name="idSelector">A selector that returns the cursor key from a row. Must match the upstream <c>OrderBy</c>.</param>
    /// <returns>A <see cref="Page{T}"/> with at most <c>pageSize.Applied</c> items and the appropriate next cursor.</returns>
    public static Page<T> FromOverFetch<T, TKey>(
        IReadOnlyList<T> overFetched,
        PageSize pageSize,
        Func<T, TKey> idSelector)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(overFetched);
        ArgumentNullException.ThrowIfNull(idSelector);
        EnsureValidated(pageSize);

        if (overFetched.Count <= pageSize.Applied)
            return new Page<T>(MaterializeAll(overFetched), Next: null, Previous: null, pageSize.Requested, pageSize.Applied);

        var kept = MaterializePrefix(overFetched, pageSize.Applied);
        var lastKept = kept[pageSize.Applied - 1];
        var next = CursorCodec.Encode(idSelector(lastKept));
        return new Page<T>(kept, next, Previous: null, pageSize.Requested, pageSize.Applied);
    }

    /// <summary>
    /// Builds a <see cref="Page{T}"/> from a composite-key over-fetched list. The caller
    /// should have asked the data source for <c>pageSize.Applied + 1</c> rows ordered by
    /// <c>(CreatedAt, Id)</c> — the same keys returned by the selectors below.
    /// </summary>
    /// <typeparam name="T">The row type.</typeparam>
    /// <typeparam name="TKey">The Id type used as the secondary sort key.</typeparam>
    /// <param name="overFetched">The over-fetched list (at most <c>pageSize.Applied + 1</c> items).</param>
    /// <param name="pageSize">The validated page-size pair.</param>
    /// <param name="createdAtSelector">A selector that returns the primary sort key from a row. Must match the upstream <c>OrderBy</c>.</param>
    /// <param name="idSelector">A selector that returns the secondary sort key from a row. Must match the upstream <c>ThenBy</c>.</param>
    /// <returns>A <see cref="Page{T}"/> with at most <c>pageSize.Applied</c> items and the appropriate next cursor.</returns>
    public static Page<T> FromOverFetch<T, TKey>(
        IReadOnlyList<T> overFetched,
        PageSize pageSize,
        Func<T, DateTimeOffset> createdAtSelector,
        Func<T, TKey> idSelector)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(overFetched);
        ArgumentNullException.ThrowIfNull(createdAtSelector);
        ArgumentNullException.ThrowIfNull(idSelector);
        EnsureValidated(pageSize);

        if (overFetched.Count <= pageSize.Applied)
            return new Page<T>(MaterializeAll(overFetched), Next: null, Previous: null, pageSize.Requested, pageSize.Applied);

        var kept = MaterializePrefix(overFetched, pageSize.Applied);
        var lastKept = kept[pageSize.Applied - 1];
        var next = CursorCodec.Encode(createdAtSelector(lastKept), idSelector(lastKept));
        return new Page<T>(kept, next, Previous: null, pageSize.Requested, pageSize.Applied);
    }

    private static void EnsureValidated(PageSize pageSize)
    {
        // PageSize is a struct, so callers can sidestep its constructor with default(PageSize).
        // Refuse the zero-shape so the next-cursor logic (which indexes kept[Applied - 1])
        // cannot reach a negative index.
        if (pageSize.Requested <= 0 || pageSize.Applied <= 0 || pageSize.Applied > pageSize.Requested)
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                "PageSize must be a validated instance with Requested > 0, Applied > 0, and Applied <= Requested.");
    }

    private static ImmutableArray<T> MaterializeAll<T>(IReadOnlyList<T> source)
    {
        var builder = ImmutableArray.CreateBuilder<T>(source.Count);
        for (var i = 0; i < source.Count; i++)
            builder.Add(source[i]);
        return builder.MoveToImmutable();
    }

    private static ImmutableArray<T> MaterializePrefix<T>(IReadOnlyList<T> source, int count)
    {
        var builder = ImmutableArray.CreateBuilder<T>(count);
        for (var i = 0; i < count; i++)
            builder.Add(source[i]);
        return builder.MoveToImmutable();
    }
}