namespace RailwayOrientedProgramming.Tests.Results;

public abstract class TestBase
{
    public Error Error1 { get; } = Error.Unexpected("Error Message");
    public Error Error2 { get; } = Error.Unexpected("Error Message2");

    protected class T
    {
        public static readonly T Value1 = new();

        public static readonly T Value2 = new();
    }

    protected class K
    {
        public static readonly K Value1 = new();

        public static readonly K Value2 = new();
    }
}
