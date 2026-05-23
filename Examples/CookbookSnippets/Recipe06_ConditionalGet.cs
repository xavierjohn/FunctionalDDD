// Cookbook Recipe 6 — Conditional GET with EntityTagValue (ETag + Last-Modified preconditions).
namespace CookbookSnippets.Recipe06;

using System.Threading;
using CookbookSnippets.Stubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Trellis;
using Trellis.Asp;

public static class ConditionalGetSample
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/blobs/{id:guid}", async (System.Guid id, IBlobRepository repo, CancellationToken ct) =>
        {
            Result<BlobContent> result = await repo.FindAsync(new BlobId(id), ct);

            return result.ToHttpResponse(opts => opts
                .WithETag(b => EntityTagValue.Strong(b.Sha256Hex))
                .WithLastModified(b => b.UploadedAt)
                .EvaluatePreconditions());
        });
}

internal static class Recipe6ConditionalGetSurface
{
    public static void ETagOverloads()
    {
        var weak = EntityTagValue.Weak("sha256-deadbeef");
        var options = new HttpResponseOptionsBuilder<BlobContent>()
            .WithETag(b => b.Sha256Hex)
            .WithETag(_ => weak);

        _ = (weak, options);
    }
}