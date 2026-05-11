// Cookbook Recipe 6 — Conditional GET with EntityTagValue and byte-range with RangeOutcome.
namespace CookbookSnippets.Recipe06;

using System.Threading;
using CookbookSnippets.Stubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trellis;
using Trellis.Asp;

public static class ConditionalGetSample
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/blobs/{id:guid}", async (System.Guid id, HttpRequest req, IBlobRepository repo, CancellationToken ct) =>
        {
            Result<BlobContent> result = await repo.FindAsync(new BlobId(id), ct);

            return result.ToHttpResponse(opts => opts
                .WithETag(b => EntityTagValue.Strong(b.Sha256Hex))
                .WithLastModified(b => b.UploadedAt)
                .Vary("Range")
                .WithAcceptRanges("bytes")
                .WithRange(b =>
                {
                    var outcome = RangeRequestEvaluator.Evaluate(req, b.Length);
                    return outcome switch
                    {
                        RangeOutcome.PartialContent pc => new System.Net.Http.Headers.ContentRangeHeaderValue(pc.From, pc.To, pc.CompleteLength),
                        _ => new System.Net.Http.Headers.ContentRangeHeaderValue(b.Length),
                    };
                })
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

    public static void RangeOutcomeCases()
    {
        RangeOutcome full = new RangeOutcome.FullRepresentation();
        RangeOutcome partial = new RangeOutcome.PartialContent(0, 99, 100);
        RangeOutcome notSatisfiable = new RangeOutcome.NotSatisfiable(100);

        long completeLength = partial switch
        {
            RangeOutcome.FullRepresentation => 0,
            RangeOutcome.PartialContent pc => pc.CompleteLength,
            RangeOutcome.NotSatisfiable ns => ns.CompleteLength,
            _ => 0,
        };

        _ = (full, partial, notSatisfiable, completeLength);
    }
}