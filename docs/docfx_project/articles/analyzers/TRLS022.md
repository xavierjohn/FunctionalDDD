# TRLS022 — `[OwnedEntity]` property uses init-only setter

- **Severity:** Warning
- **Category:** Trellis

## What it detects

Flags `{ get; init; }` properties declared on a type decorated with `[OwnedEntity]`. The supported, tested shape is `{ get; private set; }`.

## Why it matters

`[OwnedEntity]` types are materialized by EF Core through a generator-emitted private parameterless constructor: the source generator (`Trellis.EntityFrameworkCore.Generator`) emits the EF-only constructor, and EF Core then populates each property via the runtime accessor. Init-only setters work for the language-level "set once" construct but are not covered by Trellis's owned-entity round-trip tests. Behaviour with EF Core change tracking, snapshot-based update detection, and the `CompositeValueObjectJsonConverter` reflective path is not guaranteed.

Using `{ get; private set; }` matches what every Trellis owned-entity test fixture exercises today and is the shape the source generator was designed against.

## Bad examples

```csharp
[OwnedEntity]
public sealed partial class ShippingAddress : ValueObject
{
    public string Street { get; init; } = null!;                                          // TRLS022
    public string City { get; init; } = null!;                                            // TRLS022
    public string State { get; init; } = null!;                                           // TRLS022
    public string PostalCode { get; init; } = null!;                                      // TRLS022
    public string Country { get; init; } = null!;                                         // TRLS022
}
```

## Good examples

```csharp
[OwnedEntity]
public sealed partial class ShippingAddress : ValueObject
{
    public string Street { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string State { get; private set; } = null!;
    public string PostalCode { get; private set; } = null!;
    public string Country { get; private set; } = null!;

    // The source generator emits the private parameterless constructor used by EF Core.
    private ShippingAddress(string street, string city, string state, string postalCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }
}
```

## Code fix available

No. The diagnostic is a one-token edit per property (`init` → `private set`) and is best applied with a single search/replace inside the offending type.

## Configuration

Standard Roslyn configuration applies.

```ini
dotnet_diagnostic.TRLS022.severity = none
```

> [!TIP]
> Do not declare a parameterless constructor on an `[OwnedEntity]` partial — the source generator emits it. Doing so produces a duplicate-constructor error from the generator (see also TRLS037 if it fires in your build).
