namespace FunctionalDdd.Analyzers.Tests;

using Xunit;

/// <summary>
/// Tests for AsyncLambdaWithSyncMethodAnalyzer (FDDD014).
/// Verifies that async lambdas with sync methods are detected.
/// </summary>
public class AsyncLambdaWithSyncMethodAnalyzerTests
{
    [Fact]
    public async Task Map_WithAsyncLambda_ReportsDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var result = Result.Success(1).Map(async x => await ProcessAsync(x));
                }

                private Task<int> ProcessAsync(int x) => Task.FromResult(x * 2);
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<AsyncLambdaWithSyncMethodAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.UseAsyncMethodVariant)
                .WithArguments("MapAsync", "Map")
                .WithLocation(11, 40));

        await test.RunAsync();
    }

    [Fact]
    public async Task Tap_WithAsyncLambda_ReportsDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var result = Result.Success(1).Tap(async x => await LogAsync(x));
                }

                private Task LogAsync(int x) => Task.CompletedTask;
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<AsyncLambdaWithSyncMethodAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.UseAsyncMethodVariant)
                .WithArguments("TapAsync", "Tap")
                .WithLocation(11, 40));

        await test.RunAsync();
    }

    [Fact]
    public async Task MapAsync_WithAsyncLambda_NoDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public async Task TestMethod()
                {
                    var result = await Result.Success(1).MapAsync(async x => await ProcessAsync(x));
                }

                private Task<int> ProcessAsync(int x) => Task.FromResult(x * 2);
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<AsyncLambdaWithSyncMethodAnalyzer>(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task Map_WithSyncLambda_NoDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var result = Result.Success(1).Map(x => x * 2);
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<AsyncLambdaWithSyncMethodAnalyzer>(source);
        await test.RunAsync();
    }
}