namespace CleanArchitecture.Domain.Entities;

/// <summary>
/// Entity untuk menyimpan riwayat lengkap perubahan data (Audit Trail).
/// Mencatat operasi Created, Updated, dan Deleted termasuk nilai lama dan nilai baru.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Nama entity yang diubah (misalnya "Product")
    /// </summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>
    /// Aksi yang dilakukan: Created | Updated | Deleted
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Primary key dari entity terkait
    /// </summary>
    public string PrimaryKey { get; set; } = string.Empty;

    /// <summary>
    /// Nilai properti sebelum perubahan (format JSON)
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// Nilai properti setelah perubahan (format JSON)
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// Daftar nama kolom/properti yang terdampak
    /// </summary>
    public string[]? AffectedColumns { get; set; }

    /// <summary>
    /// ID pengguna yang melakukan perubahan
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Nama pengguna yang melakukan perubahan
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Waktu terjadinya perubahan (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; }
}
