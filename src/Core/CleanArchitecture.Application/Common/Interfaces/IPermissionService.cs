namespace CleanArchitecture.Application.Common.Interfaces;

/// <summary>
/// Abstraksi service untuk mengelola dan memvalidasi hak akses (Permissions)
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Mengambil seluruh permission yang dimiliki oleh suatu role
    /// </summary>
    Task<IReadOnlyList<string>> GetPermissionsForRoleAsync(string roleName, CancellationToken ct = default);

    /// <summary>
    /// Mengambil seluruh permission gabungan untuk pengguna berdasarkan semua role yang dimilikinya
    /// </summary>
    Task<IReadOnlyList<string>> GetPermissionsForUserAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Memperbarui daftar permission yang ditetapkan pada suatu role
    /// </summary>
    Task UpdateRolePermissionsAsync(string roleName, IEnumerable<string> permissions, CancellationToken ct = default);

    /// <summary>
    /// Mengambil seluruh permission sistem yang dikelompokkan berdasarkan kategori / modul
    /// </summary>
    Task<IReadOnlyDictionary<string, List<string>>> GetAllPermissionsGroupedAsync(CancellationToken ct = default);
}
