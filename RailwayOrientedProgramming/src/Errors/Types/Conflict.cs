namespace FunctionalDDD;

public sealed class Conflict : Err
{
    public Conflict(string description, string code) : base(description, code)
    {
    }

}
