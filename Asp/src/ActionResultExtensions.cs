namespace FunctionalDDD.Asp;

using FunctionalDDD.RailwayOrientedProgramming;
using FunctionalDDD.RailwayOrientedProgramming.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Net.Http.Headers;

public static class ActionResultExtensions
{
    /// <summary>
    /// Returns Okay (200) status if the result is in success state.
    /// Otherwise it returns the error code corresponding to the failure.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result">The result object.</param>
    /// <param name="controllerBase">The controller object.</param>
    /// <returns></returns>
    public static ActionResult<T> ToOkActionResult<T>(this Result<T> result, ControllerBase controllerBase)
    {
        if (result.IsSuccess)
            return controllerBase.Ok(result.Value);

        return result.ToErrorActionResult(controllerBase);
    }

    public static async Task<ActionResult<T>> ToOkActionResultAsync<T>(this Task<Result<T>> resultTask, ControllerBase controllerBase)
    {
        var result = await resultTask;
        return result.ToOkActionResult(controllerBase);
    }

    public static async ValueTask<ActionResult<T>> ToOkActionResultAsync<T>(this ValueTask<Result<T>> resultTask, ControllerBase controllerBase)
    {
        var result = await resultTask;
        return result.ToOkActionResult(controllerBase);
    }

    /// <summary>
    /// Returns the mapped HTTP failed code for the domain errors.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="error">The error object.</param>
    /// <param name="controllerBase">The controller object.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static ActionResult<T> ToErrorActionResult<T>(this Error error, ControllerBase controllerBase)
    => error switch
    {
        NotFoundError => (ActionResult<T>)controllerBase.NotFound(error),
        ValidationError validation => ValidationErrors<T>(validation, controllerBase),
        BadRequestError => (ActionResult<T>)controllerBase.BadRequest(error),
        ConflictError => (ActionResult<T>)controllerBase.Conflict(error),
        UnauthorizedError => (ActionResult<T>)controllerBase.Unauthorized(error),
        ForbiddenError => (ActionResult<T>)controllerBase.Forbid(error.Message),
        UnexpectedError => (ActionResult<T>)controllerBase.StatusCode(500, error),
        _ => throw new NotImplementedException($"Unknown error {error.Code}"),
    };

    public static ActionResult<T> ToErrorActionResult<T>(this Result<T> result, ControllerBase controllerBase)
    {
        var error = result.Error;
        return error.ToErrorActionResult<T>(controllerBase);
    }

    public static async Task<ActionResult<T>> ToErrorActionResultAsync<T>(this Task<Result<T>> resultTask, ControllerBase controllerBase)
    {
        var result = await resultTask;
        return result.ToErrorActionResult(controllerBase);
    }

    public static async ValueTask<ActionResult<T>> ToErrorActionResultAsync<T>(this ValueTask<Result<T>> resultTask, ControllerBase controllerBase)
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
    /// <returns></returns>
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

    public static ActionResult<T> ToPartialOrOkActionResult<T>(this Result<T> result, ControllerBase controllerBase, long from, long to, long totalLength)
    {
        if (result.IsSuccess)
        {
            var partialResult = to - from + 1 != totalLength;
            if (partialResult)
                return new PartialObjectResult(from, to, totalLength, result.Value);

            return controllerBase.Ok(result.Value);
        }

        var error = result.Error;
        return error.ToErrorActionResult<T>(controllerBase);
    }

    public static async Task<ActionResult<T>> ToPartialOrOkActionResultAsync<T>(this Task<Result<T>> resultTask, ControllerBase controllerBase, long from, long to, long totalLength)
    {
        var result = await resultTask;
        return result.ToPartialOrOkActionResult(controllerBase, from, to, totalLength);
    }

    public static async ValueTask<ActionResult<T>> ToPartialOrOkActionResultAsync<T>(this ValueTask<Result<T>> resultTask, ControllerBase controllerBase, long from, long to, long totalLength)
    {
        var result = await resultTask;
        return result.ToPartialOrOkActionResult(controllerBase, from, to, totalLength);
    }

    private static ActionResult<T> ValidationErrors<T>(ValidationError validation, ControllerBase controllerBase)
    {
        ModelStateDictionary modelState = new();
        foreach (var error in validation.Errors)
            modelState.AddModelError(error.FieldName, error.Message);

        return controllerBase.ValidationProblem(modelState);
    }
}
