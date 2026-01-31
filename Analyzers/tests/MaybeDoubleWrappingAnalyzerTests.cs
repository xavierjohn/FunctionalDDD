namespace FunctionalDdd.Analyzers.Tests;

using Microsoft.CodeAnalysis.Testing;
using Xunit;

public class MaybeDoubleWrappingAnalyzerTests
{
    [Fact]
    public async Task VariableDeclaration_DoubleWrapped_ReportsDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    Maybe<Maybe<string>> doubleWrapped;
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<MaybeDoubleWrappingAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.MaybeDoubleWrapping)
                .WithLocation(11, 9)
                .WithArguments("string"));

        await test.RunAsync();
    }

    [Fact]
    public async Task PropertyDeclaration_DoubleWrapped_ReportsDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public Maybe<Maybe<int>> DoubleWrapped { get; set; }
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<MaybeDoubleWrappingAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.MaybeDoubleWrapping)
                .WithLocation(9, 12)
                .WithArguments("int"));

        await test.RunAsync();
    }

    [Fact]
    public async Task MethodReturnType_DoubleWrapped_ReportsDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public Maybe<Maybe<User>> GetUser()
                {
                    return default;
                }
            }

            public class User { }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<MaybeDoubleWrappingAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.MaybeDoubleWrapping)
                .WithLocation(9, 12)
                .WithArguments("User"));

        await test.RunAsync();
    }

    [Fact]
    public async Task MethodParameter_DoubleWrapped_ReportsDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void ProcessMaybe(Maybe<Maybe<string>> maybe)
                {
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<MaybeDoubleWrappingAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.MaybeDoubleWrapping)
                .WithLocation(9, 30)
                .WithArguments("string"));

        await test.RunAsync();
    }

    [Fact]
    public async Task SingleWrappedMaybe_NoDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public Maybe<string> GetValue() => "test";
                
                public void TestMethod()
                {
                    Maybe<int> singleWrapped = 42;
                    var value = GetValue();
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<MaybeDoubleWrappingAnalyzer>(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task NestedGenericType_NotMaybeDoubleWrapping_NoDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public List<Maybe<string>> GetMaybes() => new();
                
                public void TestMethod()
                {
                    var maybes = new List<Maybe<int>>();
                }
            }

            public class List<T> { }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<MaybeDoubleWrappingAnalyzer>(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task ComplexType_DoubleWrapped_ReportsDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public Maybe<Maybe<User>> GetUser() => default;
            }

            public class User
            {
                public string Name { get; set; }
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<MaybeDoubleWrappingAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.MaybeDoubleWrapping)
                .WithLocation(9, 12)
                .WithArguments("User"));

        await test.RunAsync();
    }

    [Fact]
    public async Task MultipleDoubleWrappings_ReportsMultipleDiagnostics()
    {
        const string source = """
            public class TestClass
            {
                public Maybe<Maybe<string>> Property { get; set; }
                
                public Maybe<Maybe<int>> GetValue() => default;
                
                public void TestMethod(Maybe<Maybe<bool>> parameter)
                {
                    Maybe<Maybe<double>> local;
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<MaybeDoubleWrappingAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.MaybeDoubleWrapping)
                .WithLocation(9, 12)
                .WithArguments("string"),
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.MaybeDoubleWrapping)
                .WithLocation(11, 12)
                .WithArguments("int"),
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.MaybeDoubleWrapping)
                .WithLocation(13, 28)
                .WithArguments("bool"),
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.MaybeDoubleWrapping)
                .WithLocation(15, 9)
                .WithArguments("double"));

        await test.RunAsync();
    }
}