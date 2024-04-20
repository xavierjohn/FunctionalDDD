namespace FunctionalDdd;

public sealed class NotFoundError : Error
{
    public NotFoundError(string detail, string code, string? instance = null) : base(detail, code, instance)
    {
    }
}
