namespace FunctionalDDD;

public static partial class ResultExtensions
{
    public static Result<(T1, T2)> Combine<T1, T2>(this Result<T1> t1, Result<T2> t2)
    {
        if (t1.IsFailure || t2.IsFailure)
        {
            var errors = new ErrorList();
            if (t1.IsFailure)
                errors.AddRange(t1.Errors);
            if (t2.IsFailure)
                errors.AddRange(t2.Errors);
            return Result.Failure<(T1, T2)>(errors);
        }

        return Result.Success((t1.Value, t2.Value));
    }

    public static Result<(T1, T2, T3)> Combine<T1, T2, T3>(this Result<(T1, T2)> t1, Result<T3> t2)
    {
        if (t1.IsFailure || t2.IsFailure)
        {
            var errors = new ErrorList();
            if (t1.IsFailure)
                errors.AddRange(t1.Errors);
            if (t2.IsFailure)
                errors.AddRange(t2.Errors);
            return Result.Failure<(T1, T2, T3)>(errors);
        }

        return Result.Success((t1.Value.Item1, t1.Value.Item2, t2.Value));
    }

    public static Result<(T1, T2, T3, T4)> Combine<T1, T2, T3, T4>(this Result<(T1, T2, T3)> t1, Result<T4> t2)
    {
        if (t1.IsFailure || t2.IsFailure)
        {
            var errors = new ErrorList();
            if (t1.IsFailure)
                errors.AddRange(t1.Errors);
            if (t2.IsFailure)
                errors.AddRange(t2.Errors);
            return Result.Failure<(T1, T2, T3, T4)>(errors);
        }
        
        return Result.Success((t1.Value.Item1, t1.Value.Item2, t1.Value.Item3, t2.Value));
    }

}
