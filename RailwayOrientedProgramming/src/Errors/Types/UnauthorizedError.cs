﻿namespace FunctionalDDD;

public sealed class UnauthorizedError : Error
{
    public UnauthorizedError(string message, string code, string? target = null) : base(message, code, target)
    {
    }
}
