﻿namespace FunctionalDdd;
public interface IResult
{
    bool IsSuccess { get; }
    bool IsFailure { get; }

#pragma warning disable CA1716 // Identifiers should not match keywords
    Error Error { get; }
#pragma warning restore CA1716 // Identifiers should not match keywords
}
