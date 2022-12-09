namespace SampleWebApplication.Model;

public record AuthenticationResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Token
);
