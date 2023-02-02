namespace FunctionalDDD;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public static class ActionResultExtention
{
    public static ActionResult<T> ToOkActionResult<T>(this Result<T, Error> result, ControllerBase controllerBase)
    {
        if (result.IsSuccess)
            return controllerBase.Ok(result.Ok);

        return result.ToErrorActionResult(controllerBase);
    }

    public static async Task<ActionResult<T>> ToOkActionResultAsync<T>(this Task<Result<T, Error>> resultTask, ControllerBase controllerBase)
    {
        Result<T, Error> result = await resultTask;

        return result.ToOkActionResult(controllerBase);
    }

    public static async ValueTask<ActionResult<T>> ToOkActionResultAsync<T>(this ValueTask<Result<T, Error>> resultTask, ControllerBase controllerBase)
    {
        Result<T, Error> result = await resultTask;

        return result.ToOkActionResult(controllerBase);
    }

    public static ActionResult<T> ToErrorActionResult<T>(this Result<T, Error> result, ControllerBase controllerBase)
    {
        var error = result.Error;
        return error switch
        {
            NotFoundError => (ActionResult<T>)controllerBase.NotFound(error),
            ValidationError validation => ValidationErrors<T>(validation, controllerBase),
            ConflictError => (ActionResult<T>)controllerBase.Conflict(error),
            UnauthorizedError => (ActionResult<T>)controllerBase.Unauthorized(error),
            ForbiddenError => (ActionResult<T>)controllerBase.Forbid(error.Message),
            UnexpectedError => (ActionResult<T>)controllerBase.StatusCode(500, error),
            _ => throw new NotImplementedException($"Unknown error {error.Code}"),
        };
    }

    public static async Task<ActionResult<T>> ToErrorActionResultAsync<T>(this Task<Result<T, Error>> resultTask, ControllerBase controllerBase)
    {
        Result<T, Error> result = await resultTask;

        return result.ToErrorActionResult(controllerBase);
    }

    public static async ValueTask<ActionResult<T>> ToErrorActionResultAsync<T>(this ValueTask<Result<T, Error>> resultTask, ControllerBase controllerBase)
    {
        Result<T, Error> result = await resultTask;

        return result.ToErrorActionResult(controllerBase);
    }

    private static ActionResult<T> ValidationErrors<T>(ValidationError validation, ControllerBase controllerBase)
    {
        ModelStateDictionary modelState = new();
        foreach (var error in validation.Errors)
            modelState.AddModelError(error.FieldName, error.Message);

        return controllerBase.ValidationProblem(modelState);
    }

}
