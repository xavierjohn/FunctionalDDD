namespace FunctionalDdd;

public sealed class ForbiddenError : Error
{
    public ForbiddenError(string message, string code, string? instance = null) : base(message, code, instance)
    {
    }
}
