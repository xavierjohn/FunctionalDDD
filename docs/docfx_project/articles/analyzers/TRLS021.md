# TRLS021: HasIndex references a Maybe\<T\> property

## Cause

A `HasIndex` lambda expression in an EF Core entity configuration references a property declared as `Maybe<T>`. This silently fails to create the index because `MaybeConvention` maps `Maybe<T>` via private backing fields, making the CLR property invisible to EF Core's index builder.

## Rule Description

When you use `Maybe<T>` for optional properties, the Trellis `MaybeConvention` ignores the `Maybe<T>` CLR property and maps the private `_camelCase` backing field instead. EF Core's `HasIndex` with a lambda expression resolves properties by their CLR property name — since the `Maybe<T>` property is ignored, the index silently doesn't get created.

This rule fires as a **Warning** because the code compiles and runs without errors, but the database index simply won't exist at runtime.

## How to Fix Violations

Replace the lambda-based `HasIndex` with `HasTrellisIndex`, or use the string-based overload with the mapped backing field name.

`HasTrellisIndex` only accepts direct property access on the lambda parameter. Expressions like `e => e.Customer.SubmittedAt` are rejected with `ArgumentException` instead of being interpreted as indexes on the root entity.

For `Maybe<T>` properties, `HasTrellisIndex` validates that the expected backing field exists on the entity CLR type hierarchy or is already mapped in the EF model. If the backing field is missing, it throws `InvalidOperationException` with guidance to use `partial` properties or explicit field mapping.

### Single property index

```csharp
// ❌ Bad - index silently not created
builder.HasIndex(e => e.SubmittedAt);

// ✅ Preferred - resolves Maybe<T> to the mapped backing field for you
builder.HasTrellisIndex(e => e.SubmittedAt);

// ✅ Also valid - uses backing field name directly
builder.HasIndex("_submittedAt");
```

### Composite index with Maybe\<T\>

```csharp
// ❌ Bad - SubmittedAt part of index silently ignored
builder.HasIndex(e => new { e.Status, e.SubmittedAt });

// ✅ Preferred - regular properties stay regular, Maybe<T> resolves automatically
builder.HasTrellisIndex(e => new { e.Status, e.SubmittedAt });

// ✅ Also valid - uses string-based overload with backing field
builder.HasIndex("Status", "_submittedAt");
```

### Composite index without Maybe\<T\>

```csharp
// ✅ Fine - no Maybe<T> properties, lambda works correctly
builder.HasIndex(e => new { e.Status, e.Name });
```

## Backing Field Naming Convention

The backing field follows `_camelCase` naming from the property name:

| Property | Backing Field |
|----------|---------------|
| `SubmittedAt` | `_submittedAt` |
| `Phone` | `_phone` |
| `AlternateEmail` | `_alternateEmail` |

## Background

`Maybe<T>` is a `readonly struct`. EF Core cannot mark non-nullable struct properties as optional. The Trellis source generator emits a private nullable backing field (e.g., `DateTime? _submittedAt`) and the `MaybeConvention` maps that field instead. See the [EF Core integration guide](../integration-ef.md) for full details.

## When to Suppress Warnings

Suppress this warning only if you intentionally don't want an index on the `Maybe<T>` property and the `HasIndex` includes other non-Maybe properties that you do want indexed:

```csharp
#pragma warning disable TRLS021
builder.HasIndex(e => new { e.Status, e.SubmittedAt }); // Only Status index matters
#pragma warning restore TRLS021
```

However, in this case it's clearer to remove the `Maybe<T>` property from the index expression entirely or switch to `HasTrellisIndex`.

## See Also

- [EF Core Integration](../integration-ef.md)
- [Maybe\<T\> with EF Core](../integration-ef.md)
- [MaybeConvention source](https://github.com/xavierjohn/Trellis)
