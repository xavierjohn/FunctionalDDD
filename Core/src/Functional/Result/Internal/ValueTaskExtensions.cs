﻿namespace FunctionalDDD.Core;

internal static class ValueTaskExtensions
{
    public static ValueTask<T> AsCompletedValueTask<T>(this T obj) => ValueTask.FromResult(obj);
}
