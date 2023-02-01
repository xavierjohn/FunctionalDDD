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

    internal Error(string message, string code)
    {
        Message = message;
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

    public static Error Validation(string message, string fieldName = "", string code = "validation.error") =>
        new ValidationError(message, fieldName, code);

    public static Error Validation(List<ModelError> modelErrors, string code = "validation.error") =>
        new ValidationError(modelErrors, code);

    public static ModelError ValidationError(string message, string fieldName = "") => new ModelError(message, fieldName);

    public static Error Conflict(string message, string code = "conflict.error") =>
        new ConflictError(message, code);

    public static Error NotFound(string message, string code = "notfound.error") =>
        new NotFoundError(message, code);

    public static Error Unauthorized(string message, string code = "unauthorized.error") =>
        new UnauthorizedError(message, code);

    public static Error Forbidden(string message, string code = "forbidden.error") =>
        new ForbiddenError(message, code);

    public static Error Unexpected(string message, string code = "unexpected.error") =>
        new UnexpectedError(message, code);
}

