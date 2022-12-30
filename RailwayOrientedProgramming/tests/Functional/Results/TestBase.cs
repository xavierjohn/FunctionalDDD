namespace RailwayOrientedProgramming.Tests.Functional.Results;

using FunctionalDDD.RailwayOrientedProgramming;

public abstract class TestBase
{
    protected readonly Error Error1 = Error.Unexpected("Error Message");

    protected readonly Error Error2 = Error.Unexpected("Error Message2");

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
