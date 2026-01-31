namespace SpecificationExample.Infrastructure;

using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using FunctionalDdd.PrimitiveValueObjects;
using Microsoft.EntityFrameworkCore;
using SpecificationExample.Domain;

/// <summary>
/// In-memory database context for demonstration.
/// Configures EF Core to work with FunctionalDDD value objects.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Product>(entity =>
        {
            // Configure ProductId value object as primary key
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id)
                .HasConversion(
                    id => id.Value,
                    value => ProductId.Create(value));

            // Configure Sku value object with unique index
            entity.HasIndex(p => p.Sku).IsUnique();
            entity.Property(p => p.Sku)
                .HasMaxLength(50)
                .HasConversion(
                    sku => sku.Value,
                    value => Sku.Create(value));

            // Configure ProductName value object
            entity.Property(p => p.Name)
                .HasMaxLength(200)
                .HasConversion(
                    name => name.Value,
                    value => ProductName.Create(value));

            // Configure CategoryName value object
            entity.Property(p => p.Category)
                .HasMaxLength(100)
                .HasConversion(
                    category => category.Value,
                    value => CategoryName.Create(value));

            // Configure Money as owned type (complex value object from PrimitiveValueObjects)
            entity.OwnsOne(p => p.Price, price =>
            {
                price.Property(m => m.Amount).HasPrecision(18, 2);
                // CurrencyCode is a value object - convert to/from string
                price.Property(m => m.Currency)
                    .HasMaxLength(3)
                    .HasConversion(
                        c => c.Value,
                        v => CurrencyCode.Create(v));
            });
        });
}

/// <summary>
/// Generic repository implementing Ardalis.Specification.
/// </summary>
public class EfRepository<T> : RepositoryBase<T>, IReadRepositoryBase<T> where T : class
{
    public EfRepository(AppDbContext dbContext) : base(dbContext) { }
}