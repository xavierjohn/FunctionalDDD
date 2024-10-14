namespace FunctionalDdd;

/// <summary>
/// Combines two or more <see cref="Result{TValue}"/> into one tuple containing all the Results.
/// </summary>
public static partial class CombineExtensions
{
    /// <summary>
    /// Combine a <see cref="Result{TValue}"/> with <see cref="Result{Unit}"/> return <see cref="Result{TValue}"/>.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns>Tuple containing both the results.</returns>
    public static Result<T1> Combine<T1>(this Result<T1> t1, Result<Unit> t2)
    {
        Error? error = null;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (t2.IsFailure) error = error.Combine(t2.Error);
        if (error is not null) return Result.Failure<T1>(error);
        return Result.Success(t1.Value);
    }

    /// <summary>
    /// Combine two <see cref="Result{TValue}"/> into one <see cref="Tuple"/> containing all the Results.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns>Tuple containing both the results.</returns>
    public static Result<(T1, T2)> Combine<T1, T2>(this Result<T1> t1, Result<T2> t2)
    {
        Error? error = null;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (t2.IsFailure) error = error.Combine(t2.Error);
        if (error is not null) return Result.Failure<(T1, T2)>(error);
        return Result.Success<(T1, T2)>((t1.Value, t2.Value));
    }
}

/// <summary>
/// Combines two or more <see cref="Result{TValue}"/> into one tuple containing all the Results.
/// </summary>
public static partial class CombineExtensionsAsync
{
    /// <summary>
    /// Combine two <see cref="Result{TValue}"/> into one <see cref="Tuple"/> containing all the Results.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="tt1">Task of T1</param>
    /// <param name="t2"></param>
    /// <returns>Tuple containing both the results.</returns>
    public static async Task<Result<(T1, T2)>> CombineAsync<T1, T2>(this Task<Result<T1>> tt1, Result<T2> t2)
    {
        Error? error = null;
        var t1 = await tt1;
        if (t1.IsFailure) error = error.Combine(t1.Error);
        if (t2.IsFailure) error = error.Combine(t2.Error);
        if (error is not null) return Result.Failure<(T1, T2)>(error);
        return Result.Success((t1.Value, t2.Value));
    }
}
