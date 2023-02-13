
// Generated code
namespace FunctionalDDD;

public static partial class ResultExtensions
{

    public static Result<(T1, T2, T3), Error> Combine<T1, T2, T3>(
      this Result<(T1, T2), Error> t1, Result<T3, Error> tc)
    {
        Error? error = null;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (tc.IsFailure) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3), Error>(error);
        return Result.Success((t1.Value.Item1, t1.Value.Item2, tc.Value));
    }

    public static Result<(T1, T2, T3), Error> Combine<T1, T2, T3>(
       this Result<T1, Error> t1, Result<T2, Error> t2, Result<T3, Error> t3)
    {
        Error? error = null;
        if (t1.IsFailure) error.Combine(t1.Error);
        if (t2.IsFailure) error = error.Combine(t2.Error);
        if (t3.IsFailure) error = error.Combine(t3.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3)>(error);
        return Result.Success((t1.Value, t2.Value, t3.Value));
    }

    public static Result<(T1, T2, T3, T4), Error> Combine<T1, T2, T3, T4>(
      this Result<(T1, T2, T3), Error> t1, Result<T4, Error> tc)
    {
        Error? error = null;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (tc.IsFailure) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3, T4), Error>(error);
        return Result.Success((t1.Value.Item1, t1.Value.Item2, t1.Value.Item3, tc.Value));
    }

    public static Result<(T1, T2, T3, T4), Error> Combine<T1, T2, T3, T4>(
       this Result<T1, Error> t1, Result<T2, Error> t2, Result<T3, Error> t3, Result<T4, Error> t4)
    {
        Error? error = null;
        if (t1.IsFailure) error.Combine(t1.Error);
        if (t2.IsFailure) error = error.Combine(t2.Error);
        if (t3.IsFailure) error = error.Combine(t3.Error);
        if (t4.IsFailure) error = error.Combine(t4.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3, T4)>(error);
        return Result.Success((t1.Value, t2.Value, t3.Value, t4.Value));
    }

    public static Result<(T1, T2, T3, T4, T5), Error> Combine<T1, T2, T3, T4, T5>(
      this Result<(T1, T2, T3, T4), Error> t1, Result<T5, Error> tc)
    {
        Error? error = null;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (tc.IsFailure) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5), Error>(error);
        return Result.Success((t1.Value.Item1, t1.Value.Item2, t1.Value.Item3, t1.Value.Item4, tc.Value));
    }

    public static Result<(T1, T2, T3, T4, T5), Error> Combine<T1, T2, T3, T4, T5>(
       this Result<T1, Error> t1, Result<T2, Error> t2, Result<T3, Error> t3, Result<T4, Error> t4, Result<T5, Error> t5)
    {
        Error? error = null;
        if (t1.IsFailure) error.Combine(t1.Error);
        if (t2.IsFailure) error = error.Combine(t2.Error);
        if (t3.IsFailure) error = error.Combine(t3.Error);
        if (t4.IsFailure) error = error.Combine(t4.Error);
        if (t5.IsFailure) error = error.Combine(t5.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5)>(error);
        return Result.Success((t1.Value, t2.Value, t3.Value, t4.Value, t5.Value));
    }

    public static Result<(T1, T2, T3, T4, T5, T6), Error> Combine<T1, T2, T3, T4, T5, T6>(
      this Result<(T1, T2, T3, T4, T5), Error> t1, Result<T6, Error> tc)
    {
        Error? error = null;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (tc.IsFailure) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6), Error>(error);
        return Result.Success((t1.Value.Item1, t1.Value.Item2, t1.Value.Item3, t1.Value.Item4, t1.Value.Item5, tc.Value));
    }

    public static Result<(T1, T2, T3, T4, T5, T6), Error> Combine<T1, T2, T3, T4, T5, T6>(
       this Result<T1, Error> t1, Result<T2, Error> t2, Result<T3, Error> t3, Result<T4, Error> t4, Result<T5, Error> t5, Result<T6, Error> t6)
    {
        Error? error = null;
        if (t1.IsFailure) error.Combine(t1.Error);
        if (t2.IsFailure) error = error.Combine(t2.Error);
        if (t3.IsFailure) error = error.Combine(t3.Error);
        if (t4.IsFailure) error = error.Combine(t4.Error);
        if (t5.IsFailure) error = error.Combine(t5.Error);
        if (t6.IsFailure) error = error.Combine(t6.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6)>(error);
        return Result.Success((t1.Value, t2.Value, t3.Value, t4.Value, t5.Value, t6.Value));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7), Error> Combine<T1, T2, T3, T4, T5, T6, T7>(
      this Result<(T1, T2, T3, T4, T5, T6), Error> t1, Result<T7, Error> tc)
    {
        Error? error = null;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (tc.IsFailure) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6, T7), Error>(error);
        return Result.Success((t1.Value.Item1, t1.Value.Item2, t1.Value.Item3, t1.Value.Item4, t1.Value.Item5, t1.Value.Item6, tc.Value));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7), Error> Combine<T1, T2, T3, T4, T5, T6, T7>(
       this Result<T1, Error> t1, Result<T2, Error> t2, Result<T3, Error> t3, Result<T4, Error> t4, Result<T5, Error> t5, Result<T6, Error> t6, Result<T7, Error> t7)
    {
        Error? error = null;
        if (t1.IsFailure) error.Combine(t1.Error);
        if (t2.IsFailure) error = error.Combine(t2.Error);
        if (t3.IsFailure) error = error.Combine(t3.Error);
        if (t4.IsFailure) error = error.Combine(t4.Error);
        if (t5.IsFailure) error = error.Combine(t5.Error);
        if (t6.IsFailure) error = error.Combine(t6.Error);
        if (t7.IsFailure) error = error.Combine(t7.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6, T7)>(error);
        return Result.Success((t1.Value, t2.Value, t3.Value, t4.Value, t5.Value, t6.Value, t7.Value));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7, T8), Error> Combine<T1, T2, T3, T4, T5, T6, T7, T8>(
      this Result<(T1, T2, T3, T4, T5, T6, T7), Error> t1, Result<T8, Error> tc)
    {
        Error? error = null;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (tc.IsFailure) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6, T7, T8), Error>(error);
        return Result.Success((t1.Value.Item1, t1.Value.Item2, t1.Value.Item3, t1.Value.Item4, t1.Value.Item5, t1.Value.Item6, t1.Value.Item7, tc.Value));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7, T8), Error> Combine<T1, T2, T3, T4, T5, T6, T7, T8>(
       this Result<T1, Error> t1, Result<T2, Error> t2, Result<T3, Error> t3, Result<T4, Error> t4, Result<T5, Error> t5, Result<T6, Error> t6, Result<T7, Error> t7, Result<T8, Error> t8)
    {
        Error? error = null;
        if (t1.IsFailure) error.Combine(t1.Error);
        if (t2.IsFailure) error = error.Combine(t2.Error);
        if (t3.IsFailure) error = error.Combine(t3.Error);
        if (t4.IsFailure) error = error.Combine(t4.Error);
        if (t5.IsFailure) error = error.Combine(t5.Error);
        if (t6.IsFailure) error = error.Combine(t6.Error);
        if (t7.IsFailure) error = error.Combine(t7.Error);
        if (t8.IsFailure) error = error.Combine(t8.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6, T7, T8)>(error);
        return Result.Success((t1.Value, t2.Value, t3.Value, t4.Value, t5.Value, t6.Value, t7.Value, t8.Value));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Error> Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
      this Result<(T1, T2, T3, T4, T5, T6, T7, T8), Error> t1, Result<T9, Error> tc)
    {
        Error? error = null;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (tc.IsFailure) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Error>(error);
        return Result.Success((t1.Value.Item1, t1.Value.Item2, t1.Value.Item3, t1.Value.Item4, t1.Value.Item5, t1.Value.Item6, t1.Value.Item7, t1.Value.Item8, tc.Value));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Error> Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
       this Result<T1, Error> t1, Result<T2, Error> t2, Result<T3, Error> t3, Result<T4, Error> t4, Result<T5, Error> t5, Result<T6, Error> t6, Result<T7, Error> t7, Result<T8, Error> t8, Result<T9, Error> t9)
    {
        Error? error = null;
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
        return Result.Success((t1.Value, t2.Value, t3.Value, t4.Value, t5.Value, t6.Value, t7.Value, t8.Value, t9.Value));
    }


}