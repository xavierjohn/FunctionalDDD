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
    /// Otherwise it returns the error code corresponding to the failure error object.
    /// </summary>
    /// <typeparam name="TValue">The type of the data contained within the <see cref="Result{TValue}"/></typeparam>
    /// <param name="result">The result object.</param>
    /// <returns><see cref="Microsoft.AspNetCore.Http.IResult"/> </returns>
    public static Microsoft.AspNetCore.Http.IResult ToHttpResult<TValue>(this Result<TValue> result)
        => result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToHttpResult();

    /// <summary>
    /// <see cref="Error"/> extension method that maps domain errors to failed <see cref="Microsoft.AspNetCore.Http.IResult"/>.
    /// </summary>
    /// <param name="error">The domain error object.</param>
    /// <returns>
    ///     <para>Converts domain errors to failed <see cref="Microsoft.AspNetCore.Http.IResult"/>:</para>
    /// </returns>
    public static Microsoft.AspNetCore.Http.IResult ToHttpResult(this Error error)
    {
        if (error is ValidationError validationError)
        {
            Dictionary<string, List<string>> dictionary = [];
            foreach (var item in validationError.Errors)
            {
                if (dictionary.TryGetValue(item.Name, out var value))
                    value.AddRange(item.Details);
                else
                    dictionary.Add(item.Name, new List<string>(item.Details));
            }

            Dictionary<string, string[]> errors = [];
            foreach (KeyValuePair<string, List<string>> item in dictionary)
                errors.Add(item.Key, [.. item.Value]);

            return Results.ValidationProblem(errors, validationError.Detail, validationError.Instance);
        }

        var status = error switch
        {
            NotFoundError => StatusCodes.Status404NotFound,
            BadRequestError => StatusCodes.Status400BadRequest,
            ConflictError => StatusCodes.Status409Conflict,
            UnauthorizedError => StatusCodes.Status401Unauthorized,
            ForbiddenError => StatusCodes.Status403Forbidden,
            UnexpectedError => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };
        return Results.Problem(error.Detail, error.Instance, status);
    }
}
