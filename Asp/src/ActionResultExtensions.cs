namespace FunctionalDdd;

using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
        => result.IsSuccess ? (ActionResult<TValue>)controllerBase.Ok(result.Value) : result.ToErrorActionResult(controllerBase);

    /// <summary>
    /// <see cref="Error"/> extension method that maps domain errors to failed <see cref="ObjectResult"/> using <see cref="ControllerBase"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the <see cref="ActionResult{TValue}"/></typeparam>
    /// <param name="error">The domain error object.</param>
    /// <param name="controllerBase">The controller object.</param>
    /// <returns>
    ///     <para>Converts domain errors to failed <see cref="ObjectResult"/>:</para>
    ///     <list type="table">
    ///         <listheader>
    ///             <term>Domain Error</term>
    ///             <description>HTTP error</description>
    ///         </listheader>
    ///         <item>
    ///             <term><see cref="NotFoundError"/></term>
    ///             <description><see cref="ControllerBase.NotFound()"/></description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="ValidationError"/></term>
    ///             <description><see cref="ControllerBase.ValidationProblem()"/></description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="BadRequestError"/></term>
    ///             <description><see cref="ControllerBase.BadRequest()"/></description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="ConflictError"/></term>
    ///             <description><see cref="ControllerBase.Conflict()"/></description>
    ///         </item>    
    ///         <item>
    ///             <term><see cref="UnauthorizedError"/></term>
    ///             <description><see cref="ControllerBase.Unauthorized()"/></description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="ForbiddenError"/></term>
    ///             <description>403 Forbidden</description>
    ///         </item>    
    ///         <item>
    ///             <term><see cref="UnexpectedError"/></term>
    ///             <description>500 Internal Server Error</description>
    ///         </item>
    ///         <item>
    ///             <term>Everything else</term>
    ///             <description>500 Internal Server Error</description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static ActionResult<TValue> ToErrorActionResult<TValue>(this Error error, ControllerBase controllerBase)
    => error switch
    {
        NotFoundError => (ActionResult<TValue>)controllerBase.Problem(error.Message, error.Instance, StatusCodes.Status404NotFound),
        ValidationError validation => ValidationErrors<TValue>(string.IsNullOrEmpty(error.Message) ? null : error.Message, validation, error.Instance, controllerBase),
        BadRequestError => (ActionResult<TValue>)controllerBase.Problem(error.Message, error.Instance, StatusCodes.Status400BadRequest),
        ConflictError => (ActionResult<TValue>)controllerBase.Problem(error.Message, error.Instance, StatusCodes.Status409Conflict),
        UnauthorizedError => (ActionResult<TValue>)controllerBase.Problem(error.Message, error.Instance, StatusCodes.Status401Unauthorized),
        ForbiddenError => (ActionResult<TValue>)controllerBase.Problem(error.Message, error.Instance, StatusCodes.Status403Forbidden),
        UnexpectedError => (ActionResult<TValue>)controllerBase.StatusCode(StatusCodes.Status500InternalServerError, error),
        _ => (ActionResult<TValue>)controllerBase.StatusCode(StatusCodes.Status500InternalServerError, error),
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
    /// If <see cref="Result{TValue}"/> is in success state this extension method returns
    /// <list type="bullet">
    /// <item><description>Partial Content (206) with header <see cref="ContentRangeHeaderValue "/> for partial results</description></item>
    /// <item><description>Okay (200) status for complete results.</description></item>
    /// </list>
    /// Otherwise it returns the error code corresponding to the failure error object.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="result">The result object.</param>
    /// <param name="controllerBase">Controller object.</param>
    /// <param name="from">The start of the range.</param>
    /// <param name="to">The end of the range.</param>
    /// <param name="length">The total number of items.</param>
    /// <returns></returns>
    public static ActionResult<TValue> ToPartialOrOkActionResult<TValue>(this Result<TValue> result, ControllerBase controllerBase, long from, long to, long length)
    {
        if (result.IsSuccess)
        {
            var partialResult = to - from + 1 != length;
            if (partialResult)
                return new PartialObjectResult(from, to, length, result.Value);

            return controllerBase.Ok(result.Value);
        }

        var error = result.Error;
        return error.ToErrorActionResult<TValue>(controllerBase);
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
    /// <param name="result"></param>
    /// <param name="controllerBase"></param>
    /// <param name="funcRange">Function is called if the <see cref="Result{TIn}"/> is in success state to get the <see cref="ContentRangeHeaderValue "/>.</param>
    /// <param name="funcValue">Function is called if the <see cref="Result{TIn}"/> is in success state to get the value.</param>
    /// <returns></returns>
    public static ActionResult<TOut> ToPartialOrOkActionResult<TIn, TOut>(
        this Result<TIn> result,
        ControllerBase controllerBase,
        Func<TIn, ContentRangeHeaderValue> funcRange,
        Func<TIn, TOut> funcValue)
    {
        if (result.IsSuccess)
        {
            var contentRange = funcRange(result.Value);
            var value = funcValue(result.Value);
            var partialResult = contentRange.To - contentRange.From + 1 != contentRange.Length;
            if (partialResult)
                return new PartialObjectResult(contentRange, value);

            return controllerBase.Ok(value);
        }

        var error = result.Error;
        return error.ToErrorActionResult<TOut>(controllerBase);
    }

    private static ActionResult<TValue> ValidationErrors<TValue>(string? detail, ValidationError validation, string? instance, ControllerBase controllerBase)
    {
        ModelStateDictionary modelState = new();
        foreach (var error in validation.Errors)
            foreach (var detailError in error.Details)
                modelState.AddModelError(error.Name, detailError);

        return controllerBase.ValidationProblem(detail, instance, modelStateDictionary: modelState);
    }
}
