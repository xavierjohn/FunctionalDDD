
// Generated code
namespace FunctionalDDD;

public static partial class ResultExtensions
{

    public static Result<(T1, T2, T3), Error> Combine<T1, T2, T3>(
      this Result<(T1, T2), Error> t1, Result<T3, Error> tc)
    {
        Error? error = null;
        if (t1.IsError) error = error.Combine(t1.Error);
        if (tc.IsError) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3), Error>(error);
        return Result.Success((t1.Ok.Item1, t1.Ok.Item2, tc.Ok));
    }

    public static Result<(T1, T2, T3), Error> Combine<T1, T2, T3>(
       this Result<T1, Error> t1, Result<T2, Error> t2, Result<T3, Error> t3)
    {
        Error? error = null;
        if (t1.IsError) error.Combine(t1.Error);
        if (t2.IsError) error = error.Combine(t2.Error);
        if (t3.IsError) error = error.Combine(t3.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3)>(error);
        return Result.Success((t1.Ok, t2.Ok, t3.Ok));
    }

    public static Result<(T1, T2, T3, T4), Error> Combine<T1, T2, T3, T4>(
      this Result<(T1, T2, T3), Error> t1, Result<T4, Error> tc)
    {
        Error? error = null;
        if (t1.IsError) error = error.Combine(t1.Error);
        if (tc.IsError) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3, T4), Error>(error);
        return Result.Success((t1.Ok.Item1, t1.Ok.Item2, t1.Ok.Item3, tc.Ok));
    }

    public static Result<(T1, T2, T3, T4), Error> Combine<T1, T2, T3, T4>(
       this Result<T1, Error> t1, Result<T2, Error> t2, Result<T3, Error> t3, Result<T4, Error> t4)
    {
        Error? error = null;
        if (t1.IsError) error.Combine(t1.Error);
        if (t2.IsError) error = error.Combine(t2.Error);
        if (t3.IsError) error = error.Combine(t3.Error);
        if (t4.IsError) error = error.Combine(t4.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3, T4)>(error);
        return Result.Success((t1.Ok, t2.Ok, t3.Ok, t4.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5), Error> Combine<T1, T2, T3, T4, T5>(
      this Result<(T1, T2, T3, T4), Error> t1, Result<T5, Error> tc)
    {
        Error? error = null;
        if (t1.IsError) error = error.Combine(t1.Error);
        if (tc.IsError) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5), Error>(error);
        return Result.Success((t1.Ok.Item1, t1.Ok.Item2, t1.Ok.Item3, t1.Ok.Item4, tc.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5), Error> Combine<T1, T2, T3, T4, T5>(
       this Result<T1, Error> t1, Result<T2, Error> t2, Result<T3, Error> t3, Result<T4, Error> t4, Result<T5, Error> t5)
    {
        Error? error = null;
        if (t1.IsError) error.Combine(t1.Error);
        if (t2.IsError) error = error.Combine(t2.Error);
        if (t3.IsError) error = error.Combine(t3.Error);
        if (t4.IsError) error = error.Combine(t4.Error);
        if (t5.IsError) error = error.Combine(t5.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5)>(error);
        return Result.Success((t1.Ok, t2.Ok, t3.Ok, t4.Ok, t5.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5, T6), Error> Combine<T1, T2, T3, T4, T5, T6>(
      this Result<(T1, T2, T3, T4, T5), Error> t1, Result<T6, Error> tc)
    {
        Error? error = null;
        if (t1.IsError) error = error.Combine(t1.Error);
        if (tc.IsError) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6), Error>(error);
        return Result.Success((t1.Ok.Item1, t1.Ok.Item2, t1.Ok.Item3, t1.Ok.Item4, t1.Ok.Item5, tc.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5, T6), Error> Combine<T1, T2, T3, T4, T5, T6>(
       this Result<T1, Error> t1, Result<T2, Error> t2, Result<T3, Error> t3, Result<T4, Error> t4, Result<T5, Error> t5, Result<T6, Error> t6)
    {
        Error? error = null;
        if (t1.IsError) error.Combine(t1.Error);
        if (t2.IsError) error = error.Combine(t2.Error);
        if (t3.IsError) error = error.Combine(t3.Error);
        if (t4.IsError) error = error.Combine(t4.Error);
        if (t5.IsError) error = error.Combine(t5.Error);
        if (t6.IsError) error = error.Combine(t6.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6)>(error);
        return Result.Success((t1.Ok, t2.Ok, t3.Ok, t4.Ok, t5.Ok, t6.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7), Error> Combine<T1, T2, T3, T4, T5, T6, T7>(
      this Result<(T1, T2, T3, T4, T5, T6), Error> t1, Result<T7, Error> tc)
    {
        Error? error = null;
        if (t1.IsError) error = error.Combine(t1.Error);
        if (tc.IsError) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6, T7), Error>(error);
        return Result.Success((t1.Ok.Item1, t1.Ok.Item2, t1.Ok.Item3, t1.Ok.Item4, t1.Ok.Item5, t1.Ok.Item6, tc.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7), Error> Combine<T1, T2, T3, T4, T5, T6, T7>(
       this Result<T1, Error> t1, Result<T2, Error> t2, Result<T3, Error> t3, Result<T4, Error> t4, Result<T5, Error> t5, Result<T6, Error> t6, Result<T7, Error> t7)
    {
        Error? error = null;
        if (t1.IsError) error.Combine(t1.Error);
        if (t2.IsError) error = error.Combine(t2.Error);
        if (t3.IsError) error = error.Combine(t3.Error);
        if (t4.IsError) error = error.Combine(t4.Error);
        if (t5.IsError) error = error.Combine(t5.Error);
        if (t6.IsError) error = error.Combine(t6.Error);
        if (t7.IsError) error = error.Combine(t7.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6, T7)>(error);
        return Result.Success((t1.Ok, t2.Ok, t3.Ok, t4.Ok, t5.Ok, t6.Ok, t7.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7, T8), Error> Combine<T1, T2, T3, T4, T5, T6, T7, T8>(
      this Result<(T1, T2, T3, T4, T5, T6, T7), Error> t1, Result<T8, Error> tc)
    {
        Error? error = null;
        if (t1.IsError) error = error.Combine(t1.Error);
        if (tc.IsError) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6, T7, T8), Error>(error);
        return Result.Success((t1.Ok.Item1, t1.Ok.Item2, t1.Ok.Item3, t1.Ok.Item4, t1.Ok.Item5, t1.Ok.Item6, t1.Ok.Item7, tc.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7, T8), Error> Combine<T1, T2, T3, T4, T5, T6, T7, T8>(
       this Result<T1, Error> t1, Result<T2, Error> t2, Result<T3, Error> t3, Result<T4, Error> t4, Result<T5, Error> t5, Result<T6, Error> t6, Result<T7, Error> t7, Result<T8, Error> t8)
    {
        Error? error = null;
        if (t1.IsError) error.Combine(t1.Error);
        if (t2.IsError) error = error.Combine(t2.Error);
        if (t3.IsError) error = error.Combine(t3.Error);
        if (t4.IsError) error = error.Combine(t4.Error);
        if (t5.IsError) error = error.Combine(t5.Error);
        if (t6.IsError) error = error.Combine(t6.Error);
        if (t7.IsError) error = error.Combine(t7.Error);
        if (t8.IsError) error = error.Combine(t8.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6, T7, T8)>(error);
        return Result.Success((t1.Ok, t2.Ok, t3.Ok, t4.Ok, t5.Ok, t6.Ok, t7.Ok, t8.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Error> Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
      this Result<(T1, T2, T3, T4, T5, T6, T7, T8), Error> t1, Result<T9, Error> tc)
    {
        Error? error = null;
        if (t1.IsError) error = error.Combine(t1.Error);
        if (tc.IsError) error = error.Combine(tc.Error);
        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Error>(error);
        return Result.Success((t1.Ok.Item1, t1.Ok.Item2, t1.Ok.Item3, t1.Ok.Item4, t1.Ok.Item5, t1.Ok.Item6, t1.Ok.Item7, t1.Ok.Item8, tc.Ok));
    }

    public static Result<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Error> Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
       this Result<T1, Error> t1, Result<T2, Error> t2, Result<T3, Error> t3, Result<T4, Error> t4, Result<T5, Error> t5, Result<T6, Error> t6, Result<T7, Error> t7, Result<T8, Error> t8, Result<T9, Error> t9)
    {
        Error? error = null;
        if (t1.IsError) error.Combine(t1.Error);
        if (t2.IsError) error = error.Combine(t2.Error);
        if (t3.IsError) error = error.Combine(t3.Error);
        if (t4.IsError) error = error.Combine(t4.Error);
        if (t5.IsError) error = error.Combine(t5.Error);
        if (t6.IsError) error = error.Combine(t6.Error);
        if (t7.IsError) error = error.Combine(t7.Error);
        if (t8.IsError) error = error.Combine(t8.Error);
        if (t9.IsError) error = error.Combine(t9.Error);

        if (error is not null) return Result.Failure<(T1, T2, T3, T4, T5, T6, T7, T8, T9)>(error);
        return Result.Success((t1.Ok, t2.Ok, t3.Ok, t4.Ok, t5.Ok, t6.Ok, t7.Ok, t8.Ok, t9.Ok));
    }


}