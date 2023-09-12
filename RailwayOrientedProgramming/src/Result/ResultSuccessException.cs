namespace FunctionalDDD.RailwayOrientedProgramming;

/// <summary>
/// Attempted to access the Error property for a successful result. A successful result has no Error.
/// </summary>
public class ResultSuccessException : Exception
{
    internal ResultSuccessException()
        : base(Result.Messages.ErrorIsInaccessibleForSuccess)
    {
    }
}
