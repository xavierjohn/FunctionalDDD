namespace FunctionalDDD.RailwayOrientedProgramming;
public class ResultFailureException : Exception
{
    public ErrorList Errors { get; }

    internal ResultFailureException(ErrorList errors)
        : base(Result.Messages.ValueIsInaccessibleForFailure())
    {
        Errors = errors;
    }
}
