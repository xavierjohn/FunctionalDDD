using System.Runtime.CompilerServices;

namespace FunctionalDDD;

internal static class TaskExtensions
{
    public static Task<T> AsCompletedTask<T>(this T obj) => Task.FromResult(obj);

    public static ConfiguredTaskAwaitable DefaultAwait(this Task task) =>
        task.ConfigureAwait(Result.Configuration.DefaultConfigureAwait);

    public static ConfiguredTaskAwaitable<T> DefaultAwait<T>(this Task<T> task) =>
        task.ConfigureAwait(Result.Configuration.DefaultConfigureAwait);
}
