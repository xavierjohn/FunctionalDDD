namespace FunctionalDDD;

public sealed class Forbidden : Error
{
    public Forbidden(string code, string message) : base(code, message)
    {
    }
}
