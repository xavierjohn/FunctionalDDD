namespace FunctionalDDD;
public class ResultFailureException : Exception
{
    public Errs Errors { get; }

    internal ResultFailureException(Errs errors)
        : base(Result.Messages.ValueIsInaccessibleForFailure())
    {
        Errors = errors;
    }
}
