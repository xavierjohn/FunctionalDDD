namespace Trellis.Analyzers.Tests;

using Xunit;

/// <summary>
/// Tests for UseSaveChangesResultCodeFixProvider (TRLS020).
/// Verifies that SaveChangesAsync/SaveChanges is correctly replaced with SaveChangesResultUnitAsync.
/// </summary>
public class UseSaveChangesResultCodeFixProviderTests
{
    private const string EfCoreStubSource = """
        namespace Trellis
        {
            public record struct Unit;
        }

        namespace Microsoft.EntityFrameworkCore
        {
            using System.Threading;
            using System.Threading.Tasks;

            public class DbContext
            {
                public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
                    => Task.FromResult(0);

                public virtual int SaveChanges() => 0;
            }
        }

        namespace Trellis.EntityFrameworkCore
        {
            using Microsoft.EntityFrameworkCore;
            using System.Threading;
            using System.Threading.Tasks;

            public static class DbContextExtensions
            {
                public static Task<Trellis.Result<int>> SaveChangesResultAsync(
                    this DbContext context,
                    CancellationToken cancellationToken = default)
                    => Task.FromResult(Trellis.Result.Success(0));

                public static Task<Trellis.Result<Trellis.Unit>> SaveChangesResultUnitAsync(
                    this DbContext context,
                    CancellationToken cancellationToken = default)
                    => Task.FromResult(Trellis.Result.Success(default(Trellis.Unit)));
            }
        }
        """;

    [Fact]
    public async Task SaveChangesAsync_ReplacedWith_SaveChangesResultUnitAsync()
    {
        const string source = """
            using Microsoft.EntityFrameworkCore;
            using Trellis.EntityFrameworkCore;
            using System.Threading;
            using System.Threading.Tasks;

            public class TestService
            {
                private readonly DbContext _dbContext;
                public TestService(DbContext dbContext) => _dbContext = dbContext;

                public async Task DoWork(CancellationToken ct)
                {
                    await _dbContext.SaveChangesAsync(ct);
                }
            }
            """;

        const string fixedSource = """
            using Microsoft.EntityFrameworkCore;
            using Trellis.EntityFrameworkCore;
            using System.Threading;
            using System.Threading.Tasks;

            public class TestService
            {
                private readonly DbContext _dbContext;
                public TestService(DbContext dbContext) => _dbContext = dbContext;

                public async Task DoWork(CancellationToken ct)
                {
                    await _dbContext.SaveChangesResultUnitAsync(ct);
                }
            }
            """;

        var test = CodeFixTestHelper.CreateCodeFixTest<UseSaveChangesResultAnalyzer, UseSaveChangesResultCodeFixProvider>(
            source,
            fixedSource,
            CodeFixTestHelper.Diagnostic(DiagnosticDescriptors.UseSaveChangesResult)
                .WithLocation(19, 30)
                .WithArguments("SaveChangesAsync"));
        test.TestState.Sources.Add(("EfCoreStubs.cs", EfCoreStubSource));
        test.FixedState.Sources.Add(("EfCoreStubs.cs", EfCoreStubSource));

        await test.RunAsync();
    }

    [Fact]
    public async Task SaveChanges_Sync_ReplacedWith_SaveChangesResultUnitAsync()
    {
        const string source = """
            using Microsoft.EntityFrameworkCore;
            using Trellis.EntityFrameworkCore;

            public class TestService
            {
                private readonly DbContext _dbContext;
                public TestService(DbContext dbContext) => _dbContext = dbContext;

                public void DoWork()
                {
                    _dbContext.SaveChanges();
                }
            }
            """;

        const string fixedSource = """
            using Microsoft.EntityFrameworkCore;
            using Trellis.EntityFrameworkCore;

            public class TestService
            {
                private readonly DbContext _dbContext;
                public TestService(DbContext dbContext) => _dbContext = dbContext;

                public async Task DoWork()
                {
                    await _dbContext.SaveChangesResultUnitAsync();
                }
            }
            """;

        var test = CodeFixTestHelper.CreateCodeFixTest<UseSaveChangesResultAnalyzer, UseSaveChangesResultCodeFixProvider>(
            source,
            fixedSource,
            CodeFixTestHelper.Diagnostic(DiagnosticDescriptors.UseSaveChangesResult)
                .WithLocation(17, 24)
                .WithArguments("SaveChanges"));
        test.TestState.Sources.Add(("EfCoreStubs.cs", EfCoreStubSource));
        test.FixedState.Sources.Add(("EfCoreStubs.cs", EfCoreStubSource));

        await test.RunAsync();
    }

    [Fact]
    public async Task SaveChanges_InAssignment_NoCodeFixOffered()
    {
        const string source = """
            using Microsoft.EntityFrameworkCore;
            using Trellis.EntityFrameworkCore;

            public class TestService
            {
                private readonly DbContext _dbContext;
                public TestService(DbContext dbContext) => _dbContext = dbContext;

                public void DoWork()
                {
                    var count = _dbContext.SaveChanges();
                }
            }
            """;

        // The code fix should not be offered, so fixedSource == source (no transformation)
        var test = CodeFixTestHelper.CreateCodeFixTest<UseSaveChangesResultAnalyzer, UseSaveChangesResultCodeFixProvider>(
            source,
            source,
            CodeFixTestHelper.Diagnostic(DiagnosticDescriptors.UseSaveChangesResult)
                .WithLocation(17, 36)
                .WithArguments("SaveChanges"));
        test.TestState.Sources.Add(("EfCoreStubs.cs", EfCoreStubSource));
        test.FixedState.Sources.Add(("EfCoreStubs.cs", EfCoreStubSource));

        await test.RunAsync();
    }
}
