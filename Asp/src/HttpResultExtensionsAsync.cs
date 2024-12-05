namespace FunctionalDdd;

/// <summary>
/// These extension methods are used to convert the <see cref="Result{TValue}"/> object to <see cref="Microsoft.AspNetCore.Http.IResult"/>.
/// If the Result is in a failed state, it returns the corresponding HTTP error code.
/// </summary>
public static class HttpResultExtensionsAsync
{

    /// <summary>
    /// <see cref="Result{TValue}"/> extension method that returns Okay (200) status if the result is in success state.<br/>
    /// Otherwise it returns the error code corresponding to the failure error object.
    /// </summary>
    /// <typeparam name="TValue">The type of the data contained within the <see cref="Result{TValue}"/></typeparam>
    /// <param name="resultTask">The result object.</param>
    /// <returns><see cref="Microsoft.AspNetCore.Http.IResult"/></returns>
    public static async Task<Microsoft.AspNetCore.Http.IResult> ToHttpResultAsync<TValue>(this Task<Result<TValue>> resultTask)
    {
        var result = await resultTask;
        return result.ToHttpResult();
    }

    /// <summary>
    /// <see cref="Result{TValue}"/> extension method that returns Okay (200) status if the result is in success state.<br/>
    /// Otherwise it returns the error code corresponding to the failure error object.
    /// </summary>
    /// <typeparam name="TValue">The type of the data contained within the <see cref="Result{TValue}"/></typeparam>
    /// <param name="resultTask">The result object.</param>
    /// <returns><see cref="Microsoft.AspNetCore.Http.IResult"/> </returns>
    public static async ValueTask<Microsoft.AspNetCore.Http.IResult> ToHttpResultAsync<TValue>(this ValueTask<Result<TValue>> resultTask)
    {
        var result = await resultTask;
        return result.ToHttpResult();
    }
}
