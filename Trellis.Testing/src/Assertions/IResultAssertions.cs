namespace Trellis.Testing;

using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

/// <summary>
/// FluentAssertions extension for <see cref="IResult"/> (the non-generic Trellis result
/// interface). Lets tests call <c>.Should().BeSuccess()</c> / <c>.Should().BeFailure()</c>
/// against members typed as <c>IResult</c> — most commonly
/// <see cref="Trellis.Authorization.IAuthorizeResource{TResource}.Authorize"/> and
/// <see cref="Trellis.Authorization.IAuthorizeResourceVia{TOwner}.Authorize"/>, which both
/// return <see cref="IResult"/> by contract.
/// </summary>
/// <remarks>
/// The typed <see cref="ResultAssertions{TValue}"/> entry point continues to apply for
/// concrete <see cref="Result{TValue}"/> values via the existing
/// <see cref="ResultAssertionsExtensions.Should{TValue}(Result{TValue})"/> extension —
/// <c>Result&lt;T&gt;</c> is a struct, more specific than the <c>IResult</c> interface,
/// so overload resolution prefers the typed extension at the call site.
/// </remarks>
public static class IResultAssertionsExtensions
{
    /// <summary>
    /// Returns an assertions object for fluent assertions on the non-generic
    /// <see cref="IResult"/> interface.
    /// </summary>
    public static IResultAssertions Should(this IResult result) => new(result);
}

/// <summary>
/// Contains assertion methods for the non-generic <see cref="IResult"/> interface. Mirrors
/// the failure-side surface of <see cref="ResultAssertions{TValue}"/> (no success-value
/// assertions exist because <see cref="IResult"/> carries no typed value).
/// </summary>
public class IResultAssertions : ReferenceTypeAssertions<IResult, IResultAssertions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IResultAssertions"/> class.
    /// </summary>
    public IResultAssertions(IResult result) : base(result)
    {
    }

    /// <inheritdoc/>
    protected override string Identifier => "result";

    /// <summary>Asserts that the result is a success.</summary>
    public AndConstraint<IResultAssertions> BeSuccess(
        string because = "",
        params object[] becauseArgs)
    {
        Subject.TryGetError(out var error);
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsSuccess)
            .FailWith("Expected {context:result} to be success{reason}, but it failed with error: {0}",
                error);

        return new AndConstraint<IResultAssertions>(this);
    }

    /// <summary>Asserts that the result is a failure.</summary>
    public AndWhichConstraint<IResultAssertions, Error> BeFailure(
        string because = "",
        params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsFailure)
            .FailWith("Expected {context:result} to be failure{reason}, but it succeeded.");

        Subject.TryGetError(out var error);
        return new AndWhichConstraint<IResultAssertions, Error>(this, error!);
    }

    /// <summary>Asserts that the result is a failure with a specific error type.</summary>
    /// <typeparam name="TError">The expected error type.</typeparam>
    /// <remarks>
    /// Wrong-type behavior mirrors <see cref="ResultAssertions{TValue}.BeFailureOfType{TError}"/>:
    /// without an active <see cref="AssertionScope"/> the assertion throws immediately;
    /// with an active scope it records the failure and returns <c>default(TError)!</c> so
    /// the scope's eventual <c>Dispose</c> surfaces the recorded assertion rather than a
    /// chained <see cref="System.NullReferenceException"/>.
    /// </remarks>
    public AndWhichConstraint<IResultAssertions, TError> BeFailureOfType<TError>(
        string because = "",
        params object[] becauseArgs)
        where TError : Error
    {
        BeFailure(because, becauseArgs);

        Subject.TryGetError(out var error);
        var matches = error is TError;
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(matches)
            .FailWith("Expected {context:result} error to be of type {0}{reason}, but found {1}",
                FormatErrorTypeName(typeof(TError)),
                error is null ? null : FormatErrorTypeName(error.GetType()));

        return new AndWhichConstraint<IResultAssertions, TError>(
            this,
            matches ? (TError)error! : default!);
    }

    /// <summary>Asserts that the failure has a specific error code.</summary>
    public AndConstraint<IResultAssertions> HaveErrorCode(
        string expectedCode,
        string because = "",
        params object[] becauseArgs)
    {
        BeFailure(because, becauseArgs);

        Subject.TryGetError(out var error);
        error!.Code.Should().Be(expectedCode, because, becauseArgs);

        return new AndConstraint<IResultAssertions>(this);
    }

    /// <summary>Asserts that the failure has a specific error detail.</summary>
    public AndConstraint<IResultAssertions> HaveErrorDetail(
        string expectedDetail,
        string because = "",
        params object[] becauseArgs)
    {
        BeFailure(because, becauseArgs);

        Subject.TryGetError(out var error);
        error!.Detail!.Should().Be(expectedDetail, because, becauseArgs);

        return new AndConstraint<IResultAssertions>(this);
    }

    /// <summary>Asserts that the failure error detail contains the specified substring.</summary>
    public AndConstraint<IResultAssertions> HaveErrorDetailContaining(
        string substring,
        string because = "",
        params object[] becauseArgs)
    {
        BeFailure(because, becauseArgs);

        Subject.TryGetError(out var error);
        error!.Detail!.Should().Contain(substring, because, becauseArgs);

        return new AndConstraint<IResultAssertions>(this);
    }

    private static string FormatErrorTypeName(System.Type t)
    {
        var declaring = t.DeclaringType;
        return declaring is null ? t.Name : $"{declaring.Name}.{t.Name}";
    }
}
