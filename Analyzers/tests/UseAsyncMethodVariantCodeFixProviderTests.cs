namespace FunctionalDdd.Analyzers.Tests;

using Xunit;

/// <summary>
/// Tests for UseAsyncMethodVariantCodeFixProvider (FDDD014).
/// Verifies that sync methods are replaced with async variants when lambda is async.
/// </summary>
public class UseAsyncMethodVariantCodeFixProviderTests
{
    [Fact]
    public async Task Map_WithAsyncLambda_ReplacedWithMapAsync()
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

        const string fixedSource = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var result = Result.Success(1).MapAsync(async x => await ProcessAsync(x));
                }

                private Task<int> ProcessAsync(int x) => Task.FromResult(x * 2);
            }
            """;

        var test = CodeFixTestHelper.CreateCodeFixTest<AsyncLambdaWithSyncMethodAnalyzer, UseAsyncMethodVariantCodeFixProvider>(
            source,
            fixedSource,
            CodeFixTestHelper.Diagnostic(DiagnosticDescriptors.UseAsyncMethodVariant)
                .WithArguments("MapAsync", "Map")
                .WithLocation(11, 44));

        await test.RunAsync();
    }
}