# Fluent Validation extension

This library will help convert Fluent Validation Errors toFunctionalDdd ValidationError.

Example:

```csharp
 public static Result<User, Error> TryCreate(FirstName firstName, LastName lastName, EmailAddress email)
{
  var user = new User(firstName, lastName, email);
  return s_validator.ValidateToResult(user);
}

static readonly InlineValidator<User> s_validator = new()
{
  v => v.RuleFor(x => x.FirstName).NotNull(),
  v => v.RuleFor(x => x.LastName).NotNull(),
  v => v.RuleFor(x => x.Email).NotNull(),
};
```