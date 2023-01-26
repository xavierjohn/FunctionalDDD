namespace FunctionalDDD;

public sealed class NotFound : Error
{
    public NotFound(string description, string code) : base(description, code)
    {
    }
}
