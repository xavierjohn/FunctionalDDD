namespace FunctionalDdd;

public sealed class ConflictError : Error
{
    public ConflictError(string detail, string code, string? instance = null) : base(detail, code, instance)
    {
    }
}
