# TRLS021 — EF configuration duplicates Trellis conventions

- **Severity:** Warning
- **Category:** Trellis

## What it detects

Flags `IEntityTypeConfiguration<T>` (or inline `modelBuilder.Entity<T>()`) code that manually calls `HasConversion(...)`, `OwnsOne(...)`, or `Ignore(...)` for a property whose type is already owned by Trellis conventions:

- A `Maybe<T>` property — `MaybeConvention` already maps the underlying storage member (`_camelCase` backing field) when `ApplyTrellisConventions(...)` is wired.
- An `[OwnedEntity]`-decorated composite value object — `CompositeValueObjectConvention` already configures ownership and column splitting.

Activates only when `ApplyTrellisConventions(...)` or `ApplyTrellisConventionsFor<TContext>()` is reachable from the consumer's compilation.

## Why it matters

Manual EF mapping for property shapes that Trellis already handles is at best redundant and at worst silently conflicts with the convention-generated model. Common failure modes:

- A manual `HasConversion(maybe => maybe.AsNullable(), nullable => Maybe.From(nullable))` on a `Maybe<T>` property overrides the backing-field-based convention and produces double-serialization or column mismatches at runtime.
- A manual `OwnsOne(c => c.Address, ...)` on an `[OwnedEntity]` composite competes with `CompositeValueObjectConvention`, producing two ownership records and a `Cannot add ... already owned` migration error.
- A manual `Ignore(c => c.SomeMaybe)` masks a property the convention would have correctly persisted, silently losing data.

## Bad examples

```csharp
public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        // ❌ Maybe<T> backed by a convention — manual conversion conflicts.
        builder.Property(c => c.PhoneNumber)
            .HasConversion(
                m => m.AsNullable(),
                v => Maybe.From(v));                                                     // TRLS021

        // ❌ [OwnedEntity] composite — CompositeValueObjectConvention already owns it.
        builder.OwnsOne(c => c.ShippingAddress);                                         // TRLS021
    }
}
```

## Good examples

```csharp
public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Email).IsRequired();
        builder.HasIndex(c => c.Email).IsUnique();

        // No mapping for PhoneNumber or ShippingAddress — conventions own them.
        // ApplyTrellisConventions(typeof(CustomerId).Assembly) in ConfigureConventions
        // does the wiring once for the whole DbContext.
    }
}
```

## Code fix available

No. The diagnostic is informational about a redundant mapping; the right action is to *delete* the manual configuration, which the developer should verify by running `dotnet ef dbcontext optimize` (or inspecting the generated model snapshot) to confirm the property is still mapped by the convention.

## Configuration

Standard Roslyn configuration applies.

```ini
dotnet_diagnostic.TRLS021.severity = none
```

> [!TIP]
> If a property genuinely needs custom mapping that the convention cannot express (a non-default column name, a project-specific value converter, a check constraint), suppress the diagnostic with a `#pragma` at the call site and leave a one-line comment naming the constraint. The rule is opinionated, not absolute.
