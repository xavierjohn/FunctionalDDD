namespace FunctionalDDD;

using System.Collections.Generic;
using System.Diagnostics;

[DebuggerDisplay("{Message}")]
public class Error : ValueObject
{
    public string Code { get; }
    public string Message { get; }

    internal Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Code;
        yield return Message;
    }

    public static Error Conflict(string code, string message) =>
        new Conflict(code, message);

    public static Error NotFound(string code, string message) =>
        new NotFound(code, message);

    public static Error Validation(string code, string message) =>
       new Validation(code, message);

    public static Error Unauthorized(string code, string message) =>
        new Unauthorized(code, message);
    
    public static Error Unexpected(string code, string message) =>
    new Unexpected(code, message);

    public static Error Conflict(string message) => Conflict("conflict.error", message);

    public static Error NotFound(string message) => NotFound("notfound.error", message);
    
    public static Error Validation(string message) => Validation("validation.error", message);
    
    public static Error Unauthorized(string message) => Unauthorized("unauthorized.error", message);
    
    public static Error Unexpected(string message) => Unexpected("unexpected.error", message);
}

