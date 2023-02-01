namespace FunctionalDDD;

public sealed class UnexpectedError : Error
{
    public UnexpectedError(string message, string code) : base(message, code)
    {
    }
}
