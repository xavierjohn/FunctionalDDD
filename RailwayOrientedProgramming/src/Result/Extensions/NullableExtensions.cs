﻿namespace FunctionalDDD;

public static class NullableExtensions
{
    public static Result<T> ToResult<T>(in this T? nullable, Error error)
        where T : struct
    {
        if (!nullable.HasValue)
            return Result.Failure<T>(error);

        return Result.Success<T>(nullable.Value);
    }
    public static Result<T> ToResult<T>(this T? obj, Error error)
        where T : class
    {
        if (obj == null)
            return Result.Failure<T>(error);

        return Result.Success<T>(obj);
    }

    public static async Task<Result<T>> ToResultAsync<T>(this Task<T?> nullableTask, Error errors)
        where T : struct
    {
        var nullable = await nullableTask.ConfigureAwait(false);
        return nullable.ToResult(errors);
    }

    public static async Task<Result<T>> ToResultAsync<T>(this Task<T?> nullableTask, Error errors)
    where T : class
    {
        var nullable = await nullableTask.ConfigureAwait(false);
        return nullable.ToResult(errors);
    }

    public static async ValueTask<Result<T>> ToResultAsync<T>(this ValueTask<T?> nullableTask, Error errors)
        where T : struct
    {
        var nullable = await nullableTask.ConfigureAwait(false);
        return nullable.ToResult(errors);
    }

    public static async ValueTask<Result<T>> ToResultAsync<T>(this ValueTask<T?> nullableTask, Error errors)
        where T : class
    {
        var nullable = await nullableTask.ConfigureAwait(false);
        return nullable.ToResult(errors);
    }
}
