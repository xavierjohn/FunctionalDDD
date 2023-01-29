namespace FunctionalDDD;

public sealed class Forbidden : Err
{
    public Forbidden(string description, string code) : base(description, code)
    {
    }
}
