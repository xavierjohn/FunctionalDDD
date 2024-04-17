namespace FunctionalDdd;

public sealed class ForbiddenError : Error
{
    public ForbiddenError(string detail, string code, string? instance = null) : base(detail, code, instance)
    {
    }
}
