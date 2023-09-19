namespace FunctionalDDD.Results;

using System.Linq;
using FunctionalDDD.Results.Errors;

public static class FluentValidationExtension
{
    public static Result<T> ToResult<T>(this global::FluentValidation.Results.ValidationResult validationResult, T value)
    {
        if (validationResult.IsValid)
            return Result.Success<T>(value);

        var errors = validationResult.Errors
            .Select(x => new ValidationError.ModelError(x.ErrorMessage, x.PropertyName))
            .ToList();

        return Result.Failure<T>(Error.Validation(errors));
    }

    public static Result<T> ValidateToResult<T>(this global::FluentValidation.IValidator<T> validator, T value) =>
        validator.Validate(value).ToResult(value);
}
