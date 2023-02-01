namespace FunctionalDDD;

public sealed class ConflictError : Error
{
    public ConflictError(string message, string code) : base(message, code)
    {
    }

}
