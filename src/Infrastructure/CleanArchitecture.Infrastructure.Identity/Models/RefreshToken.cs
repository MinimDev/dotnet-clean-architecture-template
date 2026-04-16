using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Infrastructure.Identity.Models;

/// <summary>
/// Refresh token entity untuk menyimpan refresh token di database
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }

    /// <summary>
    /// Token string (opaque random value)
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// ID user pemilik token
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Waktu kadaluarsa token
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Apakah token sudah direvoke (logout atau dipakai)
    /// </summary>
    public bool IsRevoked { get; set; } = false;

    /// <summary>
    /// Waktu token dibuat
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ApplicationUser User { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}
