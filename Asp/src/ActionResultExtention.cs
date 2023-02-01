namespace FunctionalDDD;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public static class ActionResultExtention
{
    public static ActionResult<T> ToActionResult<T>(this Result<T, Error> result, ControllerBase controllerBase)
    {
        if (result.IsSuccess)
            return controllerBase.Ok(result.Ok);

        return ConvertToHttpError<T>(result.Error, controllerBase);
    }

    public static async Task<ActionResult<T>> ToActionResultAsync<T>(this Task<Result<T, Error>> resultTask, ControllerBase controllerBase)
    {
        Result<T, Error> result = await resultTask;

        return result.ToActionResult(controllerBase);
    }

    public static async ValueTask<ActionResult<T>> ToActionResultAsync<T>(this ValueTask<Result<T, Error>> resultTask, ControllerBase controllerBase)
    {
        Result<T, Error> result = await resultTask;

        return result.ToActionResult(controllerBase);
    }

    public static ActionResult<T> ToCreatedResult<T>(this Result<T, Error> result, ControllerBase controller, string location)
    {
        if (result.IsSuccess)
            return controller.Created(location, result.Ok);

        return ConvertToHttpError<T>(result.Error, controller);
    }


    private static ActionResult<T> ConvertToHttpError<T>(Error error, ControllerBase controllerBase)
    {
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
    private static ActionResult<T> ValidationErrors<T>(ValidationError validation, ControllerBase controllerBase)
    {
        ModelStateDictionary modelState = new();
        foreach (var error in validation.Errors)
            modelState.AddModelError(validation.Message, error.FieldName);

        return controllerBase.ValidationProblem(modelState);
    }

}
