namespace Trellis.EntityFrameworkCore.Tests.Helpers;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// SQLite in-memory DbContext for testing Trellis.EntityFrameworkCore integration.
/// </summary>
internal class TestDbContext : DbContext
{
    public DbSet<TestCustomer> Customers => Set<TestCustomer>();
    public DbSet<TestOrder> Orders => Set<TestOrder>();

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public TestDbContext(SqliteConnection connection)
        : base(BuildOptions(connection, withInterceptors: false))
    {
    }

    public TestDbContext(SqliteConnection connection, bool withInterceptors)
        : base(BuildOptions(connection, withInterceptors))
    {
    }

    private static DbContextOptions<TestDbContext> BuildOptions(SqliteConnection connection, bool withInterceptors)
    {
        var builder = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .IgnoreManyServiceProvidersCreatedWarning();
        if (withInterceptors)
            builder.AddTrellisInterceptors();
        return builder.Options;
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.ApplyTrellisConventions(typeof(TestCustomerId).Assembly);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestCustomer>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Name).HasMaxLength(100).IsRequired();
            b.Property(c => c.Email).HasMaxLength(254).IsRequired();
            b.HasIndex(c => c.Email).IsUnique(); // For duplicate key tests
            b.Property(c => c.CreatedAt).IsRequired();
            // Phone — MaybeConvention auto-configures the generated storage member
        });

        modelBuilder.Entity<TestOrder>(b =>
        {
            b.HasKey(o => o.Id);
            b.HasOne(o => o.Customer).WithMany(c => c.Orders).HasForeignKey(o => o.CustomerId);
            b.Property(o => o.Amount).IsRequired();
            b.Property(o => o.Status).IsRequired();
            // OptionalStatus and SubmittedAt — MaybeConvention auto-configures generated storage members
        });
    }

    /// <summary>
    /// Creates a new test database context with an open SQLite in-memory connection.
    /// The connection stays open for the lifetime of the returned context.
    /// Caller is responsible for disposing the connection.
    /// </summary>
    /// <param name="withInterceptors">
    /// When <c>true</c>, registers the Trellis EF Core interceptors so query expressions
    /// like <c>c => c.Id.Value</c> (scalar value-object projections) translate to SQL.
    /// Defaults to <c>false</c> to keep existing tests unaffected.
    /// </param>
    public static (TestDbContext Context, SqliteConnection Connection) CreateInMemory(bool withInterceptors = false)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var context = new TestDbContext(connection, withInterceptors);
        context.Database.EnsureCreated();

        return (context, connection);
    }
}