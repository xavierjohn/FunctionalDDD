
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

    public static Result<(T1, T2, T3)> Combine<T1, T2, T3>(this Result<T1> t1, Result<T2> t2, Result<T3> t3)
    {
        if (t1.IsFailure || t2.IsFailure || t3.IsFailure)
        {
            var errors = new ErrorList();
            if (t1.IsFailure) errors.AddRange(t1.Errors);
            if (t2.IsFailure) errors.AddRange(t2.Errors);
            if (t3.IsFailure) errors.AddRange(t3.Errors);
            return Result.Failure<(T1, T2, T3)>(errors);
        }

        return Result.Success((t1.Value, t2.Value, t3.Value));
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

    public static Result<(T1, T2, T3, T4)> Combine<T1, T2, T3, T4>(this Result<T1> t1, Result<T2> t2, Result<T3> t3, Result<T4> t4)
    {
        if (t1.IsFailure || t2.IsFailure || t3.IsFailure || t4.IsFailure)
        {
            var errors = new ErrorList();
            if (t1.IsFailure) errors.AddRange(t1.Errors);
            if (t2.IsFailure) errors.AddRange(t2.Errors);
            if (t3.IsFailure) errors.AddRange(t3.Errors);
            if (t4.IsFailure) errors.AddRange(t4.Errors);
            return Result.Failure<(T1, T2, T3, T4)>(errors);
        }

        return Result.Success((t1.Value, t2.Value, t3.Value, t4.Value));
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

    public static Result<(T1, T2, T3, T4, T5)> Combine<T1, T2, T3, T4, T5>(this Result<T1> t1, Result<T2> t2, Result<T3> t3, Result<T4> t4, Result<T5> t5)
    {
        if (t1.IsFailure || t2.IsFailure || t3.IsFailure || t4.IsFailure || t5.IsFailure)
        {
            var errors = new ErrorList();
            if (t1.IsFailure) errors.AddRange(t1.Errors);
            if (t2.IsFailure) errors.AddRange(t2.Errors);
            if (t3.IsFailure) errors.AddRange(t3.Errors);
            if (t4.IsFailure) errors.AddRange(t4.Errors);
            if (t5.IsFailure) errors.AddRange(t5.Errors);
            return Result.Failure<(T1, T2, T3, T4, T5)>(errors);
        }

        return Result.Success((t1.Value, t2.Value, t3.Value, t4.Value, t5.Value));
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

    public static Result<(T1, T2, T3, T4, T5, T6)> Combine<T1, T2, T3, T4, T5, T6>(this Result<T1> t1, Result<T2> t2, Result<T3> t3, Result<T4> t4, Result<T5> t5, Result<T6> t6)
    {
        if (t1.IsFailure || t2.IsFailure || t3.IsFailure || t4.IsFailure || t5.IsFailure || t6.IsFailure)
        {
            var errors = new ErrorList();
            if (t1.IsFailure) errors.AddRange(t1.Errors);
            if (t2.IsFailure) errors.AddRange(t2.Errors);
            if (t3.IsFailure) errors.AddRange(t3.Errors);
            if (t4.IsFailure) errors.AddRange(t4.Errors);
            if (t5.IsFailure) errors.AddRange(t5.Errors);
            if (t6.IsFailure) errors.AddRange(t6.Errors);
            return Result.Failure<(T1, T2, T3, T4, T5, T6)>(errors);
        }

        return Result.Success((t1.Value, t2.Value, t3.Value, t4.Value, t5.Value, t6.Value));
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

    public static Result<(T1, T2, T3, T4, T5, T6, T7)> Combine<T1, T2, T3, T4, T5, T6, T7>(this Result<T1> t1, Result<T2> t2, Result<T3> t3, Result<T4> t4, Result<T5> t5, Result<T6> t6, Result<T7> t7)
    {
        if (t1.IsFailure || t2.IsFailure || t3.IsFailure || t4.IsFailure || t5.IsFailure || t6.IsFailure || t7.IsFailure)
        {
            var errors = new ErrorList();
            if (t1.IsFailure) errors.AddRange(t1.Errors);
            if (t2.IsFailure) errors.AddRange(t2.Errors);
            if (t3.IsFailure) errors.AddRange(t3.Errors);
            if (t4.IsFailure) errors.AddRange(t4.Errors);
            if (t5.IsFailure) errors.AddRange(t5.Errors);
            if (t6.IsFailure) errors.AddRange(t6.Errors);
            if (t7.IsFailure) errors.AddRange(t7.Errors);
            return Result.Failure<(T1, T2, T3, T4, T5, T6, T7)>(errors);
        }

        return Result.Success((t1.Value, t2.Value, t3.Value, t4.Value, t5.Value, t6.Value, t7.Value));
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

    public static Result<(T1, T2, T3, T4, T5, T6, T7, T8)> Combine<T1, T2, T3, T4, T5, T6, T7, T8>(this Result<T1> t1, Result<T2> t2, Result<T3> t3, Result<T4> t4, Result<T5> t5, Result<T6> t6, Result<T7> t7, Result<T8> t8)
    {
        if (t1.IsFailure || t2.IsFailure || t3.IsFailure || t4.IsFailure || t5.IsFailure || t6.IsFailure || t7.IsFailure || t8.IsFailure)
        {
            var errors = new ErrorList();
            if (t1.IsFailure) errors.AddRange(t1.Errors);
            if (t2.IsFailure) errors.AddRange(t2.Errors);
            if (t3.IsFailure) errors.AddRange(t3.Errors);
            if (t4.IsFailure) errors.AddRange(t4.Errors);
            if (t5.IsFailure) errors.AddRange(t5.Errors);
            if (t6.IsFailure) errors.AddRange(t6.Errors);
            if (t7.IsFailure) errors.AddRange(t7.Errors);
            if (t8.IsFailure) errors.AddRange(t8.Errors);
            return Result.Failure<(T1, T2, T3, T4, T5, T6, T7, T8)>(errors);
        }

        return Result.Success((t1.Value, t2.Value, t3.Value, t4.Value, t5.Value, t6.Value, t7.Value, t8.Value));
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

    public static Result<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Result<T1> t1, Result<T2> t2, Result<T3> t3, Result<T4> t4, Result<T5> t5, Result<T6> t6, Result<T7> t7, Result<T8> t8, Result<T9> t9)
    {
        if (t1.IsFailure || t2.IsFailure || t3.IsFailure || t4.IsFailure || t5.IsFailure || t6.IsFailure || t7.IsFailure || t8.IsFailure || t9.IsFailure)
        {
            var errors = new ErrorList();
            if (t1.IsFailure) errors.AddRange(t1.Errors);
            if (t2.IsFailure) errors.AddRange(t2.Errors);
            if (t3.IsFailure) errors.AddRange(t3.Errors);
            if (t4.IsFailure) errors.AddRange(t4.Errors);
            if (t5.IsFailure) errors.AddRange(t5.Errors);
            if (t6.IsFailure) errors.AddRange(t6.Errors);
            if (t7.IsFailure) errors.AddRange(t7.Errors);
            if (t8.IsFailure) errors.AddRange(t8.Errors);
            if (t9.IsFailure) errors.AddRange(t9.Errors);
            return Result.Failure<(T1, T2, T3, T4, T5, T6, T7, T8, T9)>(errors);
        }

        return Result.Success((t1.Value, t2.Value, t3.Value, t4.Value, t5.Value, t6.Value, t7.Value, t8.Value, t9.Value));
    }


}