namespace FunctionalDdd;

public sealed class UnauthorizedError : Error
{
    public UnauthorizedError(string detail, string code, string? instance = null) : base(detail, code, instance)
    {
    }
}
