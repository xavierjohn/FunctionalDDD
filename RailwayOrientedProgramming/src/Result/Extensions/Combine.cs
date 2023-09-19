namespace FunctionalDDD.Results;

using FunctionalDDD.Results.Errors;

public static partial class CombineExtensions
{
    public static Error Combine(this Error? err, Error other)
    {
        if (err is null) return other;
        if (other is null) throw new ArgumentNullException(nameof(other));
        if (err is ValidationError validation && other is ValidationError otherValidation)
        {
            var validationErrors = validation.Errors.Concat(otherValidation.Errors).ToList();
            return new ValidationError(validationErrors, validation.Code);
        }

        List<Error> errors = new();
        AddError(err);
        AddError(other);

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

    public static Result<(T1, T2)> Combine<T1, T2>(this Result<T1> t1, Result<T2> t2)
    {
        Error? error = null;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (t2.IsFailure) error = error.Combine(t2.Error);
        if (error is not null) return Result.Failure<(T1, T2)>(error);
        return Result.Success<(T1, T2)>((t1.Value, t2.Value));
    }
}
