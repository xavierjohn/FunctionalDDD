namespace FunctionalDDD;

public sealed class Transient : Err
{
    public Transient(string description, string code) : base(description, code)
    {
    }
}
