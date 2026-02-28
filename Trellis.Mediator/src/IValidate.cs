namespace Trellis.Mediator;

/// <summary>
/// Marker interface for commands/queries that can self-validate before reaching the handler.
/// Implement on command/query records to enable the <see cref="ValidationBehavior{TMessage, TResponse}"/>.
/// </summary>
public interface IValidate
{
    /// <summary>
    /// Validates this message and returns a result indicating success or validation failure.
    /// Returning a failure short-circuits the pipeline — the handler is never called.
    /// </summary>
    IResult Validate();
}
