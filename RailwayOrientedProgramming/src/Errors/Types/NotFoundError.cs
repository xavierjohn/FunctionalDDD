namespace FunctionalDDD;

public sealed class NotFoundError : Error
{
    public NotFoundError(string description, string code) : base(description, code)
    {
    }
}
