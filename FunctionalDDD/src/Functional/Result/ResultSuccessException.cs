namespace FunctionalDDD;

public class ResultSuccessException : Exception
{
    internal ResultSuccessException()
        : base(Result.Messages.ErrorIsInaccessibleForSuccess)
    {
    }
}
