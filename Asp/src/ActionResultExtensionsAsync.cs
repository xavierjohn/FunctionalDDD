namespace FunctionalDDD.Results.Asp;

using System.Net.Http.Headers;
using FunctionalDDD.Results;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// These extension methods are used to convert the Result object to ActionResult.
/// If the Result is in a failed state, it returns the corresponding HTTP error code.
/// </summary>
public static class ActionResultExtensionsAsync
{

    /// <summary>
    /// <see cref="Result{TValue}"/> extension method that returns Okay (200) status if the result is in success state.<br/>
    /// Otherwise it returns the error code corresponding to the failure error object.
    /// </summary>
    /// <typeparam name="TValue">The type of the data contained within the <see cref="Result{TValue}"/></typeparam>
    /// <param name="result">The result object.</param>
    /// <param name="controllerBase">The controller object.</param>
    /// <returns><see cref="ActionResult{TValue}"/> </returns>
    public static async Task<ActionResult<TValue>> ToOkActionResultAsync<TValue>(this Task<Result<TValue>> resultTask, ControllerBase controllerBase)
    {
        var result = await resultTask;
        return result.ToOkActionResult(controllerBase);
    }

    /// <summary>
    /// <see cref="Result{TValue}"/> extension method that returns Okay (200) status if the result is in success state.<br/>
    /// Otherwise it returns the error code corresponding to the failure error object.
    /// </summary>
    /// <typeparam name="TValue">The type of the data contained within the <see cref="Result{TValue}"/></typeparam>
    /// <param name="result">The result object.</param>
    /// <param name="controllerBase">The controller object.</param>
    /// <returns><see cref="ActionResult{TValue}"/></returns>
    public static async ValueTask<ActionResult<TValue>> ToOkActionResultAsync<TValue>(this ValueTask<Result<TValue>> resultTask, ControllerBase controllerBase)
    {
        var result = await resultTask;
        return result.ToOkActionResult(controllerBase);
    }

    public static async Task<ActionResult<TValue>> ToErrorActionResultAsync<TValue>(this Task<Result<TValue>> resultTask, ControllerBase controllerBase)
    {
        var result = await resultTask;
        return result.ToErrorActionResult(controllerBase);
    }

    public static async ValueTask<ActionResult<TValue>> ToErrorActionResultAsync<TValue>(this ValueTask<Result<TValue>> resultTask, ControllerBase controllerBase)
    {
        var result = await resultTask;
        return result.ToErrorActionResult(controllerBase);
    }


    /// <summary>
    /// <para>If <paramref name="result"/> is in success state, returns Okay (200) or Partial (206) status code based on the <see cref="ContentRangeHeaderValue"/>.</para>
    /// <para>If <paramref name="result"/> is in failed state, returns a failed <see cref="ObjectResult"/> based on mapping <see cref="ActionResultExtensions.ToErrorActionResult"/>.</para>
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="resultTask"></param>
    /// <param name="controllerBase"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static async Task<ActionResult<TOut>> ToPartialOrOkActionResultAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        ControllerBase controllerBase,
        Func<TIn, ContentRangeHeaderValue> funcRange,
        Func<TIn, TOut> funcValue)
    {
        var result = await resultTask;
        return result.ToPartialOrOkActionResult(controllerBase, funcRange, funcValue);
    }

    public static async ValueTask<ActionResult<TOut>> ToPartialOrOkActionResultAsync<TIn, TOut>(
    this ValueTask<Result<TIn>> resultTask,
    ControllerBase controllerBase,
    Func<TIn, ContentRangeHeaderValue> funcRange,
    Func<TIn, TOut> funcValue)
    {
        var result = await resultTask;
        return result.ToPartialOrOkActionResult(controllerBase, funcRange, funcValue);
    }


    public static async Task<ActionResult<TValue>> ToPartialOrOkActionResultAsync<TValue>(
        this Task<Result<TValue>> resultTask,
        ControllerBase controllerBase,
        long from, long to, long totalLength)
    {
        var result = await resultTask;
        return result.ToPartialOrOkActionResult(controllerBase, from, to, totalLength);
    }

    public static async ValueTask<ActionResult<TValue>> ToPartialOrOkActionResultAsync<TValue>(
        this ValueTask<Result<TValue>> resultTask,
        ControllerBase controllerBase,
        long from, long to, long totalLength)
    {
        var result = await resultTask;
        return result.ToPartialOrOkActionResult(controllerBase, from, to, totalLength);
    }
}
