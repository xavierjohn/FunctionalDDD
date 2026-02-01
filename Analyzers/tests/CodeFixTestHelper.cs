namespace FunctionalDdd.Analyzers.Tests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Helper for creating code fix tests with FunctionalDDD references.
/// </summary>
public static class CodeFixTestHelper
{
    /// <summary>
    /// Creates a code fix test with the specified source and expected fixed code.
    /// </summary>
    public static CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier> CreateCodeFixTest<TAnalyzer, TCodeFix>(
        string source,
        string fixedSource,
        params DiagnosticResult[] expectedDiagnostics)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
        {
            TestCode = WrapInNamespace(source),
            FixedCode = WrapInNamespace(fixedSource),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80
        };

        AddFunctionalDddStubSource(test.TestState);
        AddFunctionalDddStubSource(test.FixedState);
        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);
        return test;
    }

    /// <summary>
    /// Creates a code fix test with multiple code actions (for testing specific code action index).
    /// </summary>
    public static CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier> CreateCodeFixTest<TAnalyzer, TCodeFix>(
        string source,
        string fixedSource,
        int codeActionIndex,
        params DiagnosticResult[] expectedDiagnostics)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        var test = CreateCodeFixTest<TAnalyzer, TCodeFix>(source, fixedSource, expectedDiagnostics);
        test.CodeActionIndex = codeActionIndex;
        return test;
    }

    /// <summary>
    /// Creates a diagnostic result for the specified descriptor.
    /// </summary>
    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor) =>
        new DiagnosticResult(descriptor);

    private static void AddFunctionalDddStubSource(SolutionState state) =>
        state.Sources.Add(("FunctionalDddStubs.cs", FunctionalDddStubSource));

    private static string WrapInNamespace(string source) =>
        $$"""
            using FunctionalDdd;
            using System;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                {{source}}
            }
            """;

    /// <summary>
    /// Stub source code for FunctionalDdd types used in code fix tests.
    /// </summary>
    private const string FunctionalDddStubSource = """
        namespace FunctionalDdd
        {
            using System;
            using System.Threading.Tasks;

            // IScalarValue interface stub
            public interface IScalarValue<TSelf, TPrimitive>
                where TSelf : IScalarValue<TSelf, TPrimitive>
                where TPrimitive : IComparable
            {
                static abstract Result<TSelf> TryCreate(TPrimitive value, string? fieldName = null);
                TPrimitive Value { get; }
            }

            // Result<T> stub
            public readonly struct Result<T>
            {
                private readonly bool _isSuccess;
                private readonly T _value;
                private readonly Error _error;

                private Result(bool isSuccess, T value, Error error)
                {
                    _isSuccess = isSuccess;
                    _value = value;
                    _error = error;
                }

                public bool IsSuccess => _isSuccess;
                public bool IsFailure => !_isSuccess;
                public T Value => _isSuccess ? _value : throw new InvalidOperationException("Result is in failure state");
                public Error Error => !_isSuccess ? _error : throw new InvalidOperationException("Result is in success state");

                public static implicit operator Result<T>(T value) => new Result<T>(true, value, default);
                public static implicit operator Result<T>(Error error) => new Result<T>(false, default, error);

                public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure) =>
                    _isSuccess ? onSuccess(_value) : onFailure(_error);

                public T GetValueOrDefault() => _isSuccess ? _value : default;
                public T GetValueOrDefault(T defaultValue) => _isSuccess ? _value : defaultValue;
            }

            // Result extensions stub
            public static class ResultExtensions
            {
                public static Result<TResult> Map<T, TResult>(this Result<T> result, Func<T, TResult> func)
                {
                    if (result.IsSuccess)
                        return Result.Success(func(result.Value));
                    return Result.Failure<TResult>(result.Error);
                }

                public static Result<TResult> Bind<T, TResult>(this Result<T> result, Func<T, Result<TResult>> func)
                {
                    if (result.IsSuccess)
                        return func(result.Value);
                    return Result.Failure<TResult>(result.Error);
                }

                public static Task<Result<TResult>> MapAsync<T, TResult>(this Result<T> result, Func<T, Task<TResult>> func)
                {
                    if (result.IsSuccess)
                        return func(result.Value).ContinueWith(t => Result.Success(t.Result));
                    return Task.FromResult(Result.Failure<TResult>(result.Error));
                }

                public static Task<Result<TResult>> BindAsync<T, TResult>(this Result<T> result, Func<T, Task<Result<TResult>>> func)
                {
                    if (result.IsSuccess)
                        return func(result.Value);
                    return Task.FromResult(Result.Failure<TResult>(result.Error));
                }
            }

            // Error stub
            public record Error(string Code, string Detail)
            {
                public static Error Validation(string detail) => new Error("Validation", detail);
                public static Error NotFound(string detail) => new Error("NotFound", detail);
            }

            // Static Result factory class
            public static class Result
            {
                public static Result<T> Success<T>(T value) => value;
                public static Result<T> Failure<T>(Error error) => error;
            }

            // EmailAddress stub (for TryCreate tests)
            public class EmailAddress : IScalarValue<EmailAddress, string>
            {
                private readonly string _value;
                private EmailAddress(string value) { _value = value; }
                public string Value => _value;

                public static Result<EmailAddress> TryCreate(string value, string? fieldName = null)
                {
                    if (string.IsNullOrEmpty(value) || !value.Contains('@'))
                        return Result.Failure<EmailAddress>(Error.Validation("Invalid email"));
                    return Result.Success(new EmailAddress(value));
                }

                public static EmailAddress Create(string value)
                {
                    var result = TryCreate(value);
                    if (result.IsFailure)
                        throw new InvalidOperationException($"Failed to create EmailAddress: {result.Error.Detail}");
                    return result.Value;
                }
            }
        }
        """;
}
