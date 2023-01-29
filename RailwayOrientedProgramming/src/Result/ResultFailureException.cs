namespace FunctionalDDD;
public class ResultFailureException<TErr> : Exception
{
    public Errs<TErr> Errs { get; }

    internal ResultFailureException(Errs<TErr> errors)
        : base(Result.Messages.ValueIsInaccessibleForFailure())
    {
        Errs = errors;
    }
}
