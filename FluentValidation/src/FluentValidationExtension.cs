namespace FunctionalDDD.FluentValidationExt;

using System.Linq;
using FunctionalDDD;

public static class FluentValidationExtension
{
    public static Result<T> ToResult<T>(this global::FluentValidation.Results.ValidationResult validationResult, T value)
    {
        if (validationResult.IsValid)
            return Result.Success<T>(value);

        var errors = validationResult.Errors
            .Select(x => Error.Validation(x.PropertyName, x.ErrorMessage));

        return Result.Failure<T>(new ErrorList(errors));
    }

    public static Result<T> ValidateToResult<T>(this global::FluentValidation.IValidator<T> validator, T value) =>
        validator.Validate(value).ToResult(value);
}
