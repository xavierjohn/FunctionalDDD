namespace Trellis.Authorization;

/// <summary>
/// Marker interface for commands/queries that require resource-based authorization.
/// The <see cref="Authorize"/> method receives the current actor and returns a Result
/// indicating whether the operation is permitted.
/// </summary>
public interface IAuthorizeResource
{
    /// <summary>
    /// Determines whether the given actor is authorized to execute this command/query.
    /// Return a success Result to proceed, or a failure Result (typically <see cref="Error.Forbidden(string, string?)"/>)
    /// to short-circuit the pipeline.
    /// </summary>
    /// <param name="actor">The current authenticated actor.</param>
    IResult Authorize(Actor actor);
}