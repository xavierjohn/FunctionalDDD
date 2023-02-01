namespace FunctionalDDD;

public sealed class ForbiddenError : Error
{
    public ForbiddenError(string description, string code) : base(description, code)
    {
    }
}
