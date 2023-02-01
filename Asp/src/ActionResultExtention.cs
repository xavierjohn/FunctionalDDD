﻿namespace FunctionalDDD;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public static class ActionResultExtention
{
    public static ActionResult<T> ToActionResult<T>(this Result<T, Err> result, ControllerBase controllerBase)
    {
        if (result.IsSuccess)
            return controllerBase.Ok(result.Ok);

        return ConvertToHttpError<T>(result.Error, controllerBase);
    }

    public static async Task<ActionResult<T>> ToActionResultAsync<T>(this Task<Result<T, Err>> resultTask, ControllerBase controllerBase)
    {
        Result<T, Err> result = await resultTask;

        return result.ToActionResult(controllerBase);
    }

    public static async ValueTask<ActionResult<T>> ToActionResultAsync<T>(this ValueTask<Result<T, Err>> resultTask, ControllerBase controllerBase)
    {
        Result<T, Err> result = await resultTask;

        return result.ToActionResult(controllerBase);
    }

    public static ActionResult<T> ToCreatedResult<T>(this Result<T, Err> result, ControllerBase controller, string location)
    {
        if (result.IsSuccess)
            return controller.Created(location, result.Ok);

        return ConvertToHttpError<T>(result.Error, controller);
    }


    private static ActionResult<T> ConvertToHttpError<T>(Err error, ControllerBase controllerBase)
    {
        return error switch
        {
            NotFound => (ActionResult<T>)controllerBase.NotFound(error),
            Validation validation => ValidationErrors<T>(validation, controllerBase),
            Conflict => (ActionResult<T>)controllerBase.Conflict(error),
            Unauthorized => (ActionResult<T>)controllerBase.Unauthorized(error),
            Forbidden => (ActionResult<T>)controllerBase.Forbid(error.Description),
            Unexpected => (ActionResult<T>)controllerBase.StatusCode(500, error),
            _ => throw new NotImplementedException($"Unknown error {error.Code}"),
        };
    }
    private static ActionResult<T> ValidationErrors<T>(Validation validation, ControllerBase controllerBase)
    {
        ModelStateDictionary modelState = new();
        foreach (var error in validation.Errors)
            modelState.AddModelError(validation.Description, error.FieldName);

        return controllerBase.ValidationProblem(modelState);
    }

}
