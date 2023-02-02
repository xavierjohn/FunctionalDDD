namespace SampleWebApplication.Model;

public record RegisterRequest(
    string firstName,
    string lastName,
    string email,
    string password
);
