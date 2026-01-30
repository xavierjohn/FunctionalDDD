namespace FunctionalDdd.Analyzers.Tests;

using Microsoft.CodeAnalysis.Testing;
using Xunit;

public class ErrorBaseClassAnalyzerTests
{
    [Fact]
    public async Task DirectErrorInstantiation_ReportsDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var error = new Error("Something went wrong");
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<ErrorBaseClassAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.UseSpecificErrorType)
                .WithLocation(11, 25));

        await test.RunAsync();
    }

    [Fact]
    public async Task ValidationError_NoDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var error = new ValidationError("Invalid input");
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<ErrorBaseClassAnalyzer>(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task NotFoundError_NoDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var error = new NotFoundError("Not found");
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<ErrorBaseClassAnalyzer>(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task ErrorFactoryMethods_NoDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var validation = Error.Validation("Invalid");
                    var notFound = Error.NotFound("Missing");
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<ErrorBaseClassAnalyzer>(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task CustomErrorType_NoDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var error = new CustomError("Custom error");
                }
            }

            public class CustomError : Error
            {
                public CustomError(string message) : base(message) { }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<ErrorBaseClassAnalyzer>(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task MultipleDirectInstantiations_ReportsMultipleDiagnostics()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var error1 = new Error("Error 1");
                    var error2 = new Error("Error 2");
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<ErrorBaseClassAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.UseSpecificErrorType)
                .WithLocation(11, 26),
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.UseSpecificErrorType)
                .WithLocation(12, 26));

        await test.RunAsync();
    }
}
