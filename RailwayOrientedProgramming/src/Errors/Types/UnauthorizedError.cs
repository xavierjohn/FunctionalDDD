namespace FunctionalDDD;

public sealed class UnauthorizedError : Error
{
    public UnauthorizedError(string message, string code) : base(message, code)
    {
    }
}
