namespace SampleMinimalApiNoAot;

using FunctionalDdd;
using SampleUserLibrary;

// =============================================================================
// DTOs
// =============================================================================

/// <summary>
/// DTO with value objects - automatically validated during JSON deserialization.
/// Properties are non-nullable because WithValueObjectValidation() guarantees
/// the endpoint only executes when all validations pass.
/// </summary>
public record CreateUserRequest(
    FirstName FirstName,       // Automatically validated - guaranteed non-null if endpoint executes
    LastName LastName,         // Automatically validated - guaranteed non-null if endpoint executes
    EmailAddress Email,        // Automatically validated - guaranteed non-null if endpoint executes
    string? Password           // Regular string - validated manually in User.TryCreate
);

// Note: NameTestRequest is defined in SampleUserLibrary

/// <summary>
/// DTO with primitive types - requires manual validation.
/// </summary>
public record ManualRegisterRequest(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Password
);
