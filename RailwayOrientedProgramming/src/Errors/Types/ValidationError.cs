namespace FunctionalDdd;

using System.Collections.Immutable;
using System.Text;

/// <summary>
/// Represents validation errors for one or more fields. Used when input data fails business rules or constraints.
/// </summary>
/// <remarks>
/// <para>
/// ValidationError can hold errors for multiple fields, making it ideal for form validation scenarios.
/// Use the fluent <see cref="And(string, string)"/> method to add multiple field errors.
/// </para>
/// <para>
/// Maps to HTTP 400 Bad Request when converted to HTTP responses.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Single field validation
/// var error = Error.Validation("Email is required", "email");
/// 
/// // Multiple field validation using fluent API
/// var multiError = ValidationError.For("email", "Email is required")
///     .And("password", "Password must be at least 8 characters")
///     .And("age", "Age must be 18 or older");
/// 
/// // Multiple errors for same field
/// var complexError = ValidationError.For("password", "Must be at least 8 characters")
///     .And("password", "Must contain a number")
///     .And("password", "Must contain a special character");
/// </code>
/// </example>
public sealed class ValidationError : Error, IEquatable<ValidationError>
{
    /// <summary>
    /// Represents an error for a specific field, which may have multiple detail messages.
    /// </summary>
    /// <param name="FieldName">Name of the field that failed validation.</param>
    /// <param name="Details">Collection of validation error messages for this field.</param>
    public readonly record struct FieldError(string FieldName, ImmutableArray<string> Details)
    {
        /// <summary>
        /// Creates a field error with the specified name and error messages.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="details">One or more error messages for this field.</param>
        /// <exception cref="ArgumentException">Thrown when no detail messages are provided.</exception>
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

    /// <summary>
    /// Gets the collection of field-specific validation errors.
    /// </summary>
    /// <value>An immutable array of <see cref="FieldError"/> instances.</value>
    public ImmutableArray<FieldError> FieldErrors { get; }

    /// <summary>
    /// Creates a validation error for a single field.
    /// </summary>
    /// <param name="fieldDetail">Error message describing what's wrong with the field.</param>
    /// <param name="fieldName">Name of the field that failed validation.</param>
    /// <param name="code">Machine-readable error code (defaults to "validation.error").</param>
    /// <param name="detail">Optional overall error detail. If null, uses fieldDetail.</param>
    /// <param name="instance">Optional identifier for the instance being validated.</param>
    /// <exception cref="ArgumentException">Thrown when fieldDetail is null or whitespace.</exception>
    // Single field convenience
    public ValidationError(string fieldDetail, string fieldName, string code, string? detail = null, string? instance = null)
        : base(detail ?? fieldDetail, code, instance)
    {
        if (string.IsNullOrWhiteSpace(fieldDetail))
            throw new ArgumentException("Field detail cannot be null/empty.", nameof(fieldDetail));
        FieldErrors = [new FieldError(fieldName, new[] { fieldDetail })];
    }

    /// <summary>
    /// Creates a validation error with multiple field errors.
    /// </summary>
    /// <param name="fieldErrors">Collection of field-specific errors.</param>
    /// <param name="code">Machine-readable error code.</param>
    /// <param name="detail">Overall error description.</param>
    /// <param name="instance">Optional identifier for the instance being validated.</param>
    /// <exception cref="ArgumentException">Thrown when no field errors are provided.</exception>
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

    /// <summary>
    /// Creates a new validation error for a single field (fluent factory method).
    /// </summary>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="message">Validation error message.</param>
    /// <param name="code">Error code (defaults to "validation.error").</param>
    /// <param name="detail">Optional overall detail message.</param>
    /// <param name="instance">Optional instance identifier.</param>
    /// <returns>A new <see cref="ValidationError"/> instance.</returns>
    /// <remarks>
    /// Use this as a starting point, then chain with <see cref="And(string, string)"/> to add more field errors.
    /// </remarks>
    // Factory: start with one field
    public static ValidationError For(string fieldName, string message, string code = "validation.error", string? detail = null, string? instance = null)
        => new(message, fieldName, code, detail, instance);

    /// <summary>
    /// Adds another field error to this validation error (fluent API).
    /// </summary>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="message">Validation error message.</param>
    /// <returns>A new <see cref="ValidationError"/> containing all field errors.</returns>
    /// <remarks>
    /// This method creates a new instance; it does not mutate the original.
    /// </remarks>
    // Add / merge (returns new instance, functional style)
    public ValidationError And(string fieldName, string message)
        => Merge(new ValidationError(message, fieldName, Code, Detail, Instance));

    /// <summary>
    /// Adds multiple error messages for a single field (fluent API).
    /// </summary>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="messages">Multiple validation error messages for this field.</param>
    /// <returns>A new <see cref="ValidationError"/> containing all field errors.</returns>
    /// <remarks>
    /// This method creates a new instance; it does not mutate the original.
    /// </remarks>
    public ValidationError And(string fieldName, params string[] messages)
        => Merge(new ValidationError(
            [new FieldError(fieldName, messages.ToImmutableArray())],
            Code,
            Detail,
            Instance));

    /// <summary>
    /// Merges this validation error with another, combining field errors and details.
    /// </summary>
    /// <param name="other">The validation error to merge with.</param>
    /// <returns>A new <see cref="ValidationError"/> containing errors from both instances.</returns>
    /// <remarks>
    /// <para>
    /// Errors for the same field are combined without duplicates.
    /// Detail messages are concatenated if they differ.
    /// Error codes are combined if they differ.
    /// </para>
    /// <para>
    /// This method creates a new instance; it does not mutate the original.
    /// </para>
    /// </remarks>
    public ValidationError Merge(ValidationError other)
    {
        if (other is null || ReferenceEquals(this, other)) return this;

        // Use a dictionary to merge field errors efficiently while preserving insertion order of detail messages
        var fieldErrorDict = new Dictionary<string, (HashSet<string> seen, List<string> ordered)>(StringComparer.Ordinal);
        var fieldOrder = new List<string>();

        void AddFieldErrors(ImmutableArray<FieldError> fieldErrors)
        {
            foreach (var fe in fieldErrors)
            {
                if (!fieldErrorDict.TryGetValue(fe.FieldName, out var detailsSet))
                {
                    fieldOrder.Add(fe.FieldName);
                    detailsSet = (new HashSet<string>(StringComparer.Ordinal), new List<string>());
                    fieldErrorDict[fe.FieldName] = detailsSet;
                }

                foreach (var detail in fe.Details)
                {
                    if (detailsSet.seen.Add(detail))
                    {
                        detailsSet.ordered.Add(detail);
                    }
                }
            }
        }

        AddFieldErrors(FieldErrors);
        AddFieldErrors(other.FieldErrors);

        var grouped = fieldOrder
            .Select(fieldName => new FieldError(fieldName, fieldErrorDict[fieldName].ordered.ToImmutableArray()))
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
