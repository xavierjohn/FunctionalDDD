namespace FunctionalDDD;

public sealed class Conflict : Error
{
    public Conflict(string description, string code) : base(description, code)
    {
    }

}
