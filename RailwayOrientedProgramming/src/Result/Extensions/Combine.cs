namespace FunctionalDDD;

public static partial class ResultExtensions
{
    public static Result<(T1, T2), Err> Combine<T1, T2>(this Result<T1, Err> t1, Result<T2, Err> t2)
    {
        if (t1.IsFailure || t2.IsFailure)
        {
            var errors = new Errs<Err>();
            if (t1.IsFailure)
                errors.Add(t1.Errs);
            if (t2.IsFailure)
                errors.Add(t2.Errs);
            return Result.Failure<(T1, T2), Err>(errors);
        }

        return Result.Success<(T1, T2), Err>((t1.Ok, t2.Ok));
    }
}
