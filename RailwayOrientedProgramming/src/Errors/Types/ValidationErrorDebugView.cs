namespace FunctionalDdd;

using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Provides an expanded debugger view for <see cref="ValidationError"/> that shows
/// field errors in a structured, easy-to-read format.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class ValidationErrorDebugView
{
    private readonly ValidationError _error;

    public ValidationErrorDebugView(ValidationError error) => _error = error;

    public string Code => _error.Code;

    public string Detail => _error.Detail;

    public string? Instance => _error.Instance;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public FieldView[] Fields => _error.FieldErrors
        .Select(f => new FieldView(f))
        .ToArray();

    [DebuggerDisplay("{FieldName}: {Summary}")]
    internal sealed class FieldView
    {
        private readonly ValidationError.FieldError _field;

        public FieldView(ValidationError.FieldError field) => _field = field;

        public string FieldName => _field.FieldName;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public ImmutableArray<string> Details => _field.Details;

        internal string Summary => string.Join("; ", _field.Details);
    }
}
