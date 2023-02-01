namespace FunctionalDDD;

using System.Collections.Generic;
using System.Diagnostics;
using static FunctionalDDD.ValidationError;

[DebuggerDisplay("{Message}")]
#pragma warning disable CA1716 // Identifiers should not match keywords
public class Error : IEquatable<Error>
#pragma warning restore CA1716 // Identifiers should not match keywords
{
    public string Message { get; }
    public string Code { get; }

    internal Error(string description, string code)
    {
        Message = description;
        Code = code;
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

    public static Error Validation(string description, string fieldName = "", string code = "validation.error") =>
        new ValidationError(description, fieldName, code);

    public static Error Validation(List<ModelError> modelErrors, string code = "validation.error") =>
        new ValidationError(modelErrors, code);

    public static ModelError ValidationError(string message, string fieldName = "") => new ModelError(message, fieldName);

    public static Error Conflict(string description, string code = "conflict.error") =>
        new ConflictError(description, code);

    public static Error NotFound(string description, string code = "notfound.error") =>
        new NotFoundError(description, code);

    public static Error Unauthorized(string description, string code = "unauthorized.error") =>
        new UnauthorizedError(description, code);

    public static Error Forbidden(string description, string code = "forbidden.error") =>
        new ForbiddenError(description, code);

    public static Error Unexpected(string description, string code = "unexpected.error") =>
        new UnexpectedError(description, code);
}

