using CleanArchitecture.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Infrastructure.Shared.Services;

/// <summary>
/// Implementasi <see cref="IIdempotencyService"/> menggunakan <see cref="IMemoryCache"/>.
/// Menyimpan response yang sudah diproses berdasarkan Idempotency Key untuk mencegah
/// pemrosesan duplikat pada request yang sama.
///
/// Konfigurasi di <c>appsettings.json</c>:
/// <code>
/// "Idempotency": {
///   "CacheDurationHours": 24
/// }
/// </code>
///
/// Catatan Arsitektur: Untuk deployment multi-instance (multiple pods/servers),
/// ganti implementasi ini dengan <c>IDistributedCache</c> (Redis) agar cache
/// bersifat shared antar semua instance.
/// </summary>
public class IdempotencyService : IIdempotencyService
{
    private const string CacheKeyPrefix = "idempotency:";

    private readonly IMemoryCache _cache;
    private readonly ILogger<IdempotencyService> _logger;
    private readonly TimeSpan _cacheDuration;

    public IdempotencyService(
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<IdempotencyService> logger)
    {
        _cache = cache;
        _logger = logger;

        var configuredHours = configuration.GetValue<int>("Idempotency:CacheDurationHours", 24);
        _cacheDuration = TimeSpan.FromHours(configuredHours);

        _logger.LogDebug("[IdempotencyService] Initialized dengan cache duration: {Duration}", _cacheDuration);
    }

    /// <inheritdoc/>
    public Task<IdempotencyResult?> GetCachedResponseAsync(string idempotencyKey)
    {
        var cacheKey = BuildCacheKey(idempotencyKey);
        _cache.TryGetValue(cacheKey, out IdempotencyResult? result);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task CacheResponseAsync(string idempotencyKey, string serializedResponse, TimeSpan ttl)
    {
        var cacheKey = BuildCacheKey(idempotencyKey);

        // Gunakan TTL dari konfigurasi, bukan yang dikirim dari behaviour (lebih aman)
        var effectiveTtl = _cacheDuration;

        var result = new IdempotencyResult(serializedResponse, DateTime.UtcNow);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = effectiveTtl,
            // Tidak gunakan sliding expiration — idempotency window harus fixed
            Priority = CacheItemPriority.Normal,
            Size = 1
        };

        _cache.Set(cacheKey, result, cacheOptions);

        _logger.LogDebug("[IdempotencyService] Cached response untuk key '{Key}', expires in {Hours}h",
            idempotencyKey, effectiveTtl.TotalHours);

        return Task.CompletedTask;
    }

    private static string BuildCacheKey(string idempotencyKey) =>
        $"{CacheKeyPrefix}{idempotencyKey.Trim().ToLowerInvariant()}";
}
