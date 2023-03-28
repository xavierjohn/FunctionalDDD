namespace FunctionalDDD;

public sealed class ConflictError : Error
{
    public ConflictError(string message, string code, string? target = null) : base(message, code, target)
    {
    }
}
