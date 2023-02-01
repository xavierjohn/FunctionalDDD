
// Generated code
namespace FunctionalDDD;

public static partial class ResultExtensions
{

    public static Result<(T1, T2, T3), Err> Combine<T1, T2, T3>(
      this Result<(T1, T2), Err> t1, Result<T3, Err> tc)
    {
        Err? error = null;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (tc.IsFailure) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3), Err>(error);
        return Result.Success((t1.Ok.Item1, t1.Ok.Item2, tc.Ok));
    }

    public static Result<(T1, T2, T3), Err> Combine<T1, T2, T3>(
       this Result<T1, Err> t1, Result<T2, Err> t2, Result<T3, Err> t3)
    {
        Err? error = null;
        if (t1.IsFailure) error.Combine(t1.Error);
        if (t2.IsFailure) error = error.Combine(t2.Error);
        if (t3.IsFailure) error = error.Combine(t3.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3)>(error);
        return Result.Success((t1.Ok, t2.Ok, t3.Ok));
    }

    public static Result<(T1, T2, T3, T4), Err> Combine<T1, T2, T3, T4>(
      this Result<(T1, T2, T3), Err> t1, Result<T4, Err> tc)
    {
        Err? error = null;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (tc.IsFailure) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3, T4), Err>(error);
        return Result.Success((t1.Ok.Item1, t1.Ok.Item2, t1.Ok.Item3, tc.Ok));
    }

    public static Result<(T1, T2, T3, T4), Err> Combine<T1, T2, T3, T4>(
       this Result<T1, Err> t1, Result<T2, Err> t2, Result<T3, Err> t3, Result<T4, Err> t4)
    {
        Err? error = null;
        if (t1.IsFailure) error.Combine(t1.Error);
        if (t2.IsFailure) error = error.Combine(t2.Error);
        if (t3.IsFailure) error = error.Combine(t3.Error);
        if (t4.IsFailure) error = error.Combine(t4.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3, T4)>(error);
        return Result.Success((t1.Ok, t2.Ok, t3.Ok, t4.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5), Err> Combine<T1, T2, T3, T4, T5>(
      this Result<(T1, T2, T3, T4), Err> t1, Result<T5, Err> tc)
    {
        Err? error = null;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (tc.IsFailure) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5), Err>(error);
        return Result.Success((t1.Ok.Item1, t1.Ok.Item2, t1.Ok.Item3, t1.Ok.Item4, tc.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5), Err> Combine<T1, T2, T3, T4, T5>(
       this Result<T1, Err> t1, Result<T2, Err> t2, Result<T3, Err> t3, Result<T4, Err> t4, Result<T5, Err> t5)
    {
        Err? error = null;
        if (t1.IsFailure) error.Combine(t1.Error);
        if (t2.IsFailure) error = error.Combine(t2.Error);
        if (t3.IsFailure) error = error.Combine(t3.Error);
        if (t4.IsFailure) error = error.Combine(t4.Error);
        if (t5.IsFailure) error = error.Combine(t5.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5)>(error);
        return Result.Success((t1.Ok, t2.Ok, t3.Ok, t4.Ok, t5.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5, T6), Err> Combine<T1, T2, T3, T4, T5, T6>(
      this Result<(T1, T2, T3, T4, T5), Err> t1, Result<T6, Err> tc)
    {
        Err? error = null;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (tc.IsFailure) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6), Err>(error);
        return Result.Success((t1.Ok.Item1, t1.Ok.Item2, t1.Ok.Item3, t1.Ok.Item4, t1.Ok.Item5, tc.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5, T6), Err> Combine<T1, T2, T3, T4, T5, T6>(
       this Result<T1, Err> t1, Result<T2, Err> t2, Result<T3, Err> t3, Result<T4, Err> t4, Result<T5, Err> t5, Result<T6, Err> t6)
    {
        Err? error = null;
        if (t1.IsFailure) error.Combine(t1.Error);
        if (t2.IsFailure) error = error.Combine(t2.Error);
        if (t3.IsFailure) error = error.Combine(t3.Error);
        if (t4.IsFailure) error = error.Combine(t4.Error);
        if (t5.IsFailure) error = error.Combine(t5.Error);
        if (t6.IsFailure) error = error.Combine(t6.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6)>(error);
        return Result.Success((t1.Ok, t2.Ok, t3.Ok, t4.Ok, t5.Ok, t6.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7), Err> Combine<T1, T2, T3, T4, T5, T6, T7>(
      this Result<(T1, T2, T3, T4, T5, T6), Err> t1, Result<T7, Err> tc)
    {
        Err? error = null;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (tc.IsFailure) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6, T7), Err>(error);
        return Result.Success((t1.Ok.Item1, t1.Ok.Item2, t1.Ok.Item3, t1.Ok.Item4, t1.Ok.Item5, t1.Ok.Item6, tc.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7), Err> Combine<T1, T2, T3, T4, T5, T6, T7>(
       this Result<T1, Err> t1, Result<T2, Err> t2, Result<T3, Err> t3, Result<T4, Err> t4, Result<T5, Err> t5, Result<T6, Err> t6, Result<T7, Err> t7)
    {
        Err? error = null;
        if (t1.IsFailure) error.Combine(t1.Error);
        if (t2.IsFailure) error = error.Combine(t2.Error);
        if (t3.IsFailure) error = error.Combine(t3.Error);
        if (t4.IsFailure) error = error.Combine(t4.Error);
        if (t5.IsFailure) error = error.Combine(t5.Error);
        if (t6.IsFailure) error = error.Combine(t6.Error);
        if (t7.IsFailure) error = error.Combine(t7.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6, T7)>(error);
        return Result.Success((t1.Ok, t2.Ok, t3.Ok, t4.Ok, t5.Ok, t6.Ok, t7.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7, T8), Err> Combine<T1, T2, T3, T4, T5, T6, T7, T8>(
      this Result<(T1, T2, T3, T4, T5, T6, T7), Err> t1, Result<T8, Err> tc)
    {
        Err? error = null;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (tc.IsFailure) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6, T7, T8), Err>(error);
        return Result.Success((t1.Ok.Item1, t1.Ok.Item2, t1.Ok.Item3, t1.Ok.Item4, t1.Ok.Item5, t1.Ok.Item6, t1.Ok.Item7, tc.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7, T8), Err> Combine<T1, T2, T3, T4, T5, T6, T7, T8>(
       this Result<T1, Err> t1, Result<T2, Err> t2, Result<T3, Err> t3, Result<T4, Err> t4, Result<T5, Err> t5, Result<T6, Err> t6, Result<T7, Err> t7, Result<T8, Err> t8)
    {
        Err? error = null;
        if (t1.IsFailure) error.Combine(t1.Error);
        if (t2.IsFailure) error = error.Combine(t2.Error);
        if (t3.IsFailure) error = error.Combine(t3.Error);
        if (t4.IsFailure) error = error.Combine(t4.Error);
        if (t5.IsFailure) error = error.Combine(t5.Error);
        if (t6.IsFailure) error = error.Combine(t6.Error);
        if (t7.IsFailure) error = error.Combine(t7.Error);
        if (t8.IsFailure) error = error.Combine(t8.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6, T7, T8)>(error);
        return Result.Success((t1.Ok, t2.Ok, t3.Ok, t4.Ok, t5.Ok, t6.Ok, t7.Ok, t8.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Err> Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
      this Result<(T1, T2, T3, T4, T5, T6, T7, T8), Err> t1, Result<T9, Err> tc)
    {
        Err? error = null;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (tc.IsFailure) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Err>(error);
        return Result.Success((t1.Ok.Item1, t1.Ok.Item2, t1.Ok.Item3, t1.Ok.Item4, t1.Ok.Item5, t1.Ok.Item6, t1.Ok.Item7, t1.Ok.Item8, tc.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Err> Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
       this Result<T1, Err> t1, Result<T2, Err> t2, Result<T3, Err> t3, Result<T4, Err> t4, Result<T5, Err> t5, Result<T6, Err> t6, Result<T7, Err> t7, Result<T8, Err> t8, Result<T9, Err> t9)
    {
        Err? error = null;
        if (t1.IsFailure) error.Combine(t1.Error);
        if (t2.IsFailure) error = error.Combine(t2.Error);
        if (t3.IsFailure) error = error.Combine(t3.Error);
        if (t4.IsFailure) error = error.Combine(t4.Error);
        if (t5.IsFailure) error = error.Combine(t5.Error);
        if (t6.IsFailure) error = error.Combine(t6.Error);
        if (t7.IsFailure) error = error.Combine(t7.Error);
        if (t8.IsFailure) error = error.Combine(t8.Error);
        if (t9.IsFailure) error = error.Combine(t9.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6, T7, T8, T9)>(error);
        return Result.Success((t1.Ok, t2.Ok, t3.Ok, t4.Ok, t5.Ok, t6.Ok, t7.Ok, t8.Ok, t9.Ok));
    }


}