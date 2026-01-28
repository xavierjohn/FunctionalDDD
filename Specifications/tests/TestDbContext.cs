namespace FunctionalDdd.Specifications.Tests;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// In-memory DbContext for testing specifications.
/// </summary>
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                  .WithOne(e => e.Order)
                  .HasForeignKey(e => e.OrderId);
        });

        modelBuilder.Entity<OrderLine>(entity => entity.HasKey(e => e.Id));
    }
}
