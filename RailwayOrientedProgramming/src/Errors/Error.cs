namespace FunctionalDDD.RailwayOrientedProgramming.Errors;

using System.Collections.Generic;
using System.Diagnostics;
using static FunctionalDDD.RailwayOrientedProgramming.Errors.ValidationError;

[DebuggerDisplay("{Message}")]
#pragma warning disable CA1716 // Identifiers should not match keywords
public class Error : IEquatable<Error>
#pragma warning restore CA1716 // Identifiers should not match keywords
{
    public string Code { get; }

    public string Message { get; }

    public string? Target { get; }

    public Error(string message, string code)
    {
        Message = message;
        Code = code;
    }
    public Error(string message, string code, string? target)
    {
        Message = message;
        Code = code;
        Target = target;
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

    public static ValidationError Validation(string message, string fieldName = "", string code = "validation.error") =>
        new(message, fieldName, code);

    public static ValidationError Validation(List<ModelError> modelErrors, string code = "validation.error") =>
        new(modelErrors, code);

    public static BadRequestError BadRequest(string message, string code = "bad.request.error", string? target = null) =>
        new(message, code, target);

    public static ModelError ValidationError(string message, string fieldName = "") => new(message, fieldName);

    public static ConflictError Conflict(string message, string code = "conflict.error", string? target = null) =>
        new(message, code, target);

    public static NotFoundError NotFound(string message, string code = "not.found.error", string? target = null) =>
        new(message, code, target);

    public static UnauthorizedError Unauthorized(string message, string code = "unauthorized.error", string? target = null) =>
        new(message, code, target);

    public static ForbiddenError Forbidden(string message, string code = "forbidden.error", string? target = null) =>
        new(message, code, target);

    public static UnexpectedError Unexpected(string message, string code = "unexpected.error", string? target = null) =>
        new(message, code, target);
}

