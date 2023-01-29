namespace FunctionalDDD;

public sealed class Unexpected : Err
{
    public Unexpected(string description, string code) : base(description, code)
    {
    }
}
