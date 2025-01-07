namespace FunctionalDdd;

using System.Collections.Generic;
using System.Diagnostics;
using static FunctionalDdd.ValidationError;

[DebuggerDisplay("{Detail}")]
#pragma warning disable CA1716 // Identifiers should not match keywords
public class Error : IEquatable<Error>
#pragma warning restore CA1716 // Identifiers should not match keywords
{
    public string Code { get; }

    public string Detail { get; }

    public string? Instance { get; }

    public Error(string detail, string code)
    {
        Detail = detail;
        Code = code;
    }
    public Error(string detail, string code, string? instance)
    {
        Detail = detail;
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

    public static ValidationError Validation(string fieldDetail, string fieldName = "", string? detail = null, string? instance = null)
        => new(fieldDetail, fieldName, "validation.error", detail, instance);

    public static ValidationError Validation(FieldDetails[] fieldDetails, string detail = "", string? instance = null)
        => new(fieldDetails, "validation.error", detail, instance);

    public static ValidationError Validation(FieldDetails[] fieldDetails, string detail, string? instance, string code)
        => new(fieldDetails, code, detail, instance);

    public static BadRequestError BadRequest(string detail, string? instance = null) =>
        new(detail, "bad.request.error", instance);

    public static ConflictError Conflict(string detail, string? instance = null) =>
        new(detail, "conflict.error", instance);

    public static NotFoundError NotFound(string detail, string? instance = null) =>
        new(detail, "not.found.error", instance);

    public static UnauthorizedError Unauthorized(string detail, string? instance = null) =>
        new(detail, "unauthorized.error", instance);

    public static ForbiddenError Forbidden(string detail, string? instance = null) =>
        new(detail, "forbidden.error", instance);

    public static UnexpectedError Unexpected(string detail, string? instance = null) =>
        new(detail, "unexpected.error", instance);


    public static BadRequestError BadRequest(string detail, string code, string? instance) =>
        new(detail, code, instance);

    public static ConflictError Conflict(string detail, string code, string? instance) =>
        new(detail, code, instance);

    public static NotFoundError NotFound(string detail, string code, string? instance) =>
        new(detail, code, instance);

    public static UnauthorizedError Unauthorized(string detail, string code, string? instance) =>
        new(detail, code, instance);

    public static ForbiddenError Forbidden(string detail, string code, string? instance) =>
        new(detail, code, instance);

    public static UnexpectedError Unexpected(string detail, string code, string? instance) =>
        new(detail, code, instance);
}

