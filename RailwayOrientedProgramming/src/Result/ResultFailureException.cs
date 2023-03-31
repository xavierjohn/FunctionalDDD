namespace FunctionalDDD;
public class ResultFailureException : Exception
{
    public Error Error { get; }

    internal ResultFailureException(Error error)
        : base(Result.Messages.ValueIsInaccessibleForFailure())
    {
        Error = error;
    }
}
