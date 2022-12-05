﻿namespace FunctionalDDD.Core.Tests;
using FunctionalDDD.Core;

internal static class ValueTaskExtensions
{
    public static ValueTask<T> AsValueTask<T>(this T obj) => obj.AsCompletedValueTask();
    public static ValueTask AsValueTask(this Exception exception) => ValueTask.FromException(exception);
    public static ValueTask<T> AsValueTask<T>(this Exception exception) => ValueTask.FromException<T>(exception);
}
