namespace SampleUserLibrary;
using FluentValidation;
using FunctionalDdd;

public class User : Aggregate<UserId>
{
    public FirstName FirstName { get; }
    public LastName LastName { get; }
    public EmailAddress Email { get; }
    public PhoneNumber Phone { get; }
    public Age Age { get; }
    public CountryCode Country { get; }
    public Url? Website { get; }
    public string Password { get; }

    public static Result<User> TryCreate(
        FirstName firstName, 
        LastName lastName, 
        EmailAddress email, 
        PhoneNumber phone,
        Age age,
        CountryCode country,
        string password,
        Url? website = null)
    {
        var user = new User(firstName, lastName, email, phone, age, country, password, website);
        var validator = new UserValidator();
        return validator.ValidateToResult(user);
    }

    private User(
        FirstName firstName, 
        LastName lastName, 
        EmailAddress email, 
        PhoneNumber phone,
        Age age,
        CountryCode country,
        string password,
        Url? website)
        : base(UserId.NewUnique())
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Age = age;
        Country = country;
        Website = website;
        Password = password;
    }

    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(user => user.FirstName).NotNull();
            RuleFor(user => user.LastName).NotNull();
            RuleFor(user => user.Email).NotNull();
            RuleFor(user => user.Phone).NotNull();
            RuleFor(user => user.Age).NotNull();
            RuleFor(user => user.Country).NotNull();
            
            // Business rule: Age must be 18 or older for registration
            RuleFor(user => user.Age)
                .Must(age => age.Value >= 18)
                .WithMessage("User must be at least 18 years old to register.");
            
            RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Password must not be empty.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
        }
    }

}
