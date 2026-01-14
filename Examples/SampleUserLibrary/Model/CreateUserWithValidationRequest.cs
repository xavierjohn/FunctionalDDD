namespace SampleUserLibrary;

using FunctionalDdd;

/// <summary>
/// A request DTO that uses value objects directly for automatic validation.
/// When used with AddValueObjectValidation(), validation happens during JSON deserialization.
/// </summary>
/// <remarks>
/// Compare this with <see cref="RegisterUserRequest"/> which uses primitive strings.
/// With this DTO, the controller action receives pre-validated value objects.
/// </remarks>
public record CreateUserWithValidationRequest(
    FirstName FirstName,
    LastName LastName,
    EmailAddress Email,
    string Password  // Password kept as string to demonstrate FluentValidation for complex rules
);
