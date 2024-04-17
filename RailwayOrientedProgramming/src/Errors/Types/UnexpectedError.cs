namespace FunctionalDdd;

public sealed class UnexpectedError : Error
{
    public UnexpectedError(string detail, string code, string? instance = null) : base(detail, code, instance)
    {
    }
}
