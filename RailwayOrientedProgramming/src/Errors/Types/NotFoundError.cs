namespace FunctionalDdd;

public sealed class NotFoundError : Error
{
    public NotFoundError(string message, string code, string? instance = null) : base(message, code, instance)
    {
    }
}
