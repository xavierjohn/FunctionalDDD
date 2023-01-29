namespace FunctionalDDD;

public sealed class Unauthorized : Err
{
    public Unauthorized(string description, string code) : base(description, code)
    {
    }
}
