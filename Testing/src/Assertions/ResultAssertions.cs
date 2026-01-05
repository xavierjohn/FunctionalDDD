namespace FunctionalDdd.Testing;

using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

/// <summary>
/// Extension methods to enable FluentAssertions on Result types.
/// </summary>
public static class ResultAssertionsExtensions
{
    /// <summary>
    /// Returns an assertions object for fluent assertions on Result.
    /// </summary>
    public static ResultAssertions<TValue> Should<TValue>(this Result<TValue> result) => new ResultAssertions<TValue>(result);
}

/// <summary>
/// Contains assertion methods for Result types.
/// </summary>
public class ResultAssertions<TValue> : ReferenceTypeAssertions<Result<TValue>, ResultAssertions<TValue>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResultAssertions{TValue}"/> class.
    /// </summary>
    public ResultAssertions(Result<TValue> result)
        : base(result)
    {
    }

    /// <inheritdoc/>
    protected override string Identifier => "result";

    /// <summary>
    /// Asserts that the result is a success.
    /// </summary>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndWhichConstraint<ResultAssertions<TValue>, TValue> BeSuccess(
        string because = "",
        params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsSuccess)
            .FailWith("Expected {context:result} to be success{reason}, but it failed with error: {0}",
                Subject.IsFailure ? Subject.Error : null);

        return new AndWhichConstraint<ResultAssertions<TValue>, TValue>(this, Subject.Value);
    }

    /// <summary>
    /// Asserts that the result is a failure.
    /// </summary>
    /// <param name="because">
    /// A formatted phrase explaining why the assertion is needed.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndWhichConstraint<ResultAssertions<TValue>, Error> BeFailure(
        string because = "",
        params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsFailure)
            .FailWith("Expected {context:result} to be failure{reason}, but it succeeded with value: {0}",
                Subject.IsSuccess ? Subject.Value : default);

        return new AndWhichConstraint<ResultAssertions<TValue>, Error>(this, Subject.Error);
    }

    /// <summary>
    /// Asserts that the result is a failure with a specific error type.
    /// </summary>
    /// <typeparam name="TError">The expected error type.</typeparam>
    /// <param name="because">
    /// A formatted phrase explaining why the assertion is needed.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndWhichConstraint<ResultAssertions<TValue>, TError> BeFailureOfType<TError>(
        string because = "",
        params object[] becauseArgs)
        where TError : Error
    {
        BeFailure(because, becauseArgs);

        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.Error is TError)
            .FailWith("Expected {context:result} error to be of type {0}{reason}, but found {1}",
                typeof(TError).Name,
                Subject.Error.GetType().Name);

        return new AndWhichConstraint<ResultAssertions<TValue>, TError>(
            this,
            (TError)Subject.Error);
    }

    /// <summary>
    /// Asserts that the success value equals the expected value.
    /// </summary>
    /// <param name="expectedValue">The expected value.</param>
    /// <param name="because">
    /// A formatted phrase explaining why the assertion is needed.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<ResultAssertions<TValue>> HaveValue(
        TValue expectedValue,
        string because = "",
        params object[] becauseArgs)
    {
        BeSuccess(because, becauseArgs);

        Subject.Value.Should().Be(expectedValue, because, becauseArgs);

        return new AndConstraint<ResultAssertions<TValue>>(this);
    }

    /// <summary>
    /// Asserts that the success value satisfies a predicate.
    /// </summary>
    /// <param name="predicate">The predicate the value should satisfy.</param>
    /// <param name="because">
    /// A formatted phrase explaining why the assertion is needed.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndConstraint<ResultAssertions<TValue>> HaveValueMatching(
        Func<TValue, bool> predicate,
        string because = "",
        params object[] becauseArgs)
    {
        BeSuccess(because, becauseArgs);

        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(predicate(Subject.Value))
            .FailWith("Expected {context:result} value to match predicate{reason}, but it did not. Value: {0}",
                Subject.Value);

        return new AndConstraint<ResultAssertions<TValue>>(this);
    }
}
