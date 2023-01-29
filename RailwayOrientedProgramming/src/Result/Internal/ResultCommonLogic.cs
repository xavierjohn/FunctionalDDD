﻿namespace FunctionalDDD;

internal static class ResultCommonLogic
{
    internal static bool ErrorStateGuard<TErr>(bool isFailure, Errs<TErr>? error)
    {
        if (isFailure)
        {
            if (error == null || error.HasErrors == false)
                throw new ArgumentNullException(nameof(error), Result.Messages.ErrorObjectIsNotProvidedForFailure);
        }
        else
        {
            if (error != null && error.HasErrors)
                throw new ArgumentException(Result.Messages.ErrorObjectIsProvidedForSuccess, nameof(error));
        }

        return isFailure;
    }

    internal static Errs<TErr> GetErrorWithSuccessGuard<TErr>(bool isFailure, Errs<TErr>? error)
    {
        if (!isFailure)
            throw new ResultSuccessException();

        if (error is null || error.HasErrors == false)
            throw new InvalidOperationException("Failed state without error object");

        return error;
    }
}
