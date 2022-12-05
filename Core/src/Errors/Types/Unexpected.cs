namespace FunctionalDDD.Core;

public sealed class Unexpected : Error
{
    public Unexpected(string code, string message) : base(code, message)
    {
    }
}
