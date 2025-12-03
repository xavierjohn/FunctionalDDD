namespace FunctionalDdd;

using System.Collections.Immutable;
using System.Text;

public sealed class ValidationError : Error, IEquatable<ValidationError>
{
    public readonly record struct FieldError(string FieldName, ImmutableArray<string> Details)
    {
        public FieldError(string fieldName, IEnumerable<string> details)
            : this(fieldName, details switch
            {
                ImmutableArray<string> ia => ia,
                _ => details.ToImmutableArray()
            })
        {
            if (Details.IsDefaultOrEmpty)
                throw new ArgumentException("At least one detail message is required.", nameof(details));
        }

        public override string ToString() => $"{FieldName}: {string.Join(", ", Details)}";
    }

    private static readonly ImmutableArray<FieldError> EmptyFieldErrors = ImmutableArray<FieldError>.Empty;

    public ImmutableArray<FieldError> FieldErrors { get; }

    // Single field convenience
    public ValidationError(string fieldDetail, string fieldName, string code, string? detail = null, string? instance = null)
        : base(detail ?? fieldDetail, code, instance)
    {
        if (string.IsNullOrWhiteSpace(fieldDetail))
            throw new ArgumentException("Field detail cannot be null/empty.", nameof(fieldDetail));
        FieldErrors = [new FieldError(fieldName, new[] { fieldDetail })];
    }

    // Multiple explicit field errors
    public ValidationError(IEnumerable<FieldError> fieldErrors, string code, string detail = "", string? instance = null)
        : base(detail, code, instance)
    {
        FieldErrors = fieldErrors switch
        {
            ImmutableArray<FieldError> ia => ia,
            _ => fieldErrors?.ToImmutableArray() ?? EmptyFieldErrors
        };

        if (FieldErrors.IsDefaultOrEmpty)
            throw new ArgumentException("At least one field error must be supplied.", nameof(fieldErrors));
    }

    // Factory: start with one field
    public static ValidationError For(string fieldName, string message, string code = "validation.error", string? detail = null, string? instance = null)
        => new(message, fieldName, code, detail, instance);

    // Add / merge (returns new instance, functional style)
    public ValidationError And(string fieldName, string message)
        => Merge(new ValidationError(message, fieldName, Code, Detail, Instance));

    public ValidationError And(string fieldName, params string[] messages)
        => Merge(new ValidationError(
            [new FieldError(fieldName, messages.ToImmutableArray())],
            Code,
            Detail,
            Instance));

    public ValidationError Merge(ValidationError other)
    {
        if (other is null || ReferenceEquals(this, other)) return this;

        // Use a dictionary to merge field errors efficiently
        var fieldErrorDict = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        var fieldOrder = new List<string>();

        void AddFieldErrors(ImmutableArray<FieldError> fieldErrors)
        {
            foreach (var fe in fieldErrors)
            {
                if (!fieldErrorDict.TryGetValue(fe.FieldName, out var detailsSet))
                {
                    fieldOrder.Add(fe.FieldName);
                    detailsSet = new HashSet<string>(StringComparer.Ordinal);
                    fieldErrorDict[fe.FieldName] = detailsSet;
                }

                foreach (var detail in fe.Details)
                    detailsSet.Add(detail);
            }
        }

        AddFieldErrors(FieldErrors);
        AddFieldErrors(other.FieldErrors);

        var grouped = fieldOrder
            .Select(fieldName => new FieldError(fieldName, fieldErrorDict[fieldName].ToImmutableArray()))
            .ToImmutableArray();

        var mergedDetail = Code == other.Code && Detail == other.Detail
            ? Detail
            : $"{Detail} | {other.Detail}".Trim(' ', '|');

        var mergedCode = Code == other.Code ? Code : $"{Code}+{other.Code}";

        return new ValidationError(grouped, mergedCode, mergedDetail, Instance ?? other.Instance);
    }

    public bool Equals(ValidationError? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (!base.Equals(other)) return false;
        if (FieldErrors.Length != other.FieldErrors.Length) return false;
        for (int i = 0; i < FieldErrors.Length; i++)
        {
            var a = FieldErrors[i];
            var b = other.FieldErrors[i];
            if (!a.FieldName.Equals(b.FieldName, StringComparison.Ordinal)) return false;
            if (!a.Details.SequenceEqual(b.Details, StringComparer.Ordinal)) return false;
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is ValidationError ve && Equals(ve);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());
        foreach (var fe in FieldErrors)
        {
            hash.Add(fe.FieldName, StringComparer.Ordinal);
            foreach (var d in fe.Details)
                hash.Add(d, StringComparer.Ordinal);
        }

        return hash.ToHashCode();
    }

    public override string ToString()
        => base.ToString() + "\r\n" + string.Join("\r\n", FieldErrors.Select(e => $"{e.FieldName}: {string.Join(", ", e.Details)}"));
}
