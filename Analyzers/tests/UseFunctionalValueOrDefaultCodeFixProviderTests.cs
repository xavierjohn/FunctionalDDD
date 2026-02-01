namespace FunctionalDdd.Analyzers.Tests;

using Xunit;

/// <summary>
/// Tests for UseFunctionalValueOrDefaultCodeFixProvider (FDDD014).
/// Verifies that ternary operators are correctly replaced with GetValueOrDefault() or Match().
/// </summary>
public class UseFunctionalValueOrDefaultCodeFixProviderTests
{
    [Fact]
    public async Task TernaryWithNull_ReplacedWithGetValueOrDefault_CodeAction0()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod(Result<string> result)
                {
                    var value = result.IsSuccess ? result.Value : null;
                }
            }
            """;

        const string fixedSource = """
            public class TestClass
            {
                public void TestMethod(Result<string> result)
                {
                    var value = result.GetValueOrDefault();
                }
            }
            """;

        var test = CodeFixTestHelper.CreateCodeFixTest<TernaryValueOrDefaultAnalyzer, UseFunctionalValueOrDefaultCodeFixProvider>(
            source,
            fixedSource,
            codeActionIndex: 0,
            CodeFixTestHelper.Diagnostic(DiagnosticDescriptors.UseFunctionalValueOrDefault).WithLocation(11, 21));

        await test.RunAsync();
    }

    [Fact]
    public async Task TernaryWithDefault_ReplacedWithGetValueOrDefault_CodeAction0()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod(Result<int> result)
                {
                    var value = result.IsSuccess ? result.Value : default;
                }
            }
            """;

        const string fixedSource = """
            public class TestClass
            {
                public void TestMethod(Result<int> result)
                {
                    var value = result.GetValueOrDefault();
                }
            }
            """;

        var test = CodeFixTestHelper.CreateCodeFixTest<TernaryValueOrDefaultAnalyzer, UseFunctionalValueOrDefaultCodeFixProvider>(
            source,
            fixedSource,
            codeActionIndex: 0,
            CodeFixTestHelper.Diagnostic(DiagnosticDescriptors.UseFunctionalValueOrDefault).WithLocation(11, 21));

        await test.RunAsync();
    }

    [Fact]
    public async Task TernaryWithNull_ReplacedWithMatch_CodeAction1()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod(Result<string> result)
                {
                    var value = result.IsSuccess ? result.Value : null;
                }
            }
            """;

        const string fixedSource = """
            public class TestClass
            {
                public void TestMethod(Result<string> result)
                {
                    var value = result.Match(value => value, error => null);
                }
            }
            """;

        var test = CodeFixTestHelper.CreateCodeFixTest<TernaryValueOrDefaultAnalyzer, UseFunctionalValueOrDefaultCodeFixProvider>(
            source,
            fixedSource,
            codeActionIndex: 1,
            CodeFixTestHelper.Diagnostic(DiagnosticDescriptors.UseFunctionalValueOrDefault).WithLocation(11, 21));

        await test.RunAsync();
    }

    [Fact]
    public async Task TernaryWithComments_PreservesTrivia()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod(Result<string> result)
                {
                    // Extract value
                    var value = result.IsSuccess ? result.Value : null; // With fallback
                }
            }
            """;

        const string fixedSource = """
            public class TestClass
            {
                public void TestMethod(Result<string> result)
                {
                    // Extract value
                    var value = result.GetValueOrDefault(); // With fallback
                }
            }
            """;

        var test = CodeFixTestHelper.CreateCodeFixTest<TernaryValueOrDefaultAnalyzer, UseFunctionalValueOrDefaultCodeFixProvider>(
            source,
            fixedSource,
            codeActionIndex: 0,
            CodeFixTestHelper.Diagnostic(DiagnosticDescriptors.UseFunctionalValueOrDefault).WithLocation(12, 21));

        await test.RunAsync();
    }
}
