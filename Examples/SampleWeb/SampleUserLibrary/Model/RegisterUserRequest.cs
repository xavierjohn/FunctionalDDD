namespace SampleUserLibrary;

/// <summary>
/// User registration request with raw string values.
/// Demonstrates manual validation using Result.Combine() vs auto-validation with DTOs.
/// </summary>
public record RegisterUserRequest(
    string firstName,
    string lastName,
    string email,
    string phone,
    int age,
    string country,
    string password
);
