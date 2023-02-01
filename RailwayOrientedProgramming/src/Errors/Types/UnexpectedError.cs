namespace FunctionalDDD;

public sealed class UnexpectedError : Error
{
    public UnexpectedError(string description, string code) : base(description, code)
    {
    }
}
