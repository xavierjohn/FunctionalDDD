namespace FunctionalDDD.Asp;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public class FunctionalDDDBase : ControllerBase
{

    public ActionResult<T> MapToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return base.Ok(result.Value);

        return ConvertToHttpError<T>(result.Errors);
    }

    public ActionResult<T> MapToCreatedResult<T>(string location, Result<T> result)
    {
        if (result.IsSuccess)
            return base.Created(location, result.Value);

        return ConvertToHttpError<T>(result.Errors);
    }

    private ActionResult<T> ConvertToHttpError<T>(ErrorList errors)
    {
        var error = errors[0];
        return error switch
        {
            FunctionalDDD.NotFound => (ActionResult<T>)base.NotFound(error),
            FunctionalDDD.Validation => ValidationErrors<T>(errors),
            FunctionalDDD.Conflict => (ActionResult<T>)base.Conflict(error),
            FunctionalDDD.Unauthorized => (ActionResult<T>)base.Unauthorized(error),
            _ => throw new NotImplementedException($"Unknown error {error.Code}"),
        };
    }

    private ActionResult<T> ValidationErrors<T>(ErrorList errors)
    {
        ModelStateDictionary modelState = new();
        foreach (var error in errors)
        {
            if (error is Validation validation)
                modelState.AddModelError(validation.Code, validation.Message);
        }

        return ValidationProblem(modelState);
    }
}
