namespace FunctionalDDD;
public class ResultFailureException<TErr> : Exception
{
    public TErr Error { get; }

    internal ResultFailureException(TErr error)
        : base(Result.Messages.ValueIsInaccessibleForFailure())
    {
        Error = error;
    }
}
