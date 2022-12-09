namespace FunctionalDDD.Asp;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public class FunctionalDDDBase : ControllerBase
{

    public ActionResult<T> MapToActionResult<T>(Core.Result<T> result)
    {
        if (result.IsSuccess)
            return base.Ok(result.Value);

        return ConvertToHttpError<T>(result.Errors);
    }

    public ActionResult<T> MapToCreatedResult<T>(string location, Core.Result<T> result)
    {
        if (result.IsSuccess)
            return base.Created(location, result.Value);

        return ConvertToHttpError<T>(result.Errors);
    }

    private ActionResult<T> ConvertToHttpError<T>(Core.ErrorList errors)
    {
        var error = errors[0];
        return error switch
        {
            Core.NotFound => (ActionResult<T>)base.NotFound(error),
            Core.Validation => ValidationErrors<T>(errors),
            Core.Conflict => (ActionResult<T>)base.Conflict(error),
            _ => throw new NotImplementedException($"Unknown error {error.Code}"),
        };
    }

    private ActionResult<T> ValidationErrors<T>(Core.ErrorList errors)
    {
        ModelStateDictionary modelState = new();
        foreach (var error in errors)
        {
            if (error is Core.Validation validation)
                modelState.AddModelError(validation.Code, validation.Message);
        }

        return ValidationProblem(modelState);
    }
}
