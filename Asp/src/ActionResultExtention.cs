namespace FunctionalDDD;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public static class ActionResultExtention
{
    public static ActionResult<T> ToActionResult<T>(this Result<T> result, ControllerBase controllerBase)
    {
        if (result.IsSuccess)
            return controllerBase.Ok(result.Value);

        return ConvertToHttpError<T>(result.Errors, controllerBase);
    }

    public static async Task<ActionResult<T>> ToActionResultAsync<T>(this Task<Result<T>> resultTask, ControllerBase controllerBase)
    {
        Result<T> result = await resultTask;

        return result.ToActionResult(controllerBase);
    }

    public static async ValueTask<ActionResult<T>> ToActionResultAsync<T>(this ValueTask<Result<T>> resultTask, ControllerBase controllerBase)
    {
        Result<T> result = await resultTask;

        return result.ToActionResult(controllerBase);
    }

    public static ActionResult<T> ToCreatedResult<T>(this Result<T> result, ControllerBase controller, string location)
    {
        if (result.IsSuccess)
            return controller.Created(location, result.Value);

        return ConvertToHttpError<T>(result.Errors, controller);
    }


    private static ActionResult<T> ConvertToHttpError<T>(ErrorList errors, ControllerBase controllerBase)
    {
        var error = errors[0];
        return error switch
        {
            NotFound => (ActionResult<T>)controllerBase.NotFound(error),
            Validation => ValidationErrors<T>(errors, controllerBase),
            Conflict => (ActionResult<T>)controllerBase.Conflict(error),
            Unauthorized => (ActionResult<T>)controllerBase.Unauthorized(error),
            Forbidden => (ActionResult<T>)controllerBase.Forbid(error.Message),
            _ => throw new NotImplementedException($"Unknown error {error.Code}"),
        };
    }
    private static ActionResult<T> ValidationErrors<T>(ErrorList errors, ControllerBase controllerBase)
    {
        ModelStateDictionary modelState = new();
        foreach (var error in errors)
        {
            if (error is Validation validation)
                modelState.AddModelError(validation.Code, validation.Message);
        }

        return controllerBase.ValidationProblem(modelState);
    }

}
