namespace Trellis.Analyzers.Tests;

using Xunit;

/// <summary>
/// Tests for HasIndexMaybePropertyAnalyzer (TRLS021).
/// Verifies that HasIndex lambda expressions referencing Maybe&lt;T&gt; properties produce a warning.
/// </summary>
public class HasIndexMaybePropertyAnalyzerTests
{
    /// <summary>
    /// Stub source for EF Core builder types used in TRLS021 analyzer tests.
    /// </summary>
    private const string EfCoreBuilderStubSource = """
        namespace Microsoft.EntityFrameworkCore.Metadata.Builders
        {
            using System;
            using System.Linq.Expressions;

            public class EntityTypeBuilder<TEntity> where TEntity : class
            {
                public virtual IndexBuilder HasIndex(Expression<Func<TEntity, object>> indexExpression) => new IndexBuilder();
                public virtual IndexBuilder HasIndex(params string[] propertyNames) => new IndexBuilder();
            }

            public class IndexBuilder
            {
            }
        }
        """;

    #region HasIndex with Maybe<T> property produces warning

    [Fact]
    public async Task HasIndex_AnonymousType_WithMaybeProperty_ProducesWarning()
    {
        const string source = """
            using Microsoft.EntityFrameworkCore.Metadata.Builders;

            public class Order
            {
                public int Id { get; set; }
                public string Status { get; set; } = "";
                public Maybe<DateTime> SubmittedAt { get; set; }
            }

            public class TestConfig
            {
                public void Configure(EntityTypeBuilder<Order> builder)
                {
                    builder.HasIndex(e => new { e.Status, e.SubmittedAt });
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<HasIndexMaybePropertyAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.HasIndexMaybeProperty)
                .WithLocation(20, 49)
                .WithArguments("SubmittedAt", "_submittedAt"));
        test.TestState.Sources.Add(("EfCoreBuilderStubs.cs", EfCoreBuilderStubSource));

        await test.RunAsync();
    }

    [Fact]
    public async Task HasIndex_SingleProperty_WithMaybeProperty_ProducesWarning()
    {
        const string source = """
            using Microsoft.EntityFrameworkCore.Metadata.Builders;

            public class Order
            {
                public int Id { get; set; }
                public Maybe<DateTime> SubmittedAt { get; set; }
            }

            public class TestConfig
            {
                public void Configure(EntityTypeBuilder<Order> builder)
                {
                    builder.HasIndex(e => e.SubmittedAt);
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<HasIndexMaybePropertyAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.HasIndexMaybeProperty)
                .WithLocation(19, 33)
                .WithArguments("SubmittedAt", "_submittedAt"));
        test.TestState.Sources.Add(("EfCoreBuilderStubs.cs", EfCoreBuilderStubSource));

        await test.RunAsync();
    }

    #endregion

    #region HasIndex with multiple Maybe<T> properties produces multiple warnings

    [Fact]
    public async Task HasIndex_MultipleMaybeProperties_ProducesMultipleWarnings()
    {
        const string source = """
            using Microsoft.EntityFrameworkCore.Metadata.Builders;

            public class Order
            {
                public int Id { get; set; }
                public Maybe<DateTime> SubmittedAt { get; set; }
                public Maybe<DateTime> CompletedAt { get; set; }
            }

            public class TestConfig
            {
                public void Configure(EntityTypeBuilder<Order> builder)
                {
                    builder.HasIndex(e => new { e.SubmittedAt, e.CompletedAt });
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<HasIndexMaybePropertyAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.HasIndexMaybeProperty)
                .WithLocation(20, 39)
                .WithArguments("SubmittedAt", "_submittedAt"),
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.HasIndexMaybeProperty)
                .WithLocation(20, 54)
                .WithArguments("CompletedAt", "_completedAt"));
        test.TestState.Sources.Add(("EfCoreBuilderStubs.cs", EfCoreBuilderStubSource));

        await test.RunAsync();
    }

    #endregion

    #region HasIndex without Maybe<T> does not produce warning

    [Fact]
    public async Task HasIndex_NoMaybeProperties_NoDiagnostic()
    {
        const string source = """
            using Microsoft.EntityFrameworkCore.Metadata.Builders;

            public class Order
            {
                public int Id { get; set; }
                public string Status { get; set; } = "";
                public string Name { get; set; } = "";
            }

            public class TestConfig
            {
                public void Configure(EntityTypeBuilder<Order> builder)
                {
                    builder.HasIndex(e => new { e.Status, e.Name });
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<HasIndexMaybePropertyAnalyzer>(source);
        test.TestState.Sources.Add(("EfCoreBuilderStubs.cs", EfCoreBuilderStubSource));

        await test.RunAsync();
    }

    #endregion

    #region String-based HasIndex does not produce warning

    [Fact]
    public async Task HasIndex_StringBased_NoDiagnostic()
    {
        const string source = """
            using Microsoft.EntityFrameworkCore.Metadata.Builders;

            public class Order
            {
                public int Id { get; set; }
                public string Status { get; set; } = "";
                public Maybe<DateTime> SubmittedAt { get; set; }
            }

            public class TestConfig
            {
                public void Configure(EntityTypeBuilder<Order> builder)
                {
                    builder.HasIndex("Status", "_submittedAt");
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<HasIndexMaybePropertyAnalyzer>(source);
        test.TestState.Sources.Add(("EfCoreBuilderStubs.cs", EfCoreBuilderStubSource));

        await test.RunAsync();
    }

    #endregion

    #region Non-EntityTypeBuilder HasIndex does not produce warning

    [Fact]
    public async Task HasIndex_NotEntityTypeBuilder_NoDiagnostic()
    {
        const string source = """
            public class MyBuilder
            {
                public void HasIndex(System.Func<object, object> expr) { }
            }

            public class TestConfig
            {
                public void Configure(MyBuilder builder)
                {
                    builder.HasIndex(e => e);
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<HasIndexMaybePropertyAnalyzer>(source);

        await test.RunAsync();
    }

    #endregion

    #region No EF Core reference does not activate analyzer

    [Fact]
    public async Task NoEfCoreReference_NoDiagnostic()
    {
        const string source = """
            public class TestClass
            {
                public void TestMethod() { }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<HasIndexMaybePropertyAnalyzer>(source);

        await test.RunAsync();
    }

    #endregion
}
