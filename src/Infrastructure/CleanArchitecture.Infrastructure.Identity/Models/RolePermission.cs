namespace CleanArchitecture.Infrastructure.Identity.Models;

/// <summary>
/// Pemetaan relasi antara Role dan Permission granular
/// </summary>
public class RolePermission
{
    public int Id { get; set; }

    /// <summary>
    /// Nama role (misal: "Admin", "Member")
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Nilai permission (misal: "Permissions.Products.View")
    /// </summary>
    public string Permission { get; set; } = string.Empty;
}
