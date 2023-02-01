namespace FunctionalDDD;

using System.Linq;

public static class FluentValidationExtension
{
    public static Result<T, Err> ToResult<T>(this global::FluentValidation.Results.ValidationResult validationResult, T value)
    {
        if (validationResult.IsValid)
            return Result.Success<T, Err>(value);

        var errors = validationResult.Errors
            .Select(x => new Validation.ModelError(x.ErrorMessage, x.PropertyName))
            .ToList();

        return Result.Failure<T, Err>(Err.Validation(errors));
    }

    public static Result<T, Err> ValidateToResult<T>(this global::FluentValidation.IValidator<T> validator, T value) =>
        validator.Validate(value).ToResult(value);
}
