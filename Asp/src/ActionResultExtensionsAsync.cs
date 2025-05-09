﻿namespace FunctionalDdd;

using System.Net.Http.Headers;
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
    /// <param name="resultTask">The result object.</param>
    /// <param name="controllerBase">The controller object.</param>
    /// <returns><see cref="ActionResult{TValue}"/> </returns>
    public static async Task<ActionResult<TValue>> ToActionResultAsync<TValue>(this Task<Result<TValue>> resultTask, ControllerBase controllerBase)
    {
        Result<TValue> result = await resultTask;
        return result.ToActionResult(controllerBase);
    }

    /// <summary>
    /// <see cref="Result{TValue}"/> extension method that returns Okay (200) status if the result is in success state.<br/>
    /// Otherwise it returns the error code corresponding to the failure error object.
    /// </summary>
    /// <typeparam name="TValue">The type of the data contained within the <see cref="Result{TValue}"/></typeparam>
    /// <param name="resultTask">The result object.</param>
    /// <param name="controllerBase">The controller object.</param>
    /// <returns><see cref="ActionResult{TValue}"/></returns>
    public static async ValueTask<ActionResult<TValue>> ToActionResultAsync<TValue>(this ValueTask<Result<TValue>> resultTask, ControllerBase controllerBase)
    {
        Result<TValue> result = await resultTask;
        return result.ToActionResult(controllerBase);
    }

    /// <summary>
    /// If <see cref="Result{TIn}"/> is in success state this extension method returns
    /// <list type="bullet">
    /// <item><description>Partial Content (206) with header <see cref="ContentRangeHeaderValue "/> for partial results</description></item>
    /// <item><description>Okay (200) status for complete results.</description></item>
    /// </list>
    /// Otherwise it returns the error code corresponding to the failure error object.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="resultTask"></param>
    /// <param name="controllerBase"></param>
    /// <param name="funcRange">Function is called if the <see cref="Result{TIn}"/> is in success state to get the <see cref="ContentRangeHeaderValue "/>.</param>
    /// <param name="funcValue">Function is called if the <see cref="Result{TIn}"/> is in success state to get the value.</param>
    /// <returns></returns>
    public static async Task<ActionResult<TOut>> ToActionResultAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        ControllerBase controllerBase,
        Func<TIn, ContentRangeHeaderValue> funcRange,
        Func<TIn, TOut> funcValue)
    {
        var result = await resultTask;
        return result.ToActionResult(controllerBase, funcRange, funcValue);
    }

    /// <summary>
    /// If <see cref="Result{TIn}"/> is in success state this extension method returns
    /// <list type="bullet">
    /// <item><description>Partial Content (206) with header <see cref="ContentRangeHeaderValue "/> for partial results</description></item>
    /// <item><description>Okay (200) status for complete results.</description></item>
    /// </list>
    /// Otherwise it returns the error code corresponding to the failure error object.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="resultTask"></param>
    /// <param name="controllerBase"></param>
    /// <param name="funcRange">Function is called if the <see cref="Result{TIn}"/> is in success state to get the <see cref="ContentRangeHeaderValue "/>.</param>
    /// <param name="funcValue">Function is called if the <see cref="Result{TIn}"/> is in success state to get the value.</param>
    /// <returns></returns>
    public static async ValueTask<ActionResult<TOut>> ToActionResultAsync<TIn, TOut>(
    this ValueTask<Result<TIn>> resultTask,
    ControllerBase controllerBase,
    Func<TIn, ContentRangeHeaderValue> funcRange,
    Func<TIn, TOut> funcValue)
    {
        var result = await resultTask;
        return result.ToActionResult(controllerBase, funcRange, funcValue);
    }

    public static async Task<ActionResult<TValue>> ToActionResultAsync<TValue>(
        this Task<Result<TValue>> resultTask,
        ControllerBase controllerBase,
        long from, long to, long totalLength)
    {
        var result = await resultTask;
        return result.ToActionResult(controllerBase, from, to, totalLength);
    }

    public static async ValueTask<ActionResult<TValue>> ToActionResultAsync<TValue>(
        this ValueTask<Result<TValue>> resultTask,
        ControllerBase controllerBase,
        long from, long to, long totalLength)
    {
        var result = await resultTask;
        return result.ToActionResult(controllerBase, from, to, totalLength);
    }
}
