namespace FluentValidation;

using System.Linq;
using FluentValidation.Results;
using FunctionalDDD.Results;
using FunctionalDDD.Results.Errors;

/// <summary>
/// Converts a <see cref="ValidationResult"/> to a <see cref="Result{T}"/>.
/// </summary>
/// <example>
/// Validation of an user object using FluentValidation:
/// <code>
/// class User : AggregateRoot&lt;UserId&gt;
///{
///    public FirstName FirstName { get; }
///    public LastName LastName { get; }
///    public EmailAddress Email { get; }
///    public string Password { get; }
///
///    public static Result&lt;User&gt; New(FirstName firstName, LastName lastName, EmailAddress email, string password)
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
///        v =&gt; v.RuleFor(x =&gt; x.Password).NotEmpty()
///    };
///}
/// </code>
/// </example>
public static class FunctionalDDDValidationExtension
{
    public static Result<T> ToResult<T>(this ValidationResult validationResult, T value)
    {
        if (validationResult.IsValid)
            return Result.Success<T>(value);

        var errors = validationResult.Errors
            .Select(x => new ValidationError.ModelError(x.ErrorMessage, x.PropertyName))
            .ToList();

        return Result.Failure<T>(Error.Validation(errors));
    }

    public static Result<T> ValidateToResult<T>(this IValidator<T> validator, T value) =>
        validator.Validate(value).ToResult(value);
}
