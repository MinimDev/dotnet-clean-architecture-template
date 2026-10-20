using CleanArchitecture.Application.Features.AppSettings.DTOs;

namespace CleanArchitecture.Application.Common.Interfaces;

/// <summary>
/// Layanan untuk mengakses dan mengubah AppSettings serta Feature Flags dengan in-memory cache dan fallback
/// </summary>
public interface IAppSettingsService
{
    /// <summary>
    /// Mendapatkan nilai setting sebagai string. Jika tidak ada di DB, fallback ke defaultValue lalu ke IConfiguration.
    /// </summary>
    Task<string?> GetAsync(string key, string? defaultValue = null, CancellationToken ct = default);

    /// <summary>
    /// Mendapatkan nilai setting terkonversi ke tipe data tertentu (e.g. bool, int, double).
    /// </summary>
    Task<T?> GetAsync<T>(string key, T? defaultValue = default, CancellationToken ct = default);

    /// <summary>
    /// Memperbarui nilai setting di database dan meng-invalidate cache.
    /// </summary>
    Task SetAsync(string key, string value, CancellationToken ct = default);

    /// <summary>
    /// Mengecek apakah feature flag tertentu aktif (boolean true).
    /// </summary>
    Task<bool> IsFeatureEnabledAsync(string featureKey, CancellationToken ct = default);

    /// <summary>
    /// Mendapatkan seluruh daftar settings (opsional filter berdasarkan grup).
    /// </summary>
    Task<IReadOnlyList<AppSettingDto>> GetAllAsync(string? group = null, CancellationToken ct = default);
}
