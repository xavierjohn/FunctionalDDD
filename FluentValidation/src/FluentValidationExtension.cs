namespace FunctionalDdd;

using FluentValidation;
using FluentValidation.Results;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using static FunctionalDdd.ValidationError;

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
            return Result.Success(value);

        ImmutableArray<FieldError> errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .Select(g => new FieldError(g.Key, g.Select(e => e.ErrorMessage).ToArray()))
            .ToImmutableArray();

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
        [CallerArgumentExpression(nameof(value))] string paramName = "value",
        string? message = null)
    {
        ValidationResult result = value is null
        ? new ValidationResult([new ValidationFailure(paramName, message ?? $"'{paramName}' must not be empty.")])
        : validator.Validate(value);

        return result.ToResult(value);
    }

    /// <summary>
    /// Asynchronously validates the specified instance and returns a <see cref="Result{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type being validated</typeparam>
    /// <param name="validator">The validator.</param>
    /// <param name="value">The value being validated.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <param name="message">Optional error message if the value is null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success if valid. Failure with validation errors if invalid.</returns>
    /// <example>
    /// <code>
    /// var createProductValidator = new CreateProductRequestValidator();
    /// return await createProductValidator.ValidateToResultAsync(request, cancellationToken: cancellationToken)
    ///     .BindAsync(req => Product.CreateAsync(req.Name, req.Price, cancellationToken), cancellationToken);
    /// </code>
    /// </example>
    public static async Task<Result<T>> ValidateToResultAsync<T>(
        this IValidator<T> validator,
        T value,
        [CallerArgumentExpression(nameof(value))] string paramName = "value",
        string? message = null,
        CancellationToken cancellationToken = default)
    {
        ValidationResult result = value is null
            ? new ValidationResult([new ValidationFailure(paramName, message ?? $"'{paramName}' must not be empty.")])
            : await validator.ValidateAsync(value, cancellationToken).ConfigureAwait(false);

        return result.ToResult(value);
    }
}
