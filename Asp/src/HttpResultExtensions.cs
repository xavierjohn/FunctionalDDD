namespace FunctionalDdd;
using Microsoft.AspNetCore.Http;

/// <summary>
/// These extension methods are used to convert the Result object to <see cref="Results"/>.
/// If the Result is in a failed state, it returns the corresponding HTTP error code.
/// </summary>
public static class HttpResultExtensions
{
    /// <summary>
    /// <see cref="Result{TValue}"/> extension method that returns Okay (200) status if the result is in success state.
    /// For <see cref="Result{Unit}"/> success, returns 204 No Content instead.
    /// Otherwise it returns the error code corresponding to the failure error object.
    /// </summary>
    /// <typeparam name="TValue">The type of the data contained within the <see cref="Result{TValue}"/></typeparam>
    /// <param name="result">The result object.</param>
    /// <returns><see cref="Microsoft.AspNetCore.Http.IResult"/> </returns>
    public static Microsoft.AspNetCore.Http.IResult ToHttpResult<TValue>(this Result<TValue> result)
    {
        if (result.IsSuccess)
        {
            // If TValue is Unit, return 204 No Content
            if (typeof(TValue) == typeof(Unit))
                return Results.NoContent();
            
            return Results.Ok(result.Value);
        }
        
        return result.Error.ToHttpResult();
    }

    /// <summary>
    /// <see cref="Error"/> extension method that maps domain errors to failed <see cref="Microsoft.AspNetCore.Http.IResult"/>.
    /// </summary>
    /// <param name="error">The domain error object.</param>
    /// <returns>
    ///     <para>Converts domain errors to failed <see cref="Microsoft.AspNetCore.Http.IResult"/>:</para>
    ///     <list type="table">
    ///         <listheader>
    ///             <term>Domain Error</term>
    ///             <description>HTTP Status</description>
    ///         </listheader>
    ///         <item>
    ///             <term><see cref="ValidationError"/></term>
    ///             <description>400 Bad Request (with validation details)</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="BadRequestError"/></term>
    ///             <description>400 Bad Request</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="UnauthorizedError"/></term>
    ///             <description>401 Unauthorized</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="ForbiddenError"/></term>
    ///             <description>403 Forbidden</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="NotFoundError"/></term>
    ///             <description>404 Not Found</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="ConflictError"/></term>
    ///             <description>409 Conflict</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="DomainError"/></term>
    ///             <description>422 Unprocessable Entity</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="RateLimitError"/></term>
    ///             <description>429 Too Many Requests</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="UnexpectedError"/></term>
    ///             <description>500 Internal Server Error</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="ServiceUnavailableError"/></term>
    ///             <description>503 Service Unavailable</description>
    ///         </item>
    ///         <item>
    ///             <term>Unknown error types</term>
    ///             <description>500 Internal Server Error</description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static Microsoft.AspNetCore.Http.IResult ToHttpResult(this Error error)
    {
        if (error is ValidationError validationError)
        {
            Dictionary<string, string[]> errors = validationError.FieldErrors
                .GroupBy(x => x.FieldName)
                .ToDictionary(x => x.Key, x => x.SelectMany(y => y.Details).ToArray());

            return Results.ValidationProblem(errors, validationError.Detail, validationError.Instance);
        }

        var status = error switch
        {
            NotFoundError => StatusCodes.Status404NotFound,
            BadRequestError => StatusCodes.Status400BadRequest,
            ConflictError => StatusCodes.Status409Conflict,
            UnauthorizedError => StatusCodes.Status401Unauthorized,
            ForbiddenError => StatusCodes.Status403Forbidden,
            DomainError => StatusCodes.Status422UnprocessableEntity,
            RateLimitError => StatusCodes.Status429TooManyRequests,
            UnexpectedError => StatusCodes.Status500InternalServerError,
            ServiceUnavailableError => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };
        return Results.Problem(error.Detail, error.Instance, status);
    }
}
