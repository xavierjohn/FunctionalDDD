namespace FunctionalDDD;

public sealed class ForbiddenError : Error
{
    public ForbiddenError(string message, string code, string? target = null) : base(message, code, target)
    {
    }
}
