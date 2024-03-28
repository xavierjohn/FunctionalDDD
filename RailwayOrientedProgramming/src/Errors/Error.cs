namespace FunctionalDdd;

using System.Collections.Generic;
using System.Diagnostics;
using static FunctionalDdd.ValidationError;

[DebuggerDisplay("{Message}")]
#pragma warning disable CA1716 // Identifiers should not match keywords
public class Error : IEquatable<Error>
#pragma warning restore CA1716 // Identifiers should not match keywords
{
    public string Code { get; }

    public string Message { get; }

    public string? Instance { get; }

    public Error(string message, string code)
    {
        Message = message;
        Code = code;
    }
    public Error(string message, string code, string? instance)
    {
        Message = message;
        Code = code;
        Instance = instance;
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

    public static BadRequestError BadRequest(string message, string? instance = null) =>
        new(message, "bad.request.error", instance);

    public static ModelError ValidationError(string message, string fieldName = "") => new(message, fieldName);

    public static ConflictError Conflict(string message, string? instance = null) =>
        new(message, "conflict.error", instance);

    public static NotFoundError NotFound(string message, string? instance = null) =>
        new(message, "not.found.error", instance);

    public static UnauthorizedError Unauthorized(string message, string? instance = null) =>
        new(message, "unauthorized.error", instance);

    public static ForbiddenError Forbidden(string message, string? instance = null) =>
        new(message, "forbidden.error", instance);

    public static UnexpectedError Unexpected(string message, string? instance = null) =>
        new(message, "unexpected.error", instance);


    public static BadRequestError BadRequest(string message, string code, string? instance) =>
        new(message, code, instance);

    public static ConflictError Conflict(string message, string code, string? instance) =>
        new(message, code, instance);

    public static NotFoundError NotFound(string message, string code, string? instance) =>
        new(message, code, instance);

    public static UnauthorizedError Unauthorized(string message, string code, string? instance) =>
        new(message, code, instance);

    public static ForbiddenError Forbidden(string message, string code, string? instance) =>
        new(message, code, instance);

    public static UnexpectedError Unexpected(string message, string code, string? instance) =>
        new(message, code, instance);
}

