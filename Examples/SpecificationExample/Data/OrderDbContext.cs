namespace SpecificationExample.Data;

using Microsoft.EntityFrameworkCore;
using SpecificationExample.Entities;
using SpecificationExample.ValueObjects;
using System.Globalization;

/// <summary>
/// Application DbContext with value object converters.
/// </summary>
public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Value object converters
            entity.Property(e => e.Id)
                  .HasConversion(
                      v => v.Value.ToString(),
                      v => OrderId.Create(Ulid.Parse(v, CultureInfo.InvariantCulture)));

            entity.Property(e => e.CustomerId)
                  .HasConversion(
                      v => v.Value.ToString(),
                      v => CustomerId.Create(Ulid.Parse(v, CultureInfo.InvariantCulture)));

            entity.Property(e => e.CustomerName)
                  .HasConversion(
                      v => v.Value,
                      v => CustomerName.Create(v));

            entity.HasMany(e => e.Lines)
                  .WithOne(e => e.Order)
                  .HasForeignKey(e => e.OrderId);
        });

        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.OrderId)
                  .HasConversion(
                      v => v.Value.ToString(),
                      v => OrderId.Create(Ulid.Parse(v, CultureInfo.InvariantCulture)));
        });
    }
}
