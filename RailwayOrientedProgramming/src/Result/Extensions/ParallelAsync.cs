namespace FunctionalDDD.RailwayOrientedProgramming;

public static partial class ParallelExtensionsAsync
{
    public static (Task<Result<T1>>, Task<Result<T2>>) ParallelAsync<T1, T2>(this Task<Result<T1>> resultTask1, Task<Result<T2>> resultTask2)
        => (resultTask1, resultTask2);

}
