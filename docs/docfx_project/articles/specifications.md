# Specifications

**Level:** Intermediate | **Status:** Planned

Specifications express business rules as composable, storage-agnostic expression trees. The same specification works for querying EF Core, Cosmos DB, or in-memory collections.

> **Note:** The Specification pattern is planned for `Trellis.DomainDrivenDesign`. This article describes the design and intended usage.

## Why Specifications?

Business rules like "find all overdue orders over $500 in the West region" appear in multiple places — queries, validation, reporting. Without specifications, this logic gets duplicated across repositories, services, and controllers.

Specifications solve this by encapsulating business rules as **reusable expression trees** that any LINQ provider can evaluate.

## Concept

A specification is a predicate expressed as an `Expression<Func<T, bool>>`:

```csharp
public class OverdueOrderSpec : Specification<Order>
{
    public override Expression<Func<Order, bool>> ToExpression()
        => order => order.DueDate < DateTime.UtcNow && order.Status != OrderStatus.Completed;
}
```

## Composability

Specifications combine using `And`, `Or`, and `Not`:

```csharp
// Compose multiple specifications
var spec = new OverdueOrderSpec()
    .And(new OrderValueExceedsSpec(500m))
    .And(new CustomerInRegionSpec("West"));

// Use with any LINQ provider
var results = await _dbContext.Orders
    .Where(spec.ToExpression())
    .ToListAsync();
```

## Design Principles

- **Domain concept** — Specifications express business rules, not persistence concerns
- **Storage-agnostic** — Works with EF Core, Cosmos DB, or any LINQ provider
- **Composable** — `And`, `Or`, `Not` for building complex queries from simple parts
- **Testable** — Specifications can be tested against in-memory collections
- **Owned by the domain layer** — No dependency on any persistence library

## Relationship to Trellis

Specifications integrate with the rest of Trellis:

- Value objects in specification predicates enforce type safety
- Result types for specification evaluation failures
- Aggregate boundaries respected in specification queries

## Next Steps

- [Clean Architecture](clean-architecture.md) — Architecture patterns that use specifications
- [Entity Framework Core](integration-ef.md) — Repository patterns for persistence
- [Trellis for AI Code Generation](ai-code-generation.md) — How specs map to code
