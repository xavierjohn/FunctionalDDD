namespace FunctionalDDD.Results;
using System.Runtime.CompilerServices;

internal static class TaskExtensions
{
    public static Task<T> AsCompletedTask<T>(this T obj) => Task.FromResult(obj);
}
