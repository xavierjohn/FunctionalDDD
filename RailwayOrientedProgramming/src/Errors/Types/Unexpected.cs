namespace FunctionalDDD;

public sealed class Unexpected : Error
{
    public Unexpected(string description, string code) : base(description, code)
    {
    }
}
