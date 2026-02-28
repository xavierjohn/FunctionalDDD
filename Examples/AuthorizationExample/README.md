# 🔐 Authorization Example

**Complexity:** ⭐⭐ (Intermediate) | **Time to Learn:** 30–45 minutes

Demonstrates the same authorization rules enforced two ways — with and without CQRS — using `Trellis.Authorization` and `Trellis.Mediator`.

## 🎯 What You'll Learn

- **`Actor`** — how to represent users with permissions
- **`IAuthorize`** — static permission checks (AND logic)
- **`IAuthorizeResource`** — resource-based authorization (owner checks)
- **Direct vs. Pipeline** — same domain rules, different execution models

## The Scenario

A document management system where:
- Any user can create documents
- Only the **owner** (or users with `Documents.EditAny`) can edit
- Only users with `Documents.Publish` can publish
- Three actors: Alice (owner), Bob (no permissions), Charlie (admin)

## Running the Example

```bash
cd Examples/AuthorizationExample
dotnet run
```

## Output

Both parts produce identical authorization outcomes:

```
Alice creates 'Design Doc'                    → ✅ Success
Alice edits her document                      → ✅ Success
Bob tries to edit Alice's document            → ❌ Only the owner can edit this document
Charlie edits Alice's document                → ✅ Success
Bob tries to publish                          → ❌ Missing required permission: Documents.Publish
Alice publishes her document                  → ✅ Published
```

## Part 1: Direct Service (No CQRS)

Uses only `Trellis.Authorization` — no Mediator dependency. Authorization logic is mixed into each service method:

```csharp
public Result<Document> EditDocument(Actor actor, string documentId, string newContent)
{
    var doc = _store.Get(documentId);
    // ⚠️ Auth logic mixed with business logic
    if (actor.Id != doc.OwnerId && !actor.HasPermission("Documents.EditAny"))
        return Result.Failure<Document>(Error.Forbidden("Only the owner can edit"));

    var updated = doc with { Content = newContent };
    _store.Update(updated);
    return Result.Success(updated);
}
```

## Part 2: Mediator Pipeline (CQRS)

Authorization is **declared** on commands and **enforced** by pipeline behaviors. Handlers contain only business logic:

```csharp
// Command declares its auth requirements
public sealed record EditDocumentCommand(string DocumentId, string OwnerId, string NewContent)
    : ICommand<Result<Document>>, IAuthorizeResource
{
    public IResult Authorize(Actor actor) =>
        actor.Id == OwnerId || actor.HasPermission("Documents.EditAny")
            ? Result.Success()
            : Result.Failure(Error.Forbidden("Only the owner can edit"));
}

// Handler has ZERO auth code
public sealed class EditDocumentHandler(DocumentStore store)
    : ICommandHandler<EditDocumentCommand, Result<Document>>
{
    public ValueTask<Result<Document>> Handle(
        EditDocumentCommand command, CancellationToken cancellationToken)
    {
        var doc = store.Get(command.DocumentId);
        var updated = doc! with { Content = command.NewContent };
        store.Update(updated);
        return new ValueTask<Result<Document>>(Result.Success(updated));
    }
}
```

## Key Takeaway

The authorization rules are identical. The difference is **where** they execute:

| Approach | Auth lives in | Handlers contain auth? | Dependency |
|----------|-------------|----------------------|------------|
| Direct Service | Service methods | Yes — mixed with business logic | `Trellis.Authorization` only |
| Mediator Pipeline | Command declarations + behaviors | No — zero auth code | `Trellis.Mediator` |

## Files

- `Actors.cs` — Test actors with varying permissions
- `Document.cs` — Simple document record and in-memory store
- `DirectServiceExample.cs` — Part 1: Manual authorization in service methods
- `MediatorExample.cs` — Part 2: Declarative authorization with pipeline behaviors
- `Program.cs` — Runs both parts and compares results
