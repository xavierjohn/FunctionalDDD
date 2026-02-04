namespace EfCoreExample.Data;

using System.Globalization;
using EfCoreExample.Entities;
using EfCoreExample.EnumValueObjects;
using EfCoreExample.ValueObjects;
using FunctionalDdd;
using FunctionalDdd.PrimitiveValueObjects;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// EF Core DbContext demonstrating seamless integration with FunctionalDDD value objects.
/// Shows how to configure value converters for RequiredUlid, RequiredGuid, RequiredString, and EmailAddress.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    /// <summary>
    /// Orders using SmartEnum for state - demonstrates rich domain behavior.
    /// </summary>
    public DbSet<SmartOrder> SmartOrders => Set<SmartOrder>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureCustomer(modelBuilder);
        ConfigureProduct(modelBuilder);
        ConfigureOrder(modelBuilder);
        ConfigureOrderLine(modelBuilder);
        ConfigureSmartOrder(modelBuilder);
    }

    private static void ConfigureCustomer(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Customer>(builder =>
        {
            builder.HasKey(c => c.Id);

            // RequiredUlid<CustomerId> -> Ulid -> stored as string in database
            // ULIDs are 26-character Crockford Base32 strings
            builder.Property(c => c.Id)
                .HasConversion(
                    id => id.Value.ToString(),
                    str => CustomerId.Create(Ulid.Parse(str, CultureInfo.InvariantCulture)))
                .HasMaxLength(26)
                .IsRequired();

            // RequiredString<CustomerName> -> string
            builder.Property(c => c.Name)
                .HasConversion(
                    name => name.Value,
                    str => CustomerName.Create(str))
                .HasMaxLength(100)
                .IsRequired();

            // EmailAddress -> string (built-in RFC 5322 validation)
            builder.Property(c => c.Email)
                .HasConversion(
                    email => email.Value,
                    str => EmailAddress.Create(str))
                .HasMaxLength(254)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired();
        });

    private static void ConfigureProduct(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Product>(builder =>
        {
            builder.HasKey(p => p.Id);

            // RequiredGuid<ProductId> -> Guid
            // Demonstrates mixing GUID and ULID in the same domain
            builder.Property(p => p.Id)
                .HasConversion(
                    id => id.Value,
                    guid => ProductId.Create(guid))
                .IsRequired();

            // RequiredString<ProductName> -> string
            builder.Property(p => p.Name)
                .HasConversion(
                    name => name.Value,
                    str => ProductName.Create(str))
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(p => p.Price)
                .IsRequired();

            builder.Property(p => p.StockQuantity)
                .IsRequired();
        });

    private static void ConfigureOrder(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Order>(builder =>
        {
            builder.HasKey(o => o.Id);

            // RequiredUlid<OrderId> -> Ulid -> string
            // Orders benefit from ULID's time-ordering for natural chronological queries
            builder.Property(o => o.Id)
                .HasConversion(
                    id => id.Value.ToString(),
                    str => OrderId.Create(Ulid.Parse(str, CultureInfo.InvariantCulture)))
                .HasMaxLength(26)
                .IsRequired();

            // Foreign key to Customer using ULID
            builder.Property(o => o.CustomerId)
                .HasConversion(
                    id => id.Value.ToString(),
                    str => CustomerId.Create(Ulid.Parse(str, CultureInfo.InvariantCulture)))
                .HasMaxLength(26)
                .IsRequired();

            builder.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(o => o.CreatedAt)
                .IsRequired();

            builder.Property(o => o.ConfirmedAt);

            // Ignore computed property
            builder.Ignore(o => o.Total);

            // Configure one-to-many relationship with OrderLines
            builder.HasMany(o => o.Lines)
                .WithOne()
                .HasForeignKey(l => l.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

    private static void ConfigureOrderLine(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<OrderLine>(builder =>
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Id)
                .ValueGeneratedOnAdd();

            // Foreign key using ULID
            builder.Property(l => l.OrderId)
                .HasConversion(
                    id => id.Value.ToString(),
                    str => OrderId.Create(Ulid.Parse(str, CultureInfo.InvariantCulture)))
                .HasMaxLength(26)
                .IsRequired();

            // Foreign key using GUID
            builder.Property(l => l.ProductId)
                .HasConversion(
                    id => id.Value,
                    guid => ProductId.Create(guid))
                .IsRequired();

            builder.Property(l => l.ProductName)
                .HasConversion(
                    name => name.Value,
                    str => ProductName.Create(str))
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(l => l.UnitPrice)
                .IsRequired();

            builder.Property(l => l.Quantity)
                .IsRequired();

            // Ignore computed property
            builder.Ignore(l => l.LineTotal);
        });

    /// <summary>
    /// Configures the SmartOrder entity demonstrating SmartEnum persistence.
    /// </summary>
    private static void ConfigureSmartOrder(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<SmartOrder>(builder =>
        {
            builder.HasKey(o => o.Id);

            // RequiredUlid<OrderId> -> Ulid -> string
            builder.Property(o => o.Id)
                .HasConversion(
                    id => id.Value.ToString(),
                    str => OrderId.Create(Ulid.Parse(str, CultureInfo.InvariantCulture)))
                .HasMaxLength(26)
                .IsRequired();

            // Foreign key to Customer using ULID
            builder.Property(o => o.CustomerId)
                .HasConversion(
                    id => id.Value.ToString(),
                    str => CustomerId.Create(Ulid.Parse(str, CultureInfo.InvariantCulture)))
                .HasMaxLength(26)
                .IsRequired();

            // ✨ SmartEnum -> stored as string Name for readability
            // Alternative: store as int Value for efficiency
            builder.Property(o => o.State)
                .HasConversion(
                    state => state.Name,  // Store the name (human-readable)
                    name => OrderState.FromName(name))  // Restore from name
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(o => o.CreatedAt).IsRequired();
            builder.Property(o => o.ConfirmedAt);
            builder.Property(o => o.ShippedAt);
            builder.Property(o => o.DeliveredAt);
            builder.Property(o => o.CancelledAt);

            // Ignore computed property
            builder.Ignore(o => o.Total);

            // Note: For simplicity, we're not configuring the Lines relationship here
            // In a real app, you'd configure it similar to Order
        });
}