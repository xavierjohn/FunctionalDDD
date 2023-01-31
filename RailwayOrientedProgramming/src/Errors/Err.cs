namespace FunctionalDDD;

using System.Diagnostics;

[DebuggerDisplay("{Description}")]
public class Err : IEquatable<Err>
{
    public string Description { get; }
    public string Code { get; }

    internal Err(string description, string code)
    {
        Description = description;
        Code = code;
    }

    public bool Equals(Err? other)
    {
        if (other == null) return false;
        return Code == other.Code;
    }
    public override bool Equals(object? obj)
    {
        if (obj is Err error)
            return Equals(error);
        else
            return false;
    }

    public override int GetHashCode() => Code.GetHashCode();

    public static Err Validation(string description, string fieldName = "", string code = "validation.error") =>
        new Validation(description, fieldName, code);

    public static Err Conflict(string description, string code = "conflict.error") =>
        new Conflict(description, code);

    public static Err NotFound(string description, string code = "notfound.error") =>
        new NotFound(description, code);

    public static Err Unauthorized(string description, string code = "unauthorized.error") =>
        new Unauthorized(description, code);

    public static Err Forbidden(string description, string code = "forbidden.error") =>
        new Forbidden(description, code);

    public static Err Unexpected(string description, string code = "unexpected.error") =>
        new Unexpected(description, code);
}

