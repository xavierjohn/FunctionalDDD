namespace Trellis.Mediator.Tests.Helpers;

using global::Mediator;
using Trellis.Authorization;

/// <summary>
/// Command that implements <see cref="IValidate"/> for testing validation behavior.
/// </summary>
internal sealed record TestCommand(string Name)
    : ICommand<Result<string>>, IValidate
{
    public IResult Validate() =>
        string.IsNullOrWhiteSpace(Name)
            ? Result.Failure<string>(Error.Validation("Name is required.", "Name"))
            : Result.Success(Name);
}

/// <summary>
/// Command that does NOT implement <see cref="IValidate"/> (should skip validation behavior).
/// </summary>
internal sealed record TestCommandNoValidation(string Name)
    : ICommand<Result<string>>;

/// <summary>
/// Command with static permission-based authorization.
/// </summary>
internal sealed record AdminCommand(string Data)
    : ICommand<Result<string>>, IAuthorize, IValidate
{
    public IReadOnlyList<string> RequiredPermissions => ["Admin.Write"];

    public IResult Validate() =>
        string.IsNullOrWhiteSpace(Data)
            ? Result.Failure<string>(Error.Validation("Data is required.", "Data"))
            : Result.Success(Data);
}

/// <summary>
/// Command with resource-based authorization.
/// </summary>
internal sealed record OwnerOnlyCommand(string ResourceOwnerId, string Data)
    : ICommand<Result<string>>, IAuthorizeResource
{
    public IResult Authorize(Actor actor) =>
        actor.Id == ResourceOwnerId
            ? Result.Success()
            : Result.Failure(Error.Forbidden("Not the resource owner"));
}

/// <summary>
/// Command with both static and resource-based authorization.
/// </summary>
internal sealed record DualAuthCommand(string ResourceOwnerId)
    : ICommand<Result<string>>, IAuthorize, IAuthorizeResource
{
    public IReadOnlyList<string> RequiredPermissions => ["Orders.Write"];

    public IResult Authorize(Actor actor) =>
        actor.Id == ResourceOwnerId || actor.HasPermission("Orders.WriteAny")
            ? Result.Success()
            : Result.Failure(Error.Forbidden("Cannot modify another user's resource"));
}

/// <summary>
/// Command with empty required permissions list (should always pass authorization).
/// </summary>
internal sealed record NoPermissionsCommand(string Data)
    : ICommand<Result<string>>, IAuthorize
{
    public IReadOnlyList<string> RequiredPermissions => [];
}

/// <summary>
/// Query that implements <see cref="IValidate"/> for testing validation with queries.
/// </summary>
internal sealed record TestQuery(int Id)
    : IQuery<Result<string>>, IValidate
{
    public IResult Validate() =>
        Id <= 0
            ? Result.Failure<string>(Error.Validation("Id must be positive.", "Id"))
            : Result.Success(Id.ToString(System.Globalization.CultureInfo.InvariantCulture));
}