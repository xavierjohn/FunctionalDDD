namespace RailwayOrientedProgramming.Tests.Results;

using FunctionalDDD.Results.Errors;

public abstract class TestBase
{
    public Error Error1 { get; } = Error.Unexpected("Error Message");
    public Error Error2 { get; } = Error.Unexpected("Error Message2");

    protected class T
    {
        public static readonly T Value = new();

        public static readonly T Value2 = new();
    }

    protected class K
    {
        public static readonly K Value = new();

        public static readonly K Value2 = new();
    }
}
