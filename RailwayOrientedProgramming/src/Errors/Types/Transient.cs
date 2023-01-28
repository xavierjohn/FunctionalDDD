namespace FunctionalDDD;

public sealed class Transient : Error
{
    public Transient(string description, string code) : base(description, code)
    {
    }
}
