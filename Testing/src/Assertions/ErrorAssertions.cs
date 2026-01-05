namespace FunctionalDdd.Testing;

using FluentAssertions;
using FluentAssertions.Primitives;

/// <summary>
/// Extension methods to enable FluentAssertions on Error types.
/// </summary>
public static class ErrorAssertionsExtensions
{
    /// <summary>
    /// Returns an assertions object for fluent assertions on Error.
    /// </summary>
    public static ErrorAssertions Should(this Error error)
    {
        return new ErrorAssertions(error);
    }
}

/// <summary>
/// Contains assertion methods for Error types.
/// </summary>
public class ErrorAssertions : ReferenceTypeAssertions<Error, ErrorAssertions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorAssertions"/> class.
    /// </summary>
    public ErrorAssertions(Error error)
        : base(error)
    {
    }

    /// <inheritdoc/>
    protected override string Identifier => "error";

    /// <summary>
    /// Asserts that the error has the specified code.
    /// </summary>
    /// <param name="expectedCode">The expected error code.</param>
    /// <param name="because">
    /// A formatted phrase explaining why the assertion is needed.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<ErrorAssertions> HaveCode(
        string expectedCode,
        string because = "",
        params object[] becauseArgs)
    {
        Subject.Code.Should().Be(expectedCode, because, becauseArgs);
        return new AndConstraint<ErrorAssertions>(this);
    }

    /// <summary>
    /// Asserts that the error has the specified detail message.
    /// </summary>
    /// <param name="expectedDetail">The expected error detail.</param>
    /// <param name="because">
    /// A formatted phrase explaining why the assertion is needed.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<ErrorAssertions> HaveDetail(
        string expectedDetail,
        string because = "",
        params object[] becauseArgs)
    {
        Subject.Detail.Should().Be(expectedDetail, because, becauseArgs);
        return new AndConstraint<ErrorAssertions>(this);
    }

    /// <summary>
    /// Asserts that the error detail contains the specified substring.
    /// </summary>
    /// <param name="substring">The substring to search for.</param>
    /// <param name="because">
    /// A formatted phrase explaining why the assertion is needed.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<ErrorAssertions> HaveDetailContaining(
        string substring,
        string because = "",
        params object[] becauseArgs)
    {
        Subject.Detail.Should().Contain(substring, because, becauseArgs);
        return new AndConstraint<ErrorAssertions>(this);
    }
}
