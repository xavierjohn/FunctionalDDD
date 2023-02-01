namespace FunctionalDDD;

public sealed class NotFoundError : Error
{
    public NotFoundError(string message, string code) : base(message, code)
    {
    }
}
