using System.Collections.Concurrent;

namespace Example.Tests;

public class TenantMetadataExamples
{
    [Fact]
    public async Task Fetch_tenant_metadata_with_cache_fallback()
    {
        var tenantId = "tenant-123";

        // First call: cache miss -> storage load -> cache populate
        var first = await GetTenantMetadataAsync(tenantId);
        first.IsSuccess.Should().BeTrue();
        first.Value.Source.Should().Be("storage");

        // Second call: should hit cache
        var second = await GetTenantMetadataAsync(tenantId);
        second.IsSuccess.Should().BeTrue();
        second.Value.Source.Should().Be("cache");
    }

    // Public API that callers would use.
    public static Task<Result<TenantMetadata>> GetTenantMetadataAsync(string tenantId) =>
        GetFromCacheAsync(tenantId)
            .ToResultAsync(Error.NotFound($"Tenant metadata not found in cache: {tenantId}"))
            // (Optional) observe the cache miss
            .TapErrorAsync(err => Log($"Cache miss for {tenantId}: {err.Detail}"))
            // On failure (cache miss) compensate by loading from storage and updating the cache
            .CompensateAsync(() => LoadFromStorageAndUpdateCacheAsync(tenantId));

    // ----- Cache Layer -----
    static Task<TenantMetadata?> GetFromCacheAsync(string tenantId)
    {
        TenantMetadata? value = InMemoryTenantCache.TryGet(tenantId);
        return Task.FromResult(value);
    }

    static Task UpdateCacheAsync(TenantMetadata metadata)
    {
        InMemoryTenantCache.Set(metadata);
        return Task.CompletedTask;
    }

    // ----- Storage Fallback -----
    static async Task<Result<TenantMetadata>> LoadFromStorageAndUpdateCacheAsync(string tenantId)
    {
        var storageEntity = await FakeTenantStorage.GetAsync(tenantId);
        if (storageEntity is null)
            return Result.Failure<TenantMetadata>(Error.NotFound($"Tenant not found in storage: {tenantId}"));

        // Mark the source as storage (will be rewritten as cache on subsequent retrieval)
        var metadata = storageEntity with { Source = "storage" };

        await UpdateCacheAsync(metadata with { Source = "cache" }); // Store a cache-flavored copy

        return Result.Success(metadata);
    }

    static void Log(string message) { /* hook in real logging */ }

    // ----- Domain Type -----
    public sealed record TenantMetadata(string TenantId, string Name, DateTimeOffset LastUpdatedUtc, string Source);

    // ----- Simple In-Memory Cache (for demo only) -----
    static class InMemoryTenantCache
    {
        static readonly ConcurrentDictionary<string, TenantMetadata> _cache = new();

        public static TenantMetadata? TryGet(string tenantId) =>
            _cache.TryGetValue(tenantId, out var meta) ? meta : null;

        public static void Set(TenantMetadata metadata) =>
            _cache[metadata.TenantId] = metadata;
    }

    // ----- Fake Storage -----
    static class FakeTenantStorage
    {
        public static Task<TenantMetadata?> GetAsync(string tenantId)
        {
            // Simulate existing tenant in persistent storage
            var meta = new TenantMetadata(
                tenantId,
                Name: "Contoso",
                LastUpdatedUtc: DateTimeOffset.UtcNow,
                Source: "storage");

            return Task.FromResult<TenantMetadata?>(meta);
        }
    }
}