namespace FunctionalDdd;

public sealed class UnauthorizedError : Error
{
    public UnauthorizedError(string message, string code, string? instance = null) : base(message, code, instance)
    {
    }
}
