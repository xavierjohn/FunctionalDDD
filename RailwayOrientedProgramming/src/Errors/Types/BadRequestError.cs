namespace FunctionalDdd;

public sealed class BadRequestError : Error
{
    public BadRequestError(string detail, string code, string? instance = null) : base(detail, code, instance)
    {
    }
}
