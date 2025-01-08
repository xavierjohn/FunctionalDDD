namespace FunctionalDdd;

/// <summary>
/// Combine errors into one.
/// If both the errors types are <see cref="ValidationError"/>, the validation errors will be merged.
/// Otherwise, the errors will be wrapped into an <see cref="AggregateError"/>.
/// </summary>
public static class CombineErrorExtensions
{
    /// <summary>
    /// Combine two errors into one.
    /// If both the errors types are <see cref="ValidationError"/>, the errors will be merged.
    /// Otherwise, the errors will be wrapped in an <see cref="AggregateError"/>.
    /// </summary>
    /// <param name="thisError"></param>
    /// <param name="otherError"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Error Combine(this Error? thisError, Error otherError)
    {
        if (thisError is null) return otherError;
        ArgumentNullException.ThrowIfNull(otherError);
        if (thisError is ValidationError thisValidation && otherError is ValidationError otherValidation)
        {
            ValidationError.FieldDetails[] validationErrors = [.. thisValidation.Errors, .. otherValidation.Errors];
            var detail = thisValidation.Detail == otherValidation.Detail ? thisValidation.Detail : string.Empty;
            return Error.Validation(validationErrors, detail, thisValidation.Instance, thisValidation.Code);
        }

        List<Error> errors = [];
        AddError(thisError);
        AddError(otherError);

        return new AggregateError(errors);

        void AddError(Error error)
        {
            if (error is AggregateError aggregate)
                errors.AddRange(aggregate.Errors);
            else
                errors.Add(error);
        }
    }
}
