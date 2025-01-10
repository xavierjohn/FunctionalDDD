namespace FluentValidationExt.Tests;
using FluentValidation;

internal class User : Aggregate<UserId>
{
    public FirstName FirstName { get; }
    public LastName LastName { get; }
    public EmailAddress Email { get; }
    public string Password { get; }

    public static Result<User> TryCreate(FirstName firstName, LastName lastName, EmailAddress email, string password) // password shown as string to demo validation but you should have a Password Type.
    {
        var user = new User(firstName, lastName, email, password);
        var validator = new UserValidator();
        return validator.ValidateToResult(user);
    }

    private User(FirstName firstName, LastName lastName, EmailAddress email, string password)
        : base(UserId.NewUnique())
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = password;
    }

    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            _ = RuleFor(user => user.FirstName).NotNull();
            _ = RuleFor(user => user.LastName).NotNull();
            _ = RuleFor(user => user.Email).NotNull();
            _ = RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Password must not be empty.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
        }
    }
}
