namespace FunctionalDDD;

public sealed class ConflictError : Error
{
    public ConflictError(string description, string code) : base(description, code)
    {
    }

}
