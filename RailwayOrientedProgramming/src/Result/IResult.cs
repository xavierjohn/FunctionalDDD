namespace FunctionalDdd;
public interface IResult
{
    bool IsFailure { get; }
    bool IsSuccess { get; }

#pragma warning disable CA1716 // Identifiers should not match keywords
    Error Error { get; }
#pragma warning restore CA1716 // Identifiers should not match keywords
}
