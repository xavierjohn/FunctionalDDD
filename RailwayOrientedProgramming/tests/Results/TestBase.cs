namespace RailwayOrientedProgramming.Tests.Results;

using FunctionalDDD;

public abstract class TestBase
{
    public Err Error1 { get; } = Err.Unexpected("Error Message");
    public Err Error2 { get; } = Err.Unexpected("Error Message2");

    protected class T
    {
        public static readonly T Value = new T();

        public static readonly T Value2 = new T();
    }

    protected class K
    {
        public static readonly K Value = new K();

        public static readonly K Value2 = new K();
    }
}
