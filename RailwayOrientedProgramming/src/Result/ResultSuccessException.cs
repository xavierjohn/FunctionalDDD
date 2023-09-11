namespace FunctionalDDD.RailwayOrientedProgramming;

public class ResultSuccessException : Exception
{
    internal ResultSuccessException()
        : base(Result.Messages.ErrorIsInaccessibleForSuccess)
    {
    }
}
