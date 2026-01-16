namespace SampleUserLibrary;

using FunctionalDdd;

/// <summary>
/// A request DTO that uses value objects directly for automatic validation.
/// When used with AddValueObjectValidation(), validation happens during JSON deserialization.
/// </summary>
/// <remarks>
/// <para>
/// Compare this with <see cref="RegisterUserRequest"/> which uses primitive strings.
/// With this DTO, the controller action receives pre-validated value objects.
/// </para>
/// <para>
/// Note: The property names (fname, lname, mail) are intentionally different from 
/// the type names (FirstName, LastName, EmailAddress) to demonstrate that validation 
/// errors use the actual property names, not the type names.
/// </para>
/// </remarks>
public record CreateUserWithValidationRequest(
    FirstName fname,
    LastName lname,
    EmailAddress mail,
    string Password  // Password kept as string to demonstrate FluentValidation for complex rules
);
