namespace FunctionalDDD.RailwayOrientedProgramming;

using FunctionalDDD.RailwayOrientedProgramming.Errors;

/// <summary>
/// Attempted to access the Value for a failed result. A failed result has no Value.
/// </summary>
public class ResultFailureException : Exception
{
    public Error Error { get; }

    internal ResultFailureException(Error error)
        : base(Result.Messages.ValueIsInaccessibleForFailure())
    {
        Error = error;
    }
}
