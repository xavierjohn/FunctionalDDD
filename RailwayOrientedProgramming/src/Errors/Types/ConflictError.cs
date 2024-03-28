namespace FunctionalDdd;

public sealed class ConflictError : Error
{
    public ConflictError(string message, string code, string? instance = null) : base(message, code, instance)
    {
    }
}
