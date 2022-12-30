namespace FunctionalDDD.RailwayOrientedProgramming;

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
}
