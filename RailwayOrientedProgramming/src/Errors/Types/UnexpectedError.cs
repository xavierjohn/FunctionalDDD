namespace FunctionalDdd;

public sealed class UnexpectedError : Error
{
    public UnexpectedError(string message, string code, string? instance = null) : base(message, code, instance)
    {
    }
}
