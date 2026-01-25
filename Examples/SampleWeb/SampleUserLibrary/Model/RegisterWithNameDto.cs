namespace SampleUserLibrary;

using FunctionalDdd;
using FunctionalDdd.PrimitiveValueObjects;

/// <summary>
/// Registration DTO using the same Name value object for both first and last name.
/// This tests that validation errors correctly use the property name (FirstName, LastName)
/// rather than the type name (Name).
/// </summary>
public record RegisterWithNameDto
{
    /// <summary>
    /// User's first name - uses the generic Name value object.
    /// Validation errors should show "FirstName" as the field name.
    /// </summary>
    public Name FirstName { get; init; } = null!;

    /// <summary>
    /// User's last name - uses the same Name value object type.
    /// Validation errors should show "LastName" as the field name.
    /// </summary>
    public Name LastName { get; init; } = null!;

    /// <summary>
    /// User's email address.
    /// </summary>
    public EmailAddress Email { get; init; } = null!;
}