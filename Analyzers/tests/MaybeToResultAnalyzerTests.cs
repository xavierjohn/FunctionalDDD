namespace FunctionalDdd.Analyzers.Tests;

using Microsoft.CodeAnalysis.Testing;
using Xunit;

public class MaybeToResultAnalyzerTests
{
    [Fact]
    public async Task ToResult_WithoutError_ReportsDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    Maybe<string> maybe = "test";
                    var result = maybe.ToResult();
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<MaybeToResultAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.MaybeToResultWithoutError)
                .WithLocation(12, 28)
                .WithArguments("string"));

        await test.RunAsync();
    }

    [Fact]
    public async Task ToResult_WithError_NoDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    Maybe<string> maybe = "test";
                    var result = maybe.ToResult(Error.NotFound("Not found"));
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<MaybeToResultAnalyzer>(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task ToResult_ChainedCall_WithoutError_ReportsDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public Maybe<User> GetUser() => new User();
                
                public void TestMethod()
                {
                    var result = GetUser().ToResult();
                }
            }

            public class User { }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<MaybeToResultAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.MaybeToResultWithoutError)
                .WithLocation(13, 32)
                .WithArguments("User"));

        await test.RunAsync();
    }

    [Fact]
    public async Task ToResult_WithErrorFactory_NoDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    Maybe<int> maybe = 42;
                    var result = maybe.ToResult(Error.Validation("Invalid value"));
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<MaybeToResultAnalyzer>(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task NonMaybeToResult_NoDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var custom = new CustomType();
                    custom.ToResult();
                }
            }

            public class CustomType
            {
                public void ToResult() { }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<MaybeToResultAnalyzer>(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task MultipleToResultCalls_SomeWithoutError_ReportsDiagnostics()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    Maybe<int> maybe1 = 1;
                    Maybe<string> maybe2 = "test";
                    Maybe<bool> maybe3 = true;
                    
                    var result1 = maybe1.ToResult();
                    var result2 = maybe2.ToResult(Error.NotFound("Missing"));
                    var result3 = maybe3.ToResult();
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<MaybeToResultAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.MaybeToResultWithoutError)
                .WithLocation(15, 30)
                .WithArguments("int"),
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.MaybeToResultWithoutError)
                .WithLocation(17, 30)
                .WithArguments("bool"));

        await test.RunAsync();
    }

    [Fact]
    public async Task ToResult_ComplexType_WithoutError_ReportsDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    Maybe<User> maybeUser = new User();
                    var userResult = maybeUser.ToResult();
                }
            }

            public class User
            {
                public string Name { get; set; }
                public int Age { get; set; }
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<MaybeToResultAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.MaybeToResultWithoutError)
                .WithLocation(12, 36)
                .WithArguments("User"));

        await test.RunAsync();
    }

    [Fact]
    public async Task ToResult_InLinqExpression_WithoutError_ReportsDiagnostic()
    {
        const string source = """
            using System.Linq;

            public class TestClass
            {
                public void TestMethod()
                {
                    Maybe<int>[] maybes = [1, 2, 3];
                    var results = maybes.Select(m => m.ToResult());
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<MaybeToResultAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.MaybeToResultWithoutError)
                .WithLocation(14, 44)
                .WithArguments("int"));

        await test.RunAsync();
    }
}
