namespace FunctionalDdd;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Provides an expanded debugger view for <see cref="AggregateError"/> that shows
/// each contained error with its type and detail.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class AggregateErrorDebugView
{
    private readonly AggregateError _error;

    public AggregateErrorDebugView(AggregateError error) => _error = error;

    public string Code => _error.Code;

    public int Count => _error.Errors.Count;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public ErrorView[] Errors => _error.Errors
        .Select(e => new ErrorView(e))
        .ToArray();

    [DebuggerDisplay("{ErrorType}: {Detail}")]
    internal sealed class ErrorView
    {
        private readonly Error _error;

        public ErrorView(Error error) => _error = error;

        public string ErrorType => _error.GetType().Name;

        public string Code => _error.Code;

        public string Detail => _error.Detail;

        public string? Instance => _error.Instance;

        /// <summary>
        /// Provides access to the original error for full inspection.
        /// </summary>
        public Error Error => _error;
    }
}
