namespace FunctionalDdd;

public sealed class BadRequestError : Error
{
    public BadRequestError(string message, string code, string? instance = null) : base(message, code, instance)
    {
    }
}
