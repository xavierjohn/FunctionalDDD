﻿namespace FunctionalDDD;

public sealed class ForbiddenError : Error
{
    public ForbiddenError(string message, string code) : base(message, code)
    {
    }
}
