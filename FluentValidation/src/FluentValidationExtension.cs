namespace FunctionalDdd;

using System.Linq;
using System.Runtime.CompilerServices;
using FluentValidation;
using FluentValidation.Results;

/// <summary>
/// Converts a <see cref="ValidationResult"/> to a <see cref="Result{T}"/>.
/// </summary>
/// <example>
/// Validation of an user object using FluentValidation:
/// <code>
/// class User : Aggregate&lt;UserId&gt;
///{
///    public FirstName FirstName { get; }
///    public LastName LastName { get; }
///    public EmailAddress Email { get; }
///    public string Password { get; }
///
///    public static Result&lt;User&gt; TryCreate(FirstName firstName, LastName lastName, EmailAddress email, string password)
///    {
///        var user = new User(firstName, lastName, email, password);
///        return s_validator.ValidateToResult(user);
///    }
///
///
///    private User(FirstName firstName, LastName lastName, EmailAddress email, string password)
///        : base(UserId.NewUnique())
///    {
///        FirstName = firstName;
///        LastName = lastName;
///        Email = email;
///        Password = password;
///    }
///
///    static readonly InlineValidator&lt;User&gt; s_validator = new()
///    {
///        v =&gt; v.RuleFor(x =&gt; x.FirstName).NotNull(),
///        v =&gt; v.RuleFor(x =&gt; x.LastName).NotNull(),
///        v =&gt; v.RuleFor(x =&gt; x.Email).NotNull(),
///        v =&gt; v.RuleFor(x => x.Password).Matches("(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^A-Za-z0-9])(?=.{8,})")
///              .WithMessage("'Password' must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit and one special character.")
///    };
///}
/// </code>
/// </example>
public static class FunctionalDDDValidationExtension
{
    /// <summary>
    /// Convert ValidationResult to <see cref="Result{T}"/>."/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="validationResult"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Result<T> ToResult<T>(this ValidationResult validationResult, T value)
    {
        if (validationResult.IsValid)
            return Result.Success<T>(value);

        var errors = validationResult.Errors
            .Select(x => new ValidationError.FieldDetails(x.PropertyName, [x.ErrorMessage]))
            .ToArray();

        return Result.Failure<T>(Error.Validation(errors));
    }

    /// <summary>
    /// Calls the FluentValidation Validate function and converts the result to <see cref="Result{T}"/>."/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="validator"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Result<T> ValidateToResult<T>(
        this IValidator<T> validator,
        T value,
        [CallerArgumentExpression(nameof(value))] string paramName = "value")
    {
        var result = value is null
        ? new ValidationResult(new[] { new ValidationFailure(paramName, $"{paramName} must not be empty.") })
        : validator.Validate(value);

        return result.ToResult(value);
    }
}
