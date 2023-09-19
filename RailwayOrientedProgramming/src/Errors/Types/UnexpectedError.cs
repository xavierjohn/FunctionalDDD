namespace FunctionalDDD.Results.Errors;

public sealed class UnexpectedError : Error
{
    public UnexpectedError(string message, string code, string? target = null) : base(message, code, target)
    {
    }
}
