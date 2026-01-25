namespace FunctionalDdd.Testing.Builders;

/// <summary>
/// Fluent builder for creating complex ValidationError instances in tests.
/// </summary>
public class ValidationErrorBuilder
{
    private readonly List<(string FieldName, List<string> Details)> _fieldErrors = new();

    /// <summary>
    /// Creates a new instance of ValidationErrorBuilder.
    /// </summary>
    public static ValidationErrorBuilder Create() => new();

    /// <summary>
    /// Adds a field error with the specified detail.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <param name="detail">The error detail.</param>
    public ValidationErrorBuilder WithFieldError(string fieldName, string detail)
    {
        var existingField = _fieldErrors.FirstOrDefault(fe => fe.FieldName == fieldName);
        if (existingField != default)
        {
            existingField.Details.Add(detail);
        }
        else
        {
            _fieldErrors.Add((fieldName, new List<string> { detail }));
        }

        return this;
    }

    /// <summary>
    /// Adds multiple error details for a field.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <param name="details">The error details.</param>
    public ValidationErrorBuilder WithFieldError(string fieldName, params string[] details)
    {
        foreach (var detail in details)
        {
            WithFieldError(fieldName, detail);
        }

        return this;
    }

    /// <summary>
    /// Builds the ValidationError.
    /// </summary>
    public ValidationError Build()
    {
        if (_fieldErrors.Count == 0)
            throw new InvalidOperationException("At least one field error must be added before building.");

        var error = Error.Validation(_fieldErrors[0].Details[0], _fieldErrors[0].FieldName);

        // Add remaining details for the first field
        foreach (var detail in _fieldErrors[0].Details.Skip(1))
        {
            error = error.And(_fieldErrors[0].FieldName, detail);
        }

        // Add all other fields
        foreach (var (fieldName, details) in _fieldErrors.Skip(1))
        {
            foreach (var detail in details)
            {
                error = error.And(fieldName, detail);
            }
        }

        return error;
    }

    /// <summary>
    /// Builds a failed Result with the ValidationError.
    /// </summary>
    public Result<T> BuildFailure<T>() => Result.Failure<T>(Build());
}