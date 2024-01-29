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
        if (thisError is ValidationError validation && otherError is ValidationError otherValidation)
        {
            var validationErrors = validation.Errors.Concat(otherValidation.Errors).ToList();
            return new ValidationError(validationErrors, validation.Code);
        }

        List<Error> errors = new();
        AddError(thisError);
        AddError(otherError);

        return new AggregateError(errors);

        void AddError(Error error)
        {
            if (error is AggregateError aggregate)
                errors.AddRange(aggregate.Errors);
            else if (error is ValidationError validation)
                errors.AddRange(validation.Errors.Select(e => new ValidationError(e.Message, e.FieldName, validation.Code)));
            else
                errors.Add(error);
        }
    }
}
