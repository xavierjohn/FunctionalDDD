namespace FunctionalDdd.Analyzers.Tests;

using Microsoft.CodeAnalysis.Testing;
using Xunit;

public class TryCreateValueAccessAnalyzerTests
{
    [Fact]
    public async Task TryCreateValue_ReportsDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var money = Money.TryCreate(100m).Value;
                }
            }

            public class Money : IScalarValue<Money, decimal> { decimal IScalarValue<Money, decimal>.Value => 0m; public static Result<Money> TryCreate(decimal value, string? fieldName = null) => default; public static Money Create(decimal value) => default;
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<TryCreateValueAccessAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.UseCreateInsteadOfTryCreateValue)
                .WithLocation(11, 43)
                .WithArguments("Money"));

        await test.RunAsync();
    }

    [Fact]
    public async Task TryCreateValue_InLineExpression_ReportsDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    ProcessMoney(Money.TryCreate(100m).Value);
                }

                private void ProcessMoney(Money money) { }
            }

            public class Money : IScalarValue<Money, decimal> { decimal IScalarValue<Money, decimal>.Value => 0m; public static Result<Money> TryCreate(decimal value, string? fieldName = null) => default; public static Money Create(decimal value) => default;
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<TryCreateValueAccessAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.UseCreateInsteadOfTryCreateValue)
                .WithLocation(11, 44)
                .WithArguments("Money"));

        await test.RunAsync();
    }

    [Fact]
    public async Task Create_NoDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var money = Money.Create(100m);
                }
            }

            public class Money : IScalarValue<Money, decimal> { decimal IScalarValue<Money, decimal>.Value => 0m; public static Result<Money> TryCreate(decimal value, string? fieldName = null) => default; public static Money Create(decimal value) => default;
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<TryCreateValueAccessAnalyzer>(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task TryCreateAssignedToVariable_NoDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var result = Money.TryCreate(100m);
                    if (result.IsSuccess)
                    {
                        var money = result.Value;
                    }
                }
            }

            public class Money : IScalarValue<Money, decimal> { decimal IScalarValue<Money, decimal>.Value => 0m; public static Result<Money> TryCreate(decimal value, string? fieldName = null) => default; public static Money Create(decimal value) => default;
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<TryCreateValueAccessAnalyzer>(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task TryCreateChainedWithBind_NoDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var result = Money.TryCreate(100m)
                        .Bind(m => ProcessMoney(m));
                }

                private Result<string> ProcessMoney(Money money) => "Processed";
            }

            public class Money : IScalarValue<Money, decimal> { decimal IScalarValue<Money, decimal>.Value => 0m; public static Result<Money> TryCreate(decimal value, string? fieldName = null) => default; public static Money Create(decimal value) => default;
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<TryCreateValueAccessAnalyzer>(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task TypeWithoutCreateMethod_NoDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    // Type only has TryCreate, not Create
                    var value = SomeType.TryCreate("test").Value;
                }
            }

            public class SomeType {
                public static Result<SomeType> TryCreate(string value) => default;
                // No Create method
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<TryCreateValueAccessAnalyzer>(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task TypeWithOnlyCreateMethod_NoDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    // Not calling TryCreate at all
                    var value = SomeType.Create("test");
                }
            }

            public class SomeType {
                // No TryCreate method
                public static SomeType Create(string value) => default;
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<TryCreateValueAccessAnalyzer>(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task TryCreateValue_MultipleValueObjects_ReportsDiagnosticForEach()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var money = Money.TryCreate(100m).Value;
                    var email = EmailAddress.TryCreate("test@example.com").Value;
                }
            }

            public class Money : IScalarValue<Money, decimal> { decimal IScalarValue<Money, decimal>.Value => 0m; public static Result<Money> TryCreate(decimal value, string? fieldName = null) => default; public static Money Create(decimal value) => default;
            }

            public class EmailAddress : IScalarValue<EmailAddress, string> { string IScalarValue<EmailAddress, string>.Value => string.Empty; public static Result<EmailAddress> TryCreate(string value, string? fieldName = null) => default; public static EmailAddress Create(string value) => default;
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<TryCreateValueAccessAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.UseCreateInsteadOfTryCreateValue)
                .WithLocation(11, 43)
                .WithArguments("Money"),
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.UseCreateInsteadOfTryCreateValue)
                .WithLocation(12, 64)
                .WithArguments("EmailAddress"));

        await test.RunAsync();
    }

    [Fact]
    public async Task TryCreateValue_InReturnStatement_ReportsDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public Money TestMethod()
                {
                    return Money.TryCreate(100m).Value;
                }
            }

            public class Money : IScalarValue<Money, decimal> { decimal IScalarValue<Money, decimal>.Value => 0m; public static Result<Money> TryCreate(decimal value, string? fieldName = null) => default; public static Money Create(decimal value) => default;
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<TryCreateValueAccessAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.UseCreateInsteadOfTryCreateValue)
                .WithLocation(11, 38)
                .WithArguments("Money"));

        await test.RunAsync();
    }

    [Fact]
    public async Task ResultValueAccessOnOtherResult_NoDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var result = GetResult();
                    if (result.IsSuccess)
                    {
                        var value = result.Value;
                    }
                }

                private Result<string> GetResult() => "test";
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<TryCreateValueAccessAnalyzer>(source);
        await test.RunAsync();
    }
}