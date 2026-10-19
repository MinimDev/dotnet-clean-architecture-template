namespace CleanArchitecture.Application.Common.Interfaces;

/// <summary>
/// Record yang mewakili response yang telah di-cache untuk request idempoten.
/// </summary>
/// <param name="SerializedResponse">Response body yang sudah di-serialize ke JSON string.</param>
/// <param name="CachedAt">Waktu saat response pertama kali di-cache.</param>
public record IdempotencyResult(string SerializedResponse, DateTime CachedAt);

/// <summary>
/// Abstraksi untuk menyimpan dan mengambil cached response berdasarkan Idempotency Key.
/// Digunakan oleh <see cref="CleanArchitecture.Application.Common.Behaviours.IdempotencyBehaviour{TRequest, TResponse}"/>
/// untuk mencegah pemrosesan duplikat pada request yang sama.
///
/// Implementasi: <see cref="CleanArchitecture.Infrastructure.Shared.Services.IdempotencyService"/>
///
/// Cara menggunakan (dari HTTP Client):
/// <code>
/// POST /api/v1/products
/// Idempotency-Key: 550e8400-e29b-41d4-a716-446655440000
/// </code>
/// </summary>
public interface IIdempotencyService
{
    /// <summary>
    /// Mengambil cached response berdasarkan idempotency key.
    /// Mengembalikan null jika key belum pernah diproses atau sudah expired.
    /// </summary>
    /// <param name="idempotencyKey">Key unik yang dikirim oleh client (biasanya Guid).</param>
    Task<IdempotencyResult?> GetCachedResponseAsync(string idempotencyKey);

    /// <summary>
    /// Menyimpan response ke cache setelah request berhasil diproses.
    /// </summary>
    /// <param name="idempotencyKey">Key unik yang dikirim oleh client.</param>
    /// <param name="serializedResponse">Response body dalam bentuk JSON string.</param>
    /// <param name="ttl">Durasi cache. Default mengikuti konfigurasi <c>Idempotency:CacheDurationHours</c>.</param>
    Task CacheResponseAsync(string idempotencyKey, string serializedResponse, TimeSpan ttl);
}
