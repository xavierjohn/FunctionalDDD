namespace SpecificationExample.Infrastructure;

using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SpecificationExample.Domain;

/// <summary>
/// In-memory database context for demonstration.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.Sku).IsUnique();
            entity.Property(p => p.Name).HasMaxLength(200);
            entity.Property(p => p.Sku).HasMaxLength(50);
            entity.Property(p => p.Category).HasMaxLength(100);
            entity.Property(p => p.Price).HasPrecision(18, 2);
        });
}

/// <summary>
/// Generic repository implementing Ardalis.Specification.
/// </summary>
public class EfRepository<T> : RepositoryBase<T>, IReadRepositoryBase<T> where T : class
{
    public EfRepository(AppDbContext dbContext) : base(dbContext) { }
}
