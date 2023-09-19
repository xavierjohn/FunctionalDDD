namespace FunctionalDDD.Results.Errors;

public sealed class BadRequestError : Error
{
    public BadRequestError(string message, string code, string? target = null) : base(message, code, target)
    {
    }
}
