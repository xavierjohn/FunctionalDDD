using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Trellis.Authorization;

namespace SsoExample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeController(IActorProvider actorProvider) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var maybeActor = await actorProvider.GetCurrentActorAsync(ct);
        if (!maybeActor.TryGetValue(out var actor))
            return Unauthorized();

        return Ok(new
        {
            actor.Id,
            Permissions = actor.Permissions.ToList(),
            Attributes = actor.Attributes
        });
    }
}