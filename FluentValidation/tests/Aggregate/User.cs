namespace FluentValidationExt.Tests;
using FluentValidation;
using FunctionalDDD.Domain.ValueObjects;
using FunctionalDDD.Domain;
using FunctionalDDD.Results;
using FunctionalDDD.Results.FluentValidation;

internal class User : AggregateRoot<UserId>
{
    public FirstName FirstName { get; }
    public LastName LastName { get; }
    public EmailAddress Email { get; }
    public string Password { get; }

    public static Result<User> New(FirstName firstName, LastName lastName, EmailAddress email, string password)
    {
        var user = new User(firstName, lastName, email, password);
        return s_validator.ValidateToResult(user);
    }


    private User(FirstName firstName, LastName lastName, EmailAddress email, string password)
        : base(UserId.NewUnique())
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = password;
    }

    static readonly InlineValidator<User> s_validator = new()
    {
        v => v.RuleFor(x => x.FirstName).NotNull(),
        v => v.RuleFor(x => x.LastName).NotNull(),
        v => v.RuleFor(x => x.Email).NotNull(),
        v => v.RuleFor(x => x.Password).NotEmpty()
    };
}
