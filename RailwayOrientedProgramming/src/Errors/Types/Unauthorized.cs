namespace FunctionalDDD;

public sealed class Unauthorized : Error
{
    public Unauthorized(string description, string code) : base(description, code)
    {
    }
}
