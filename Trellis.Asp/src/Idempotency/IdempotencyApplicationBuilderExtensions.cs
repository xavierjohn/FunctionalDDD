namespace Trellis.Asp.Idempotency;

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IApplicationBuilder"/> extensions that mount the Trellis Idempotency-Key
/// middleware in the request pipeline.
/// </summary>
public static class IdempotencyApplicationBuilderExtensions
{
    /// <summary>
    /// Mounts the <see cref="IdempotencyMiddleware"/> in the pipeline. Throws if the application
    /// has not registered an <see cref="IIdempotencyStore"/> (call
    /// <see cref="IdempotencyServiceCollectionExtensions.AddInMemoryIdempotencyStore"/> or
    /// supply your own implementation).
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseTrellisIdempotency(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var marker = app.ApplicationServices.GetService<IdempotencyServiceCollectionExtensions.IdempotencyMarker>();
        if (marker is null)
        {
            throw new InvalidOperationException(
                "UseTrellisIdempotency() was called but AddTrellisIdempotency() was not. " +
                "Call services.AddTrellisIdempotency() at startup before mounting the middleware.");
        }

        var store = app.ApplicationServices.GetService<IIdempotencyStore>();
        if (store is null)
        {
            throw new InvalidOperationException(
                "UseTrellisIdempotency() requires an IIdempotencyStore. " +
                "Call services.AddInMemoryIdempotencyStore() for an in-process store, " +
                "or register your own IIdempotencyStore implementation.");
        }

        return app.UseMiddleware<IdempotencyMiddleware>();
    }
}
