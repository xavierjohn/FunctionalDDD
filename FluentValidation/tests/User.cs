namespace BuberDinner.Domain.User.Entities;

public class User : Entity<Guid>
{
    public FirstName FirstName { get; }
    public LastName LastName { get; }
    public EmailAddress Email { get; }
    public string Password { get; }

    public static Result<User, ErrorList> Create(FirstName firstName, LastName lastName, EmailAddress email, string password)
    {
        var user = new User(firstName, lastName, email, password);
        return s_validator.ValidateToResult(user);
    }


    private User(FirstName firstName, LastName lastName, EmailAddress email, string password)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = password;
    }

    static readonly InlineValidator<User> s_validator = new()
    {
        v => v.RuleFor(x => x.FirstName).NotEmpty(),
        v => v.RuleFor(x => x.LastName).NotEmpty(),
        v => v.RuleFor(x => x.Email).NotNull(),
        v => v.RuleFor(x => x.Password).NotEmpty()
    };
}
