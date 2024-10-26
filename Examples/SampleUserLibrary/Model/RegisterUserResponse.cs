namespace SampleUserLibrary;

public record RegisterUserResponse(
    Guid id,
    string firstName,
    string lastName,
    string email,
    string password
);
