
// Generated code
namespace FunctionalDDD;

public static partial class ResultExtensions
{

    public static Result<(T1, T2, T3)> Combine<T1, T2, T3>(this Result<(T1, T2)> t1, Result<T3> tc)
    {
	    if (t1.IsFailure || tc.IsFailure)
        {
            var errors = new ErrorList();
            if (t1.IsFailure)
                errors.AddRange(t1.Errors);
            if (tc.IsFailure)
                errors.AddRange(tc.Errors);
            return Result.Failure<(T1, T2, T3)>(errors);
        }
		
        return Result.Success((t1.Value.Item1, t1.Value.Item2, tc.Value));
    }

    public static Result<(T1, T2, T3, T4)> Combine<T1, T2, T3, T4>(this Result<(T1, T2, T3)> t1, Result<T4> tc)
    {
	    if (t1.IsFailure || tc.IsFailure)
        {
            var errors = new ErrorList();
            if (t1.IsFailure)
                errors.AddRange(t1.Errors);
            if (tc.IsFailure)
                errors.AddRange(tc.Errors);
            return Result.Failure<(T1, T2, T3, T4)>(errors);
        }
		
        return Result.Success((t1.Value.Item1, t1.Value.Item2, t1.Value.Item3, tc.Value));
    }

    public static Result<(T1, T2, T3, T4, T5)> Combine<T1, T2, T3, T4, T5>(this Result<(T1, T2, T3, T4)> t1, Result<T5> tc)
    {
	    if (t1.IsFailure || tc.IsFailure)
        {
            var errors = new ErrorList();
            if (t1.IsFailure)
                errors.AddRange(t1.Errors);
            if (tc.IsFailure)
                errors.AddRange(tc.Errors);
            return Result.Failure<(T1, T2, T3, T4, T5)>(errors);
        }
		
        return Result.Success((t1.Value.Item1, t1.Value.Item2, t1.Value.Item3, t1.Value.Item4, tc.Value));
    }

    public static Result<(T1, T2, T3, T4, T5, T6)> Combine<T1, T2, T3, T4, T5, T6>(this Result<(T1, T2, T3, T4, T5)> t1, Result<T6> tc)
    {
	    if (t1.IsFailure || tc.IsFailure)
        {
            var errors = new ErrorList();
            if (t1.IsFailure)
                errors.AddRange(t1.Errors);
            if (tc.IsFailure)
                errors.AddRange(tc.Errors);
            return Result.Failure<(T1, T2, T3, T4, T5, T6)>(errors);
        }
		
        return Result.Success((t1.Value.Item1, t1.Value.Item2, t1.Value.Item3, t1.Value.Item4, t1.Value.Item5, tc.Value));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7)> Combine<T1, T2, T3, T4, T5, T6, T7>(this Result<(T1, T2, T3, T4, T5, T6)> t1, Result<T7> tc)
    {
	    if (t1.IsFailure || tc.IsFailure)
        {
            var errors = new ErrorList();
            if (t1.IsFailure)
                errors.AddRange(t1.Errors);
            if (tc.IsFailure)
                errors.AddRange(tc.Errors);
            return Result.Failure<(T1, T2, T3, T4, T5, T6, T7)>(errors);
        }
		
        return Result.Success((t1.Value.Item1, t1.Value.Item2, t1.Value.Item3, t1.Value.Item4, t1.Value.Item5, t1.Value.Item6, tc.Value));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7, T8)> Combine<T1, T2, T3, T4, T5, T6, T7, T8>(this Result<(T1, T2, T3, T4, T5, T6, T7)> t1, Result<T8> tc)
    {
	    if (t1.IsFailure || tc.IsFailure)
        {
            var errors = new ErrorList();
            if (t1.IsFailure)
                errors.AddRange(t1.Errors);
            if (tc.IsFailure)
                errors.AddRange(tc.Errors);
            return Result.Failure<(T1, T2, T3, T4, T5, T6, T7, T8)>(errors);
        }
		
        return Result.Success((t1.Value.Item1, t1.Value.Item2, t1.Value.Item3, t1.Value.Item4, t1.Value.Item5, t1.Value.Item6, t1.Value.Item7, tc.Value));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Result<(T1, T2, T3, T4, T5, T6, T7, T8)> t1, Result<T9> tc)
    {
	    if (t1.IsFailure || tc.IsFailure)
        {
            var errors = new ErrorList();
            if (t1.IsFailure)
                errors.AddRange(t1.Errors);
            if (tc.IsFailure)
                errors.AddRange(tc.Errors);
            return Result.Failure<(T1, T2, T3, T4, T5, T6, T7, T8, T9)>(errors);
        }
		
        return Result.Success((t1.Value.Item1, t1.Value.Item2, t1.Value.Item3, t1.Value.Item4, t1.Value.Item5, t1.Value.Item6, t1.Value.Item7, t1.Value.Item8, tc.Value));
    }


}