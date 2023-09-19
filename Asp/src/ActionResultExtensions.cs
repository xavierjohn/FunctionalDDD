namespace FunctionalDDD.Asp;

using FunctionalDDD.Results;
using FunctionalDDD.Results.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Net.Http.Headers;

/// <summary>
/// These extension methods are used to convert the Result object to ActionResult.
/// If the Result is in a failed state, it returns the corresponding HTTP error code.
/// </summary>
public static class ActionResultExtensions
{
    /// <summary>
    /// <see cref="Result{TValue}"/> extension method that returns Okay (200) status if the result is in success state.
    /// Otherwise it returns the error code corresponding to the failure error object.
    /// </summary>
    /// <typeparam name="TValue">The type of the data contained within the <see cref="Result{TValue}"/></typeparam>
    /// <param name="result">The result object.</param>
    /// <param name="controllerBase">The controller object.</param>
    /// <returns><see cref="ActionResult{TValue}"/> </returns>
    public static ActionResult<TValue> ToOkActionResult<TValue>(this Result<TValue> result, ControllerBase controllerBase)
    {
        if (result.IsSuccess)
            return controllerBase.Ok(result.Value);

        return result.ToErrorActionResult(controllerBase);
    }

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

    /// <summary>
    /// <see cref="Error"/> extension method that maps domain errors to failed <see cref="ObjectResult"/> using <see cref="ControllerBase"/>.
    /// <para>
    /// For Example:<br/>
    /// <see cref="ValidationError"/> returns <see cref="BadRequestObjectResult"/> <br/>
    /// <see cref="NotFoundError"/> returns <see cref="NotFoundObjectResult"/> <br/>
    /// </para>
    /// </summary>
    /// <typeparam name="TValue">The type of the <see cref="ActionResult{TValue}"/></typeparam>
    /// <param name="error">The error object.</param>
    /// <param name="controllerBase">The controller object.</param>
    /// <returns><see cref="ActionResult{TValue}"/> </returns>
    /// <exception cref="NotImplementedException">Thrown if the error type is unknown.</exception>
    public static ActionResult<TValue> ToErrorActionResult<TValue>(this Error error, ControllerBase controllerBase)
    => error switch
    {
        NotFoundError => (ActionResult<TValue>)controllerBase.NotFound(error),
        ValidationError validation => ValidationErrors<TValue>(validation, controllerBase),
        BadRequestError => (ActionResult<TValue>)controllerBase.BadRequest(error),
        ConflictError => (ActionResult<TValue>)controllerBase.Conflict(error),
        UnauthorizedError => (ActionResult<TValue>)controllerBase.Unauthorized(error),
        ForbiddenError => (ActionResult<TValue>)controllerBase.Forbid(error.Message),
        UnexpectedError => (ActionResult<TValue>)controllerBase.StatusCode(500, error),
        _ => throw new NotImplementedException($"Unknown error {error.Code}"),
    };

    /// <summary>
    /// <see cref="Result{TValue}"/> extension method that maps domain errors to failed <see cref="ObjectResult"/> using <see cref="ControllerBase"/>.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="result"></param>
    /// <param name="controllerBase"></param>
    /// <returns></returns>
    public static ActionResult<TValue> ToErrorActionResult<TValue>(this Result<TValue> result, ControllerBase controllerBase)
    {
        var error = result.Error;
        return error.ToErrorActionResult<TValue>(controllerBase);
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
    /// Returns partial status code (206) when partial result is returned. 
    /// Returns Okay status code (200) when all data is returned.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="result">The result object.</param>
    /// <param name="controllerBase">Controller object.</param>
    /// <param name="func">Callback function that returns the range and data.</param>
    /// <returns><see cref="ActionResult{TValue}"/> </returns>
    public static ActionResult<TOut> ToPartialOrOkActionResult<TIn, TOut>(this Result<TIn> result, ControllerBase controllerBase, Func<TIn, (ContentRangeHeaderValue, TOut)> func)
    {
        if (result.IsSuccess)
        {
            var (contentRangeHeaderValue, tout) = func(result.Value);
            var partialResult = contentRangeHeaderValue.To - contentRangeHeaderValue.From + 1 != contentRangeHeaderValue.Length;
            if (partialResult)
                return new PartialObjectResult(contentRangeHeaderValue, tout);

            return controllerBase.Ok(result.Value);
        }

        var error = result.Error;
        return error.ToErrorActionResult<TOut>(controllerBase);
    }

    /// <summary>
    /// Returns partial status code (206) and adds header <see cref="ContentRangeHeaderValue "/>  when partial result is returned.
    /// Otherwise returns Okay (200).
    /// 
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="result">The result object.</param>
    /// <param name="controllerBase">Controller object.</param>
    /// <param name="from">The start of the range.</param>
    /// <param name="to">The end of the range.</param>
    /// <param name="totalLength">The total number of items.</param>
    /// <returns></returns>
    public static ActionResult<TValue> ToPartialOrOkActionResult<TValue>(this Result<TValue> result, ControllerBase controllerBase, long from, long to, long totalLength)
    {
        if (result.IsSuccess)
        {
            var partialResult = to - from + 1 != totalLength;
            if (partialResult)
                return new PartialObjectResult(from, to, totalLength, result.Value);

            return controllerBase.Ok(result.Value);
        }

        var error = result.Error;
        return error.ToErrorActionResult<TValue>(controllerBase);
    }

    public static async Task<ActionResult<TValue>> ToPartialOrOkActionResultAsync<TValue>(this Task<Result<TValue>> resultTask, ControllerBase controllerBase, long from, long to, long totalLength)
    {
        var result = await resultTask;
        return result.ToPartialOrOkActionResult(controllerBase, from, to, totalLength);
    }

    public static async ValueTask<ActionResult<TValue>> ToPartialOrOkActionResultAsync<TValue>(this ValueTask<Result<TValue>> resultTask, ControllerBase controllerBase, long from, long to, long totalLength)
    {
        var result = await resultTask;
        return result.ToPartialOrOkActionResult(controllerBase, from, to, totalLength);
    }

    private static ActionResult<TValue> ValidationErrors<TValue>(ValidationError validation, ControllerBase controllerBase)
    {
        ModelStateDictionary modelState = new();
        foreach (var error in validation.Errors)
            modelState.AddModelError(error.FieldName, error.Message);

        return controllerBase.ValidationProblem(modelState);
    }
}
