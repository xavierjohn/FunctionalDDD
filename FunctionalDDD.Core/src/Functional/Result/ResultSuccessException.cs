namespace FunctionalDDD.Core;

public class ResultSuccessException : Exception
{
    internal ResultSuccessException()
        : base(Result.Messages.ErrorIsInaccessibleForSuccess)
    {
    }
}
