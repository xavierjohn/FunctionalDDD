namespace FunctionalDDD;

public static partial class ResultExtensions
{
    public static Err Combine(this Err? err, Err other)
    {
        if (err is null) return other;
        if (other is null) throw new ArgumentNullException(nameof(other));
        if (err is Validation validation && other is Validation otherValidation)
        {
            var validationErrors = validation.Errors.Concat(otherValidation.Errors).ToList();
            return new Validation(validationErrors, validation.Code);
        }

        List<Err> errors = new();
        AddError(err);
        AddError(other);

        return new Aggregate(errors);

        void AddError(Err error)
        {
            if (error is Aggregate aggregate)
                errors.AddRange(aggregate.Errors);
            else if (error is Validation validation)
                errors.AddRange(validation.Errors.Select(e => new Validation(e.Message, e.FieldName, validation.Code)));
            else
                errors.Add(error);
        }
    }

    public static Result<(T1, T2), Err> Combine<T1, T2>(this Result<T1, Err> t1, Result<T2, Err> t2)
    {
        Err? error = null;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (t2.IsFailure) error = error.Combine(t2.Error);
        if (error is not null) return Result.Failure<(T1, T2), Err>(error);
        return Result.Success<(T1, T2), Err>((t1.Ok, t2.Ok));
    }
}
