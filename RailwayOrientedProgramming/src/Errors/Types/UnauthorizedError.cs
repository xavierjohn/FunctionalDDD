namespace FunctionalDDD;

public sealed class UnauthorizedError : Error
{
    public UnauthorizedError(string description, string code) : base(description, code)
    {
    }
}
