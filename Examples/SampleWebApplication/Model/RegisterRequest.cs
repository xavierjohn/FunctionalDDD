namespace SampleWebApplication.Model;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password
);
