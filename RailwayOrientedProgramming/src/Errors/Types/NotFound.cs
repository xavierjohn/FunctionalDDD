namespace FunctionalDDD;

public sealed class NotFound : Err
{
    public NotFound(string description, string code) : base(description, code)
    {
    }
}
