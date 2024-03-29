﻿namespace FunctionalDdd;

public sealed class NotFoundError : Error
{
    public NotFoundError(string message, string code, string? target = null) : base(message, code, target)
    {
    }
}
