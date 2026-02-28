# Trellis.Authorization

Lightweight authorization primitives for [Trellis](https://github.com/xavierjohn/Trellis).

Provides `Actor`, `IActorProvider`, `IAuthorize`, and `IAuthorizeResource` types that integrate with the Trellis `Result<T>` type system. No dependency on any mediator or web framework.

## Types

| Type | Purpose |
|------|---------|
| `Actor` | Represents the current user with `Id` and `Permissions` |
| `IActorProvider` | Abstraction for resolving the current actor (implement in API layer) |
| `IAuthorize` | Marker for static permission-based authorization |
| `IAuthorizeResource` | Marker for resource-based authorization |

## Usage

```csharp
using Trellis.Authorization;

// Implement IActorProvider in your API layer
public class HttpActorProvider(IHttpContextAccessor accessor) : IActorProvider
{
    public Actor GetCurrentActor()
    {
        var user = accessor.HttpContext!.User;
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var permissions = user.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToHashSet();
        return Actor.Create(id, permissions);
    }
}
```

See the [full documentation](https://xavierjohn.github.io/Trellis/) for details.
