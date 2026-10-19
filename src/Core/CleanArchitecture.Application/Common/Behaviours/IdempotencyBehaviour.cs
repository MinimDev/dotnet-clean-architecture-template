using System.Text.Json;
using CleanArchitecture.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Common.Behaviours;

/// <summary>
/// MediatR Pipeline Behaviour untuk mencegah pemrosesan request duplikat (idempotency).
///
/// CARA KERJA:
/// 1. Membaca <c>Idempotency-Key</c> melalui abstraksi <see cref="IIdempotencyKeyAccessor"/>.
/// 2. Jika key sudah ada di cache → langsung kembalikan cached response (tidak dieksekusi ulang).
/// 3. Jika key belum ada → proses request normal, lalu cache hasilnya.
///
/// SCOPE: Hanya berlaku jika Idempotency-Key disediakan (biasanya oleh HTTP client pada commands).
/// Behaviour ini aman untuk dilewati jika tidak ada Idempotency-Key header.
///
/// CONTOH PENGGUNAAN (dari HTTP Client):
/// <code>
/// POST /api/v1/products
/// Idempotency-Key: 550e8400-e29b-41d4-a716-446655440000
/// Content-Type: application/json
/// </code>
/// </summary>
public class IdempotencyBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IIdempotencyService _idempotencyService;
    private readonly IIdempotencyKeyAccessor _idempotencyKeyAccessor;
    private readonly ILogger<IdempotencyBehaviour<TRequest, TResponse>> _logger;

    public IdempotencyBehaviour(
        IIdempotencyService idempotencyService,
        IIdempotencyKeyAccessor idempotencyKeyAccessor,
        ILogger<IdempotencyBehaviour<TRequest, TResponse>> logger)
    {
        _idempotencyService = idempotencyService;
        _idempotencyKeyAccessor = idempotencyKeyAccessor;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var idempotencyKey = _idempotencyKeyAccessor.GetIdempotencyKey();
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return await next();

        _logger.LogDebug("[Idempotency] Memeriksa key: {Key} untuk request: {Request}",
            idempotencyKey, typeof(TRequest).Name);

        // Cek apakah request dengan key ini sudah pernah diproses
        var cached = await _idempotencyService.GetCachedResponseAsync(idempotencyKey);
        if (cached is not null)
        {
            _logger.LogInformation(
                "[Idempotency] ✓ Key '{Key}' sudah diproses pada {CachedAt:O}. Mengembalikan cached response.",
                idempotencyKey, cached.CachedAt);

            // Deserialize cached response dan kembalikan langsung
            var cachedResponse = JsonSerializer.Deserialize<TResponse>(cached.SerializedResponse, JsonOptions);
            if (cachedResponse is not null)
                return cachedResponse;
        }

        // Proses request secara normal
        var response = await next();

        // Simpan response ke cache setelah berhasil diproses
        try
        {
            var serialized = JsonSerializer.Serialize(response, JsonOptions);
            await _idempotencyService.CacheResponseAsync(
                idempotencyKey,
                serialized,
                TimeSpan.FromHours(24) // Default fallback; implementasi akan override dari config
            );

            _logger.LogDebug("[Idempotency] Response untuk key '{Key}' berhasil di-cache.", idempotencyKey);
        }
        catch (Exception ex)
        {
            // Jangan gagalkan request utama hanya karena caching gagal
            _logger.LogWarning(ex, "[Idempotency] Gagal menyimpan cache untuk key '{Key}'.", idempotencyKey);
        }

        return response;
    }
}
