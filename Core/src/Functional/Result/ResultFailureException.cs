namespace FunctionalDDD;
public class ResultFailureException : Exception
{
    public ErrorList Error { get; }

    internal ResultFailureException(ErrorList error)
        : base(Result.Messages.ValueIsInaccessibleForFailure())
    {
        Error = error;
    }
}
