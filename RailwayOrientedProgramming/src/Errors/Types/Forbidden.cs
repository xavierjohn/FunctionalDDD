namespace FunctionalDDD;

public sealed class Forbidden : Error
{
    public Forbidden(string description, string code) : base(description, code)
    {
    }
}
