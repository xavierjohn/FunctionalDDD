namespace FunctionalDDD;

public static partial class ResultExtensions
{
    public static Result<(T1, T2)> Combine<T1, T2>(this Result<T1> t1, Result<T2> t2)
    {
        if (t1.IsFailure || t2.IsFailure)
        {
            var errors = new Errs();
            if (t1.IsFailure)
                errors.AddRange(t1.Errs);
            if (t2.IsFailure)
                errors.AddRange(t2.Errs);
            return Result.Failure<(T1, T2)>(errors);
        }

        return Result.Success((t1.Ok, t2.Ok));
    }
}
