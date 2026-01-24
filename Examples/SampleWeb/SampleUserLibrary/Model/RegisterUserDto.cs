namespace SampleUserLibrary;

using FunctionalDdd;

/// <summary>
/// User registration DTO with automatic validation via value objects.
/// When used with [ApiController], ASP.NET Core automatically validates
/// all value objects during model binding and returns 400 Bad Request
/// if validation fails.
/// </summary>
public record RegisterUserDto
{
    /// <summary>
    /// User's first name (automatically validated - cannot be null or empty).
    /// </summary>
    public FirstName FirstName { get; init; } = null!;

    /// <summary>
    /// User's last name (automatically validated - cannot be null or empty).
    /// </summary>
    public LastName LastName { get; init; } = null!;

    /// <summary>
    /// User's email address (automatically validated - must be valid email format).
    /// </summary>
    public EmailAddress Email { get; init; } = null!;

    /// <summary>
    /// User's phone number (automatically validated - must be E.164 format).
    /// </summary>
    public PhoneNumber Phone { get; init; } = null!;

    /// <summary>
    /// User's age (automatically validated - must be between 0 and 150).
    /// </summary>
    public Age Age { get; init; } = null!;

    /// <summary>
    /// User's country (automatically validated - must be ISO 3166-1 alpha-2 code).
    /// </summary>
    public CountryCode Country { get; init; } = null!;

    /// <summary>
    /// User's website URL (optional, automatically validated - must be valid HTTP/HTTPS URL).
    /// </summary>
    public Url? Website { get; init; }

    /// <summary>
    /// User's password (plain string - not a value object as it shouldn't be stored in domain).
    /// </summary>
    public string Password { get; init; } = null!;
}
