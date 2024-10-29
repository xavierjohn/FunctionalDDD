namespace SampleUserLibrary;

public record RegisterUserRequest(
    string firstName,
    string lastName,
    string email,
    string password
);
