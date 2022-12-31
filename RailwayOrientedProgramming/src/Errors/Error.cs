namespace FunctionalDDD;

using System.Diagnostics;

[DebuggerDisplay("{Message}")]
#pragma warning disable CA1716 // Identifiers should not match keywords
public class Error : IEquatable<Error>
#pragma warning restore CA1716 // Identifiers should not match keywords
{

    public string Code { get; }
    public string Message { get; }

    internal Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public bool Equals(Error? other)
    {
        if (other == null) return false;
        return Code == other.Code;
    }
    public override bool Equals(object? obj)
    {
        if (obj is Error error)
            return Equals(error);
        else
            return false;
    }

    public override int GetHashCode() => Code.GetHashCode();

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

