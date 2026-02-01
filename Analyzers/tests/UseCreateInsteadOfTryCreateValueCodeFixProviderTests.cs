namespace FunctionalDdd.Analyzers.Tests;

using Xunit;

/// <summary>
/// Tests for UseCreateInsteadOfTryCreateValueCodeFixProvider (FDDD007).
/// Verifies that TryCreate().Value is correctly replaced with Create().
/// </summary>
public class UseCreateInsteadOfTryCreateValueCodeFixProviderTests
{
    [Fact]
    public async Task TryCreateValue_InVariableDeclaration_ReplacedWithCreate()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var email = EmailAddress.TryCreate("test@example.com").Value;
                }
            }
            """;

        const string fixedSource = """
            public class TestClass
            {
                public void TestMethod()
                {
                    var email = EmailAddress.Create("test@example.com");
                }
            }
            """;

        var test = CodeFixTestHelper.CreateCodeFixTest<TryCreateValueAccessAnalyzer, UseCreateInsteadOfTryCreateValueCodeFixProvider>(
            source,
            fixedSource,
            CodeFixTestHelper.Diagnostic(DiagnosticDescriptors.UseCreateInsteadOfTryCreateValue)
                .WithArguments("EmailAddress")
                .WithLocation(11, 21));

        await test.RunAsync();
    }

    [Fact]
    public async Task TryCreateValue_InAssignment_ReplacedWithCreate()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    EmailAddress email;
                    email = EmailAddress.TryCreate("test@example.com").Value;
                }
            }
            """;

        const string fixedSource = """
            public class TestClass
            {
                public void TestMethod()
                {
                    EmailAddress email;
                    email = EmailAddress.Create("test@example.com");
                }
            }
            """;

        var test = CodeFixTestHelper.CreateCodeFixTest<TryCreateValueAccessAnalyzer, UseCreateInsteadOfTryCreateValueCodeFixProvider>(
            source,
            fixedSource,
            CodeFixTestHelper.Diagnostic(DiagnosticDescriptors.UseCreateInsteadOfTryCreateValue)
                .WithArguments("EmailAddress")
                .WithLocation(12, 17));

        await test.RunAsync();
    }

    [Fact]
    public async Task TryCreateValue_WithComments_PreservesTrivia()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod()
                {
                    // Create email address
                    var email = EmailAddress.TryCreate("test@example.com").Value; // Should use Create
                }
            }
            """;

        const string fixedSource = """
            public class TestClass
            {
                public void TestMethod()
                {
                    // Create email address
                    var email = EmailAddress.Create("test@example.com"); // Should use Create
                }
            }
            """;

        var test = CodeFixTestHelper.CreateCodeFixTest<TryCreateValueAccessAnalyzer, UseCreateInsteadOfTryCreateValueCodeFixProvider>(
            source,
            fixedSource,
            CodeFixTestHelper.Diagnostic(DiagnosticDescriptors.UseCreateInsteadOfTryCreateValue)
                .WithArguments("EmailAddress")
                .WithLocation(12, 21));

        await test.RunAsync();
    }

    [Fact]
    public async Task TryCreateValue_InReturnStatement_ReplacedWithCreate()
    {
        const string source = """
            public class TestClass
            {
                public EmailAddress TestMethod()
                {
                    return EmailAddress.TryCreate("test@example.com").Value;
                }
            }
            """;

        const string fixedSource = """
            public class TestClass
            {
                public EmailAddress TestMethod()
                {
                    return EmailAddress.Create("test@example.com");
                }
            }
            """;

        var test = CodeFixTestHelper.CreateCodeFixTest<TryCreateValueAccessAnalyzer, UseCreateInsteadOfTryCreateValueCodeFixProvider>(
            source,
            fixedSource,
            CodeFixTestHelper.Diagnostic(DiagnosticDescriptors.UseCreateInsteadOfTryCreateValue)
                .WithArguments("EmailAddress")
                .WithLocation(11, 16));

        await test.RunAsync();
    }
}
